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
    public DateOnly Event_Start_Date { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateOnly Event_End_Date { get; set; }

    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event Start Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_Start_Time { get; set; }

    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event End Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_End_Time { get; set; }

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
    public DateOnly Event_Start_Date { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateOnly Event_End_Date { get; set; }

    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event Start Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_Start_Time { get; set; }
    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event End Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_End_Time { get; set; }

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
    public DateOnly Event_Start_Date { get; set; }

    [Display(Name = "Event End Date")]
    [DataType(DataType.Date)]
    public DateOnly Event_End_Date { get; set; }

    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event Start Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_Start_Time { get; set; }
    [Required(ErrorMessage = "Event time is required")]
    [Display(Name = "Event End Time")]
    [DataType(DataType.Time)]
    public TimeSpan Event_End_Time { get; set; }

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
            TimeSpan difference = Event_Start_Date.ToDateTime(TimeOnly.MinValue) - DateTime.Today;
            return (int)difference.TotalDays;
        }
    }

    [Display(Name = "Is Past Event")]
    public bool IsPastEvent
    {
        get
        {
            return Event_End_Date < DateOnly.FromDateTime(DateTime.Today);
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

    //Payment Fields
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string? CreditCardNumber { get; set; }
}
<<<<<<< HEAD

public class VolunteerVM
{
    public string VolunteerID { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Too Long! >:( (100 character max)")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format (e.g., example@mail.com).")]
    public string Email { get; set; }


    [RegularExpression(@"^(?:\+60|0)[1-9]\d{7,9}$",
            ErrorMessage = "Please enter valid phone number")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please confirm your age.")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    public string Event_Id { get; set;}



}

//Many to many relationship as volunteer can recruit for many event and event can have many recruited volunteer

public class VolunteerEventVM
{
    public string VolunteerName { get; set; }
    public string EventName { get; set; }
    public int Point { get; set; }
    public DateTime ShiftStart { get; set; }
    public int WorkHours { get; set; }
    public string EventCompletion { get; set; }
}
=======
//    @*  Volunteer View Model
public class VolunteerVM
{
    public int VolunteerID { get; set; }
    public string UserEmail { get; set; }
    public string EventID { get; set; }
    public string EventTitle { get; set; }
    public DateTime EventDate { get; set; }
    public string EventLocation { get; set; }
    public DateTime VolunteerDate { get; set; }
    public int Points { get; set; }
    public string Status { get; set; }
    public bool IsPastEvent => EventDate < DateTime.Today;
}

// Leaderboard View Model
public class LeaderboardVM
{
    public int Rank { get; set; }
    public string UserEmail { get; set; }
    public string UserName { get; set; }
    public int TotalPoints { get; set; }
    public int TotalEvents { get; set; }
    public DateTime LastActivity { get; set; }

    // Helper properties for display
    public string DisplayName => string.IsNullOrEmpty(UserName) ? UserEmail : UserName;
    public string LastActivityFormatted => LastActivity.ToString("dd MMM yyyy");
} // *@
>>>>>>> 4d67e1af2d65d6d0fe6771e4e80d63ff54221b05
