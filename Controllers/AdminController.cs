using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NGO_Web_Demo.Models;
using System.Linq;

namespace NGO_Web_Demo.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;
    public AdminController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    //GET: Admin/Dashboard
    public IActionResult Dashboard()
    {
        var totalMembers = db.Members.Count();
        var recentRegistration = db.Users.OrderByDescending(u => u.ParticipatedDate).Take(10).ToList();

        //pass data to view using ViewModel or ViewBag
        ViewBag.TotalMembers = totalMembers;
        ViewBag.RecentRegistration = recentRegistration;

        return View();
    }

    //GET: Admin/ManageUsers
    public IActionResult ManageUsers()
    {
        var users = db.Users.ToList();
        return View(users);
    }

    //GET: Admin/EditUser/{id}
    public IActionResult EditUser(string name)
    {
        // find user to edit
        var user = db.Users.Find(name);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    //POST: Admin/EditUser/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditUser(string name, [Bind("Name,Email,Phone,Address,ParticipatedDate")] User updatedUser)
    {
        if (name != updatedUser.Name)
        {
            return BadRequest();
        }
        if (ModelState.IsValid)
        {
            try
            {
                db.Update(updatedUser);
                db.SaveChanges();
            }
            catch (Exception)
            {
                if (!db.Users.Any(u => u.Name == name))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(ManageUsers));
        }
        return View(updatedUser);
    }

    //GET: Admin/DeleteUser/{id}
    public IActionResult DeleteUser(string name)
    {
        var user = db.Users.Find(name);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }
    //POST: Admin/DeleteUser/{id}
    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteUserConfirmed(string name)
    {
        var user = db.Users.Find(name);
        if (user == null)
        {
            return NotFound();
        }
        db.Users.Remove(user);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }

    //GET: Admin/ManageEvents
    public IActionResult ManageEvents()
    {
        var events = db.Events.ToList();
        return View(events);
    }
    //GET: Admin/CreateEvent
    public IActionResult CreateEvent()
    {
        return View();
    }
    //POST: Admin/CreateEvent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateEvent([Bind("EventID,EventTitle,EventStartDate,EventEndDate,EventStartTime,EventEndTime,EventStatus,EventLocation,EventDescription")] Event newEvent, IFormFile? EventPhoto)
    {
        if (ModelState.IsValid)
        {
            // Handle file upload
            if (EventPhoto != null && EventPhoto.Length > 0)
            {
                var uploads = Path.Combine(en.WebRootPath, "uploads");
                var filePath = Path.Combine(uploads, EventPhoto.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    EventPhoto.CopyTo(fileStream);
                }
                newEvent.EventPhotoURL = "/uploads/" + EventPhoto.FileName;
            }
            db.Events.Add(newEvent);
            db.SaveChanges();
            return RedirectToAction(nameof(ManageEvents));
        }
        return View(newEvent);
    }
    [HttpPost]
    public IActionResult UpdateEventStatus(string eventId, string newStatus)
    {
        var ev = db.Events.Find(eventId);
        if (ev == null)
        {
            return NotFound();
        }
        ev.EventStatus = newStatus;
        db.SaveChanges();
        return RedirectToAction(nameof(ManageEvents));
    }
    [HttpPost]
    public IActionResult DeleteEvent(string eventId)
    {
        var ev = db.Events.Find(eventId);
        if (ev == null)
        {
            return NotFound();
        }
        db.Events.Remove(ev);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageEvents));
    }
    [HttpPost]
    public IActionResult PromoteToAdmin(string email)
    {
        var user = db.Users.Find(email);
        if (user == null)
        {
            return NotFound();
        }
        var existingAdmin = db.Admins.Find(email);
        if (existingAdmin != null)
        {
            return BadRequest("User is already an admin.");
        }
        var newAdmin = new Admin
        {
            Email = user.Email,
            Hash = user.Hash,
            Name = user.Name
        };
        db.Admins.Add(newAdmin);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }
    [HttpPost]
    public IActionResult DemoteFromAdmin(string email)
    {
        var admin = db.Admins.Find(email);
        if (admin == null)
        {
            return NotFound();
        }
        db.Admins.Remove(admin);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }
    [HttpPost]
    public IActionResult ResetUserPassword(string email)
    {
        var user = db.Users.Find(email);
        if (user == null)
        {
            return NotFound();
        }
        var defaultPassword = "Password123"; // Set a default password
        user.Hash = hp.HashPassword(defaultPassword);
        db.SaveChanges();
        // In a real application, you would send this password to the user's email
        return RedirectToAction(nameof(ManageUsers));
    }
    [HttpPost]
    public IActionResult DeleteUserAccount(string email)
    {
        var user = db.Users.Find(email);
        if (user == null)
        {
            return NotFound();
        }
        db.Users.Remove(user);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }
    [HttpPost]
    public IActionResult DeleteMemberAccount(string email)
    {
        var member = db.Members.Find(email);
        if (member == null)
        {
            return NotFound();
        }
        db.Members.Remove(member);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }
    [HttpPost]
    public IActionResult DeleteVolunteerAccount(string email)
    {
        var volunteer = db.Volunteers.Find(email);
        if (volunteer == null)
        {
            return NotFound();
        }
        db.Volunteers.Remove(volunteer);
        db.SaveChanges();
        return RedirectToAction(nameof(ManageUsers));
    }
}
