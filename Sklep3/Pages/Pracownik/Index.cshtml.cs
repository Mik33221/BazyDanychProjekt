using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;

namespace Sklep3.Pages.Pracownik
{
	public class IndexModel : PageModel
	{
		public List<ProduktInfo> ProduktyLista = new List<ProduktInfo>();

		public void OnGet()
		{
			try
			{
				String connectionString = "Server=localhost;" +
										  "Database=sklep;" +
										  "Uid=root;" +
										  "Pwd=qwertyuiop;";

				using (MySqlConnection connection = new MySqlConnection(connectionString))
				{
					connection.Open();
					String sql = "SELECT * FROM produkty_view";
					using (MySqlCommand command = new MySqlCommand(sql, connection))
					{
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

								ProduktyLista.Add(produktInfo);
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

	public class ProduktInfo
	{
		public string id;
		public string nazwa;
		public string ilosc;
		public string kategoria;
		public string platforma;
		public string cena;
	}
}
