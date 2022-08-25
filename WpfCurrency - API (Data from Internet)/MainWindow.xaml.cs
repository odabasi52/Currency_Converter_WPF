using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json; //Install from Nuget Manager

namespace WpfCurrency
{
    public partial class MainWindow : Window
    {
        /*If you want to get data from internet you have to use exact same names as the site which
        you get GET response then convert it to .json file and store it as Root object*/
        public class Root
        {
            public Currencies rates { get; set; } 
            //In the site I used, rates are Currencies as double vallue so I give the name exactly same.
        }

        public class Currencies //You can add more currencies with same name from url.
        {
            public double USD { get; set; }
            public double EUR { get; set; }
            public double TRY { get; set; }
            public double AED { get; set; }
            public double CAD { get; set; }
            public double RUB { get; set; }
            public double KWD { get; set; }
            public double AUD { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            CurrencyText.Focus();
            GetValue();
        }

        #region First get GET response from url and return it as Root Object
        //with async, code can work parallel. It does not have to wait.
        public static async Task<Root> GetData(string url)
        {
            Root myRoot = new Root();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30); //wait 30 seconds to get GET response else timeout
                    HttpResponseMessage response = await client.GetAsync(url); //GET response (from url)
                    if (response.StatusCode == HttpStatusCode.OK) //If GET response is gotten from url the status code will be 200(OK)
                    {
                        string ResponseString = await response.Content.ReadAsStringAsync(); //turn GET response content to a string
                        Root ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString); //convert string to .json then read it as Root object
                        return ResponseObject;
                    }
                    return myRoot;
                }
            } catch { return myRoot; }
        }
        #endregion

        Root value = new Root();
        async void GetValue()
        {
            value = await GetData("https://openexchangerates.org/api/latest.json?app_id=eed04fa1b50e414783fad7bd9b14898e"); //await to get resualt as Root object
            CurrencyTable(); // after value is assigned get Currency Table
        }

        #region Set ComboBoxes Source with Values from Internet
        void CurrencyTable()
        {
            DataTable currencies = new DataTable();

            currencies.Columns.Add("Name");
            currencies.Columns.Add("Value");

            currencies.Rows.Add("-------SELECT-------", 0);
            currencies.Rows.Add("USD", value.rates.USD);
            currencies.Rows.Add("EUR", value.rates.EUR);
            currencies.Rows.Add("TRY", value.rates.TRY);
            currencies.Rows.Add("AED", value.rates.AED);
            currencies.Rows.Add("CAD", value.rates.CAD);
            currencies.Rows.Add("RUB", value.rates.RUB);
            currencies.Rows.Add("KWD", value.rates.KWD);
            currencies.Rows.Add("AUD", value.rates.AUD);

            cBoxFrom.DisplayMemberPath = "Name";
            cBoxFrom.SelectedValuePath = "Value";
            cBoxFrom.ItemsSource = currencies.DefaultView;
            cBoxFrom.SelectedIndex = 0;

            cBoxTo.DisplayMemberPath = "Name";
            cBoxTo.SelectedValuePath = "Value";
            cBoxTo.ItemsSource = currencies.DefaultView;
            cBoxTo.SelectedIndex = 0;
        }
        #endregion

        #region CheckConversionThenConvert
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (CurrencyText.Text == null || CurrencyText.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrencyText.Focus(); //after messagebox gone focus keyboard to text box
            }

            else if ((cBoxFrom.SelectedItem == null || cBoxFrom.SelectedIndex == 0) || (cBoxTo.SelectedItem == null || cBoxTo.SelectedIndex == 0))
            {
                MessageBox.Show("Select Currency", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            #region CurrencyCalculation
            else
            {
                string number = CurrencyText.Text;

                if (double.TryParse(number, out ConvertedValue))
                {
                    if (ConvertedValue < 0)
                    {
                        MessageBox.Show("Currency can not have negative value", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                        CurrencyText.Focus();
                    }

                    else
                    {
                        ConvertedValue = ConvertedValue * (double.Parse(cBoxTo.SelectedValue.ToString()) / double.Parse(cBoxFrom.SelectedValue.ToString()));
                        ConvertedLabel.Content = cBoxTo.Text + "  " + ConvertedValue.ToString("N3");
                    }
                }
                else
                {
                    MessageBox.Show("Enter correct form of currency", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                    CurrencyText.Focus();
                }
            }
            #endregion
        }
        #endregion

        #region Clear TextBox and Conversions
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cBoxFrom.SelectedIndex = 0;
            cBoxTo.SelectedIndex = 0;
            ConvertedLabel.Content = "00.000";
            CurrencyText.Text = "";
            CurrencyText.Focus();
        }
        #endregion

    }
}
