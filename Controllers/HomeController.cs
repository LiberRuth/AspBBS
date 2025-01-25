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
                // 사용자의 이메일 주소를 찾기 위해 ClaimsPrincipal에서 Claims 중 이메일 주소를 찾습니다.
                string userEmail = User.FindFirst(ClaimTypes.Email!)?.Value!;

                if (userEmail != null)
                {
                    // userEmail을 기반으로 사용자 정보를 가져옵니다.
                    UserModel user = _userService.GetUserByUsername(userEmail);

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
                string userEmail = User.FindFirst(ClaimTypes.Email!)?.Value!;

                if (userEmail != null)
                {
                    UserModel user = _userService.GetUserByUsername(userEmail);

                    if (user != null)
                    {
                        return View(user);
                    }
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
                // 이미 로그인한 사용자인 경우 홈 페이지로 리디렉션
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
                // 인증 성공
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username!), // 사용자명을 Name 클레임으로 설정
                    new Claim(ClaimTypes.Email, user.Email!),
                    // 다른 클레임 추가
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

            // 인증 실패
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        [HttpGet]
        [Route("/register")]
        public IActionResult Register()
        {
            // 이미 로그인한 사용자인 경우 홈 페이지로 리디렉션
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

            // Cloudflare가 전달한 헤더에서 클라이언트 IP 가져오기
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
                // 기본 IP
                clientIp = HttpContext.Connection.RemoteIpAddress?.ToString()!;
            }

            string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // 이메일 중복 검사
            bool isEmailUnique = _userService.IsEmailUnique(user.Email!);
            if (!isEmailUnique)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(user);
            }

            // 사용자 등록 시도
            bool isSuccess = _userService.RegisterUser(user, clientIp, createdAt);
            if (isSuccess)
            {
                return RedirectToAction("Login");
            }

            // 회원가입 실패 시
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
