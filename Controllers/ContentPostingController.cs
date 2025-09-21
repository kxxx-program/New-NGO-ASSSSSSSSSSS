using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NGO_Web_Demo.Models;
using X.PagedList.Extensions;

namespace NGO_Web_Demo.Controllers
{
    [Authorize] // Require authentication for all actions
    public class ContentPostingController : Controller
    {
        private readonly DB db;
        private readonly Helper hp;

        public ContentPostingController(DB db, Helper hp)
        {
            this.db = db;
            this.hp = hp;
        }

        // GET: ContentPosting/Posting_Index
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Posting_Index(string? name, string? sort, string? dir, int page = 1)
        {
            // (1) Searching ------------------------
            ViewBag.Name = name = name?.Trim() ?? "";

            // Start with all postings
            var query = db.Postings.AsQueryable();

            // Apply role-based filtering
            var currentUserEmail = User.Identity?.Name;
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (currentUser is Organiser) // If user is an Organiser, show only their postings
            {
                query = query.Where(p => p.CreatedBy == currentUserEmail);
                ViewBag.UserRole = "Organiser";
                ViewBag.ShowingMyPostings = true;
            }
            else if (currentUser is Admin) // If user is Admin, show all postings
            {
                // No filtering needed for Admin
                ViewBag.UserRole = "Admin";
                ViewBag.ShowingMyPostings = false;
            }
            else
            {
                // If user is not authenticated or not found, redirect to login
                return RedirectToAction("Login", "Account");
            }

            // Apply search filter
            var searched = query.Where(p => p.ImageTitle.Contains(name));

            // (2) Sorting --------------------------
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<PostImageModel, object> fn = sort switch
            {
                "Photo" => p => p.PhotoURL,
                "Title" => p => p.ImageTitle,
                "Created By" => p => p.CreatedBy,
                _ => p => p.Id,
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

        // GET: ContentPosting/CheckId - Fixed for AJAX validation
        public JsonResult CheckId(string Id)
        {
            bool isAvailable = !db.Postings.Any(p => p.Id == Id);
            return Json(isAvailable);
        }

        private string NextId()
        {
            try
            {
                string max = db.Postings.Max(p => p.Id) ?? "P000";
                int n = int.Parse(max[1..]);
                return $"P{(n + 1):000}"; // Fixed string formatting
            }
            catch
            {
                return "P001";
            }
        }

        // GET: ContentPosting/Posting_Insert
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Posting_Insert()
        {
            var vm = new PostingInsertVM
            {
                Id = NextId(),
                ImageTitle = ""
            };

            return View(vm);
        }

        // POST: ContentPosting/Posting_Insert
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Posting_Insert(PostingInsertVM vm)
        {
            // Debug information
            System.Diagnostics.Debug.WriteLine($"Id: {vm.Id}");
            System.Diagnostics.Debug.WriteLine($"ImageTitle: {vm.ImageTitle}");
            System.Diagnostics.Debug.WriteLine($"Photo: {vm.Photo?.FileName ?? "No file"}");
            System.Diagnostics.Debug.WriteLine($"Photo Size: {vm.Photo?.Length ?? 0} bytes");

            // Check for duplicate ID
            if (!string.IsNullOrEmpty(vm.Id) && db.Postings.Any(p => p.Id == vm.Id))
            {
                ModelState.AddModelError("Id", "Duplicated Id.");
            }

            // Validate photo
            if (vm.Photo == null)
            {
                ModelState.AddModelError("Photo", "Photo is required.");
            }
            else
            {
                var validationError = hp.ValidatePhoto(vm.Photo);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ModelState.AddModelError("Photo", validationError);
                }
            }

            // Check ImageTitle
            if (string.IsNullOrWhiteSpace(vm.ImageTitle))
            {
                ModelState.AddModelError("ImageTitle", "Image title is required.");
            }

            // Debug ModelState
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? photoUrl = null;
                    if (vm.Photo != null)
                    {
                        photoUrl = hp.SavePhoto(vm.Photo, "postings");
                        System.Diagnostics.Debug.WriteLine($"Photo saved as: {photoUrl}");
                    }

                    // Get the current user's email from authentication
                    string? currentUserEmail = GetCurrentUserEmail();
                    System.Diagnostics.Debug.WriteLine($"Current user: {currentUserEmail}");

                    var posting = new PostImageModel
                    {
                        Id = vm.Id,
                        ImageTitle = vm.ImageTitle,
                        PhotoURL = photoUrl,
                        CreatedBy = currentUserEmail // Set who created this posting
                    };

                    db.Postings.Add(posting);
                    var saveResult = db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Save result: {saveResult} rows affected");

                    TempData["Info"] = "Posting inserted successfully.";
                    return RedirectToAction("Posting_Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error saving posting: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(vm);
        }

        // GET: ContentPosting/Posting_Update
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Posting_Update(string? id)
        {
            var p = db.Postings.Find(id);

            if (p == null)
            {
                TempData["Info"] = "Posting not found.";
                return RedirectToAction("Posting_Index");
            }

            // Check if user can edit this posting
            var currentUserEmail = GetCurrentUserEmail();
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (currentUser is Organiser && p.CreatedBy != currentUserEmail)
            {
                TempData["Info"] = "You can only edit postings you created.";
                return RedirectToAction("Posting_Index");
            }

            var vm = new PostingUpdateVM
            {
                Id = p.Id,
                ImageTitle = p.ImageTitle,
                PhotoURL = p.PhotoURL,
            };

            return View(vm);
        }

        // POST: ContentPosting/Posting_Update
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Posting_Update(PostingUpdateVM vm)
        {
            var p = db.Postings.Find(vm.Id);

            if (p == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Posting not found in database with ID: {vm.Id}");
                TempData["Info"] = "Posting not found when updating.";
                return RedirectToAction("Posting_Index");
            }

            System.Diagnostics.Debug.WriteLine($"Found posting in DB: {p.ImageTitle}");

            // Check if user can edit this posting
            var currentUserEmail = GetCurrentUserEmail();
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (currentUser is Organiser && p.CreatedBy != currentUserEmail)
            {
                TempData["Info"] = "You can only edit postings you created.";
                return RedirectToAction("Posting_Index");
            }

            // Validate photo if provided
            if (vm.Photo != null)
            {
                var error = hp.ValidatePhoto(vm.Photo);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("Photo", error);
                }
            }

            // Check ImageTitle
            if (string.IsNullOrWhiteSpace(vm.ImageTitle))
            {
                ModelState.AddModelError("ImageTitle", "Image title is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("ModelState is valid, updating entity...");

                    // Log old values
                    System.Diagnostics.Debug.WriteLine($"OLD - Title: {p.ImageTitle}");

                    // Update the entity properties
                    p.ImageTitle = vm.ImageTitle;

                    // Log new values
                    System.Diagnostics.Debug.WriteLine($"NEW - Title: {p.ImageTitle}");

                    // Handle photo update
                    if (vm.Photo != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Updating photo...");
                        // Delete old photo if exists
                        if (!string.IsNullOrEmpty(p.PhotoURL))
                        {
                            hp.DeletePhoto(p.PhotoURL, "postings");
                        }
                        // Save new photo
                        p.PhotoURL = hp.SavePhoto(vm.Photo, "postings");
                    }

                    // Check if entity is being tracked
                    var entry = db.Entry(p);
                    System.Diagnostics.Debug.WriteLine($"Entity State: {entry.State}");
                    System.Diagnostics.Debug.WriteLine($"Entity Modified Properties: {string.Join(", ", entry.Properties.Where(prop => prop.IsModified).Select(prop => prop.Metadata.Name))}");

                    // Save changes to database
                    int changes = db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"SaveChanges returned: {changes} rows affected");

                    TempData["Info"] = "Posting updated successfully.";
                    return RedirectToAction("Posting_Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"EXCEPTION during save: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error updating posting: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState is NOT valid, returning to view...");
            }

            // If we get here, there were validation errors or an exception
            // Make sure to reload the photo URL for display
            vm.PhotoURL = p.PhotoURL;
            return View(vm);
        }

        // POST: ContentPosting/Delete
        [HttpPost]
        [Authorize(Roles = "Admin,Organiser")]
        public IActionResult Delete(string? id)
        {
            var p = db.Postings.Find(id);

            if (p == null)
            {
                TempData["Info"] = "Posting not found.";
                return RedirectToAction("Posting_Index");
            }

            // Check if user can delete this posting
            var currentUserEmail = GetCurrentUserEmail();
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (currentUser is Organiser && p.CreatedBy != currentUserEmail)
            {
                TempData["Info"] = "You can only delete postings you created.";
                return RedirectToAction("Posting_Index");
            }

            try
            {
                // Delete associated photo
                if (!string.IsNullOrEmpty(p.PhotoURL))
                {
                    hp.DeletePhoto(p.PhotoURL, "postings");
                }

                db.Postings.Remove(p);
                db.SaveChanges();

                TempData["Info"] = "Posting deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Info"] = $"Error deleting posting: {ex.Message}";
            }

            return RedirectToAction("Posting_Index");
        }

        // GET: ContentPosting/Posting_Details
        public IActionResult Posting_Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Info"] = "Posting ID is required.";
                return RedirectToAction("Posting_Index");
            }

            var p = db.Postings.Find(id);

            if (p == null)
            {
                TempData["Info"] = "Posting not found.";
                return RedirectToAction("Posting_Index");
            }

            ViewBag.IsOrganiser = IsUserOrganiser();
            ViewBag.CanEdit = CanUserEditPosting(p);

            return View(p);
        }

        // Helper method to get current user's email
        private string? GetCurrentUserEmail()
        {
            // Method 1: If using Identity with Claims-based authentication
            return User?.FindFirst(ClaimTypes.Email)?.Value ??
                   User?.FindFirst(ClaimTypes.Name)?.Value ??
                   User?.Identity?.Name;

            // Method 2: If storing user info in session
            // return HttpContext.Session.GetString("UserEmail");

            // Method 3: If using a different authentication method
            // Modify this based on how your authentication works
        }

        private bool IsUserOrganiser()
        {
            // Using ASP.NET Identity Roles
            if (User.IsInRole("Admin") || User.IsInRole("Organiser"))
            {
                return true;
            }

            return false;
        }

        private bool CanUserEditPosting(PostImageModel posting)
        {
            var currentUserEmail = GetCurrentUserEmail();
            var currentUser = db.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            // Admin can edit everything
            if (currentUser is Admin)
            {
                return true;
            }

            // Organiser can only edit their own postings
            if (currentUser is Organiser)
            {
                return posting.CreatedBy == currentUserEmail;
            }

            return false;
        }

        // GET: Gallery
        [AllowAnonymous] // Allow access to all users regardless of authentication
        public IActionResult Gallery(int page = 1)
        {
            // Get all postings without role filtering
            var query = db.Postings.AsQueryable();

            // Apply pagination (10 items per page)
            var pagedPosts = query.OrderBy(p => p.Id).ToPagedList(page, 10);

            return View(pagedPosts);
        }
    }
}