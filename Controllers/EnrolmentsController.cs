using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers;

[Authorize(Roles = "Participant")]
public class EnrolmentsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EnrolmentsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            ViewBag.ErrorMessage =
                "Your API session is missing. Please log out and log in again.";

            return View(new List<EnrolmentViewModel>());
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Enrolment/mine");

            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"RaceDayAuth={apiCookie}");

            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                ViewBag.ErrorMessage =
                    "Your API session has expired or is not authorised. Please log in again.";

                return View(new List<EnrolmentViewModel>());
            }

            response.EnsureSuccessStatusCode();

            var enrolments =
                await response.Content.ReadFromJsonAsync<List<EnrolmentViewModel>>();

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }
        catch (HttpRequestException ex)
        {
            ViewBag.ErrorMessage =
                $"API connection error: {ex.Message}";

            return View(new List<EnrolmentViewModel>());
        }
        catch (System.Text.Json.JsonException)
        {
            ViewBag.ErrorMessage =
                "The enrolment data could not be read.";

            return View(new List<EnrolmentViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (string.IsNullOrWhiteSpace(apiCookie))
        {
            TempData["ErrorMessage"] =
                "Your API session is missing. Please log out and log in again.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/Enrolment/{id}");

            request.Headers.TryAddWithoutValidation(
                "Cookie",
                $"RaceDayAuth={apiCookie}");

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Your enrolment has been cancelled successfully.";
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] =
                    "The enrolment could not be found.";
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized ||
                     response.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] =
                    "You are not authorised to cancel this enrolment. Please log in again.";
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "The enrolment could not be cancelled."
                        : errorMessage;
            }
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Could not connect to the RaceDay API. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }
}
