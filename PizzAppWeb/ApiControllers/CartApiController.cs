using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzAppWeb.Data;
using PizzAppWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PizzAppWeb.ApiControllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize] // Coșul este accesibil doar utilizatorilor autentificați
    public class CartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// 🔹 **1. Obține produsele din coș**
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cartItems = await _context.CartItems
                .Where(c => c.ApplicationUserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            return Ok(cartItems);
        }

        /// 🔹 **2. Adaugă un produs în coș**
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItem cartItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                Console.WriteLine("❌ Eroare: Utilizator neautentificat.");
                return Unauthorized();
            }

            // Verificăm dacă produsul există în baza de date
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
            {
                Console.WriteLine($"❌ Eroare: Produsul cu ID {cartItem.ProductId} nu există.");
                return NotFound(new { message = "Produsul nu există." });
            }

            Console.WriteLine($"🔹 UserID: {userId}, ProdusID: {cartItem.ProductId}, Cantitate: {cartItem.Quantity}");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.ProductId == cartItem.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                Console.WriteLine($"🟢 Cantitate actualizată: {existingItem.Quantity}");
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ApplicationUserId = userId, // ✅ Adăugăm user-ul
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity
                };

                _context.CartItems.Add(newCartItem);
                Console.WriteLine($"🟢 Produs adăugat în coș: {cartItem.ProductId}, Cantitate: {cartItem.Quantity}");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Produs adăugat în coș" });
        }



        /// 🔹 **3. Șterge un produs din coș**
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.ApplicationUserId == userId);
            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produs eliminat din coș" });
        }

        /// 🔹 **4. Plasează comanda**
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cartItems = await _context.CartItems
                .Where(c => c.ApplicationUserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest(new { message = "Coșul este gol!" });
            }

            var order = new Order
            {
                ApplicationUserId = userId,
                OrderDate = System.DateTime.UtcNow,
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price)
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comanda a fost plasată cu succes!" });
        }
    }
}
