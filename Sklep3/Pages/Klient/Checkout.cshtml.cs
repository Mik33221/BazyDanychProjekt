using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Helpers;

namespace Sklep3.Pages.Klient
{
    public class CheckoutModel : PageModel
    {
        public Zamowienie zamowienie = new Zamowienie();
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
            zamowienie.adres = Request.Form["adres"];
            var cartItems = HttpContext.Session.Get<List<CartItem>>("CartItems");
            
            zamowienie.imie = Request.Form["imie"];
            zamowienie.nazwisko = Request.Form["nazwisko"];
            zamowienie.mail = Request.Form["mail"];

            if (zamowienie.adres.Length == 0 || zamowienie.imie.Length == 0 || zamowienie.nazwisko.Length == 0 || zamowienie.mail.Length == 0)
            {
                errorMessage = "Wszystkie pola są wymagane";
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
                            // 0. Dodanie klienta do bazy
                            zamowienie.idKlienta = pobierzIDklienta(connection, transaction);
                            if (!czyKlientIstnieje(zamowienie.idKlienta))
                            {
                                zamowienie.idKlienta = pobierzIDklientaMail(connection, transaction);
                                if(czyKlientIstnieje(zamowienie.idKlienta))
                                {
                                    errorMessage = "Ten mail był podany przez innego klienta";
                                    return;
                                }

                                dodajKlienta(connection, transaction);
                                zamowienie.idKlienta = pobierzOstatniWstawionyIndeks(connection, transaction);
                            }
                            // 1. Utwórz zamówienie
                            utworzZamowienie(connection, transaction);
                            zamowienie.idZamowienia = pobierzOstatniWstawionyIndeks(connection, transaction);

                            // 2. Dodaj produkty do zamówienia i zaktualizuj stany magazynowe
                            foreach (var item in cartItems)
                            {
                                // Dodaj produkt do zamówienia
                                dodajProdukt(connection, transaction, item);

                                // Zaktualizuj stan magazynowy
                                zaktualizujMagazyn(connection, transaction, item);
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

        public int pobierzIDklienta(MySqlConnection connection, MySqlTransaction transaction)
        {
            string sqlKlient = "SELECT idKlienta FROM klienci WHERE nazwisko = @nazwisko AND imie = @imie AND mail = @mail";
            int idKlienta = -1;
            using (MySqlCommand cmd = new MySqlCommand(sqlKlient, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@nazwisko", zamowienie.nazwisko);
                cmd.Parameters.AddWithValue("@imie", zamowienie.imie);
                cmd.Parameters.AddWithValue("@mail", zamowienie.mail);
                cmd.ExecuteNonQuery();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        idKlienta = reader.GetInt32(0);
                    }
                }
            }
           return idKlienta;
        }

        public bool czyKlientIstnieje(int idKlienta)
        {
            if (idKlienta == -1)
                return false;
            return true;
        }

        public int pobierzIDklientaMail(MySqlConnection connection, MySqlTransaction transaction)
        {
            // Sprawdź czy mail istnieje
            string sqlKlient = "SELECT idKlienta FROM klienci WHERE mail = @mail";
            int idKlienta = -1;
            using (MySqlCommand cmd = new MySqlCommand(sqlKlient, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@mail", zamowienie.mail);
                cmd.ExecuteNonQuery();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        idKlienta = reader.GetInt32(0);
                    }
                }
            }
            return idKlienta;
        }

        public void dodajKlienta(MySqlConnection connection, MySqlTransaction transaction)
        {
            // Utwórz klienta
            string sqlKlient = "INSERT INTO klienci (nazwisko, imie, mail) VALUES (@nazwisko, @imie, @mail)";
            using (MySqlCommand cmd = new MySqlCommand(sqlKlient, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@nazwisko", zamowienie.nazwisko);
                cmd.Parameters.AddWithValue("@imie", zamowienie.imie);
                cmd.Parameters.AddWithValue("@mail", zamowienie.mail);
                cmd.ExecuteNonQuery();
            }
        }
        public int pobierzOstatniWstawionyIndeks(MySqlConnection connection, MySqlTransaction transaction)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection, transaction))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void utworzZamowienie(MySqlConnection connection, MySqlTransaction transaction)
        {
            string sqlZamowienie = "INSERT INTO zamowienia (idklienta, stan, adres, datazłożenia) VALUES (@idklienta, @stan, @adres, @data)";
            using (MySqlCommand cmd = new MySqlCommand(sqlZamowienie, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@idklienta", zamowienie.idKlienta);
                cmd.Parameters.AddWithValue("@stan", "Złożone");
                cmd.Parameters.AddWithValue("@adres", zamowienie.adres);
                cmd.Parameters.AddWithValue("@data", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public void dodajProdukt(MySqlConnection connection, MySqlTransaction transaction, CartItem item)
        {
            string sqlZamowienieProdukty = "INSERT INTO zamowienia_produkty (idzamowienia, idproduktu, ilosc) VALUES (@idzamowienia, @idproduktu, @ilosc)";
            using (MySqlCommand cmd = new MySqlCommand(sqlZamowienieProdukty, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@idzamowienia", zamowienie.idZamowienia);
                cmd.Parameters.AddWithValue("@idproduktu", item.ProductId);
                cmd.Parameters.AddWithValue("@ilosc", item.Quantity);
                cmd.ExecuteNonQuery();
            }
        }

        public void zaktualizujMagazyn(MySqlConnection connection, MySqlTransaction transaction, CartItem item)
        {
            string sqlUpdateStanMagazynowy = "UPDATE produkty SET ilosc = ilosc - @ilosc WHERE idproduktu = @idproduktu";
            using (MySqlCommand cmd = new MySqlCommand(sqlUpdateStanMagazynowy, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@ilosc", item.Quantity);
                cmd.Parameters.AddWithValue("@idproduktu", item.ProductId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class Zamowienie
    {
        public int idZamowienia;
        public int idKlienta;
        public string imie;
        public string nazwisko;
        public string mail;
        public string adres;
    }
} 