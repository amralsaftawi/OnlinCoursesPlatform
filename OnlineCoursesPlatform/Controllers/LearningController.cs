using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineCoursesPlatform.Controllers
{
    public class LearningController : Controller
    {
        // GET: CatigoriesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CatigoriesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CatigoriesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CatigoriesController/Create
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

        // GET: CatigoriesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CatigoriesController/Edit/5
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

        // GET: CatigoriesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CatigoriesController/Delete/5
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
