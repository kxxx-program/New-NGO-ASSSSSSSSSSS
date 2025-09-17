using Demo.Models;
using Microsoft.AspNetCore.Authorization;
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
            var model = db.Events.ToList();
            return View(model);
        }

        public IActionResult Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Info"] = "Event ID is required.";
                return RedirectToAction("Index");
            }

            var e = db.Events.Find(id);

            if (e == null)
            {
                TempData["Info"] = "Event not found.";
                return RedirectToAction("Index");
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

        // Personal Volunteer History
        [Authorize]
        public IActionResult MyVolunteer()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Info"] = "User not authenticated.";
                return RedirectToAction("Login", "Account");
            }

            // Get user's volunteer history with event details
            var volunteers = (from v in db.Volunteers
                              join e in db.Events on v.EventID equals e.EventID
                              where v.UserEmail == userEmail
                              select new VolunteerVM
                              {
                                  VolunteerID = v.VolunteerID,
                                  UserEmail = v.UserEmail,
                                  EventID = v.EventID,
                                  EventTitle = e.EventTitle,
                                  EventDate = e.EventStartDate, // Using StartDate as main event date
                                  EventLocation = e.EventLocation,
                                  VolunteerDate = v.VolunteerDate,
                                  Points = v.Points,
                                  Status = v.Status
                              }).OrderByDescending(v => v.VolunteerDate).ToList();

            ViewBag.TotalPoints = volunteers.Sum(v => v.Points);
            ViewBag.TotalVolunteers = volunteers.Count();

            return View(volunteers);
        }

        // Leaderboard - Show top volunteers by points
        public IActionResult Leaderboard()
        {
            var leaderboard = (from v in db.Volunteers
                               join u in db.Users on v.UserEmail equals u.Email
                               group v by new { v.UserEmail, u.Name } into g
                               select new LeaderboardVM
                               {
                                   UserEmail = g.Key.UserEmail,
                                   UserName = g.Key.Name,
                                   TotalPoints = g.Sum(x => x.Points),
                                   TotalEvents = g.Count(),
                                   LastActivity = g.Max(x => x.VolunteerDate)
                               })
                              .OrderByDescending(x => x.TotalPoints)
                              .ThenByDescending(x => x.LastActivity)
                              .Take(50) // Show top 50
                              .ToList();

            // Add ranking
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return View(leaderboard);
        }

        // TESTING ONLY: Volunteer for an event (comment out in production)
        [HttpPost]
        [Authorize]
        public IActionResult VolunteerForEvent(string eventId)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(eventId))
            {
                TempData["Info"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            // Check if user already volunteered for this event
            var existingVolunteer = db.Volunteers.FirstOrDefault(v => v.UserEmail == userEmail && v.EventID == eventId);
            if (existingVolunteer != null)
            {
                TempData["Info"] = "You have already volunteered for this event.";
                return RedirectToAction("Index");
            }

            // Add volunteer record
            var volunteer = new Volunteer
            {
                UserEmail = userEmail,
                EventID = eventId,
                VolunteerDate = DateTime.Now,
                Points = 100, // Fixed points for testing
                Status = "Active"
            };

            db.Volunteers.Add(volunteer);
            db.SaveChanges();

            TempData["Info"] = $"Thank you for volunteering! You earned {volunteer.Points} points.";
            return RedirectToAction("MyVolunteer");
        }

        // TESTING ONLY - Add test volunteer data
        [HttpGet]
        [Authorize]
        public IActionResult AddTestData()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Info"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            // Get first few events for testing
            var events = db.Events.Take(3).ToList();

            foreach (var evt in events)
            {
                // Check if user already has volunteer record for this event
                if (!db.Volunteers.Any(v => v.UserEmail == userEmail && v.EventID == evt.EventID))
                {
                    var volunteer = new Volunteer
                    {
                        UserEmail = userEmail,
                        EventID = evt.EventID,
                        VolunteerDate = DateTime.Now.AddDays(-new Random().Next(1, 30)),
                        Points = new Random().Next(50, 150),
                        Status = new string[] { "Active", "Completed", "Donated" }[new Random().Next(3)]
                    };

                    db.Volunteers.Add(volunteer);
                }
            }

            db.SaveChanges();
            TempData["Info"] = "Test volunteer data added successfully!";
            return RedirectToAction("MyVolunteer");
        }

        // TESTING ONLY: Donate for an event (comment out in production)
        [HttpPost]
        [Authorize]
        public IActionResult DonateForEvent(string eventId, decimal amount = 50.00m)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(eventId))
            {
                TempData["Info"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            // Add donation record with points
            var donation = new Donation
            {
                UserEmail = userEmail,
                EventId = eventId,
                Amount = amount,
                PaymentMethod = "Test",
                DonationDate = DateTime.Now
            };

            db.Donations.Add(donation);

            // Also add volunteer points for donating (for testing)
            var volunteer = new Volunteer
            {
                UserEmail = userEmail,
                EventID = eventId,
                VolunteerDate = DateTime.Now,
                Points = 50, // Fixed points for donating
                Status = "Donated"
            };

            db.Volunteers.Add(volunteer);
            db.SaveChanges();

            TempData["Info"] = $"Thank you for donating RM{amount}! You earned {volunteer.Points} points.";
            return RedirectToAction("MyVolunteer");
        }
    }
}