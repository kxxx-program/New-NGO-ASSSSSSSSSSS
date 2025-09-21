using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NGO_Web_Demo.Models;
using Demo.Models;
using System;
using System.Linq;
using NGO_Web_Demo.Helpers;

namespace NGO_Web_Demo.Controllers;

public class PaymentController : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    public PaymentController(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }

    //// Show payment page
    [Authorize]
    public IActionResult Donation(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Info"] = "Event ID is required.";
            return RedirectToAction("Event_Index", "NGO_Event_");
        }

        var e = db.Events.Find(id);
        var u = db.Users.Find(User.Identity?.Name);

        if (e == null || u == null)
        {
            TempData["Info"] = "User or event not found. Please log in.";
            return RedirectToAction("Event_Index", "NGO_Event_");
        }

        var model = new PaymentVM
        {
            EventID = e.EventID,
            EventTitle = e.EventTitle,
            EventLocation = e.EventLocation,
            UserEmail = u.Email
        };

        return View("../Payment/Donation", model);
    }

    // Process donation
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]  // Add this back
    public IActionResult Process(PaymentVM model)
    {
        // Skip credit card validation if not card payment
        if (model.PaymentMethod != "Credit Card" && model.PaymentMethod != "Debit Card")
        {
            ModelState.Remove(nameof(model.CreditCardNumber));
        }

        // Check validation
        if (!ModelState.IsValid)
        {
            // Show validation errors on the page
            var errorMessages = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                .ToList();

            TempData["Error"] = "Please fix the following errors: " + string.Join(" | ", errorMessages);

            return View("../NGO_Event_/Event_Payment", model);
        }

        try
        {
            // Save donation to database
            var donation = new Donation
            {
                UserEmail = User.Identity?.Name,
                EventId = model.EventID,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                DonationDate = DateTime.Now,
                CreditCardNumber = (model.PaymentMethod == "Credit Card" || model.PaymentMethod == "Debit Card")
                                   ? EncryptionHelper.Encrypt(model.CreditCardNumber!)
                                   : null
            };

            db.Donations.Add(donation);
            db.SaveChanges();

            TempData["Success"] = $"Thank you for donating RM {model.Amount} to {model.EventTitle}!";
            TempData["DonationAmount"] = model.Amount.ToString("0.00");
            TempData["DonorEmail"] = model.UserEmail;
            TempData["DonationTime"] = DateTime.Now.ToString("o");
            return RedirectToAction("Event_Details", "NGO_Event_", new { id = model.EventID });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while processing your donation: {ex.Message}";
            return View("../NGO_Event_/Event_Payment", model);
        }
    }

    // Admin: List of donations
    [Authorize(Roles = "Admin")]
    public IActionResult DonationList()
    {
        var donationList = (from d in db.Donations
                            join e in db.Events on d.EventId equals e.EventID
                            orderby d.DonationDate descending
                            select new DonationListVM
                            {
                                DonationID = d.DonationID,
                                UserEmail = d.UserEmail,
                                EventID = e.EventID,
                                EventTitle = e.EventTitle,
                                Amount = d.Amount,
                                PaymentMethod = d.PaymentMethod,
                                DonationDate = d.DonationDate,
                                Status = "Completed"
                            }).ToList();

        ViewBag.TotalDonations = donationList.Sum(d => d.Amount);
        ViewBag.TotalCount = donationList.Count;
        ViewBag.TodayDonations = donationList.Where(d => d.DonationDate.Date == DateTime.Today).Sum(d => d.Amount);

        return View("../NGO_Event_/DonationList", donationList);
    }

    // Success page
    [Authorize]
    public IActionResult Success()
    {
        return View("PaymentSuccess");
    }

}