﻿using System;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using TS.Classes;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace TS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration
        public static SQLiteConnection? m_dbConnection;
        List<Products> ProductsList = new List<Products>();
        List<CitiesClass> MRCityNames = new List<CitiesClass>();
        List<CitiesClass> DECityNames = new List<CitiesClass>();
        List<TarifClass> tarifClassList = new List<TarifClass>();
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
                SendungsnummerTextBox.Text = System.DateTime.Now.Month.ToString(); 
            }
            catch(Exception ex)
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

                        TransactionList();

                        ProductsList.Add
                        (
                        new Products
                        {
                            Weight = Convert.ToDecimal(ProductWeigthTextBox.Text.Trim()),
                            Describe = ProductDescribeTextBox.Text.Trim(),
                            SenderId = SenderId,
                            RecipientId = SenderId,

                        }
                        ) ;
                        AddProducts(ProductsList[0]);
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
                        TotalCostTextBlock.Text = Convert.ToString(Convert.ToDouble(TotalWeightTextBlock.Text)*Convert.ToDouble(TarifCmb.SelectedValue));

                        /*DataSet dataSet = new DataSet();
                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                        dataAdapter.Fill(dataSet, "Balance");
                        dg_balance.DataContext = dataSet;
                        dg_balance.Items.Refresh();*/

                        SenderFirstNameTextBox.Text = string.Empty;
                        SenderLastNameTextBox.Text = string.Empty;
                        SenderPhoneTextBox.Text = string.Empty;
                        SenderCitiesComboBox.SelectedIndex = -1;

                        RecipientFirstNameTextBox.Text = string.Empty;
                        RecipientLastNameTextBox.Text = string.Empty;
                        RecipientPhoneTextBox.Text = string.Empty;
                        RecipientCitiesComboBox.SelectedIndex = -1;

                        ProductDescribeTextBox.Text = string.Empty;
                        ProductWeigthTextBox .Text = string.Empty;
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

                    command.CommandText = "INSERT INTO Products(Weight,Type, Sender_id, Recipient_id) Values(@param1, @param2, @param3, @param4)";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@param1", products.Weight));
                    command.Parameters.Add(new SQLiteParameter("@param2", products.Describe));
                    command.Parameters.Add(new SQLiteParameter("@param3", products.SenderId));
                    command.Parameters.Add(new SQLiteParameter("@param4", products.SenderId));
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
                                            PRODUCTS.ID AS ID,
                                            (SELECT SENDERS.FirstName || ' ' || SENDERS.LastName FROM SENDERS WHERE SENDERS.id = PRODUCTS.Sender_Id) AS SENDERNAME, 
                                            (SELECT SENDERS.Phone FROM SENDERS WHERE SENDERS.id = PRODUCTS.Sender_Id)AS SENDERSPHONE,
                                            (Select (Select City from Addresses where id = SENDERS.Address_Id )as Address from Senders where id = Products.Sender_Id )AS SENDERSADDRESS,
                                            (SELECT RECIPIENTS.FirstName || ' ' || RECIPIENTS.LastName FROM RECIPIENTS WHERE RECIPIENTS.id = PRODUCTS.Recipient_Id) AS RECIPIENTSNAME,
                                            (SELECT RECIPIENTS.Phone FROM RECIPIENTS WHERE RECIPIENTS.id = PRODUCTS.Recipient_Id)AS RECIPIENTSPHONE,
                                            (Select (Select City from Addresses where id = Recipients.Address_Id )as Address from Recipients where id = Products.Recipient_Id )AS RECIPIENTSADDRESS,
                                            PRODUCTS.Type AS PRODUCTSTYPE, 
                                            PRODUCTS.Weight AS PRODUCTSWEIGHT FROM PRODUCTS";
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
                    int id = Convert.ToInt16(row["Id"]);
                    using (SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                    {

                        SQLiteCommand command = new SQLiteCommand(connection);
                        command.CommandText= "Select count(Sender_id) from products where Sender_id = (Select Sender_id from Products where id = @param1)";
                        command.CommandType= CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@param1", id));
                        int CountOfSenderId = Convert.ToInt16(command.ExecuteScalar());
                        command.Parameters.Clear();
                        if (CountOfSenderId>1)
                        {
                            command.CommandText = "DELETE FROM PRODUCTS WHERE ID =  @param1;";
                            command.CommandType= CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", id));
                            command.ExecuteScalar();
                            command.Parameters.Clear();
                            MessageBox.Show("Deleted!!");
                            TransactionList();
                        }
                        else
                        {
                            command.CommandText = "Select Sender_id from Products where products.id=@param1;";
                            command.CommandType= CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", id));
                            SenderId = Convert.ToInt16(command.ExecuteScalar());
                            command.Parameters.Clear();
                            command.CommandText = "DELETE FROM PRODUCTS WHERE ID =  @param1;";
                            command.CommandType = CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", id));
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "DELETE FROM Senders WHERE ID =  @param1;";
                            command.CommandType = CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", SenderId));
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "DELETE FROM RECIPIENTS WHERE ID =  @param1;";
                            command.CommandType = CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@param1", SenderId));
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            MessageBox.Show("Deleted!!");
                            TransactionList();

                        }
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
                using(SQLiteConnection connection = new SQLiteConnection(MainWindow.m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    for(int i=0; i<ProductsList.Count; i++) 
                    {
                        AddProducts(ProductsList[i]);
                    }
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
    }
    
}