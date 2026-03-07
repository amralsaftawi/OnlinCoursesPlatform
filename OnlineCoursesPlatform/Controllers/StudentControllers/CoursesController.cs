using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers.StudentControllers
{
    public class CoursesController : Controller
    {
        // GET: CoursesController
        public IActionResult Index()
        {
            return View();
        }

        // GET: CoursesController/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: CoursesController/Search
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        // POST: CoursesController/Search/5
        [HttpPost]
        public IActionResult Search(int id)
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

        // GET: CoursesController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: CoursesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
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
