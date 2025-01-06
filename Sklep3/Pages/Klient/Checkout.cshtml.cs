using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Helpers;

namespace Sklep3.Pages.Klient
{
    public class CheckoutModel : PageModel
    {
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems");
            if (cartItems == null || !cartItems.Any())
            {
                Response.Redirect("/Klient/Cart");
            }
        }

        public void OnPost()
        {
            string adres = Request.Form["adres"];
            var cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems");

            if (string.IsNullOrEmpty(adres))
            {
                errorMessage = "Proszę podać adres dostawy";
                return;
            }

            if (cartItems == null || !cartItems.Any())
            {
                errorMessage = "Koszyk jest pusty";
                return;
            }

            try
            {
                String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Rozpocznij transakcję
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Utwórz zamówienie
                            string sqlZamowienie = "INSERT INTO zamowienia (idklienta, stan, adres, datazłożenia) VALUES (@idklienta, @stan, @adres, @data)";
                            using (MySqlCommand cmd = new MySqlCommand(sqlZamowienie, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@idklienta", 1); // TODO: Dodać prawdziwe ID klienta
                                cmd.Parameters.AddWithValue("@stan", "Złożone");
                                cmd.Parameters.AddWithValue("@adres", adres);
                                cmd.Parameters.AddWithValue("@data", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }

                            // Pobierz ID utworzonego zamówienia
                            long idZamowienia;
                            using (MySqlCommand cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection, transaction))
                            {
                                idZamowienia = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            // 2. Dodaj produkty do zamówienia i zaktualizuj stany magazynowe
                            foreach (var item in cartItems)
                            {
                                // Dodaj produkt do zamówienia
                                string sqlZamowienieProdukty = "INSERT INTO zamowienia_produkty (idzamowienia, idproduktu, ilosc) VALUES (@idzamowienia, @idproduktu, @ilosc)";
                                using (MySqlCommand cmd = new MySqlCommand(sqlZamowienieProdukty, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@idzamowienia", idZamowienia);
                                    cmd.Parameters.AddWithValue("@idproduktu", item.ProductId);
                                    cmd.Parameters.AddWithValue("@ilosc", item.Quantity);
                                    cmd.ExecuteNonQuery();
                                }

                                // Zaktualizuj stan magazynowy
                                string sqlUpdateStanMagazynowy = "UPDATE produkty SET ilosc = ilosc - @ilosc WHERE idproduktu = @idproduktu";
                                using (MySqlCommand cmd = new MySqlCommand(sqlUpdateStanMagazynowy, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ilosc", item.Quantity);
                                    cmd.Parameters.AddWithValue("@idproduktu", item.ProductId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Zatwierdź transakcję
                            transaction.Commit();

                            // Wyczyść koszyk
                            HttpContext.Session.Remove("CartItems");

                            successMessage = "Zamówienie zostało złożone pomyślnie!";
                            Response.Redirect("/Klient/Index");
                        }
                        catch (Exception ex)
                        {
                            // W przypadku błędu, wycofaj transakcję
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }
    }
} 