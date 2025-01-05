using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Sklep3.Pages.Pracownik.Zamowienia
{
    public class IndexModel : PageModel
    {
		public List<ZamowienieInfo> ZamowieniaLista = new List<ZamowienieInfo>();

		public void OnGet()
        {
			try
			{
				string connectionString = "Server=localhost;" +
										  "Database=sklep;" +
										  "Uid=root;" +
										  "Pwd=bazunia;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					string sql = "SELECT * FROM zamowienia_view";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ZamowienieInfo zamowienieInfo = new ZamowienieInfo();
								zamowienieInfo.id = "" + reader.GetInt32(0);
								zamowienieInfo.nazwaProduktu = reader.GetString(1);
								zamowienieInfo.ilosc = "" + reader.GetInt32(2);
								zamowienieInfo.stan = reader.GetString(3);
								zamowienieInfo.adres = reader.GetString(4);
								zamowienieInfo.data = reader.GetDateTime(5).ToString("yyyy-MM-dd");

								ZamowieniaLista.Add(zamowienieInfo);
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

	public class ZamowienieInfo
	{
		public string id;
		public string nazwaProduktu;
		public string ilosc;
		public string stan;
		public string adres;
		public string data;
	}
}
