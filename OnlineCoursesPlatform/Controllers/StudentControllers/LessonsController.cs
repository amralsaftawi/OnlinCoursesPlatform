using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers.StudentControllers
{
    public class LessonsController : Controller
    {
        // GET: LessonsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: LessonsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
