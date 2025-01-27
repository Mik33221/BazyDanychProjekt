using System.Net.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Shared;

namespace Sklep3.Pages.Pracownik.Zamowienia
{
    public class EditModel : PageModel
    {
		public ZamowienieInfo zamowienieInfo = new ZamowienieInfo();
		public Slownik stany = new Slownik("stany");
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
					string sql = "SELECT Stan FROM zamowienia_view WHERE ID_zamowienia=@id";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								zamowienieInfo.stan = reader.GetString(0);
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
			zamowienieInfo.stan = Request.Form["stan"];
			//zamowienieInfo.id = Request.Query["id"];

			if (zamowienieInfo.stan.Length == 0)
			{
				errorMessage = "Wszystkie pola s¹ wymagane";
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
					string sql = "UPDATE zamowienia_view " +
						"SET Stan=@stan WHERE ID_zamowienia=@id";

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@stan", zamowienieInfo.stan);
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

			Response.Redirect("/Pracownik/Zamowienia");
		}
	}
}

