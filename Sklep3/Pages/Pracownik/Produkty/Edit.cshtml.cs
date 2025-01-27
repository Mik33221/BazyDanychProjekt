using System.Net.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Shared;

namespace Sklep3.Pages.Pracownik.Produkty
{
    public class EditModel : PageModel
    {
        public ProduktInfo produktInfo = new ProduktInfo();
        public Slownik kategorie = new Slownik("kategorie");
        public Slownik platformy = new Slownik("platformy");
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
                                         "Pwd=bazunia;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM produkty_view WHERE ID_produktu=@id";

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
            produktInfo.id = Request.Query["id"];
            produktInfo.nazwa = Request.Form["nazwa"];
            produktInfo.ilosc = Request.Form["ilosc"];
            produktInfo.kategoria = Request.Form["kategoria"];
            produktInfo.platforma = Request.Form["platforma"];
            produktInfo.cena = Request.Form["cena"];

            errorMessage = produktInfo.sprawdzPoprawnoscDanych();
            if (errorMessage.Length > 0)
            {
				return;
            }

            try
            {
                string connectionString = "Server=localhost;" +
                                         "Database=sklep;" +
                                         "Uid=root;" +
                                         "Pwd=bazunia;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE produkty_view " +
                        "SET Nazwa=@nazwa, Ilość=@ilosc, Kategoria=@kategoria, Platforma=@platforma, Cena=@cena " +
                        "WHERE ID_produktu=@id";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nazwa", produktInfo.nazwa);
                        command.Parameters.AddWithValue("@ilosc", produktInfo.ilosc);
                        command.Parameters.AddWithValue("@kategoria", produktInfo.kategoria);
						command.Parameters.AddWithValue("@platforma", produktInfo.platforma);
                        command.Parameters.AddWithValue("@cena", produktInfo.cena);
                        command.Parameters.AddWithValue("@id", produktInfo.id);

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
