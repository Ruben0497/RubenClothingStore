using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RubenClothingStore.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserProfile
        public IActionResult Index()
        {
            // Retrieve logged-in user's ID from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Try to get the user's profile
            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);

            // Create profile if it doesn't exist
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FullName = HttpContext.Session.GetString("FullName")
                };

                _context.UserProfiles.Add(profile);
                _context.SaveChanges();
            }

            // Check completeness for reminder banner
            bool isComplete = !string.IsNullOrWhiteSpace(profile.PhoneNumber)
                              && !string.IsNullOrWhiteSpace(profile.Address)
                              && profile.DateOfBirth != null;

            ViewBag.ProfileIncomplete = !isComplete;

            return View(profile);
        }

        // GET: /UserProfile/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userProfile = await _context.UserProfiles.FindAsync(id);
            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }

        // POST: /UserProfile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FullName,PhoneNumber,Address,DateOfBirth")] UserProfile userProfile)
        {
            if (id != userProfile.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userProfile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserProfiles.Any(e => e.Id == userProfile.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(userProfile);
        }
    }
}

