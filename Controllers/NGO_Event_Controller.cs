using System.Diagnostics.Eventing.Reader;
using Demo.Models; // Added for Event model
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NGO_Web_Demo;
using NGO_Web_Demo.Models;

namespace NGO_Web_Demo.Controllers;

public class NGO_Event_Controller : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    public NGO_Event_Controller(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }

    // GET: NGO_Event_/Event_Index
    public IActionResult Event_Index()
    {
        var model = db.Events.ToList(); // Added ToList() to ensure data is loaded

        return View(model);
    }

    // GET: NGO_Event_/CheckId - Fixed for AJAX validation
    public JsonResult CheckId(string Event_Id)
    {
        bool isAvailable = !db.Events.Any(e => e.EventID == Event_Id);
        return Json(isAvailable);
    }

    private string NextId()
    {
        try
        {
            string max = db.Events.Max(e => e.EventID) ?? "E000";
            int n = int.Parse(max[1..]);
            return $"E{(n + 1):000}"; // Fixed string formatting
        }
        catch
        {
            return "E001";
        }
    }

    // GET: NGO_Event_/Event_Insert
    public IActionResult Event_Insert()
    {
        var vm = new EventInsertVM
        {
            Event_Id = NextId(),
            Event_Title = "",
            Event_Start_Date = DateOnly.MinValue,
            Event_End_Date = DateOnly.MaxValue,
            Event_Status = "",
            Event_Location = "",
            Event_Description = ""
        };

        return View(vm);
    }

    // POST: NGO_Event_/Event_Insert
    [HttpPost]
    public IActionResult Event_Insert(EventInsertVM vm)
    {
        // Check for duplicate ID
        if (!string.IsNullOrEmpty(vm.Event_Id) && db.Events.Any(e => e.EventID == vm.Event_Id))
        {
            ModelState.AddModelError("Event_Id", "Duplicated Event ID.");
        }

        // Validate photo if provided
        if (vm.Event_Photo != null)
        {
            var error = hp.ValidatePhoto(vm.Event_Photo);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("Event_Photo", error);
            }
        }

        if (vm.Event_Start_Date < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError("Event_Start_Date", "Start date cannot be in the past.");
        }

        // This check is fine as it's DateOnly to DateOnly
        if (vm.Event_End_Date < vm.Event_Start_Date)
        {
            ModelState.AddModelError("Event_End_Date", "End date cannot be before start date.");
        }

        // Validation for time-of-day still applies if needed
        if (vm.Event_Start_Time < TimeSpan.FromHours(8) || vm.Event_Start_Time > TimeSpan.FromHours(18))
        {
            ModelState.AddModelError("Event_Start_Time", "Start time must be between 08:00 and 18:00.");
        }

        // 2. Validate that the end time is after the start time.
        if (vm.Event_End_Time <= vm.Event_Start_Time)
        {
            ModelState.AddModelError("Event_End_Time", "End date and time must be after the start date and time.");
        }

        // 3. Validate that the event duration is not more than 24 hours.
        TimeSpan duration = vm.Event_End_Time - vm.Event_Start_Time;
        if (duration > TimeSpan.FromHours(24))
        {
            ModelState.AddModelError("Event_End_Time", "Event duration cannot exceed 24 hours.");
        }

        if (vm.Event_Start_Date > DateOnly.FromDateTime(DateTime.Today))
        {
            vm.Event_Status = "Upcoming";
        }
        else if (vm.Event_End_Date < DateOnly.FromDateTime(DateTime.Today))
        {
            vm.Event_Status = "Concluded";
        }
        else
        {
            vm.Event_Status = "Ongoing";
        }

        if (ModelState.IsValid)
        {
            try
            {
                string? photoUrl = null;
                if (vm.Event_Photo != null)
                {
                    photoUrl = hp.SavePhoto(vm.Event_Photo, "events");
                }

                // FIXED: Only save photo once and don't duplicate
                db.Events.Add(new Event
                {
                    EventID = vm.Event_Id,
                    EventTitle = vm.Event_Title,
                    EventStartDate = vm.Event_Start_Date,
                    EventEndDate = vm.Event_End_Date,
                    EventStartTime = vm.Event_Start_Time,
                    EventEndTime = vm.Event_End_Time,
                    EventStatus = vm.Event_Status,
                    EventLocation = vm.Event_Location,
                    EventDescription = vm.Event_Description,
                    EventPhotoURL = photoUrl // FIXED: Use the photoUrl variable, not save again
                });

                db.SaveChanges();

                TempData["Info"] = "Event inserted successfully.";
                return RedirectToAction("Event_Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving event: {ex.Message}");
            }
        }

        // If we get here, something failed
        return View(vm);
    }

    // GET: NGO_Event_/Event_Update - Fixed typo in method name
    public IActionResult Event_Update(string? id)
    {
        var e = db.Events.Find(id);

        if (e == null)
        {
            TempData["Info"] = "Event not found when getting.";
            return RedirectToAction("Event_Index");
        }

        var vm = new EventUpdateVM
        {
            Event_Id = e.EventID,
            Event_Title = e.EventTitle,
            Event_Start_Date = e.EventStartDate,
            Event_End_Date = e.EventEndDate,
            Event_Start_Time = e.EventStartTime,
            Event_End_Time = e.EventEndTime,
            Event_Location = e.EventLocation,
            Event_Description = e.EventDescription,
            Event_PhotoURL = e.EventPhotoURL,
            Event_Photo = null,
        };

        return View(vm);
    }

    // POST: NGO_Event_/Event_Update
    [HttpPost]
    public IActionResult Event_Update(EventUpdateVM vm)
    {

        var e = db.Events.Find(vm.Event_Id);

        if (e == null)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR: Event not found in database with ID: {vm.Event_Id}");
            TempData["Info"] = "Event not found when posting.";
            return RedirectToAction("Event_Index");
        }

        System.Diagnostics.Debug.WriteLine($"Found event in DB: {e.EventTitle}");

        // Validate photo if provided
        if (vm.Event_Photo != null)
        {
            var error = hp.ValidatePhoto(vm.Event_Photo);
            if (!string.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("Event_Photo", error);
            }
        }

        if (vm.Event_Start_Date < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError("Event_Start_Date", "Start date cannot be in the past.");
        }

        // This check is fine as it's DateOnly to DateOnly
        if (vm.Event_End_Date < vm.Event_Start_Date)
        {
            ModelState.AddModelError("Event_End_Date", "End date cannot be before start date.");
        }

        // Validation for time-of-day still applies if needed
        if (vm.Event_Start_Time < TimeSpan.FromHours(8) || vm.Event_Start_Time > TimeSpan.FromHours(18))
        {
            ModelState.AddModelError("Event_Start_Time", "Start time must be between 08:00 and 18:00.");
        }

        // 2. Validate that the end time is after the start time.
        if (vm.Event_End_Time <= vm.Event_Start_Time)
        {
            ModelState.AddModelError("Event_End_Time", "End date and time must be after the start date and time.");
        }

        // 3. Validate that the event duration is not more than 24 hours.
        TimeSpan duration = vm.Event_End_Time - vm.Event_Start_Time;
        if (duration > TimeSpan.FromHours(24))
        {
            ModelState.AddModelError("Event_End_Time", "Event duration cannot exceed 24 hours.");
        }

        if (vm.Event_Start_Date > DateOnly.FromDateTime(DateTime.Today))
        {
            vm.Event_Status = "Upcoming";
        }
        else if (vm.Event_End_Date < DateOnly.FromDateTime(DateTime.Today))
        {
            vm.Event_Status = "Concluded";
        }
        else
        {
            vm.Event_Status = "Ongoing";
        }

        if (ModelState.IsValid)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ModelState is valid, updating entity...");

                // Log old values
                System.Diagnostics.Debug.WriteLine($"OLD - Title: {e.EventTitle}, Date: {e.EventStartDate}");

                // Update the entity properties
                e.EventTitle = vm.Event_Title;
                e.EventStartDate = vm.Event_Start_Date;
                e.EventEndDate = vm.Event_End_Date;
                e.EventStartTime = vm.Event_Start_Time;
                e.EventEndTime = vm.Event_End_Time;
                e.EventStatus = vm.Event_Status;
                e.EventLocation = vm.Event_Location;
                e.EventDescription = vm.Event_Description;

                // Log new values
                System.Diagnostics.Debug.WriteLine($"NEW - Title: {e.EventTitle}, Start Date: {e.EventStartDate}, End Date: {e.EventEndDate}");

                // Handle photo update
                if (vm.Event_Photo != null)
                {
                    System.Diagnostics.Debug.WriteLine("Updating photo...");
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(e.EventPhotoURL))
                    {
                        hp.DeletePhoto(e.EventPhotoURL, "events");
                    }
                    // Save new photo
                    e.EventPhotoURL = hp.SavePhoto(vm.Event_Photo, "events");
                }

                // Check if entity is being tracked
                var entry = db.Entry(e);
                System.Diagnostics.Debug.WriteLine($"Entity State: {entry.State}");
                System.Diagnostics.Debug.WriteLine($"Entity Modified Properties: {string.Join(", ", entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name))}");

                // Save changes to database
                int changes = db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges returned: {changes} rows affected");

                TempData["Info"] = "Event updated successfully.";
                return RedirectToAction("Event_Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION during save: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error updating event: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ModelState is NOT valid, returning to view...");
        }

        // If we get here, there were validation errors or an exception
        // Make sure to reload the photo URL for display
        vm.Event_PhotoURL = e.EventPhotoURL;

        return View(vm);
    }

    // POST: NGO_Event_/Delete
    [HttpPost]
    public IActionResult Delete(string? id)
    {

        var e = db.Events.Find(id);

        if (e != null)
        {
            try
            {
                // Delete associated photo
                if (!string.IsNullOrEmpty(e.EventPhotoURL))
                {
                    hp.DeletePhoto(e.EventPhotoURL, "events");
                }

                db.Events.Remove(e);
                db.SaveChanges();

                TempData["Info"] = "Event deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Info"] = $"Error deleting event: {ex.Message}";
            }
        }
        else
        {
            TempData["Info"] = "Event not found.";
        }

        return RedirectToAction("Event_Index");
    }

    public IActionResult Event_Details(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["Info"] = "Event ID is required.";
            return RedirectToAction("Event_Index");
        }

        var e = db.Events.Find(id);

        if (e == null)
        {
            TempData["Info"] = "Event not found.";
            return RedirectToAction("Event_Index");
        }

        var vm = new EventDetailsVM
        {
            Event_Id = e.EventID,
            Event_Title = e.EventTitle,
            Event_Start_Date = e.EventStartDate,
            Event_End_Date = e.EventEndDate,
            Event_Start_Time = e.EventStartTime,
            Event_End_Time = e.EventEndTime,
            Event_Status = e.EventStatus,
            Event_Location = e.EventLocation,
            Event_Description = e.EventDescription,
            Event_PhotoURL = e.EventPhotoURL
        };

        return View(vm);
    }

    public IActionResult Event_Payment()
    {
        return View();
    }
}