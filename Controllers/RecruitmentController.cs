using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_Web_Demo.Models;
using System.Net.Mail;


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

        var mail = new MailMessage();
        mail.To.Add(new MailAddress(vm.Email, "Volunteer Submission"));
        mail.Subject = "Your submission is being reviewed";
        mail.Body = "Do not reply to this message as it is auto generated";
        mail.IsBodyHtml = true;
        hp.SendEmail(mail);

        //no duplicate email and name can be submitted for the same event
        var dupEmail = await db.VolunteerEvents
          .Include(ve => ve.Volunteer)
          .AnyAsync(ve => ve.EventID == eventId && ve.Volunteer.Email == vm.Email);


        if (dupEmail)
        {
            TempData["Error"] = "This email/user already signed up for this event!";
            return RedirectToAction("Index", "Home");
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
            Points = vm.WorkHours * 10,
            EventCompletion = EventStatus.Waiting,
            ApprovalStatus = EventApprovalStatus.Pending
        };

        db.VolunteerEvents.Add(ve);
        await db.SaveChangesAsync();


        TempData["Info"] = "You have submitted your form! Please await for the organizer to approve :D";
        return RedirectToAction("Index", "Home");

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
                 Points= ve.Points,
                 Id = ve.Id,
             })
       .ToList();

        return View(list);
    }

    //Approve/Reject volunteer

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var ve = await db.VolunteerEvents.FindAsync(id);
        if (ve == null)
        {
            return NotFound();
        }

        ve.ApprovalStatus = EventApprovalStatus.Approved;

        db.VolunteerEvents.Update(ve);
        await db.SaveChangesAsync();


        TempData["Info"] = "Volunteer has been approved!";
        return RedirectToAction("RecruitingInfo");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var ve = await db.VolunteerEvents.FindAsync(id);
        if (ve == null)
        {
            return NotFound();
        }

        ve.ApprovalStatus = EventApprovalStatus.Rejected;

        db.VolunteerEvents.Update(ve);
        await db.SaveChangesAsync();

        TempData["Info"] = "Volunteer has been rejected!";
        return RedirectToAction("RecruitingInfo");
    }
}