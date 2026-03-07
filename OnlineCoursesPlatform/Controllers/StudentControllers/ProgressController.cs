using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers.StudentControllers
{
    public class ProgressController : Controller
    {
        // GET: ProgressController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProgressController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProgressController/Reset/5
        public ActionResult Reset(int id)
        {
            return View();
        }

        // POST: ProgressController/Reset/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reset(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
