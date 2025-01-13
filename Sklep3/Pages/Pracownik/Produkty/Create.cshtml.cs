using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Shared;

namespace Sklep3.Pages.Pracownik.Produkty
{
    public class CreateModel : PageModel
    {
        public ProduktInfo produktInfo = new ProduktInfo();
        public Slownik kategorie = new Slownik("kategorie");
        public Slownik platformy = new Slownik("platformy");

        public string errorMessage = "";
        public string successMessage = "";
        public void OnGet()
        {
		}

        public void OnPost()
        {
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

			//dodaj produkt do bazy danych
			try
            {
                string connectionString = "Server=localhost;" +
                                          "Database=sklep;" +
                                          "Uid=root;" +
                                          "Pwd=bazunia;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
					string sql = "Insert INTO produkty_view" +
                        "(nazwa, ilosc, kategoria, platforma, cena) VALUES " +
                        "(@nazwa, @ilosc, @kategoria, @platforma, @cena);";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nazwa", produktInfo.nazwa);
                        command.Parameters.AddWithValue("@ilosc", produktInfo.ilosc);
                        command.Parameters.AddWithValue("@kategoria", produktInfo.kategoria);
                        command.Parameters.AddWithValue("@platforma", produktInfo.platforma);
                        command.Parameters.AddWithValue("@cena", produktInfo.cena);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            produktInfo.nazwa = "";
            produktInfo.ilosc = "";
            produktInfo.kategoria = "";
            produktInfo.platforma = "";
            produktInfo.cena = "";
            successMessage = "Poprawnie dodano produkt";

            Response.Redirect("/Pracownik/Produkty");

        }
    }
}
