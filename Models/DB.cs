using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
}

// Entity Classes

public class Event
{
    [Key, MaxLength(4)]
    public string EventID { get; set; }
    [MaxLength(100)]
    public string EventTitle { get; set; }
    [MaxLength(200)]
    public DateTime EventDate { get; set; }
    [MaxLength(100)]
    public string EventLocation { get; set; }
    [MaxLength(500)]
    public string EventDescription { get; set; }

    public string? EventPhotoURL { get; set; }
}

public class User
{
    [Key, MaxLength(100)]
    public string Email { get; set; }
    [MaxLength(100)]
    public string Hash { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }

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
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime DonationDate { get; set; }
    public string? CreditCardNumber { get; set; }
}