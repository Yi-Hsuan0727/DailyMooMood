using System.Data.SQLite;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using DailyMooMood.Models;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyMooMood.Controllers
{
    public class HomeController : Controller
    {
        private const string connectionString = "data source=.\\mydb.db";

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SignInAsync(model.Email);

                return RedirectToAction("LandingPage");
            }

            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SignInAsync(model.Email);

                return RedirectToAction("LandingPage");
            }

            return View();
        }

        private async Task SignInAsync(string email)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,  email.Split('@')[0]),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        [Authorize]
        public async Task<IActionResult> LandingPage()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var sql =
                "SELECT * FROM MOOD where Date =@date";
            var models = await conn.QueryAsync<LandingPageViewModel>(sql, new { date = DateTime.Today.ToString("yyyy/MM/dd") });
            var model = models.FirstOrDefault();

            await SetJson();

            return View(model);
        }

        private async Task SetJson()
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var sql =
                "SELECT Type,Date FROM MOOD";
            var models = await conn.QueryAsync<LandingPageViewModel>(sql);

            ViewBag.Json = JsonSerializer.Serialize(models);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LandingPage(LandingPageViewModel model)
        {
            var date = DateTime.Today.ToString("yyyy/MM/dd");

            if (ModelState.IsValid)
            {
                using var conn = new SQLiteConnection(connectionString);
                await conn.OpenAsync();

                if (string.IsNullOrEmpty(model.Id))
                {
                    var insertScript =
    "INSERT INTO Mood VALUES (@Id, @Type, @Comment, @Date)";
                    await conn.ExecuteAsync(insertScript, new
                    {
                        Id = Guid.NewGuid().ToString(),
                        model.Type,
                        model.Comment,
                        Date = date,
                    });
                }
                else
                {
                    var updateScript =
    "Update Mood SET Type = @Type, Comment=@Comment WHERE Id= @Id";
                    await conn.ExecuteAsync(updateScript, new
                    {
                        model.Id,
                        model.Type,
                        model.Comment,
                    });
                }

                return RedirectToAction("Clues", new { date });
            }

            await SetJson();

            return View();
        }

        private readonly Dictionary<int, List<CluesViewModel>> CluesDict = new()
        {
            {1,[
                new("Deep Breathing Meditation",
                "Find a quiet place, close your eyes, take 5¡V10 deep breaths, and focus on your inhale and exhale. This helps reduce stress and stabilize your emotions."),
                new("Seek Warm Companionship",
                "Talk to a close family member or friend, share your feelings, or simply spend time with them to feel emotionally supported.")
            ]},
            {2,[
                new("Move Your Body",
                "Take a walk, go for a run, or dance to your favorite music. Physical movement releases endorphins and improves your mood."),
                new("Write a Gratitude List",
                "List three things you¡¦re grateful for, even small ones. This helps shift your focus and improve your perspective.")
            ]},
            {3,[
                new("Try a Creative Challenge",
                "Draw a simple picture, write a short poem, or cook a new dish. Doing something creative adds freshness and satisfaction to your day."),
                new("Tackle a Small Goal",
                "Complete a small task you¡¦ve been putting off, like organizing a drawer or replying to an email. This gives you a sense of accomplishment.")
            ]},
            {4,[
                new("Reinforce Positive Emotions",
                "Write down things that made you feel good today, and add a few encouraging words for yourself."),
                new("Share Your Happiness",
                "Tell a friend or post on social media about the good things happening to you. Sharing positivity can amplify your happiness.")
            ]},
            {5,[
                new("Expand Your Impact",
                "Use your good mood to help others, like surprising a friend or volunteering for a cause. Spreading joy makes your happiness even greater."),
                new("Celebrate the Moment",
                "Plan a small celebration for yourself, like enjoying a delicious meal or treating yourself to a special reward.")
            ]},
        };

        [Authorize]
        public async Task<IActionResult> Clues(string date)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var sql =
                "SELECT Type FROM MOOD where Date =@date";
            var models = await conn.QueryAsync<LandingPageViewModel>(sql, new { date });
            var model = models.FirstOrDefault();

            if (model is not null
                && model.Type.HasValue
                && CluesDict.TryGetValue(model.Type.Value, out var clues))
            {
                Random random = new();
                int randomValue = random.Next(0, 2);

                await SetJson();

                return View(clues[randomValue]);
            }

            return RedirectToAction("LandingPage");
        }

        [Authorize]
        public async Task<IActionResult> Today(string date)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            var sql =
                "SELECT * FROM MOOD where Date =@date";
            var models = await conn.QueryAsync<LandingPageViewModel>(sql, new { date });
            var model = models.FirstOrDefault();

            if (model is not null)
            {
                await SetJson();

                return View(model);
            }

            return RedirectToAction("LandingPage");
        }

        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(actionName: "Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
