
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers;

[Authorize(Roles = "Organiser")]
public class CategoryController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoryController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpRequestMessage CreateApiRequest(
        HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");
        }

        return request;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Get, "api/Category");

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to retrieve categories.";
                return View(new List<CategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            return View(categories ?? new List<CategoryViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = "Unable to connect to the RaceDay API.";
            return View(new List<CategoryViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEvents();
        return View(new CategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEvents();
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Post, "api/Category");

            request.Content = JsonContent.Create(new
            {
                categoryName = model.CategoryName,
                distance = model.Distance,
                eventID = model.EventID
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("",
                $"Unable to create category: {await response.Content.ReadAsStringAsync()}");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the RaceDay API.");
        }

        await LoadEvents();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Get, $"api/Category/{id}");

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var category = await response.Content
                .ReadFromJsonAsync<CategoryViewModel>();

            if (category == null)
                return NotFound();

            await LoadEvents();
            return View(category);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, "Unable to connect to the RaceDay API.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEvents();
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Put, $"api/Category/{id}");

            request.Content = JsonContent.Create(new
            {
                categoryID = id,
                categoryName = model.CategoryName,
                distance = model.Distance
            });

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("",
                $"Unable to update category: {await response.Content.ReadAsStringAsync()}");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the RaceDay API.");
        }

        await LoadEvents();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Delete, $"api/Category/{id}");

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Category deleted successfully!";
            else
                TempData["ErrorMessage"] =
                    $"Unable to delete category: {await response.Content.ReadAsStringAsync()}";
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Unable to connect to the RaceDay API.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEvents()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            using var request = CreateApiRequest(
                HttpMethod.Get, "api/Event");

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                var organiserId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                ViewBag.Events = events?
                    .Where(e => e.OrganiserID.ToString() == organiserId)
                    .ToList() ?? new List<EventViewModel>();
            }
            else
            {
                ViewBag.Events = new List<EventViewModel>();
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.Events = new List<EventViewModel>();
        }
    }
}
