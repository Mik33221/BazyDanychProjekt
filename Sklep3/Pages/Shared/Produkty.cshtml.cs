﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Sklep3.Pages.Shared
{
	public class ProduktyModel : PageModel
	{
		public List<ProduktInfo> produktyLista = new List<ProduktInfo>();
		public Slownik slownik = new Slownik();

		public string aktualnaKategoria;
		public string aktualnaPlatforma;

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
					slownik.pobierzKategorie(connection);
					slownik.pobierzPlatformy(connection);

					aktualnaKategoria = Request.Query["kategoria"];
					aktualnaPlatforma = Request.Query["platforma"];

					String sql = zapytanieSQL();


					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
						if (!string.IsNullOrEmpty(aktualnaKategoria))
						{
							command.Parameters.AddWithValue("@kategoria", aktualnaKategoria);
						}
						if (!string.IsNullOrEmpty(aktualnaPlatforma))
						{
							command.Parameters.AddWithValue("@platforma", aktualnaPlatforma);
						}

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

			if (string.IsNullOrEmpty(aktualnaKategoria) && string.IsNullOrEmpty(aktualnaPlatforma))
			{
				return "SELECT * FROM produkty_view";
			}

			if (!string.IsNullOrEmpty(aktualnaKategoria) && string.IsNullOrEmpty(aktualnaPlatforma))
			{
				return "SELECT * FROM produkty_view WHERE kategoria = @kategoria";
			}

			if (!string.IsNullOrEmpty(aktualnaKategoria) && !string.IsNullOrEmpty(aktualnaPlatforma))
			{
				return "SELECT * FROM produkty_view WHERE kategoria = @kategoria AND platforma = @platforma";
			}

			return "SELECT * FROM produkty_view";
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
	}

	public class Slownik
	{
		public List<string> kategorieLista;
		public List<string> platformyLista;

		public Slownik()
		{
			kategorieLista = new List<string>();
			platformyLista = new List<string>();
		}

		public void pobierzKategorie(MySqlConnection connection)
		{
			string sqlKategorie = "SELECT * FROM kategorie";
			using (MySqlCommand command = new MySqlCommand(sqlKategorie, connection))
			{
				using (MySqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						kategorieLista.Add(reader.GetString(0));
					}
				}
			}
		}

		public void pobierzPlatformy(MySqlConnection connection)
		{
			string sqlPlatformy = "SELECT * FROM platformy";
			using (MySqlCommand command = new MySqlCommand(sqlPlatformy, connection))
			{
				using (MySqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						platformyLista.Add(reader.GetString(0));
					}
				}
			}
		}
	}
}
