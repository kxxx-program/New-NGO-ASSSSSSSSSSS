using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.Models;

#nullable disable warnings

public class DB : DbContext
{
    public DB(DbContextOptions options) : base(options) { }

    // DB Sets
    public DbSet<Event> Events { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Donation> Donations { get; set; }


    public DbSet<Volunteer> Volunteers { get; set; }

    public DbSet<VolunteerEvent> VolunteerEvents { get; set; }
//=======
//    public DbSet<Volunteer> Volunteers { get; set; }
//>>>>>>> 4d67e1af2d65d6d0fe6771e4e80d63ff54221b05
}

// Entity Classes
public class Event
{
    [Key, MaxLength(4)]
    public string EventID { get; set; }

    [MaxLength(100)]
    public string EventTitle { get; set; }

    public DateOnly EventStartDate { get; set; }

    public DateOnly EventEndDate { get; set; }

    public TimeSpan EventStartTime { get; set; }
    public TimeSpan EventEndTime { get; set; }

    public string EventStatus { get; set; } = "Upcoming";

    [MaxLength(100)]
    public string EventLocation { get; set; }

    [MaxLength(500)]
    public string EventDescription { get; set; }

    public string? EventPhotoURL { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    //Connect to volunteerEvent 
    public ICollection<VolunteerEvent> VolunteerEvents { get; set; } = [];
}

public class User
{
    [Key, MaxLength(100)]
    public string Email { get; set; }
    [MaxLength(100)]
    public string Hash { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    public DateTime ParticipatedDate { get; set; }
    public string Role => GetType().Name;
}

public class Admin : User
{
    
}

public class Member : User
{
    [MaxLength(100)]
    public string PhotoURL { get; set; }
}

public class Donation
{
    public int DonationID { get; set; }
    public string UserEmail { get; set; }
    public string EventId { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime DonationDate { get; set; }
    public string? CreditCardNumber { get; set; }
}


public class Volunteer
{
    [Key, MaxLength(4)]
    public string VolunteerID { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    [Range(18, 70)]
    public int Age { get; set; }


    public ICollection<VolunteerEvent> VolunteerEvents { get; set; } = [];
}

public class VolunteerEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string VolunteerID { get; set; }
    public Volunteer Volunteer { get; set; }

    [Required]
    public string EventID { get; set; }
    public Event Event { get; set; }

    //stuff like this created this class, if no bonus shit just many many relationshipasds dada
    public int Points { get; set; }
    public DateTime ShiftStart { get; set; }
    public int WorkHours { get; set; }

    public EventStatus EventCompletion { get; set; } = EventStatus.Waiting;

}
public enum EventStatus
{
    Waiting,
    Completed,
    Cancelled
}


//=======
//public class Volunteer
//{
//    [Key]
//    public int VolunteerID { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public string UserEmail { get; set; }

//    [Required]
//    [MaxLength(4)]
//    public string EventID { get; set; }

//    public DateTime VolunteerDate { get; set; }

//    public int Points { get; set; } = 100; // Default points for volunteering

//    [MaxLength(50)]
//    public string Status { get; set; } = "Active"; // Active, Completed, Donated, etc.
//}
//>>>>>>> 4d67e1af2d65d6d0fe6771e4e80d63ff54221b05
