using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_Web_Demo.Models;

namespace NGO_Web_Demo.Controllers
{
    public class UpdateProfileController : Controller
    {
        private readonly DB _context;
        private readonly IWebHostEnvironment _environment;

        public UpdateProfileController(DB context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Basic test - Visit /UpdateProfile/Test
        public IActionResult Test()
        {
            return Content("UpdateProfile controller is working!");
        }

        // GET: /UpdateProfile
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = await _context.Members.FirstOrDefaultAsync(m => m.Email == userEmail);

                if (user == null)
                {
                    // Debug info if user not found
                    var allMembers = await _context.Members.Select(m => m.Email).ToListAsync();
                    return Content($"User '{userEmail}' not found in Members table. " +
                                 $"Available members: {string.Join(", ", allMembers)}");
                }

                var vm = new UpdateProfileVM
                {
                    Email = user.Email,
                    Name = user.Name,
                    PhotoURL = user.PhotoURL
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        // POST: /UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Index(UpdateProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Use the current user's email instead of the form email
                var userEmail = User.Identity.Name;
                var user = await _context.Members.FirstOrDefaultAsync(m => m.Email == userEmail);

                if (user == null)
                {
                    return Content($"User not found during update: {userEmail} (from User.Identity.Name)");
                }

                // Update name
                user.Name = model.Name;

                // Handle photo upload
                if (model.Photo != null && model.Photo.Length > 0)
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(user.PhotoURL))
                    {
                        var oldPhotoPath = Path.Combine(_environment.WebRootPath, "photos", user.PhotoURL);
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    // Create photos directory if it doesn't exist
                    var photosFolder = Path.Combine(_environment.WebRootPath, "photos");
                    if (!Directory.Exists(photosFolder))
                    {
                        Directory.CreateDirectory(photosFolder);
                    }

                    // Save new photo
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                    var filePath = Path.Combine(photosFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(stream);
                    }

                    user.PhotoURL = fileName;
                }

                // Save changes to database
                _context.Members.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating profile: {ex.Message}";
                return View(model);
            }
        }
    }
}