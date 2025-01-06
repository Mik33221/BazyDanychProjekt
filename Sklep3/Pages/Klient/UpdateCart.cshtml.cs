using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sklep3.Helpers;
using Microsoft.AspNetCore.Antiforgery;

namespace Sklep3.Pages.Klient
{
    [IgnoreAntiforgeryToken]
    public class UpdateCartModel : PageModel
    {
        public IActionResult OnPost()
        {
            try
            {
                // Debug info
                //Console.WriteLine("Form data received:");
                //foreach (var key in Request.Form.Keys)
                //{
                //    Console.WriteLine($"{key}: {Request.Form[key]}");
                //}

                var id = Request.Form["id"].ToString();
                var quantityStr = Request.Form["quantity"].ToString();

                if (string.IsNullOrEmpty(id) || !int.TryParse(quantityStr, out int quantity))
                {
                    return BadRequest($"Invalid input - id: {id}, quantity: {quantityStr}");
                }

                var cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems") ?? new List<CartItem>();
                var existingItem = cartItems.FirstOrDefault(x => x.ProductId == id);
                
                if (existingItem != null)
                {
                    existingItem.Quantity = quantity;
                    HttpContext.Session.Set("CartItems", cartItems);
                    return new OkResult();
                }

                return NotFound($"Item not found in cart - id: {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
                return StatusCode(500, ex.Message);
            }
        }
    }
} 