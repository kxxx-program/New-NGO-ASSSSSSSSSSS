using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_Web_Demo.Models;


namespace NGO_Web_Demo.Controllers;
public class RecruitmentController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    public RecruitmentController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    public IActionResult index()
    {
        return View();
    }

    //generate next id for volunteers
    private string NextId()
    {
        var lastId = db.Volunteers
                       .OrderByDescending(v => v.VolunteerID)
                       .Select(v => v.VolunteerID)
                       .FirstOrDefault();

        if (lastId == null)
            return "V000";

        // Remove the 'V' and parse the number
        int number = int.Parse(lastId.Substring(1));

        // Increment and format with 3 digits
        return "V" + (number + 1).ToString("D3");
    }

    //GET to display the form 
    public IActionResult AddVolunteer(string eventId)
    {

        if (string.IsNullOrEmpty(eventId))
            return NotFound();

        var e = db.Events.Find(eventId);
        if (e == null) return NotFound();

        //Combine date time from danieal so it fits into shiftstart
        var eventStart = e.EventStartDate.ToDateTime(TimeOnly.MinValue)
                                 .Add(e.EventStartTime);

        var eventEnd = e.EventEndDate.ToDateTime(TimeOnly.MinValue)
                                        .Add(e.EventEndTime);

        var vm = new VolunteerVM
        {
            VolunteerID = NextId(),
        };

        ViewBag.EventTitle = e.EventTitle; // so view can display event name
        ViewBag.EventID = eventId;
        ViewBag.EventStart = eventStart.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.EventEnd = eventEnd.ToString("yyyy-MM-ddTHH:mm");

        if (DateTime.Now > eventEnd)
        {
            TempData["Error"] = "Cannot register for a concluded event.";
            ViewBag.IsEventConcluded = true;
            ViewBag.EventTitle = e.EventTitle;

            // Pass the model to show the error message in the view
            var errorModel = new VolunteerVM { EventID = eventId };
            return View(errorModel);
        }


        return View(vm);
    }

    //POST to insert volunteer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVolunteer(string eventId, VolunteerVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);




        //no duplicate email and name can be submitted for the same event
        var dupEmail = await db.VolunteerEvents
          .Include(ve => ve.Volunteer)
          .AnyAsync(ve => ve.EventID == eventId && ve.Volunteer.Email == vm.Email);


        if (dupEmail)
        {
            TempData["Error"] = "This email/user already signed up for this event!";
            return RedirectToAction("RecruitingInfo");
        }


        var v = new Volunteer
        {
            VolunteerID = vm.VolunteerID,
            Name = vm.Name,
            Email = vm.Email,
            PhoneNumber = vm.PhoneNumber,
            Age = vm.Age,

        };

        db.Volunteers.Add(v);
        await db.SaveChangesAsync();


        var ve = new VolunteerEvent
        {
            VolunteerID = v.VolunteerID,
            EventID = eventId,
            ShiftStart = vm.ShiftStart,
            WorkHours = vm.WorkHours,
            Points = 100,
            EventCompletion = EventStatus.Waiting,
            ApprovalStatus = EventApprovalStatus.Pending
        };

        db.VolunteerEvents.Add(ve);
        await db.SaveChangesAsync();


        TempData["Info"] = "You have submitted your form! Please await for the organizer to approve :D";
        return RedirectToAction("RecruitingInfo");

    }

    //GET list of Volunteer

    public IActionResult RecruitingInfo()
    {
        var list = db.Volunteers
       .Join(db.VolunteerEvents,
             v => v.VolunteerID,
             ve => ve.VolunteerID,
             (v, ve) => new VolunteerEventVM
             {
                 VolunteerID = v.VolunteerID,
                 Name = v.Name,
                 Email = v.Email,
                 PhoneNumber = v.PhoneNumber,
                 Age = v.Age,
                 WorkHours = ve.WorkHours,
                 ShiftStart = ve.ShiftStart,
                 EventCompletion = ve.EventCompletion,
                 ApprovalStatus= ve.ApprovalStatus,
                 EventID = ve.EventID,  
             })
       .ToList();

        return View(list);
    }
}