using MangaPortal.Filters;
using System.Web.Mvc;

namespace MangaPortal.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }
#if !DEBUG
        [OutputCache(Duration=200)]
#endif
        public ActionResult Index()
        {
            return View();
        }
    }
}
