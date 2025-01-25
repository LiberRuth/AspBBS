using AspBBS.Models;
using AspBBS.Service;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace AspBBS.Controllers
{
    public class ListController : Controller
    {
        private readonly WriteService _writeService;
        private readonly UserService _userService;
        private readonly DataService _dataService;

        //public int _page;
        //public string? _gall_Id;
        //public string? _search;
        public int pagenum = 0;
        //public List<PersonModels>? persons;
        private int pageSize = 50;
        public string? searchStr;

        public int backNumber = 2;
        public int nextNumber = 1;

        public ListController(WriteService writeService, UserService userService, DataService dataService)
        {
            _writeService = writeService;
            _userService = userService;
            _dataService = dataService;
        }

        [Route("/list/{tableName}")]
        public IActionResult Index(string tableName, [FromQuery] int page = 1, [FromQuery] string search = null!)
        {
            ViewBag.page = page;
            ViewBag.gall_Id = tableName;

            ViewBag.backNumber = page - 1;
            ViewBag.nextNumber = page + 1;

            if (!string.IsNullOrWhiteSpace(search)) searchStr = $"&search={search}";
            ViewBag.search = search;
            ViewBag.searchStr = searchStr;

            List<DataModel>? listData = _dataService.GetListData(tableName!, search!, page, pageSize);

            ViewBag.Gall_name = tableName;

            int total = _dataService.GetTotalCount(tableName!, search!);
            while (true) { pagenum++; total = total - pageSize; if (total <= 0) { break; } }
            ViewBag.pagenum = pagenum;

            if (listData == null) 
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            if (page > pagenum)
            {
                var queryParams = HttpContext.Request.Query
                    .ToDictionary(k => k.Key, v => v.Value.ToString());

                queryParams["page"] = pagenum.ToString();

                var newQueryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));

                var newUrl = $"{Request.Path}?{newQueryString}";

                return Redirect(newUrl);
            }

            return View(listData);
        }

        [Route("/list/view/{tableName}/{id:int}")]
        public IActionResult Views(string tableName, int id)
        {
            return View();
        }

        [HttpGet]
        [Route("/list/write")]
        public IActionResult Write([FromQuery] string id)
        {
            ViewBag.Id = id;


            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Redirect("/login");
            }

            if (string.IsNullOrWhiteSpace(id) || !_writeService.TableExists(id))
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            return View();
        }

        [HttpPost]
        [Route("/list/write")]
        public IActionResult Write([FromQuery] string id, [Bind("Title,Username,Email,Text")] WriteModel write)
        {
            UserModel user = _userService.GetUserByUsername(User.FindFirst(ClaimTypes.Email!)?.Value!);

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


            if (!ModelState.IsValid)
            {
                return View(write);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError("", "전달되지 않았습니다.");
                return View(write);
            }

            // `DateTime.Now`를 사용하여 생성 날짜를 현재 시간으로 설정
            string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            bool isSuccess = _writeService.TextWrite(id, write.Title, user.Username, user.Email, write.Text, createdAt, clientIp);

            if (isSuccess)
            {
                //return RedirectToAction(id);
                return Redirect($"/list/{id}");
            }

            ModelState.AddModelError("", "글 작성에 실패했습니다. 다시 시도해주세요.");
            return View(write);
        }
    }
}
