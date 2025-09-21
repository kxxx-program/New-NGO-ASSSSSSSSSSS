using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Demo.Models;
using NGO_Web_Demo.Models;
using X.PagedList.Extensions;

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

        // GET: EventHistory/Index - Show all events (past and upcoming)
        public IActionResult Index(string? name, string? sort, string? dir, int page = 1)
        {
            // Searching
            ViewBag.Name = name = name?.Trim() ?? "";

            // Get all events
            var query = db.Events.AsQueryable();

            // Apply search filter if name is provided
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.EventTitle.Contains(name) || e.EventLocation.Contains(name));
            }

            // Sorting
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<Event, object> fn = sort switch
            {
                "Title" => e => e.EventTitle,
                "Location" => e => e.EventLocation,
                "StartDate" => e => e.EventStartDate,
                "EndDate" => e => e.EventEndDate,
                "Status" => e => e.EventStatus,
                _ => e => e.EventStartDate, // Default sort by start date
            };

            var sorted = dir == "desc" ?
                         query.OrderByDescending(fn) :
                         query.OrderBy(fn);

            // Paging
            if (page < 1)
            {
                return RedirectToAction(null, new { name, sort, dir, page = 1 });
            }

            var events = sorted.ToPagedList(page, 6); // 6 events per page

            if (page > events.PageCount && events.PageCount > 0)
            {
                return RedirectToAction(null, new { name, sort, dir, page = events.PageCount });
            }

            return View(events);
        }

        // GET: EventHistory/Details/{id} - Show event details
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
                Event_Start_Time = e.EventStartTime,
                Event_End_Time = e.EventEndTime,
                Event_Status = e.EventStatus,
                Event_Location = e.EventLocation,
                Event_Description = e.EventDescription,
                Event_PhotoURL = e.EventPhotoURL
            };

            // FIXED: Check if user can volunteer or donate (past events = no volunteer, but can still donate)
            ViewBag.CanVolunteer = !vm.IsPastEvent; // No volunteering for past events
            ViewBag.CanDonate = true; // Always allow donations
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;

            return View(vm);
        }

        // GET: EventHistory/MyVolunteer - Show current user's volunteer history
        [Authorize]
        public IActionResult MyVolunteer()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get volunteer history
                var userVolunteers = db.Volunteers.Where(v => v.Email == userEmail).ToList();
                var volunteerHistory = new List<VolunteerHistoryVM>();

                if (userVolunteers.Any())
                {
                    var volunteerIds = userVolunteers.Select(v => v.VolunteerID).ToList();
                    volunteerHistory = (from ve in db.VolunteerEvents
                                        join e in db.Events on ve.EventID equals e.EventID
                                        join v in db.Volunteers on ve.VolunteerID equals v.VolunteerID
                                        where volunteerIds.Contains(ve.VolunteerID)
                                        orderby ve.ShiftStart descending
                                        select new VolunteerHistoryVM
                                        {
                                            VolunteerEventID = ve.Id,
                                            EventID = e.EventID,
                                            EventTitle = e.EventTitle,
                                            EventLocation = e.EventLocation,
                                            EventStartDate = e.EventStartDate,
                                            EventEndDate = e.EventEndDate,
                                            ShiftStart = ve.ShiftStart,
                                            WorkHours = ve.WorkHours,
                                            Points = ve.Points,
                                            EventCompletion = ve.EventCompletion,
                                            ApprovalStatus = ve.ApprovalStatus,
                                            IsPastEvent = e.EventEndDate < DateOnly.FromDateTime(DateTime.Today)
                                        }).ToList();
                }

                // Get donation history
                var donationHistory = (from d in db.Donations
                                       join e in db.Events on d.EventId equals e.EventID
                                       where d.UserEmail == userEmail
                                       orderby d.DonationDate descending
                                       select new DonationListVM
                                       {
                                           DonationID = d.DonationID,
                                           UserEmail = d.UserEmail,
                                           EventID = d.EventId,
                                           EventTitle = e.EventTitle,
                                           Amount = d.Amount,
                                           PaymentMethod = d.PaymentMethod,
                                           DonationDate = d.DonationDate,
                                           Status = "Completed"
                                       }).ToList();

                // Calculate statistics
                var totalVolunteerPoints = volunteerHistory.Where(v => v.ApprovalStatus == EventApprovalStatus.Approved).Sum(v => v.Points);
                var totalDonationPoints = (int)donationHistory.Sum(d => d.Amount);

                ViewBag.TotalVolunteers = volunteerHistory.Count;
                ViewBag.TotalPoints = totalVolunteerPoints;
                ViewBag.CompletedEvents = volunteerHistory.Count(v => v.EventCompletion == EventStatus.Completed);
                ViewBag.PendingApprovals = volunteerHistory.Count(v => v.ApprovalStatus == EventApprovalStatus.Pending);
                ViewBag.TotalDonations = donationHistory.Count;
                ViewBag.TotalDonationAmount = donationHistory.Sum(d => d.Amount);
                ViewBag.TotalDonationPoints = totalDonationPoints;
                ViewBag.TotalCombinedPoints = totalVolunteerPoints + totalDonationPoints;

                var combinedVM = new MyActivitiesVM
                {
                    VolunteerHistory = volunteerHistory,
                    DonationHistory = donationHistory
                };

                return View(combinedVM);
            }
            catch (Exception ex)
            {
                TempData["Info"] = $"Error loading activity history: {ex.Message}";

                ViewBag.TotalVolunteers = 0;
                ViewBag.TotalPoints = 0;
                ViewBag.CompletedEvents = 0;
                ViewBag.PendingApprovals = 0;
                ViewBag.TotalDonations = 0;
                ViewBag.TotalDonationAmount = 0;
                ViewBag.TotalDonationPoints = 0;
                ViewBag.TotalCombinedPoints = 0;

                return View(new MyActivitiesVM());
            }
        }

        // GET: EventHistory/Leaderboard - Show volunteer leaderboard
        public IActionResult Leaderboard()
        {
            try
            {
                // Get volunteer points (from approved volunteer events)
                var volunteerPoints = (from ve in db.VolunteerEvents
                                       join v in db.Volunteers on ve.VolunteerID equals v.VolunteerID
                                       where ve.ApprovalStatus == EventApprovalStatus.Approved
                                       group new { v, ve } by new { v.Email, v.Name } into g
                                       select new
                                       {
                                           UserEmail = g.Key.Email,
                                           UserName = g.Key.Name,
                                           VolunteerPoints = g.Sum(x => x.ve.Points),
                                           TotalEvents = g.Count(),
                                           LastVolunteerActivity = g.Max(x => x.ve.ShiftStart)
                                       }).ToList();

                // Get donation points (RM1 = 1 point)
                var donationPoints = (from d in db.Donations
                                      group d by d.UserEmail into g
                                      select new
                                      {
                                          UserEmail = g.Key,
                                          DonationPoints = (int)g.Sum(x => x.Amount), // RM1 = 1 point
                                          TotalDonations = g.Count(),
                                          LastDonationActivity = g.Max(x => x.DonationDate)
                                      }).ToList();

                // Get all unique users (volunteers + donors)
                var allUsers = volunteerPoints.Select(v => v.UserEmail)
                              .Union(donationPoints.Select(d => d.UserEmail))
                              .Distinct()
                              .ToList();

                // Combine points for all users
                var leaderboard = allUsers.Select(email =>
                {
                    var volunteer = volunteerPoints.FirstOrDefault(v => v.UserEmail == email);
                    var donation = donationPoints.FirstOrDefault(d => d.UserEmail == email);

                    var volunteerPts = volunteer?.VolunteerPoints ?? 0;
                    var donationPts = donation?.DonationPoints ?? 0;
                    var totalPts = volunteerPts + donationPts;

                    var lastVolunteerActivity = volunteer?.LastVolunteerActivity ?? DateTime.MinValue;
                    var lastDonationActivity = donation?.LastDonationActivity ?? DateTime.MinValue;
                    var lastActivity = lastVolunteerActivity > lastDonationActivity ? lastVolunteerActivity : lastDonationActivity;

                    return new LeaderboardVM
                    {
                        UserEmail = email,
                        UserName = volunteer?.UserName ?? GetUserName(email),
                        VolunteerPoints = volunteerPts,
                        DonationPoints = donationPts,
                        TotalPoints = totalPts,
                        TotalEvents = volunteer?.TotalEvents ?? 0,
                        TotalDonations = donation?.TotalDonations ?? 0,
                        LastActivity = lastActivity
                    };
                })
                .OrderByDescending(l => l.TotalPoints)
                .ThenByDescending(l => l.TotalEvents)
                .Select((leader, index) => new LeaderboardVM
                {
                    Rank = index + 1,
                    UserEmail = leader.UserEmail,
                    UserName = leader.UserName,
                    VolunteerPoints = leader.VolunteerPoints,
                    DonationPoints = leader.DonationPoints,
                    TotalPoints = leader.TotalPoints,
                    TotalEvents = leader.TotalEvents,
                    TotalDonations = leader.TotalDonations,
                    LastActivity = leader.LastActivity
                })
                .ToList();

                return View(leaderboard);
            }
            catch (Exception ex)
            {
                TempData["Info"] = $"Error loading leaderboard: {ex.Message}";
                return View(new List<LeaderboardVM>());
            }
        }


        private string GetUserName(string email)
        {
            try
            {
                var user = db.Users.Find(email);
                return user?.Name ?? "Unknown User";
            }
            catch
            {
                return "Unknown User";
            }
        }

        private DateTime GetLastDonationDate(string email)
        {
            try
            {
                var lastDonation = db.Donations
                    .Where(d => d.UserEmail == email)
                    .OrderByDescending(d => d.DonationDate)
                    .FirstOrDefault();

                return lastDonation?.DonationDate ?? DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }


    }
}