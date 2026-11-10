
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantDashboardController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ============================================================
        // PARTICIPANT DASHBOARD
        // ============================================================

        public IActionResult Index()
        {
            return View();
        }

        // ============================================================
        // PARTICIPANT RACE RESULTS
        // ============================================================

        public async Task<IActionResult> Results()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

            if (!string.IsNullOrEmpty(apiCookie))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cookie", $"RaceDayAuth={apiCookie}");
            }

            try
            {
                // Get the participant's race results.
                var resultsResponse =
                    await client.GetAsync("api/results/mine");

                if (!resultsResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Could not load your race results.";

                    return View(new List<ParticipantResultViewModel>());
                }

                var resultsJson =
                    await resultsResponse.Content.ReadAsStringAsync();

                var results =
                    JsonSerializer.Deserialize<List<ParticipantResultViewModel>>(
                        resultsJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<ParticipantResultViewModel>();

                // Get participant enrolments to match event and category details.
                var enrolmentsResponse =
                    await client.GetAsync("api/Enrolment/mine");

                if (enrolmentsResponse.IsSuccessStatusCode)
                {
                    var enrolmentsJson =
                        await enrolmentsResponse.Content.ReadAsStringAsync();

                    using var document = JsonDocument.Parse(enrolmentsJson);

                    foreach (var result in results)
                    {
                        foreach (var enrolment in
                            document.RootElement.EnumerateArray())
                        {
                            if (GetInt(enrolment, "enrolmentID")
                                == result.EnrolmentID)
                            {
                                if (TryGetProperty(
                                    enrolment,
                                    "category",
                                    out var category))
                                {
                                    result.CategoryName =
                                        GetString(category, "categoryName")
                                        ?? "Race category";

                                    if (TryGetProperty(
                                        category,
                                        "event",
                                        out var eventInfo))
                                    {
                                        result.EventName =
                                            GetString(eventInfo, "eventName")
                                            ?? "Race event";
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                return View(results);
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "An error occurred while loading your results.";

                return View(new List<ParticipantResultViewModel>());
            }
        }

        // ============================================================
        // JSON HELPER METHODS
        // ============================================================

        private static bool TryGetProperty(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static int GetInt(
            JsonElement element,
            string propertyName)
        {
            return TryGetProperty(element, propertyName, out var value)
                && value.TryGetInt32(out var number)
                ? number
                : 0;
        }

        private static string? GetString(
            JsonElement element,
            string propertyName)
        {
            return TryGetProperty(element, propertyName, out var value)
                && value.ValueKind == JsonValueKind.String
                    ? value.GetString()
                    : null;
        }
    }
}
