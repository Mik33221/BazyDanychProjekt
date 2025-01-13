using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Shared;

namespace Sklep3.Pages.Pracownik.Zamowienia
{
    public class IndexModel : PageModel
    {
		public List<ZamowienieInfo> ZamowieniaLista = new List<ZamowienieInfo>();
		public Slownik stany = new Slownik("stany");

		public string aktualnyStan;
		public string aktualneSortowanie;
		public string aktualnyKierunekSortowania;

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

					aktualnyStan = Request.Query["stan"];
					aktualneSortowanie = Request.Query["sortowanie"];
					aktualnyKierunekSortowania = Request.Query["kierunek"];

					string sql = zapytanieSQL();

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						if (!string.IsNullOrEmpty(aktualnyStan))
							command.Parameters.AddWithValue("@stan", aktualnyStan);

						if (!string.IsNullOrEmpty(aktualneSortowanie))
							command.Parameters.AddWithValue("@sortowanie", aktualneSortowanie);

						if (!string.IsNullOrEmpty(aktualnyKierunekSortowania))
							command.Parameters.AddWithValue("@kierunek", aktualnyKierunekSortowania);

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

		private string zapytanieSQL()
		{
			string sql = "SELECT * FROM zamowienia_view";

			if (!string.IsNullOrEmpty(aktualnyStan))
				sql += " WHERE stan = @stan";

			if (!string.IsNullOrEmpty(aktualneSortowanie))
			{
				sql += " ORDER BY " + aktualneSortowanie;
				if (aktualnyKierunekSortowania == "Malej¹co")
					sql += " DESC";
				
				if (aktualneSortowanie != "id_zamówienia")
					sql += ", id_zamówienia";
			}

			Console.WriteLine(sql);
			return sql;
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
