using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers.InstructorControllers
{
    public class SectionsController : Controller
    {
        // GET: SectionsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: SectionsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SectionsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SectionsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: SectionsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SectionsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: SectionsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SectionsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
