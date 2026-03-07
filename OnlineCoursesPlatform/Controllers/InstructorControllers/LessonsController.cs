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

        // GET: LessonsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LessonsController/Create
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

        // GET: LessonsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LessonsController/Edit/5
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

        // GET: LessonsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LessonsController/Delete/5
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
