using System.Net.Mail;
using Azure;
using Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGO_Web_Demo;
using NGO_Web_Demo.Models;
using X.PagedList.Extensions;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public HomeController(IWebHostEnvironment en, Helper hp, DB db)
    {
        this.en = en;
        this.hp = hp;
        this.db = db;
    }

    // GET: Home/Index
    public IActionResult Index(string? name, string? sort, string? dir, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Events.Where(e => e.EventTitle.Contains(name));

        // (2) Sorting --------------------------
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Event, object> fn = sort switch
        {
            "Photo" => e => e.EventPhotoURL,
            "Event Name" => e => e.EventTitle,
            "Start Date" => e => e.EventStartDate,
            "End Date" => e => e.EventEndDate,
            "Start Time" => e => e.EventStartTime,
            "End Time" => e => e.EventEndTime,
            "Location" => e => e.EventLocation,
            "Description" => e => e.EventDescription,
            "Status" => e => e.EventStatus,
            _ => e => e.EventID,
        };

        var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

        // (3) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, sort, dir, page = 1 });
        }

        var m = sorted.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, sort, dir, page = m.PageCount });
        }

        return View(m);
    }

    // GET: Home/Email
    public IActionResult Email()
    {
        return View();
    }

    //GET: Home/Both
    [Authorize]
    public IActionResult Both()
    {
        return View();
    }

    //GET: Home/Member
    [Authorize(Roles = "Member")]
    public IActionResult Member()
    {
        return View();
    }

    //GET: Home/Admin
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }

    [Authorize(Roles = "Organiser")]
    public IActionResult Organiser()
    {
        return View();
    }

    // GET: Home/OrganiserAndAdmin (if you want combined access)
    [Authorize(Roles = "Organiser,Admin")]
    public IActionResult OrganiserAndAdmin()
    {
        return View();
    }

    //POST: Home/Email
    [HttpPost]
    public IActionResult Email(EmailVM vm)
    {
        if (ModelState.IsValid)
        {
            // Construct Email
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(vm.Email, "TARUMT NGO"));
            mail.Subject = vm.Subject;
            mail.Body = vm.Body;
            mail.IsBodyHtml = vm.IsBodyHtml;

            hp.SendEmail(mail);

            TempData["Info"] = "Email sent.";
            return RedirectToAction();
        }
        return View(vm);
    }

    // POST: Home/Index
    [HttpPost]
    public IActionResult Index(IFormFile photo) // TODO
    {
        // TODO

        if (ModelState.IsValid("photo"))
        {
            // TODO
            //var path = Path.Combine(en.WebRootPath, "uploads", photo.FileName);
            //using var stream = System.IO.File.Create(path);
            //photo.CopyTo(stream);

            var e = hp.ValidatePhoto(photo);
            if (e != "") ModelState.AddModelError("photo", e);
        }

        if (ModelState.IsValid)
        {
            hp.SavePhoto(photo, "uploads");

            TempData["Info"] = "Photo uploaded.";
            return RedirectToAction();
        }

        return View();
    }

    // GET: Home/Browse
    public IActionResult Browse()
    {
        // TODO
        var path = Path.Combine(en.WebRootPath, "uploads");
        var files = Directory.GetFiles(path, "*.jpg").Select(p => Path.GetFileName(p));

        return View(files);
    }

    // POST: Home/Delete
    [HttpPost]
    public IActionResult Delete(string file)
    {
        // TODO
        hp.DeletePhoto(file, "uploads");

        TempData["Info"] = "Photo deleted.";
        return RedirectToAction("Browse");
    }

    // POST: Home/DeleteAll
    [HttpPost]
    public IActionResult DeleteAll()
    {
        var path = Path.Combine(en.WebRootPath, "uploads");
        var files = Directory.GetFiles(path, "*.jpg");

        foreach (var file in files)
        {
            System.IO.File.Delete(file);
        }

        TempData["Info"] = "All photos deleted.";
        return RedirectToAction("Browse");
    }

    
}
