using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var e = db.Events.Find(eventId);

            string currentUserEmail = User.Identity?.Name ?? "";
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                TempData["Error"] = "You must be logged in to submit feedback.";
                return RedirectToAction("Login", "Account");
            }

            var volunteerParticipation = db.VolunteerEvents
                .Include(ve => ve.Volunteer)
                .FirstOrDefault(ve => ve.EventID == eventId &&
                                      ve.Volunteer.Email == currentUserEmail &&
                                      ve.ApprovalStatus == EventApprovalStatus.Approved);

            if (volunteerParticipation == null)
            {
                TempData["Error"] = "You can only provide feedback for events you participated in as an approved volunteer.";
                return RedirectToAction("Event_Details", "NGO_Event_", new { id = eventId });
            }

            bool hasSubmittedFeedback = db.Feedbacks.Any(f => f.EventID == eventId &&
                                                               f.VolunteerID == volunteerParticipation.VolunteerID);

            if (hasSubmittedFeedback)
            {
                TempData["Info"] = "You have already submitted feedback for this event.";
                return RedirectToAction("Event_Details", "NGO_Event_", new { id = eventId });
            }

            if (e.EventEndDate >= DateOnly.FromDateTime(DateTime.Today))
            {
                TempData["Error"] = "You can only provide feedback after the event has concluded.";
                return RedirectToAction("Event_Details", "NGO_Event_", new { id = eventId });
            }



            var vm = new FeedbackVM
            {
                FeedbackID = NextId(),
                EventID = eventId,
                VolunteerID = volunteerParticipation.VolunteerID,
                EventTitle = e.EventTitle,
                EventLocation = e.EventLocation,
                EventStartDate = e.EventStartDate,
                EventEndDate = e.EventEndDate,
                VolunteerName = volunteerParticipation.Volunteer.Name,
                VolunteerEmail = volunteerParticipation.Volunteer.Email
            };


            ViewBag.EventTitle = e.EventTitle; // so view can display event name
            ViewBag.EventID = eventId;


            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedbackForm(FeedbackVM vm)
        {
           


            var feedback = new Feedback
            {
                FeedbackID = NextId(),
                EventID = vm.EventID,
                VolunteerID = vm.IsAnonymous ? null : vm.VolunteerID,
                Rating = vm.Rating,
                Comments = vm.Comments,
                SubmittedAt = DateTime.Now
            };



            db.Feedbacks.Add(feedback);
            await db.SaveChangesAsync();

            TempData["Info"] = vm.IsAnonymous ?
                "Thank you for your anonymous feedback!" :
                "Thank you for your feedback!";

            return RedirectToAction("Index", "Home");


        }

        public IActionResult FeedbackList()
        {
            var list = db.Feedbacks
         .Select(f => new FeedbackVM
         {
             FeedbackID = f.FeedbackID,
             EventID = f.EventID,
             EventTitle = f.Event.EventTitle,
             EventLocation = f.Event.EventLocation,
             Rating = f.Rating,
             Comments = f.Comments,
             IsAnonymous = f.VolunteerID == null
         })
         .ToList();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var feedback = await db.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            db.Feedbacks.Remove(feedback);
            await db.SaveChangesAsync();

            TempData["Info"] = "Feedback deleted successfully.";
            return RedirectToAction("FeedbackList");
        }
    }
}