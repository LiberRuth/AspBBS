using System.Diagnostics;
using System.Security.Claims;
using AspBBS.Models;
using AspBBS.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AspBBS.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;

        public HomeController(UserService userService)
        {
            _userService = userService;
        }

        [Route("/")]
        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                string userId = User.FindFirst("UserID")?.Value!;

                if (userId != null)
                {
                    UserModel user = _userService.GetUserByUserID(userId);

                    if (user != null)
                    {
                        return View(user);
                    }
                }
            }

            return View();
        }

        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/users")]
        public IActionResult Users()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Redirect("/login");
            }

            if (User.Identity!.IsAuthenticated)
            {
                string userId = User.FindFirst("UserID")?.Value!;

                if (userId != null)
                {
                    UserModel user = _userService.GetUserByUserID(userId);

                    if (user != null)
                    {
                        return View(user);
                    }
                }
                else 
                {
                    Response.StatusCode = 404;
                    return View("NotFound");
                }
            }

            return View();
        }

        [HttpGet]
        [Route("/login")]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                // �̹� �α����� ������� ��� Ȩ �������� ���𷺼�
                return RedirectToAction("Index", "Home");
                //return Redirect("/Home/Login");
            }
            return View();
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _userService.AuthenticateUser(email, password);
            if (user != null)
            {
                // ���� ����
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username!), // ����ڸ��� Name Ŭ�������� ����
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("UserID", user.UserID.ToString())
                    // �ٸ� Ŭ���� �߰�
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(3)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            // ���� ����
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        [HttpGet]
        [Route("/register")]
        public IActionResult Register()
        {
            // �̹� �α����� ������� ��� Ȩ �������� ���𷺼�
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("/register")]
        public IActionResult Register(UserModel user)
        {
            string clientIp;

            // Cloudflare�� ������ ������� Ŭ���̾�Ʈ IP ��������
            if (HttpContext.Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                clientIp = HttpContext.Request.Headers["CF-Connecting-IP"]!;
            }
            else if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                clientIp = HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            else
            {
                // �⺻ IP
                clientIp = HttpContext.Connection.RemoteIpAddress?.ToString()!;
            }

            string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // �̸��� �ߺ� �˻�
            bool isEmailUnique = _userService.IsEmailUnique(user.Email!);
            if (!isEmailUnique)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(user);
            }

            RandomString randomString = new RandomString();
            string ran;

            do
            {
                ran = randomString.GenerateRandomString(24);
            } while (!_userService.IsEmailUnique(ran)); // �ߺ��̸� �ٽ� ����

            // ����� ��� �õ�
            bool isSuccess = _userService.RegisterUser(user, clientIp, createdAt, ran);
            if (isSuccess)
            {
                return RedirectToAction("Login");
            }

            // ȸ������ ���� ��
            ModelState.AddModelError("", "Failed to register user.");
            return View(user);
        }


        [HttpPost]
        [Route("/logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
