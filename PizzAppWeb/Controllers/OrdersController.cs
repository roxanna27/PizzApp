using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PizzAppWeb.Data;
using PizzAppWeb.Models;

namespace PizzAppWeb.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product)
                            .Include(o => o.Review)
                            .Where(o => o.ApplicationUserId == userId)
                            .OrderByDescending(o => o.OrderDate)
                            .ToListAsync();
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product)
                            .Include(o => o.Review)
                            .FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == userId);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Orders/AddReview/5
        public async Task<IActionResult> AddReview(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == userId);
            if (order == null)
                return NotFound();

            // Dacă recenzia există deja, redirecționează spre editare
            if (order.Review != null)
                return RedirectToAction(nameof(EditReview), new { id = order.Review.Id });

            var review = new Review { OrderId = order.Id };
            return View(review);
        }

        // POST: Orders/AddReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Review review)
        {
            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = review.OrderId });
            }
            return View(review);
        }

        // GET: Orders/EditReview/5
        public async Task<IActionResult> EditReview(int? id)
        {
            if (id == null)
                return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            var order = await _context.Orders.FindAsync(review.OrderId);
            if (order == null || order.ApplicationUserId != _userManager.GetUserId(User))
                return Unauthorized();

            return View(review);
        }

        // POST: Orders/EditReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, Review review)
        {
            if (id != review.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reviews.Any(e => e.Id == review.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Details), new { id = review.OrderId });
            }
            return View(review);
        }
    }
}
