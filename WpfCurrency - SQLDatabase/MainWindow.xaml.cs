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

namespace WpfCurrency
{
    public partial class MainWindow : Window
    {
        SqlConnection connection;

        int CurrencyId = 0;

        public MainWindow()
        {
            InitializeComponent();

            string connectionString = ConfigurationManager.ConnectionStrings["WpfCurrency.Properties.Settings.ConnectionString"].ConnectionString;
            connection = new SqlConnection(connectionString);

            BindCurrency();
            ShowDataGrid();
        }

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

        #region CheckConversionThenConvert
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if(CurrencyText.Text == null || CurrencyText.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrencyText.Focus(); //after messagebox gone focus keyboard to textbox
            }
            
            else if((cBoxFrom.SelectedItem == null || cBoxFrom.SelectedIndex == 0) || (cBoxTo.SelectedItem == null || cBoxTo.SelectedIndex == 0))
            {
                MessageBox.Show("Select Currency", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            #region CurrencyCalculation
            else
            {
                string number = CurrencyText.Text;

                if (double.TryParse(number, out ConvertedValue))
                {
                    if(ConvertedValue < 0)
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

        #region Create DataTable for ComboBoxes (Currency Selection)
        void BindCurrency()
        {
            
            SqlDataAdapter adapter = new SqlDataAdapter("select * from CurrencyMaster ", connection);
            
            using(adapter)
            {
                DataTable currencies = new DataTable();
                adapter.Fill(currencies);

                DataRow newRow = currencies.NewRow(); //create new row
                newRow["Id"] = 0; //make first row SELECT (always)
                newRow["CurrencyName"] = "-------SELECT-------";
                currencies.Rows.InsertAt(newRow, 0);

                if(currencies.Rows.Count > 0)
                {
                    cBoxFrom.ItemsSource = currencies.DefaultView;
                    cBoxTo.ItemsSource = currencies.DefaultView;


                    cBoxFrom.DisplayMemberPath = "CurrencyName";
                    cBoxFrom.SelectedValuePath = "Amount";

                    cBoxTo.DisplayMemberPath = "CurrencyName";
                    cBoxTo.SelectedValuePath = "Amount";
                }
            }         

            cBoxFrom.SelectedIndex = 0;
            cBoxTo.SelectedIndex = 0;
        }
        #endregion


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Enter an amount please", "NO AMOUNT", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtAmount.Focus();
                }
                else if (txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Enter Currency Name please", "NO NAME", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtCurrencyName.Focus();
                }
                else
                {
                    if (CurrencyId != 0)
                    {
                        if (MessageBox.Show("You sure to update?", "UPDATE", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            SqlCommand command = new SqlCommand("UPDATE CurrencyMaster SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", connection);
                            connection.Open();
                            command.Parameters.AddWithValue("@Id", CurrencyId);
                            command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connection.Close();

                            MessageBox.Show("Data succesfully updated", "UPDATE", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("You sure to save?", "SAVE", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            SqlCommand command = new SqlCommand("insert into CurrencyMaster(Amount,CurrencyName) Values(@Amount,@CurrencyName)", connection);
                            connection.Open();
                            command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connection.Close();

                            MessageBox.Show("Data succesfully saved", "SAVE", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                txtAmount.Text = "";
                txtCurrencyName.Text = "";
                btnSave.Content = "Save";
                CurrencyId = 0;
                ShowDataGrid();
                BindCurrency();
            }
            catch {}
        }

        void ShowDataGrid()
        {
            SqlDataAdapter adapter = new SqlDataAdapter("select * from CurrencyMaster", connection);

            using(adapter)
            {
                DataTable table = new DataTable();
                adapter.Fill(table);

                dgvCurrency.ItemsSource = table.DefaultView;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            txtAmount.Text = "";
            txtCurrencyName.Text = "";
            btnSave.Content = "Save";
            CurrencyId = 0;
            ShowDataGrid();
            BindCurrency();
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid grid = dgvCurrency;
                DataRowView row = grid.CurrentItem as DataRowView; //get the selected cell with values
                if(row != null)
                {
                    CurrencyId = int.Parse(row["Id"].ToString());

                    if (grid.SelectedCells[0].Column.DisplayIndex == 0)
                    {
                        btnSave.Content = "Update";
                        txtAmount.Text = row["Amount"].ToString();
                        txtCurrencyName.Text = row["CurrencyName"].ToString();
                    }
                    else if (grid.SelectedCells[0].Column.DisplayIndex == 1)
                    {
                        if (MessageBox.Show("You Really Wanna Delete?", "DELETE", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            SqlCommand command = new SqlCommand("delete from CurrencyMaster where Id = @Id", connection);
                            connection.Open();
                            command.Parameters.AddWithValue("@Id", CurrencyId);
                            command.ExecuteNonQuery();
                            ShowDataGrid();
                            BindCurrency();
                            connection.Close();
                        }
                    }
                    else
                    {
                        txtAmount.Text = "";
                        txtCurrencyName.Text = "";
                        btnSave.Content = "Save";
                        ShowDataGrid();
                        BindCurrency();
                    }
                }
                
            }
            catch {}
        }
    }
}
