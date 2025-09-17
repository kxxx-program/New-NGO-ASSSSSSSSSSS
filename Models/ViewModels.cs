using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace NGO_Web_Demo.Models;

// View Models ----------------------------------------------------------------

#nullable disable warnings

public class LoginVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

public class RegisterVM
{
    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    public IFormFile Photo { get; set; }
}

public class UpdatePasswordVM
{
    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class UpdateProfileVM
{
    public string? Email { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    public string? PhotoURL { get; set; }

    public IFormFile? Photo { get; set; }
}

public class ResetPasswordVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }
}

public class EmailVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsBodyHtml { get; set; }
}

//Event View Models and stuff
public class EventInsertVM
{
    [StringLength(4)]
    [RegularExpression(@"E\d{3}", ErrorMessage = "Invalid {0} format.")] // FIXED: Changed from P to E
    [Remote("CheckId", "NGO_Event_", ErrorMessage = "Duplicated {0}.")]
    [Display(Name = "Event ID")]
    public string Event_Id { get; set; }

    [Required(ErrorMessage = "Event title is required")]
    [StringLength(100, ErrorMessage = "Event title cannot exceed 100 characters")]
    [Display(Name = "Event Title")]
    public string Event_Title { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Start Date")]
    [DataType(DataType.Date)]
    public DateTime Event_Start_Date { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateTime Event_End_Date { get; set; }

    public string Event_Status { get; set; } = "Upcoming";

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Event Description")]
    public string Event_Description { get; set; }

    [Required(ErrorMessage = "Event location is required")]
    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    [Display(Name = "Event Location")]
    public string Event_Location { get; set; }



    // Other properties
    [Display(Name = "Event Photo")]
    public IFormFile Event_Photo { get; set; }
}

public class EventUpdateVM
{
    [Display(Name = "Event ID")]
    public string Event_Id { get; set; }

    [Required(ErrorMessage = "Event title is required")]
    [StringLength(100, ErrorMessage = "Event title cannot exceed 100 characters")]
    [Display(Name = "Event Title")]
    public string Event_Title { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Start Date")]
    [DataType(DataType.Date)]
    public DateTime Event_Start_Date { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateTime Event_End_Date { get; set; }

    public string Event_Status { get; set; } = "Upcoming";

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Event Description")]
    public string Event_Description { get; set; }

    [Required(ErrorMessage = "Event location is required")]
    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    [Display(Name = "Event Location")]
    public string Event_Location { get; set; }

    // Other properties
    [Display(Name = "Current Photo")]
    public string? Event_PhotoURL { get; set; }

    [Display(Name = "New Photo")]
    public IFormFile? Event_Photo { get; set; }
}

public class EventDetailsVM
{
    [Display(Name = "Event ID")]
    public string Event_Id { get; set; } = "";

    [Display(Name = "Event Title")]
    public string Event_Title { get; set; } = "";

    [Display(Name = "Event Start Date")]
    [DataType(DataType.Date)]
    public DateTime Event_Start_Date { get; set; }

    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateTime Event_End_Date { get; set; }

    public string Event_Status { get; set; } = "Upcoming";

    [Display(Name = "Event Location")]
    public string Event_Location { get; set; } = "";

    [Display(Name = "Event Description")]
    public string Event_Description { get; set; } = "";

    [Display(Name = "Event Photo")]
    public string? Event_PhotoURL { get; set; }

    // Additional properties for the detailed view
    [Display(Name = "Days Until Event")]
    public int DaysUntilEvent
    {
        get
        {
            return (Event_Start_Date - DateTime.Today).Days;
        }
    }

    [Display(Name = "Is Past Event")]
    public bool IsPastEvent
    {
        get
        {
            return Event_End_Date < DateTime.Today;
        }
    }

    [Display(Name = "Formatted Date")]
    public string FormattedDate
    {
        get
        {
            return Event_Start_Date.ToString("dddd, MMMM dd, yyyy");
        }
    }
}

//Payment View Model

public class PaymentVM
{
    public string EventID { get; set; }
    public string EventTitle { get; set; }
    public DateTime ParticipatedDate { get; set; }
    public string EventLocation { get; set; }

    //Payemnt Fields
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string? CreditCardNumber { get; set; }
}