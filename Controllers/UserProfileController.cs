using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using System.Linq;

namespace RubenClothingStore.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == "demo-user-id");

            if (profile == null)
                return NotFound("No user profile found.");

            return View(profile);
        }

        // GET: UserProfile/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userProfile = await _context.UserProfiles.FindAsync(id);
            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }

        // POST: UserProfile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FullName,PhoneNumber,Address,DateOfBirth")] UserProfile userProfile)
        {
            Console.WriteLine("POST: Edit Profile Called");

            if (id != userProfile.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userProfile);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Profile successfully updated");
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
