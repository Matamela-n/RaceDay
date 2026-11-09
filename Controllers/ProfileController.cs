using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay_Web.Models;

namespace RaceDay_Web.Controllers;

[Authorize(Roles = "Participant,Organiser")]
public class ProfileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProfileController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var apiCookie = User.FindFirst("RaceDayApiCookie")?.Value;

        if (!string.IsNullOrEmpty(apiCookie))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "Cookie", $"RaceDayAuth={apiCookie}");
        }

        return client;
    }

    // GET: Profile
    public async Task<IActionResult> Index()
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.GetAsync("api/users/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load your profile.";
                return View(new ProfileViewModel());
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            return View(profile ?? new ProfileViewModel());
        }
        catch
        {
            TempData["Error"] = "An error occurred while loading your profile.";
            return View(new ProfileViewModel());
        }
    }

    // POST: Profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = CreateApiClient();

            var response = await client.PutAsJsonAsync(
                "api/users/me", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Your profile was updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(
                string.Empty, "Unable to update your profile. Please try again.");

            return View(model);
        }
        catch
        {
            ModelState.AddModelError(
                string.Empty, "An error occurred while updating your profile.");

            return View(model);
        }
    }
    // POST: Profile/UploadImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image to upload.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var client = CreateApiClient();

            using var content = new MultipartFormDataContent();

            using var fileStream = file.OpenReadStream();

            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");

            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync(
                "api/users/me/profile-image", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Profile picture uploaded successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Upload failed: {error}";
            }
        }
        catch
        {
            TempData["Error"] = "An error occurred while uploading your picture.";
        }

        return RedirectToAction(nameof(Index));
    }
}