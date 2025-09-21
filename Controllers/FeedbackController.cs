using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using NGO_Web_Demo.Models;

namespace NGO_Web_Demo.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly DB db;
        private readonly IWebHostEnvironment en;
        private readonly Helper hp;
        public FeedbackController(DB db, IWebHostEnvironment en, Helper hp)
        {
            this.db = db;
            this.en = en;
            this.hp = hp;
        }

        //generate next id for volunteers
        private string NextId()
        {
            var lastId = db.Feedbacks
                           .OrderByDescending(f => f.FeedbackID)
                           .Select(f => f.FeedbackID)
                           .FirstOrDefault();

            if (lastId == null)
                return "F000";

            // Remove the 'V' and parse the number
            int number = int.Parse(lastId.Substring(1));

            // Increment and format with 3 digits
            return "F" + (number + 1).ToString("D3");
        }

        //GET to display the form 
        public IActionResult FeedbackForm(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return NotFound();

            var e = db.Events.Find(eventId);
            if (e == null) return NotFound();


            var vm = new FeedbackVM
            {
                FeedbackID = NextId(),
            };


            ViewBag.EventTitle = e.EventTitle; // so view can display event name
            ViewBag.EventID = eventId;


            return View(vm);
        }
    }
}