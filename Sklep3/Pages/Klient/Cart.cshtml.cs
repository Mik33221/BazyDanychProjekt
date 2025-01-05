using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Helpers;

namespace Sklep3.Pages.Klient
{
    public class Index1Model : PageModel
    {
        public List<ProduktInfo> ProduktyWKoszykuLista = new List<ProduktInfo>();
        
        public void OnGet()
        {
            var cartItems = HttpContext.Session.Get<List<string>>("CartItems") ?? new List<string>();
            
            if (cartItems.Any())
            {
                try
                {
                    String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT * FROM produkty_view WHERE idproduktu IN (" + string.Join(",", cartItems) + ")";
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
