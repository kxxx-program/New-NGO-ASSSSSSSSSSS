using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using NGO_Web_Demo.Models;

namespace NGO_Web_Demo.Controllers
{
    public class EventHistoryController : Controller
    {
        private readonly DB db;
        private readonly Helper hp;

        public EventHistoryController(DB db, Helper hp)
        {
            this.db = db;
            this.hp = hp;
        }
        public IActionResult Index()
        {
            var model = db.Events.ToList(); // Added ToList() to ensure data is loaded
            return View(model);
        }

        public IActionResult Details(string? id) // Pull/Get the Details from Event
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Info"] = "Event ID is required.";
                return RedirectToAction("Event_Index");
            }

            var e = db.Events.Find(id);

            if (e == null)
            {
                TempData["Info"] = "Event not found.";
                return RedirectToAction("Event_Index");
            }

            var vm = new EventDetailsVM
            {
                Event_Id = e.EventID,
                Event_Title = e.EventTitle,
                Event_Start_Date = e.EventStartDate,
                Event_End_Date = e.EventEndDate,
                Event_Location = e.EventLocation,
                Event_Description = e.EventDescription,
                Event_PhotoURL = e.EventPhotoURL
            };

            return View(vm);
        }
    }
}
