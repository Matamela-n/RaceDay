
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RACEDAY_API_PROJECT.Models;

namespace RaceDay_Web.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Display the registration page.
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    // Send registration details to the existing API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/auth/register",
                new
                {
                    firstName = model.firstName,
                    lastName = model.lastName,
                    email = model.email,
                    password = model.Password,
                    role = model.Role
                });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Registration successful. You can now log in.";

                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(
                string.Empty,
                "Registration failed. Check your details or try another email.");

            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Cannot connect to RaceDay API. Please try again.");

            return View(model);
        }
    }

    // Display the login page.
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    // Validate credentials through the existing API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/auth/login",
                new
                {
                    email = model.Email,
                    password = model.Password,
                    role = model.Role
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login failed. Check your email, password and account type.");

                return View(model);
            }

            var loginResult =
                await response.Content.ReadFromJsonAsync<LoginResponseModel>();

            // Validate the response returned by the API.
            if (loginResult == null ||
                loginResult.UserID <= 0 ||
                string.IsNullOrWhiteSpace(loginResult.Email) ||
                string.IsNullOrWhiteSpace(loginResult.Role) ||
                !string.Equals(
                    loginResult.Email,
                    model.Email,
                    StringComparison.OrdinalIgnoreCase) ||
                !IsAllowedRole(loginResult.Role) ||
                !string.Equals(
                    loginResult.Role,
                    model.Role,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API returned an invalid login response.");

                return View(model);
            }

            // Create a verified MVC identity from the API response.

            // Capture the API authentication cookie.
            string? apiCookieValue = null;

            if (response.Headers.TryGetValues(
                "Set-Cookie", out var setCookieHeaders))
            {
                var authCookie = setCookieHeaders.FirstOrDefault(header =>
                    header.StartsWith(
                        "RaceDayAuth=",
                        StringComparison.OrdinalIgnoreCase));

                if (authCookie != null)
                {
                    var cookiePair = authCookie.Split(';', 2)[0];
                    apiCookieValue =
                        cookiePair["RaceDayAuth=".Length..];
                }
            }

            if (string.IsNullOrWhiteSpace(apiCookieValue))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login succeeded, but the API authentication cookie was not received.");

                return View(model);
            }

            // Create a verified MVC identity from the API response.
            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    loginResult.UserID.ToString()),

                new(
                    ClaimTypes.Name,
                    loginResult.Email),

                new(
                    ClaimTypes.Email,
                    loginResult.Email),

                new(
                    ClaimTypes.Role,
                    loginResult.Role),

                // Stored inside the protected MVC authentication ticket.
                new(
                    "RaceDayApiCookie",
                    apiCookieValue)
            };

            {


                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                // Issue the protected authentication cookie.
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                // Redirect according to the API-confirmed role.
                if (string.Equals(
                    loginResult.Role,
                    "Organiser",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Index",
                        "OrganiserDashboard");
                }

                return RedirectToAction(
                    "Index",
                    "ParticipantDashboard");
            }
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Cannot connect to RaceDay API. Please try again.");

            return View(model);
        }
        catch (JsonException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The API returned an unreadable login response.");

            return View(model);
        }
    }

    // Sign out of the MVC application.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    private static bool IsAllowedRole(string role)
    {
        return string.Equals(
                   role,
                   "Participant",
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   role,
                   "Organiser",
                   StringComparison.OrdinalIgnoreCase);
    }
}
