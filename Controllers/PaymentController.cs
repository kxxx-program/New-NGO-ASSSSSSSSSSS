using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NGO_Web_Demo.Models;
using System.Security.Cryptography;
using System.Text;

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
    public IActionResult Index(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Info"] = "Event ID is required.";
            return RedirectToAction("Event_Index", "NGO_Event_");
        }

        var e = db.Events.Find(id);
        var u = db.Users.Find(User.Identity?.Name);
        if (e == null)
        {
            TempData["Info"] = "Event not found.";
            return RedirectToAction("Event_Index", "NGO_Event_");
        }

        var model = new PaymentVM
        {
            EventID = e.EventID,
            EventTitle = e.EventTitle,
            ParticipatedDate = u.ParticipatedDate,
            EventLocation = e.EventLocation
        };

        return View("~/Views/NGO_Event/Event_Payment.cshtml", model);
    }

    //POST: Payment/ProcessPayment
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Process(PaymentVM model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/NGO_Event/Event_Payment.cshtml", model);
        }

        //Save payment info to database
        var donation = new Donation
        {
            UserEmail = User.Identity?.Name, //store email
            EventId = model.EventID,
            Amount = model.Amount,
            PaymentMethod = model.PaymentMethod,
            DonationDate = DateTime.Now,
            CreditCardNumber = model.PaymentMethod == "Credit Card" && !string.IsNullOrEmpty(model.CreditCardNumber) ? EncryptionHelper.Encrypt(model.CreditCardNumber) : null
        };

        db.Donations.Add(donation);
        db.SaveChanges();

        TempData["Success"] = $"Thank you for your donating RM {model.Amount} to {model.EventTitle}!";
        return RedirectToAction("Success");
    }

    // GET: Payment/Success
    [Authorize]
    public IActionResult Success()
    {
        return View("PaymentSuccess");
    }

    //Encryption Method
    public static class EncryptionHelper
    {
        private static readonly string key = "YourStrongSecretKey123!"; // 🔑 store securely

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // default IV (all zero)
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
