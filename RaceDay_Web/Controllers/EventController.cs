using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers;

[Authorize]
public class EventsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            var events = await client.GetFromJsonAsync<List<EventViewModel>>(
                "api/Event");

            return View(events ?? new List<EventViewModel>());
        }
        catch (HttpRequestException ex)
        {
            ViewBag.ErrorMessage = $"API connection error: {ex.Message}";
            return View(new List<EventViewModel>());
        }
        catch (System.Text.Json.JsonException ex)
        {
            ViewBag.ErrorMessage = $"API response error: {ex.Message}";
            return View(new List<EventViewModel>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            var eventItem = await client.GetFromJsonAsync<EventViewModel>(
                $"api/Event/{id}");

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Unable to retrieve the event from the RaceDay API.");
        }
        catch (System.Text.Json.JsonException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "The RaceDay API returned invalid event data.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(
        CreateEnrolmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(
                "Details",
                new { id = model.eventID });
        }

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            TempData["ErrorMessage"] =
                "Your session has expired. Please log in again.";

            return RedirectToAction("Details",
                new { id = model.eventID });
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Enrolment");

            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"RaceDayAuth={apiCookie}");

            request.Content = JsonContent.Create(new
            {
                categoryID = model.categoryID
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "You have successfully enrolled in this event category!";
            }
            else
            {
                TempData["ErrorMessage"] =
                    await response.Content.ReadAsStringAsync();
            }
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the RaceDay API.";
        }

        return RedirectToAction(
            "Details",
            new { id = model.eventID });
    }

}
