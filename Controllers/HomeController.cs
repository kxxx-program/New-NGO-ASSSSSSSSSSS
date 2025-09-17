using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGO_Web_Demo;
using NGO_Web_Demo.Models;
using System.Net.Mail;

namespace Demo.Controllers;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public HomeController(IWebHostEnvironment en, Helper hp)
    {
        this.en = en;
        this.hp = hp;
    }

    // GET: Home/Index
    public IActionResult Index()
    {
        return View();
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
