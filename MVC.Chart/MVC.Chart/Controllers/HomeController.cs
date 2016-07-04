using System.Web.Mvc;

namespace MVC.Chart.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }
    }
}