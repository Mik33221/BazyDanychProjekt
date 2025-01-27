using Sklep3.Pages.Klient;
using MySql.Data.MySqlClient;
using Sklep3.Pages.Shared;


namespace Sklep3.Tests
{
	public class Sklep3Tests
	{   
        private int zwrocIDklienta(string imie, string nazwisko, string mail)
        {
			// Arrange
			CheckoutModel checkoutModel = new CheckoutModel();
			checkoutModel.zamowienie.imie = imie;
			checkoutModel.zamowienie.nazwisko = nazwisko;
			checkoutModel.zamowienie.mail = mail;

			// Act
			String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
			int idKlienta;
			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				// Rozpocznij transakcjê
				using (MySqlTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						idKlienta = checkoutModel.pobierzIDklienta(connection, transaction);
					}
					catch (Exception ex)
					{
						return -10;
					}
				}
			}
            return idKlienta;
		}

		[Fact]
		public void pobierzIDklientaTest()
		{
            int poprawny = zwrocIDklienta("Andrzej", "Kowalski", "andKow.123");
            int niepoprawny = zwrocIDklienta("Andrzej", "Kowalski", "ako");

			// Assert 
			Assert.Equal(1, poprawny);
            Assert.Equal(-1, niepoprawny);
        }

        [Fact]
        public void dodajKlientaTest()
        {
            // Arrange
            CheckoutModel checkoutModel = new CheckoutModel();
            checkoutModel.zamowienie.imie = "T";
            checkoutModel.zamowienie.nazwisko = "E";
            checkoutModel.zamowienie.mail = "ST";

            // Act
            String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
            int res;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Rozpocznij transakcjê
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        checkoutModel.dodajKlienta(connection, transaction);
                        res = checkoutModel.pobierzOstatniWstawionyIndeks(connection, transaction);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }

            // Assert 
            Assert.NotEqual(0, res);
        }

        [Fact]
        public void czyKlientIstniejeTest()
        {
            // Arrange
            CheckoutModel checkoutModel = new CheckoutModel();

            // Act
            bool tak = checkoutModel.czyKlientIstnieje(2);
            bool nie = checkoutModel.czyKlientIstnieje(-1);

            // Assert
            Assert.True(tak);
            Assert.False(nie);
        }

        [Fact]
        public void utworzZmowienieTest()
        {
            // Arrange
            CheckoutModel checkoutModel = new CheckoutModel();
            checkoutModel.zamowienie.idKlienta = 2;
            checkoutModel.zamowienie.adres = "TEST";

            // Act
            String connectionString = "Server=localhost;Database=sklep;Uid=root;Pwd=bazunia;";
            int res;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Rozpocznij transakcjê
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        checkoutModel.utworzZamowienie(connection, transaction);
                        res = checkoutModel.pobierzOstatniWstawionyIndeks(connection, transaction);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }

            // Assert
            Assert.NotEqual(0, res); 
        }

        [Fact]
        public void sprawdzPoprawnoscDanychTest()
        {
            // Arrange
            ProduktInfo produktInfo = new ProduktInfo();
            produktInfo.nazwa = "TEST";
            produktInfo.kategoria = "Konsola";
            produktInfo.ilosc = "21";
            produktInfo.cena = "21.21";

            // Act
            string error = produktInfo.sprawdzPoprawnoscDanych();

            // Assert
            Assert.Equal("", error);
        }
    }
}