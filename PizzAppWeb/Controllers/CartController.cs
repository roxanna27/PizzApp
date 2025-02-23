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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                                .Include(c => c.Product)
                                .Where(c => c.ApplicationUserId == userId)
                                .ToListAsync();
            return View(cartItems);
        }

        // POST: Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _context.Update(cartItem);
            }
            else
            {
                cartItem = new CartItem
                {
                    ApplicationUserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.Add(cartItem);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        // POST: Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
                return NotFound();

            cartItem.Quantity = quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                                .Include(c => c.Product)
                                .Where(c => c.ApplicationUserId == userId)
                                .ToListAsync();
            if (cartItems == null || !cartItems.Any())
            {
                // Poți afișa un mesaj de eroare sau redirecționa către pagina de coș cu un mesaj "Coșul este gol"
                return RedirectToAction(nameof(Index));
            }

            // Creare comandă nouă
            var order = new Order
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            // Șterge elementele din coș
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Orders");
        }
    }
}
