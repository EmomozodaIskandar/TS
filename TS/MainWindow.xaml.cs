using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using TS.Classes;
using Microsoft.Office.Interop.Excel;

namespace TS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region Declaration
        public static SQLiteConnection? m_dbConnection;
        List<Products> ProductsList = new List<Products>();
        List<CitiesClass> MRCityNames = new List<CitiesClass>();
        List<CitiesClass> DECityNames = new List<CitiesClass>();
        List<TarifClass> tarifClassList = new List<TarifClass>();
        string? sendCode;
        int SenderId;

        #endregion
        public MainWindow()
        {
            InitializeComponent();

            try 
            {
                m_dbConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["connection"].ConnectionString);
                m_dbConnection.Open();
                DatumTextBlock.Text = "Datum: " + System.DateTime.Now.ToShortDateString();
                Time();
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void Time()
        {
            try
            {
                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                SendungsnummerTextBox.Text = DateTime.Now.ToString("yyyy MM dd HH mm ss").Replace(" ", string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void PreviewTextInput1(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AddTransactionClick(object sender, RoutedEventArgs e)
        {
            ProductsList.Clear();
            try
            {
                if ( SenderCitiesComboBox.SelectedIndex < 0  || SenderFirstNameTextBox.Text.Trim() == string.Empty || SenderLastNameTextBox.Text.Trim() == string.Empty || SenderPhoneTextBox.Text.Trim() == string.Empty
                    || RecipientCitiesComboBox.SelectedIndex < 0 || RecipientFirstNameTextBox.Text.Trim() == string.Empty || RecipientLastNameTextBox.Text.Trim() == string.Empty
                    || RecipientPhoneTextBox.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("All fields must be filled!!");
                }
                else
                {
                    if (TarifCmb.SelectedIndex < 0)
                        MessageBox.Show("Select tarif for products please !!");
                    else
                    {

                        using (SQLiteConnection Connection = new SQLiteConnection(MainWindow.m_dbConnection))
                        {
                            SQLiteCommand command = new SQLiteCommand(Connection);

                            //Insert into Senders
                            command.CommandText = "INSERT INTO SENDERS(FirstName,LastName,Phone,Address_Id) VALUES(@PARAM1, @PARAM2, @PARAM3, @PARAM4)";
                            command.CommandType = CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", SenderFirstNameTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param2", SenderLastNameTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param3", SenderPhoneTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param4", SenderCitiesComboBox.SelectedValue));
                            command.ExecuteNonQuery();


                            //Insert into Recipients
                            command.CommandText = "INSERT INTO Recipients(FirstName,LastName,Phone,Address_Id) VALUES(@PARAM1, @PARAM2, @PARAM3, @PARAM4)";
                            command.CommandType = CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", RecipientFirstNameTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param2", RecipientLastNameTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param3", RecipientPhoneTextBox.Text.Trim()));
                            command.Parameters.Add(new SQLiteParameter("@param4", RecipientCitiesComboBox.SelectedValue));
                            command.ExecuteNonQuery();

                            //Select SenderId
                            command.CommandText = "SELECT last_insert_rowid()";
                            command.CommandType = CommandType.Text;

                            SenderId = Convert.ToInt32(command.ExecuteScalar());


                            //Insert into Products


                            ProductsList.Add
                            (
                            new Products
                            {
                                Weight = Convert.ToDecimal(ProductWeigthTextBox.Text.Trim()),
                                Describe = ProductDescribeTextBox.Text.Trim(),
                                SenderId = SenderId,
                                RecipientId = SenderId,

                            }
                            );
                            MessageBox.Show("Added!!");
                            dg_Products.ItemsSource = ProductsList;
                            dg_Products.Items.Refresh();
                            AddTransactionButton.IsEnabled = false;
                            AddTransactionButton.Visibility = Visibility.Hidden;
                            AddAnotherProductButton.IsEnabled = true;
                            AddAnotherProductButton.Visibility = Visibility.Visible;
                            AddAllAddedProductsButton.IsEnabled = true;
                            AddAllAddedProductsButton.Visibility = Visibility.Visible;

                            TotalWeightTextBlock.Visibility = Visibility.Visible;
                            TotalWeightTextBlock.Text = ProductsList[0].Weight.ToString();

                            CountOfProductsTextBlock.Visibility = Visibility.Visible;
                            CountOfProductsTextBlock.Text = "1";


                            TotalCostTextBlock.Visibility = Visibility.Visible;
                            TotalCostTextBlock.Text = Convert.ToString(Convert.ToDouble(TotalWeightTextBlock.Text) * Convert.ToDouble(TarifCmb.SelectedValue));


                            SenderFirstNameTextBox.Text = string.Empty;
                            SenderLastNameTextBox.Text = string.Empty;
                            SenderPhoneTextBox.Text = string.Empty;
                            SenderCitiesComboBox.SelectedIndex = -1;

                            RecipientFirstNameTextBox.Text = string.Empty;
                            RecipientLastNameTextBox.Text = string.Empty;
                            RecipientPhoneTextBox.Text = string.Empty;
                            RecipientCitiesComboBox.SelectedIndex = -1;

                            ProductDescribeTextBox.Text = string.Empty;
                            ProductWeigthTextBox.Text = string.Empty;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void AddProducts(Products products)
        {
            try
            {
                using(SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);

                    command.CommandText = "INSERT INTO Products(Weight,Type, Sender_id, Recipient_id, Send_Code) Values(@param1, @param2, @param3, @param4, @param5)";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@param1", products.Weight));
                    command.Parameters.Add(new SQLiteParameter("@param2", products.Describe));
                    command.Parameters.Add(new SQLiteParameter("@param3", products.SenderId));
                    command.Parameters.Add(new SQLiteParameter("@param4", products.SenderId));
                    command.Parameters.Add(new SQLiteParameter("@param5", sendCode));
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message);
            }
        }

        private void TransactionLoaded(object sender, RoutedEventArgs e)
        {
            TransactionList();
            FillCityNamesList("MR", false);
            FillCityNamesList("MR", true);
        }
        private void TransactionList() 
        {
            
            try
            {
                using(SQLiteConnection Connection = new SQLiteConnection(MainWindow.m_dbConnection)) 
                {
                    SQLiteCommand command = new SQLiteCommand(Connection);
                    command.CommandText = @"SELECT 
    SEND_CODE AS sendCode,
    SUM(WEIGHT) as productsWeight,
    (Select Senders.FirstName || ' ' || Senders.LastName from senders where id = products.Sender_Id) as sendersFullName, 
    (Select recipients.FirstName || ' ' || recipients.LastName from recipients where id = products.Recipient_Id) as recipientsFullName,
    (Select (Select Addresses.City || ',' || Addresses.Country from addresses where id = senders.Address_Id ) from Senders where id = products.Sender_Id) as sendersAddress,
    (Select (Select Addresses.City || ',' || Addresses.Country from addresses where id = recipients.Address_Id ) from recipients where id = products.Recipient_Id) as recipientsAddress,
(Select senders.Phone from senders where id = products.Sender_Id) as sendersPhone,
    (Select recipients.Phone from recipients where id = products.Recipient_Id) as recipientsPhone,
    (case when products.isSended == 0 then ""Sent"" else ""Delivered"" end) as sendStatus
    
     
FROM 
    PRODUCTS
GROUP BY 
    SEND_CODE;
";
                    command.CommandType= CommandType.Text;

                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    dataAdapter.Fill(dataSet, "Transactions");
                    dg_transactions.DataContext = dataSet;
                    dg_transactions.Items.Refresh();
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteTransactionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((MessageBox.Show("Are you sure!!", "Confirming", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                {
                    var row = dg_transactions.SelectedItems[0] as DataRowView;
                    string sendCode = Convert.ToString(row["sendCode"]);
                    using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                    {
                        SQLiteCommand command = new SQLiteCommand(connection);
                        command.CommandText = "Delete from products where send_code = @param1";
                        command.CommandType= CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@param1", sendCode));
                        command.ExecuteScalar();
                        MessageBox.Show("Deleted!!");
                        TransactionList();

                        
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message );
            }
        }

        private void AddAnotherProductClick(object sender, RoutedEventArgs e)
        {
            if(ProductWeigthTextBox.Text.Trim()!=string.Empty && ProductDescribeTextBox.Text.Trim()!=string.Empty)
            {

                ProductsList.Add
                    (
                        new Products
                        {
                            Weight = Convert.ToDecimal(ProductWeigthTextBox.Text.Trim()),
                            Describe = ProductDescribeTextBox.Text.Trim(),
                            SenderId = SenderId,
                            RecipientId = SenderId,
                        }
                    );
                dg_Products.ItemsSource = ProductsList;
                dg_Products.Items.Refresh();
                CountOfProductsTextBlock.Text = ProductsList.Count.ToString();
                TotalWeightTextBlock.Text = Convert.ToString( Convert.ToDecimal(TotalWeightTextBlock.Text)+ Convert.ToDecimal(ProductWeigthTextBox.Text));
                TotalCostTextBlock.Text = Convert.ToString(Convert.ToDouble(TotalWeightTextBlock.Text)*Convert.ToDouble(TarifCmb.SelectedValue));
                ProductWeigthTextBox.Text = string.Empty;
                ProductDescribeTextBox.Text = string.Empty;

            }
            else
            {
                MessageBox.Show("Fill Weight and Describe fields!!");
            }

        }

        private void AddAllAddedProductsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                sendCode = SendungsnummerTextBox.Text;
                using(SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);

                    for(int i=0; i<ProductsList.Count; i++) 
                    {
                        AddProducts(ProductsList[i]);
                    }
                    TransactionList();
                    if(PaidInGermanyTextBlock.Text == string.Empty)
                    {
                        PaidInGermanyTextBlock.Text = "0";
                    }
                    if(PaidInMaroccoTextBlock.Text == string.Empty)
                    {
                        PaidInMaroccoTextBlock.Text = "0";
                    }
                    int sum = Convert.ToInt32(PaidInGermanyTextBlock.Text) + Convert.ToInt32(PaidInMaroccoTextBlock.Text);
                    if (sum < Convert.ToInt32(TotalCostTextBlock.Text))
                    {
                        MessageBox.Show("The sum is not enough!!");
                    }
                    else
                    {

                        command.CommandText = "INSERT INTO PAYMENTS(PAIDINGERMANY,PAIDINMAROCCO,SENDER_ID,RECIPIENTS_ID) VALUES(@PARAM1,@PARAM2,@PARAM3,@PARAM4)";
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@PARAM1", PaidInGermanyTextBlock.Text));
                        command.Parameters.Add(new SQLiteParameter("@PARAM2", PaidInMaroccoTextBlock.Text));
                        command.Parameters.Add(new SQLiteParameter("@PARAM3", SenderId));
                        command.Parameters.Add(new SQLiteParameter("@PARAM4", SenderId));
                        command.ExecuteNonQuery();
                        TransactionList();


                        dg_Products.ItemsSource = null;
                        dg_Products.Visibility = Visibility.Hidden;
                        AddAnotherProductButton.Visibility = Visibility.Hidden;
                        AddTransactionButton.Visibility = Visibility.Visible;
                        AddAllAddedProductsButton.Visibility = Visibility.Hidden;
                        AddTransactionButton.IsEnabled = true;

                    }

                    MessageBox.Show("Added!!");
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void AddAddressClick(object sender, RoutedEventArgs e)
        {
            if (AddAddressTextBox.Text.Trim() != string.Empty && CountryNameComboBox.SelectedIndex > -1)
            {
                AddAddressList();
            }
            else
                MessageBox.Show("Fill required fileds!!!");
        }
        
        private void AddAddressList()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = "INSERT INTO ADDRESSES(CITY, COUNTRY) VALUES(@PARAM1, @PARAM2)";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@PARAM1", AddAddressTextBox.Text.Trim()));
                    if (CountryNameComboBox.Text.Trim().ToLower() == "germany")
                        command.Parameters.Add(new SQLiteParameter("@PARAM2", "DE"));
                    else
                        command.Parameters.Add(new SQLiteParameter("@PARAM2", "MR"));
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("Added!!");
                AddAddressTextBox.Text = string.Empty;
                CountryNameComboBox.SelectedIndex = -1;
                AddressList();
                FillCityNamesList("MR", false);
                FillCityNamesList("DE", true);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }



        

        private void RecipientAddressToDEChangedClick(object sender, RoutedEventArgs e)
        {
            if (RecipientAddressToDEChangedCheckBox.IsChecked == true)
            {
                FillCityNamesList("DE", false);
            }
            else
            {

                FillCityNamesList("MR", false);
            }
        }

        private void FillCityNamesList(string Country, bool IsSender)
        {  
            
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = "Select id as Id, City as City  from Addresses where Country=@param1;";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@param1", Country));
                    SQLiteDataReader dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        if (Country == "DE")
                        {
                            DECityNames.Clear();
                            while (dataReader.Read())
                            {

                                DECityNames.Add(new CitiesClass
                                {
                                    CityName = Convert.ToString(dataReader["City"]),
                                    Id = Convert.ToInt16(dataReader["Id"]),
                                }); ;
                            }
                            if(IsSender)
                            {
                                SenderCitiesComboBox.ItemsSource = DECityNames;
                                SenderCitiesComboBox.Items.Refresh();
                            }
                            else
                            {
                                RecipientCitiesComboBox.ItemsSource = DECityNames;
                                RecipientCitiesComboBox.Items.Refresh();
                            }
                        }
                        else
                        {
                            MRCityNames.Clear();
                            while (dataReader.Read())
                            {

                                MRCityNames.Add(new CitiesClass
                                {
                                    CityName = Convert.ToString(dataReader["City"]),
                                    Id = Convert.ToInt16(dataReader["Id"]),
                                }); ;
                            }
                            if (IsSender)
                            {
                                SenderCitiesComboBox.ItemsSource = MRCityNames;
                                SenderCitiesComboBox.Items.Refresh();
                            }
                            else
                            {
                                RecipientCitiesComboBox.ItemsSource = MRCityNames;
                                RecipientCitiesComboBox.Items.Refresh();
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddressTabLoaded(object sender, RoutedEventArgs e)
        {
            AddressList();
        }
        private void AddressList()
        {
            try
            {
                using(SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = @"SELECT ID AS Id, CITY AS City, 
                                                CASE WHEN COUNTRY IS 'DE'
                                                THEN 'GERMANY'
                                                ELSE 'MAROCCO'
                                                END AS Country
                                                FROM ADDRESSES";
                    command.CommandType = CommandType.Text;

                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    dataAdapter.Fill(dataSet, "AddressesList");
                    dgAddresses.DataContext = dataSet;
                    dgAddresses.Items.Refresh();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteAddressClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((MessageBox.Show("Are You sure?!", "Confirming", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                {
                    var row = (DataRowView)dgAddresses.SelectedItems[0];
                    int id = Convert.ToInt16(row["Id"]);
                    using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                    {
                        SQLiteCommand command = new SQLiteCommand(connection);
                        command.CommandText = "DELETE FROM ADDRESSES WHERE ID=@PARAM1;";
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@PARAM1", id));
                        command.ExecuteNonQuery();
                        AddressList();
                        FillCityNamesList("MR", false);
                        FillCityNamesList("MR", true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HomePageLoaded(object sender, RoutedEventArgs e)
        {
            FillCityNamesList("MR", false);
            FillCityNamesList("MR", true);
            FillPriceCmb();
        }

        private void DeleteProductsListElementClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((MessageBox.Show("Are You sure?!", "Confirming", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                {
                    Products products = (Products)dg_Products.SelectedItems[0];

                    int id = Convert.ToInt16(products.id);

                    for (int i = 0; i < ProductsList.Count; i++)
                    {
                        if (ProductsList[i].id == id)
                        {
                            ProductsList.Remove(ProductsList[i]);
                            break;
                        }
                    }
                    dg_Products.ItemsSource = ProductsList;
                    dg_Products.Items.Refresh(); 
                    CountOfProductsTextBlock.Text = ProductsList.Count.ToString();
                    TotalWeightTextBlock.Text = Convert.ToString(Convert.ToDecimal(TotalWeightTextBlock.Text) - Convert.ToDecimal(products.Weight));
                    TotalCostTextBlock.Text = Convert.ToString(Convert.ToDecimal(TotalWeightTextBlock.Text) * Convert.ToDecimal(TarifCmb.SelectedValue));

                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message, "Error");
            }

        }

        private void SenderAddressToMRChangedClick(object sender, RoutedEventArgs e)
        {
            if (SenderAddressToMRChangedCheckBox.IsChecked == true)
            {
                RecipientAddressToDEChangedCheckBox.IsChecked = true;
                SenderAddressToDEChangedCheckBox.IsChecked = false;
                FillCityNamesList("MR", true);
                FillCityNamesList("DE", false);
            }
        }
        private void SenderAddressToDEChangedClick(object sender, RoutedEventArgs e)
        {
            if (SenderAddressToDEChangedCheckBox.IsChecked == true)
            {
                RecipientAddressToDEChangedCheckBox.IsChecked= false;
                SenderAddressToMRChangedCheckBox.IsChecked= false;
                FillCityNamesList("DE", true);
                FillCityNamesList("MR", false);
            }
        }

        

        

        private void PaidInGermanyTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(PaidInGermanyTextBlock.Text!=string.Empty && TotalCostTextBlock.Text != string.Empty)
                PaidInMaroccoTextBlock.Text = Convert.ToString(Convert.ToDouble(TotalCostTextBlock.Text) - Convert.ToDouble(PaidInGermanyTextBlock.Text));
            else
                PaidInGermanyTextBlock.Text = string.Empty;
        }

        private void TarifTabLoaded(object sender, RoutedEventArgs e)
        {
            TarifList();
        }

        private void AddTarifButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(TarifCostTextBox.Text!=string.Empty && TarifNameTextBox.Text!=string.Empty)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                    {
                        SQLiteCommand command = new SQLiteCommand(connection);
                        command.CommandText = "INSERT INTO TARIFS(NAME, COST) VALUES(@param1, @param2)";
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@param1", TarifNameTextBox.Text.Trim()));
                        command.Parameters.Add(new SQLiteParameter("@param2", TarifCostTextBox.Text.Trim()));
                        command.ExecuteNonQuery();
                        MessageBox.Show("Added!", "Information"); 
                        TarifList();
                        FillPriceCmb();
                        TarifNameTextBox.Text = string.Empty;
                        TarifCostTextBox.Text = string.Empty;

                    }
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message, "eror");
            }

        }
        private void TarifList()
        {
            try
            {
                using(SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = "Select Id as Id , Name as Tname, Cost as Tcost from Tarifs order by Id DESC";
                    command.CommandType = CommandType.Text;

                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    dataAdapter.Fill(dataSet, "Tarifdg");
                    dgTarif.DataContext = dataSet;
                    dgTarif.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void DeleteTarifClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MessageBox.Show("Are you sure?!", "Confirming", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var row = dgTarif.SelectedItems[0] as DataRowView;
                    int id = Convert.ToInt32(row["Id"]);
                    using(SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                    {
                        SQLiteCommand command = new SQLiteCommand(connection);
                        command.CommandText = "Delete from tarifs where id = @param1";
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@param1", id));
                        command.ExecuteNonQuery();
                        MessageBox.Show("Deleted!");
                        TarifList();
                        FillPriceCmb();

                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private void FillPriceCmb()
        {
            tarifClassList.Clear();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = "Select id as Id, Name as Tname, Cost as Tcost from tarifs order by id desc;";
                    command.CommandType = CommandType.Text;
                    var dataReader = command.ExecuteReader();
                    if(dataReader.HasRows)
                    {
                        while(dataReader.Read())
                        {
                            tarifClassList.Add(
                                new TarifClass
                                {
                                    Name = dataReader["Tname"].ToString(),
                                    Cost = Convert.ToDouble(dataReader["Tcost"]),
                                    Id = Convert.ToInt32(dataReader["Id"]),
                                }
                                );
                        }
                        TarifCmb.ItemsSource = tarifClassList;
                        TarifCmb.Items.Refresh();
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show(ex.Message, "error"); 
            }
        }

        

        private void IsSendedButtonClick(object sender, RoutedEventArgs e)
        {
            var row = dg_transactions.SelectedItems[0] as DataRowView;
            string? sendCode = row["sendCode"].ToString();
            using(SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
            {
                SQLiteCommand cmd = new SQLiteCommand(connection);
                cmd.CommandText = "Select isSended from products where send_code = @param1;";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SQLiteParameter("@param1", sendCode));
                bool sendStatus = !Convert.ToBoolean(cmd.ExecuteScalar());
                
                cmd.CommandText = "UPDATE products set isSended = @param2 where Send_Code = @param1;";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SQLiteParameter("@param1", sendCode));
                cmd.Parameters.Add(new SQLiteParameter("@param2", sendStatus));
                cmd.ExecuteNonQuery();
                TransactionList();
                cmd.Parameters.Clear();
            }
        }

        private void SearchTransactionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int pos = -1;
                string? typedText = SearchTransactionTextBox.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(typedText))
                {
                    for(int i = 0; i < dg_transactions.Items.Count; i++)
                    {
                        DataRowView row = dg_transactions.Items[i] as DataRowView;
                        string? SendersFullName = row["sendersFullName"].ToString().ToLower();
                        string? RecipientsFullName = row["recipientsFullName"].ToString().ToLower();
                        if(SendersFullName.StartsWith(typedText))
                        {
                            object item = dg_transactions.Items[i];
                            dg_transactions.SelectedItem = item;
                            dg_transactions.ScrollIntoView(item);
                            pos = dg_transactions.SelectedIndex;
                            SearchTransactionTextBox.Background = new SolidColorBrush(Colors.White);
                            break;
                        }
                        else if (RecipientsFullName.StartsWith(typedText))
                        {
                            object item = dg_transactions.Items[i];
                            dg_transactions.SelectedItem = item;
                            dg_transactions.ScrollIntoView(item);
                            pos = dg_transactions.SelectedIndex;
                            SearchTransactionTextBox.Background = new SolidColorBrush(Colors.White);
                            break;
                        }
                        else
                        {
                            SearchTransactionTextBox.Background = new SolidColorBrush(Colors.HotPink);
                        }
                    }
                }
                else
                {
                    SearchTransactionTextBox.Background = new SolidColorBrush ((Colors.White) );
                    dg_transactions.SelectedIndex = -1;
                }
                if(pos == -1 && !string.IsNullOrEmpty(typedText))
                {
                    SearchTransactionTextBox.Background = new SolidColorBrush (Colors.HotPink);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
            
        }
        private void UsersList()
        {
            try
            {
                using(SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "Select id as Id, firstname || ' ' || lastname as UserFullname, username as username from Users;"; 
                    cmd.CommandType = CommandType.Text;

                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dataSet, "Userlist");
                    dg_users.DataContext = dataSet;
                    dg_users.Items.Refresh();
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show (ex.Message, "error");
            }

        }

        private void DeleteUserButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dg_users.Items.Count > 1)
                {
                    DataRowView row = dg_users.SelectedItems[0] as DataRowView;
                    int id = Convert.ToInt16(row["Id"]);
                    if(MessageBox.Show("Are you sure?!", "Confirming", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                        {
                            SQLiteCommand cmd = new SQLiteCommand(connection);
                            cmd.CommandText = "Delete from users where id=@param1";
                            cmd.Parameters.Add(new SQLiteParameter("@param1", id));
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Deleted!!");
                            UsersList();

                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("You can't delete all users, because than nobody can enter to the programm!!");
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show (ex.Message, "error");
            }
        }

        private void UserAddButtonClick(object sender, RoutedEventArgs e)
        {
            if(UserPasswordTextBox.Password.Trim()!=string.Empty && UserLastnameTextBox.Text.Trim()!=string.Empty && UserFirstnameTextBox.Text.Trim()!=string.Empty && UserUsernameTextBox.Text.Trim()!=string.Empty)
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                    {
                        SQLiteCommand cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "insert into users(firstname, lastname, username, password) values(@param1, @param2, @param3, @param4);" ;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@param1", UserFirstnameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param2", UserLastnameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param3", UserUsernameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param4", UserPasswordTextBox.Password.Trim()));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Added!!");
                        UsersList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "error");
                }
            }
            else
            {
                MessageBox.Show("Fill all fields please!!");
            }
            
        }

        private void AccountsTabLoaded(object sender, RoutedEventArgs e)
        {
            UsersList();
        }

        private void PrintAlltransactionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string template = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\AllTransactions.xlsx";
                Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
                Workbook workbook ;
                _Worksheet worksheet ;
                workbook = application.Workbooks.Open(template, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                worksheet = (Worksheet)workbook.ActiveSheet;
                worksheet.Cells[1, 1] = "SendCode";
                worksheet.Cells[1, 2] = "Senders Fullname";
                worksheet.Cells[1, 3] = "Senders phone";
                worksheet.Cells[1, 4] = "Senders address";
                worksheet.Cells[1, 5] = "Recipients Fullname";
                worksheet.Cells[1, 6] = "Recipients phone";
                worksheet.Cells[1, 7] = "Recipients address";
                worksheet.Cells[1, 8] = "Products weight";
                worksheet.Cells[1, 9] = "Send status";
                for(int i=0; i<dg_transactions.Items.Count; i++)
                {
                    DataRowView row = dg_transactions.Items[i] as DataRowView;
                    worksheet.Cells[i + 2, 1] = row["sendCode"].ToString();
                    worksheet.Cells[i + 2, 2] = row["sendersFullName"].ToString();
                    worksheet.Cells[i + 2, 3] = row["sendersPhone"].ToString();
                    worksheet.Cells[i + 2, 4] = row["sendersAddress"].ToString();
                    worksheet.Cells[i + 2, 5] = row["recipientsFullName"].ToString();
                    worksheet.Cells[i + 2, 6] = row["recipientsPhone"].ToString();
                    worksheet.Cells[i + 2, 7] = row["recipientsAddress"].ToString();
                    worksheet.Cells[i + 2, 8] = row["productsWeight"].ToString();
                    worksheet.Cells[i + 2, 9] = row["sendStatus"].ToString();

                }

                application.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void PrintTransactionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string template = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Transaction.xlsx";
                Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application(); 
                application.Visible = true;
                Workbook workbook = application.Workbooks.Open(template, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                _Worksheet worksheet = (Worksheet)workbook.ActiveSheet;
                DataRowView row = dg_transactions.SelectedItems[0] as DataRowView;
                string sendCode = row["sendCode"].ToString();


                worksheet.Cells[1, 1] = "SendCode";
                worksheet.Cells[1, 2] = sendCode;
                worksheet.Cells[2, 1] = "Senders Fullname";
                worksheet.Cells[2, 2] = row["sendersFullName"].ToString();
                worksheet.Cells[3, 1] = "Senders phone";
                worksheet.Cells[3, 2] = row["sendersPhone"].ToString();
                worksheet.Cells[4, 1] = "Senders address";
                worksheet.Cells[4, 2] = row["sendersAddress"].ToString();
                worksheet.Cells[5, 1] = "Recipients Fullname";
                worksheet.Cells[5, 2] = row["recipientsFullName"].ToString();
                worksheet.Cells[6, 1] = "Recipients phone";
                worksheet.Cells[6, 2] = row["recipientsPhone"].ToString();
                worksheet.Cells[7, 1] = "Recipients address";
                worksheet.Cells[7, 2] = row["recipientsAddress"].ToString();

                worksheet.Cells[8, 1] = "PRODUCTS";
                int i = 9;
                using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection)) 
                {
                    SQLiteCommand cmd = new SQLiteCommand(connection);
                    
                    cmd.CommandText = "select products.Type as productDescribe, products.Weight as productWeight from products where products.Send_Code=@param1;";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@param1", sendCode));
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            worksheet.Cells[i, 1] = reader["productDescribe"].ToString() ;
                            worksheet.Cells[i, 2] = reader["productWeight"].ToString();
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
    }
    
}
