using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Sklep3.Pages.Shared
{
	public class ProduktyModel : PageModel
	{
		public List<ProduktInfo> produktyLista = new List<ProduktInfo>();
		public Slownik kategorie = new Slownik("kategorie");
		public Slownik platformy = new Slownik("platformy");

		public string aktualnaKategoria;
		public string aktualnaPlatforma;
		public string aktualneSortowanie;
		public string aktualnyKierunekSortowania;

		public void OnGet()
		{
			try
			{
				String connectionString = "Server=localhost;" +
										  "Database=sklep;" +
										  "Uid=root;" +
										  "Pwd=bazunia;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

					aktualnaKategoria = Request.Query["kategoria"];
					aktualnaPlatforma = Request.Query["platforma"];
					aktualneSortowanie = Request.Query["sortowanie"];
					aktualnyKierunekSortowania = Request.Query["kierunek"];

					String sql = zapytanieSQL();

					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						if (!string.IsNullOrEmpty(aktualnaKategoria))
							command.Parameters.AddWithValue("@kategoria", aktualnaKategoria);

						if (!string.IsNullOrEmpty(aktualnaPlatforma))
							command.Parameters.AddWithValue("@platforma", aktualnaPlatforma);

						if (!string.IsNullOrEmpty(aktualneSortowanie))
							command.Parameters.AddWithValue("@sortowanie", aktualneSortowanie);

						if (!string.IsNullOrEmpty(aktualnyKierunekSortowania))
							command.Parameters.AddWithValue("@kierunek", aktualnyKierunekSortowania);
						

						using (MySqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ProduktInfo produktInfo = new ProduktInfo();
								produktInfo.id = "" + reader.GetInt32(0);
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

								produktyLista.Add(produktInfo);
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
			string sql = "SELECT * FROM produkty_view";

			if (!string.IsNullOrEmpty(aktualnaKategoria))
				sql += " WHERE Kategoria = @kategoria";

			if (!string.IsNullOrEmpty(aktualnaPlatforma))
				sql += " AND Platforma = @platforma";

			if (!string.IsNullOrEmpty(aktualneSortowanie))
			{
				sql += " ORDER BY " + aktualneSortowanie;
				if (aktualnyKierunekSortowania == "Malejąco")
					sql += " DESC";
			}

			Console.WriteLine(sql);
			return sql;
		}
	}

	public class ProduktInfo
	{
		public string id;
		public string nazwa;
		public string ilosc;
		public string kategoria;
		public string platforma;
		public string cena;

		public string sprawdzPoprawnoscDanych()
		{
			string error = "";
			if (nazwa.Length == 0 || ilosc.Length == 0 || kategoria.Length == 0 || cena.Length == 0)
				error += "Wszystkie pola są wymagane. ";

			if (!UInt32.TryParse(ilosc, out _))
				error += "Ilość musi myć liczbą całkowitą dodatnią. ";


			if (!decimal.TryParse(cena, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _) || cena[0] == '-')
			{
				error += "Cena musi być liczbą dodatnią z maksymalnie dwoma cyframi po przecinku. ";
			}
			else
			{	
				cena = cena.Replace(',', '.');
				string[] parts = cena.Split('.');

				if (parts.Length == 2 && parts[1].Length > 2)
					error += "Cena musi być liczbą z maksymalnie dwoma cyframi po przecinku. ";
			}

			return error;
		}
	}

	public class Slownik
	{
		public List<string> slownik;
        private string baza;

		public Slownik(string baza)
		{
			slownik = new List<string>();
			this.baza = baza;
			pobierzSlownik();
		}

		public void pobierzSlownik()
		{
			try
			{
				String connectionString = "Server=localhost;" +
										  "Database=sklep;" +
										  "Uid=root;" +
										  "Pwd=bazunia;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();

                    string sql = "SELECT * FROM " + baza;
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                slownik.Add(reader.GetString(0));
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
