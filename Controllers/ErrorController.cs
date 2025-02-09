using Microsoft.AspNetCore.Mvc;

namespace AspBBS.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error/{code}")]
        public IActionResult Index(int code)
        {
            if (code == 404 || code == 0)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            return View("Error"); 
        }

        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }
    }

}
