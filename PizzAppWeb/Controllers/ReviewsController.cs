using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PizzAppWeb.Data;
using PizzAppWeb.Models;

namespace PizzAppWeb.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reviews = await _context.Reviews
                .Include(r => r.Order)
                .Where(r => r.Order.ApplicationUserId == userId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
            return View(reviews);
        }

        // GET: Reviews/Create?orderId=5
        public async Task<IActionResult> Create(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.ApplicationUserId == userId);
            if (order == null)
                return NotFound("Comanda nu a fost găsită sau nu aparține utilizatorului curent.");

            if (order.Review != null)
                return RedirectToAction("Edit", new { id = order.Review.Id });

            var review = new Review { OrderId = orderId };
            return View(review);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == review.OrderId && o.ApplicationUserId == userId);
            if (order == null)
                return NotFound("Comanda nu a fost găsită sau nu aparține utilizatorului curent.");

            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Recenzia a fost salvată cu succes!";
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == review.OrderId && o.ApplicationUserId == userId);
            if (order == null)
                return Unauthorized("Nu ești autorizat să editezi această recenzie.");

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (id != review.Id)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == review.OrderId && o.ApplicationUserId == userId);
            if (order == null)
                return Unauthorized("Nu ești autorizat să editezi această recenzie.");

            if (ModelState.IsValid)
            {
                try
                {
                    review.ReviewDate = DateTime.Now;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Recenzia a fost actualizată cu succes!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reviews.Any(r => r.Id == review.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (review.Order.ApplicationUserId != userId)
                return Unauthorized("Nu ești autorizat să ștergi această recenzie.");

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == review.OrderId && o.ApplicationUserId == userId);
            if (order == null)
                return Unauthorized("Nu ești autorizat să ștergi această recenzie.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Recenzia a fost ștearsă cu succes!";
            return RedirectToAction(nameof(Index));
        }
    }
}
