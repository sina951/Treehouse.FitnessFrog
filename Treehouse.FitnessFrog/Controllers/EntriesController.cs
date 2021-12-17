using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        // call repository to get the list of available entries
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity. Using Linq, Filters our entries with Exclude properties set to true!
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();
            // before we call View() method. We set the property values, remember viewBag helps us pass data from controller to view! 
            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

            return View(entries);
        }
        public ActionResult Add()
        {
            /// How is this possible?( shprt answer:value type to reference type which connects auto to class propeties) 
            /// MVC Model binder will recognize that our parameter is an instance
            /// of a class or reference type instead of a value type like string, int, double, or datytime
            /// and attempt to bind incoming form fields values to its properties, as long as the field names match
            /// the classes property names the entry object's properties will contain the expected values.
            /// Here our action methods pass an instance of the entry data model to our view.
            var entry = new Entry()
            {
                Date = DateTime.Today,
            };

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            ValidateEntry(entry);

            if (ModelState.IsValid)
            {
                _entriesRepository.AddEntry(entry);

                TempData["Message"] = "Your entry was successfully added!";
                // Get back to Homepage main page ---> entry's list page, thanks to the RedirectToAction() method
                // post/redirect/get common web developement design pattern for preventing duplicate form submissions - https://en.wikipedia.org/wiki/Post/Redirect/Get
                return RedirectToAction("Index");
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }
        // remember int? makes id a nullable type. if we didnt allow nulls MCV would throw a routing error. Having null makes it so that entries/delete, entries/edit pass to be successfully routed to our action methods
        // so we are checking if id parameter is null, and returning a 404 error when we reject requests that don't include an ID parameter.
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Entry entry = _entriesRepository.GetEntry((int)id);

            if (entry == null)
            {
                return HttpNotFound();
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        [HttpPost]
        public ActionResult Edit(Entry entry)
        {
            ValidateEntry(entry);
            // Remember the ModelState object automnaticaly is tracking all of form field value errors
            // it is able to tell us if our model is in a valid state
            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);

                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("Index");
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Entry entry = _entriesRepository.GetEntry((int)id);

            if (entry == null)
            {
                return HttpNotFound();
            }

            return View(entry);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            _entriesRepository.DeleteEntry(id);

            TempData["Message"] = "Your entry was successfully deleted!";

            return RedirectToAction("Index");
        }

        private void ValidateEntry(Entry entry)
        {
            // If there aren't any "Duration" field validation errors
            // then make sure that the duration is greater than "0".
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration",
                    "The Duration field value must be greater than '0'.");
            }
        }

        private void SetupActivitiesSelectListItems()
        {
            ViewBag.ActivitiesSelectListItems = new SelectList(
                Data.Data.Activities, "Id", "Name");
        }
    }
}