using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserDashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrganiserDashboardController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // ORGANISER DASHBOARD
    public async Task<IActionResult> Index()
    {
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            ViewBag.ErrorMessage =
                "Your API session is missing. Please log out and log in again.";

            return View(new List<EventViewModel>());
        }

        if (!int.TryParse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var organiserId))
        {
            ViewBag.ErrorMessage =
                "Your organiser account could not be identified.";

            return View(new List<EventViewModel>());
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            // Fetch only events created by the logged-in organiser.
            using var eventRequest = new HttpRequestMessage(
                HttpMethod.Get, "api/Event/mine");

            eventRequest.Headers.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");

            using var eventResponse = await client.SendAsync(eventRequest);

            if (eventResponse.StatusCode == HttpStatusCode.Unauthorized ||
                eventResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                ViewBag.ErrorMessage =
                    "Your API session has expired. Please log in again.";

                return View(new List<EventViewModel>());
            }

            eventResponse.EnsureSuccessStatusCode();

            var myEvents = await eventResponse.Content
                .ReadFromJsonAsync<List<EventViewModel>>()
                ?? new List<EventViewModel>();

            // Calculate event statistics.
            ViewBag.TotalEvents = myEvents.Count;

            ViewBag.UpcomingEvents = myEvents.Count(item =>
                item.EventDate >= DateTime.Now);

            // Fetch organiser enrolments.
            using var enrolmentRequest = new HttpRequestMessage(
                HttpMethod.Get, "api/Enrolment/organiser");

            enrolmentRequest.Headers.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");

            using var enrolmentResponse =
                await client.SendAsync(enrolmentRequest);

            if (enrolmentResponse.IsSuccessStatusCode)
            {
                var enrolments = await enrolmentResponse.Content
                    .ReadFromJsonAsync<List<EnrolmentViewModel>>();

                ViewBag.TotalEnrolments =
                    enrolments?.Count ?? 0;
            }
            else
            {
                ViewBag.TotalEnrolments = 0;

                ViewBag.StatisticsMessage =
                    "Enrolment statistics are temporarily unavailable.";
            }

            return View(myEvents);
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage =
                "Could not connect to the RaceDay API. Please try again.";

            return View(new List<EventViewModel>());
        }
        catch (System.Text.Json.JsonException)
        {
            ViewBag.ErrorMessage =
                "The data returned by the API could not be read.";

            return View(new List<EventViewModel>());
        }
    }

    // VIEW ORGANISER ENROLMENTS
    public async Task<IActionResult> Enrolments()
    {
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            ViewBag.ErrorMessage =
                "Your API session is missing. Please log out and log in again.";

            return View(new List<EnrolmentViewModel>());
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "api/Enrolment/organiser");

            request.Headers.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");

            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                ViewBag.ErrorMessage =
                    "Your API session has expired or is not authorised.";

                return View(new List<EnrolmentViewModel>());
            }

            response.EnsureSuccessStatusCode();

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage =
                "Could not connect to the RaceDay API. Please try again.";

            return View(new List<EnrolmentViewModel>());
        }
        catch (System.Text.Json.JsonException)
        {
            ViewBag.ErrorMessage =
                "The enrolment data returned by the API could not be read.";

            return View(new List<EnrolmentViewModel>());
        }
    }

    // GET: CREATE EVENT
    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new CreateEventViewModel());
    }

    // CREATE EVENT AND UPLOAD OPTIONAL BANNER
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(
        CreateEventViewModel model,
        IFormFile? bannerFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            ModelState.AddModelError(
                "", "Your session has expired. Please log in again.");

            return View(model);
        }

        // Validate optional banner image.
        if (bannerFile != null && bannerFile.Length > 0)
        {
            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(
                bannerFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "",
                    "Only JPG, JPEG, PNG and WEBP banner images are allowed.");

                return View(model);
            }

            if (bannerFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "", "Banner image must not exceed 5 MB.");

                return View(model);
            }
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            // First, create the event through the API.
            using var request = new HttpRequestMessage(
                HttpMethod.Post, "api/Event");

            request.Headers.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");

            request.Content = JsonContent.Create(new
            {
                eventName = model.EventName,
                eventDate = model.EventDate,
                location = model.Location,
                eventType = model.EventType,
                description = model.Description
            });

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    $"Unable to create event. API response: {error}");

                return View(model);
            }

            // Get the newly created event ID.
            using var eventJson =
                await System.Text.Json.JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync());

            if (!eventJson.RootElement.TryGetProperty(
                "eventID", out var eventIdProperty))
            {
                TempData["SuccessMessage"] =
                    "Event created, but its banner could not be uploaded.";

                return RedirectToAction(nameof(Index));
            }

            int eventId = eventIdProperty.GetInt32();

            // Upload the banner if the organiser selected one.
            if (bannerFile != null && bannerFile.Length > 0)
            {
                using var multipartContent =
                    new MultipartFormDataContent();

                using var fileStream = bannerFile.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);

                multipartContent.Add(
                    streamContent,
                    "file",
                    Path.GetFileName(bannerFile.FileName));

                using var bannerRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/Event/{eventId}/banner");

                bannerRequest.Headers.TryAddWithoutValidation(
                    "Cookie", $"RaceDayAuth={apiCookie}");

                bannerRequest.Content = multipartContent;

                using var bannerResponse =
                    await client.SendAsync(bannerRequest);

                if (bannerResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event and banner created successfully!";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        "Event created successfully, but the banner upload failed.";
                }
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Event created successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "", "Unable to connect to the RaceDay API.");
        }
        catch (System.Text.Json.JsonException)
        {
            ModelState.AddModelError(
                "", "Could not read the event information returned by the API.");
        }

        return View(model);
    }

    // EDIT EVENT - DISPLAY FORM
    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"api/Event/{id}");

        request.Headers.TryAddWithoutValidation(
            "Cookie", $"RaceDayAuth={apiCookie}");

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var eventItem = await response.Content
            .ReadFromJsonAsync<EventViewModel>();

        if (eventItem == null)
            return NotFound();

        // Check that this event belongs to the logged-in organiser.
        var organiserId = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (eventItem.OrganiserID.ToString() != organiserId)
            return Forbid();

        var model = new CreateEventViewModel
        {
            EventName = eventItem.EventName,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            EventType = eventItem.EventType,
            Description = eventItem.Description
        };

        ViewBag.EventID = id;

        return View(model);
    }

    // EDIT EVENT - SAVE CHANGES
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(
        int id, CreateEventViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventID = id;
            return View(model);
        }

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            ModelState.AddModelError(
                "", "Your session has expired. Please log in again.");

            ViewBag.EventID = id;
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Put, $"api/Event/{id}");

        request.Headers.TryAddWithoutValidation(
            "Cookie", $"RaceDayAuth={apiCookie}");

        request.Content = JsonContent.Create(new
        {
            eventName = model.EventName,
            eventDate = model.EventDate,
            location = model.Location,
            eventType = model.EventType,
            description = model.Description
        });

        try
        {
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Event updated successfully!";

                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                $"Unable to update event. API response: {error}");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "", "Unable to connect to the RaceDay API.");
        }

        ViewBag.EventID = id;

        return View(model);
    }

    // DELETE EVENT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            TempData["ErrorMessage"] =
                "Your session has expired. Please log in again.";

            return RedirectToAction(nameof(Index));
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Delete, $"api/Event/{id}");

        request.Headers.TryAddWithoutValidation(
            "Cookie", $"RaceDayAuth={apiCookie}");

        try
        {
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Event deleted successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Unable to delete event. API response: {error}";
            }
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the RaceDay API.";
        }

        return RedirectToAction(nameof(Index));
    }

    // CONFIRM OR REJECT ENROLMENT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(
        int id, string status)
    {
        if (status != "Confirmed" && status != "Rejected")
        {
            TempData["Error"] = "Invalid enrolment status.";

            return RedirectToAction(nameof(Enrolments));
        }

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
            return RedirectToAction("Login", "Auth");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/Enrolment/organiser/{id}/status");

        request.Headers.TryAddWithoutValidation(
            "Cookie", $"RaceDayAuth={apiCookie}");

        request.Content = JsonContent.Create(new { Status = status });

        try
        {
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    $"Enrolment successfully {status.ToLower()}.";
            }
            else
            {
                TempData["Error"] =
                    $"Could not update enrolment: {await response.Content.ReadAsStringAsync()}";
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] =
                "Could not connect to the RaceDay API.";
        }

        return RedirectToAction(nameof(Enrolments));
    }
}

