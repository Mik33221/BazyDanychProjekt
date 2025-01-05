using System.Net.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Klient;

namespace Sklep3.Pages.Pracownik.Produkty
{
    public class EditModel : PageModel
    {
        public ProduktInfo produktInfo = new ProduktInfo();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string id = Request.Query["id"];

            try
            {
                string connectionString = "Server=localhost;" +
                                         "Database=sklep;" +
                                         "Uid=root;" +
                                         "Pwd=qwertyuiop;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM produkty_view WHERE idproduktu=@id";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //produktInfo.id = "" + reader.GetInt32(0);
                                produktInfo.nazwa = reader.GetString(1);
                                produktInfo.ilosc = "" + reader.GetInt32(2);
                                produktInfo.kategoria = reader.GetString(3);
                                if (reader.IsDBNull(4))
                                {
                                    produktInfo.platforma = "";
                                }
                                else
                                {
                                    produktInfo.platforma = reader.GetString(4);
                                }
                                produktInfo.cena = "" + reader.GetMySqlDecimal(5);

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        public void OnPost()
        {
            string id = Request.Query["id"];
            produktInfo.nazwa = Request.Form["nazwa"];
            produktInfo.ilosc = Request.Form["ilosc"];
            produktInfo.kategoria = Request.Form["kategoria"];
            produktInfo.platforma = Request.Form["platforma"];
            produktInfo.cena = Request.Form["cena"];
            //produktInfo.id = Request.Query["id"];

            if (produktInfo.nazwa.Length == 0 || produktInfo.ilosc.Length == 0 ||
                produktInfo.kategoria.Length == 0 || produktInfo.cena.Length == 0)
            {
                errorMessage = "Wszystkie pola są wymagane";
                return;
            }

            try
            {
                string connectionString = "Server=localhost;" +
                                         "Database=sklep;" +
                                         "Uid=root;" +
                                         "Pwd=qwertyuiop;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE produkty_view " +
                        "SET nazwa=@nazwa, ilosc=@ilosc, kategoria=@kategoria, platforma=@platforma, cena=@cena " +
                        "WHERE idproduktu=@id";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nazwa", produktInfo.nazwa);
                        command.Parameters.AddWithValue("@ilosc", produktInfo.ilosc);
                        command.Parameters.AddWithValue("@kategoria", produktInfo.kategoria);
                        if (produktInfo.platforma.Length == 0)
                        {
                            command.Parameters.AddWithValue("@platforma", "");
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@platforma", produktInfo.platforma);
                        }
                        command.Parameters.AddWithValue("@cena", produktInfo.cena);
                        command.Parameters.AddWithValue("@id", id);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/Pracownik/Produkty");
        }
    }
}
