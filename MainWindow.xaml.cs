using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MySql.Data.MySqlClient;

namespace WPF_HelloWorld
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private string connectionString = Helper.ConnectionString("dbvp");
		private string dateFormat = "dddd, dd MMMM yyyy - hh:mm:ss tt";
		private int seconds = 1;
		private string[] imageSourceArray = { "platano.jpg",
											  "manzana.jpeg",
											  "sandia.jpg"};
		private int pS = 5;
		private static string code = "";
		private string mainQuery;
		
		DispatcherTimer dateTimerA = new DispatcherTimer();
		DispatcherTimer dateTimerPS = new DispatcherTimer();
		DispatcherTimer dateTimerGB = new DispatcherTimer();

		private int[] ChangeAppRes()
		{
			this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
			this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

			int[] resArray = { Convert.ToInt32(this.Width), Convert.ToInt32(this.Height) };

			return resArray;
		}

		private void InitializeClock()
		{
			DispatcherTimer dateTimer = new DispatcherTimer();
			dateTimer.Interval = TimeSpan.FromSeconds(1);
			dateTimer.Tick += DateTimeTick;
			dateTimer.Start();
		}

		private void DateTimeTick(object sender, EventArgs e)
		{
			dateTimeClock.Text = DateTime.Now.ToString(dateFormat);
		}

		private void InternalClock()
		{
			dateTimerA.Interval = TimeSpan.FromSeconds(1);
			dateTimerA.Tick += InternalClockTick;
			dateTimerA.Start();
		}

		private void ProductScreenClock()
		{
			dateTimerPS.Interval = TimeSpan.FromSeconds(1);
			dateTimerPS.Tick += pSClock;
			dateTimerPS.Start();
		}

		private void pSClock(object sender, EventArgs e)
		{

			if (pS == 0)
			{
				productInfoStack.Visibility = Visibility.Collapsed;
				codeBarStack.Visibility = Visibility.Visible;

				seconds = 1;
				dateTimerA.Start();
				logoPlaceholderText.Text = "SUPERCALIFRAGILÍSTICOESPIALIDOSO";
			}
			pS--;
		}

		private void InternalClockTick(object sender, EventArgs e)
		{

			pS = 10;

			dateTimerPS.Stop();

			if (seconds % 5 == 0)
			{
				Random randomNumber = new Random();
				barcodeLaser.Visibility = Visibility.Hidden;
				barcodeImage.Visibility = Visibility.Collapsed;

				adImage.Source = new BitmapImage(new Uri($"/Images/{imageSourceArray[randomNumber.Next(imageSourceArray.Length)]}", UriKind.Relative));

				adImage.Visibility = Visibility.Visible;

				barcodeNumber.Visibility = Visibility.Hidden;
			}
			seconds++;
		}

		private void AdaptScreenSize()
		{
			int[] screenRes = ChangeAppRes();

			//Tamaño de la fuente.
			logoPlaceholderText.FontSize = Convert.ToInt16(screenRes[0] / 22);
			welcomeText.FontSize = Convert.ToInt16(screenRes[0] / 33);
			checkPriceText.FontSize = Convert.ToInt16(screenRes[0] / 33);
			dateTimeClock.FontSize = Convert.ToInt16(screenRes[0] / 54);

			//Márgenes.
			logoPlaceholderText.Margin = new Thickness(0, 0, 0, Convert.ToInt16(screenRes[1] / 22));
			welcomeText.Margin = new Thickness(0, 0, 0, Convert.ToInt16(screenRes[1] / 22));
			checkPriceText.Margin = new Thickness(0, 0, 0, Convert.ToInt16(screenRes[1] / 22));
			dateTimeClock.Margin = new Thickness(0, 0, Convert.ToInt16(screenRes[0] / 25), Convert.ToInt16(screenRes[1] / 21));

			//SVG sizes.
			barcodeLaser.Width = Convert.ToInt16(screenRes[0] / 5);
			barcodeLaser.Height = Convert.ToInt16(screenRes[1] / 5);
			barcodeLaser.Margin = new Thickness(0, 0, 0, -Convert.ToInt32(screenRes[1] / 8));

			barcodeImage.Width = Convert.ToInt16(screenRes[0] / 5);
			barcodeImage.Height = Convert.ToInt16(screenRes[1] / 5);

			adImage.Width = Convert.ToInt16(screenRes[0] / 5);
			adImage.Height = Convert.ToInt16(screenRes[1] / 5);

			barcodeNumber.Width = Convert.ToInt16(screenRes[0] / 5);
			barcodeNumber.Height = Convert.ToInt16(screenRes[1] / 5);
			barcodeNumber.Margin = new Thickness(0, -Convert.ToInt32(screenRes[1] / 9), 0, 0);

			greenWave.Width = screenRes[0];
			greenWave.Height = Convert.ToInt16(screenRes[1] / 2);
			greenWave.Margin = new Thickness(0, -Convert.ToInt32(screenRes[1] / 3), 0, 0);

		}
		private void Window_KeyDown(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			code += e.Text;

			barcodeLaser.Visibility = Visibility.Visible;
			barcodeImage.Visibility = Visibility.Visible;
			adImage.Visibility = Visibility.Collapsed;
			barcodeNumber.Visibility = Visibility.Visible;

			seconds = 1;

			pS = 10;
		}

		private void Enter_KeyDown(object sender, KeyEventArgs e)
		{
			
			if (!String.IsNullOrWhiteSpace(code))
			{
				if (e.Key == Key.Enter)
				{
					logoPlaceholderText.Text = "SUPERCALIFRAGILÍSTICOESPIALIDOSO";
					dateTimerA.Stop();
					pS = 10;

					if (barcodeLaser.IsVisible && barcodeImage.IsVisible && barcodeNumber.IsVisible)
					{
						barcodeLaser.Visibility = Visibility.Hidden;
						barcodeImage.Visibility = Visibility.Hidden;
						barcodeNumber.Visibility = Visibility.Hidden;
					}


					using (MySqlConnection serverConnection = new MySqlConnection(connectionString))
					{
						try
						{
							serverConnection.Open();
							mainQuery = $"SELECT producto_nombre, producto_precio, producto_stock, producto_imagen FROM productos WHERE producto_codigo = {code};";
							MySqlCommand query = new MySqlCommand(mainQuery, serverConnection);
							MySqlDataReader queryResult = query.ExecuteReader();

							if (queryResult.HasRows)
							{
								ProductScreenClock();
								checkPriceText.Text = "DATOS DEL PRODUCTO:";
								queryResult.Read();

								if (!queryResult.IsDBNull(3))
								{
									BitmapImage src = new BitmapImage();
									src.BeginInit();
									src.UriSource = new Uri(queryResult.GetString(3), UriKind.RelativeOrAbsolute);
									src.EndInit();
									productImage.Source = src;
								}

								productName.Text = $"PRODUCTO: {queryResult.GetString(0)}";
								productPrice.Text = $"PRECIO: ${String.Format("{0:0.##}", Convert.ToDecimal(queryResult.GetString(1)))}";
								productStock.Text = $"EXISTENCIA: {queryResult.GetString(2)}";

								codeBarStack.Visibility = Visibility.Collapsed;
								productInfoStack.Visibility = Visibility.Visible;

							}
							else
							{
								logoPlaceholderText.Text = "No se encontró el producto.";
							}
						}
						catch (MySqlException ex)
						{
							logoPlaceholderText.Text = "Lo sentimos, no se encontró este producto. Para más información consulte con algún encargado.";
						}
					}
					code = "";
				}

			}

		}

		public MainWindow()
		{
			InitializeClock();
			InternalClock();
			InitializeComponent();
			ChangeAppRes();
			AdaptScreenSize();

		}

	}
	}
