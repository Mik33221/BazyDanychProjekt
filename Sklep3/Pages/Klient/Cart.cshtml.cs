using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Helpers;
using Sklep3.Pages.Shared;

namespace Sklep3.Pages.Klient
{
    public class CartItem
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartModel : PageModel
    {
        public List<ProduktInfo> ProduktyWKoszykuLista = new List<ProduktInfo>();
        public Dictionary<string, int> Quantities = new Dictionary<string, int>();
        
        public void OnGet()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems") ?? new List<CartItem>();
            
            if (cartItems.Any())
            {
                try
                {
                    String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT * FROM produkty_view WHERE idproduktu IN (" + string.Join(",", cartItems.Select(x => x.ProductId)) + ")";
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ProduktInfo produktInfo = new ProduktInfo();
                                    produktInfo.id = "" + reader.GetInt32(0);
                                    produktInfo.nazwa = reader.GetString(1);
                                    produktInfo.ilosc = "" + reader.GetInt32(2);
                                    produktInfo.kategoria = reader.GetString(3);
                                    produktInfo.platforma = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                    produktInfo.cena = "" + reader.GetMySqlDecimal(5);

                                    ProduktyWKoszykuLista.Add(produktInfo);
                                    
                                    var cartItem = cartItems.First(x => x.ProductId == produktInfo.id);
                                    Quantities[produktInfo.id] = cartItem.Quantity;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }
            }
        }
    }
}
