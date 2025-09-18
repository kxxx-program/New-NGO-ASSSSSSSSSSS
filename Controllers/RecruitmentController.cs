using Microsoft.AspNetCore.Mvc;
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
    
        var eventDetails = db.Events.FirstOrDefault(e => e.EventID == eventId);
        if (eventDetails == null)
            return NotFound();

        var vm = new VolunteerVM
        {
            VolunteerID = NextId(),
            Event_Id = eventId
        };
        return View(vm);
      
    }

    //POST to insert volunteer
    [HttpPost]
    public IActionResult AddVolunteer(VolunteerVM vm)
    {

        if (ModelState.IsValid)
        {

            db.Volunteers.Add(new Volunteer
            {
                VolunteerID = vm.VolunteerID,
                Name = vm.Name,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                Age = vm.Age
            });
            db.SaveChanges();

            TempData["Info"] = "Volunteer added!";
            return RedirectToAction("RecruitingInfo");
        }

        return View();
  
    }

    //GET list of Volunteer

    public IActionResult RecruitingInfo()
    {
        var list = db.Volunteers.Select(v => new VolunteerVM
        {
            VolunteerID = v.VolunteerID,
            Name = v.Name,
            Email = v.Email,
            PhoneNumber = v.PhoneNumber,
            Age = v.Age
        }).ToList();

        return View(list);
    }











}
