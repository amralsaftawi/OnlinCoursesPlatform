using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers.StudentControllers
{
    public class EnrollmentsController : Controller
    {
        // GET: EnrollmentsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: EnrollmentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EnrollmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)// ViewModel made of student & course info
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

        // GET: EnrollmentsController/Delete/5
        public ActionResult Delete(int StudentId, int CourseId)
        {
            return View();
        }

        // POST: EnrollmentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int StudentId, int CourseId, IFormCollection collection)
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
