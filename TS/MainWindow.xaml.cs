﻿using System;
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
        List<Client> clientsList = new List<Client>();

        int SenderId;
        int RecipientsId;
        string Role;

        #endregion
        public MainWindow( string role)
        {
            InitializeComponent();
            try 
            {
                                                                                                                                                                                                                                                                                                                                                                         
                m_dbConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["connection"].ConnectionString);
                m_dbConnection.Open();
                Time();
                Role = role;
                productsAddDate.SelectedDate = DateTime.Now;
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

        
        

        private void TransactionLoaded(object sender, RoutedEventArgs e)
        {
            TransactionList();
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
    (Select clients.FirstName || ' ' || clients.LastName from clients where id = products.Sender_Id) as sendersFullName, 
    (Select clients.FirstName || ' ' || clients.LastName from clients where id = products.Recipient_Id) as recipientsFullName,
    (Select (Select Addresses.City || ',' || Addresses.Country from addresses where id = clients.Address_Id ) from clients where id = products.Sender_Id) as sendersAddress,
    (Select (Select Addresses.City || ',' || Addresses.Country from addresses where id = clients.Address_Id ) from clients where id = products.Recipient_Id) as recipientsAddress,
(Select clients.Phone from clients where id = products.Sender_Id) as sendersPhone,
    (Select clients.Phone from clients where id = products.Recipient_Id) as recipientsPhone,
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
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
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
                        fillTariffs();
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

                    }
                }
            }
            catch( Exception ex )
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
                    cmd.CommandText = "Select id as Id, firstname || ' ' || lastname as UserFullname, username as username, role as Role from Users;"; 
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
            if(UserPasswordTextBox.Password.Trim()!=string.Empty && UserLastnameTextBox.Text.Trim()!=string.Empty && UserFirstnameTextBox.Text.Trim()!=string.Empty && UserUsernameTextBox.Text.Trim()!=string.Empty && UserRoleCmb.SelectedIndex>-1)
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                    {
                        SQLiteCommand cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "insert into users(firstname, lastname, username, password, role) values(@param1, @param2, @param3, @param4, @param5);" ;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@param1", UserFirstnameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param2", UserLastnameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param3", UserUsernameTextBox.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param4", UserPasswordTextBox.Password.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@param5", (UserRoleCmb.Text.ToLower())));
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

        
        private void ProductsAddTabLoaded(object sender, RoutedEventArgs e)
        {
            fillClients();
            fillTariffs();
        }
        private void fillTariffs()
        {
            try
            {
                if(tarifClassList.Count > 0)
                {
                    tarifClassList.Clear();
                }
                using(var connection =  new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "Select id as Id, Name as Name, cost as Cost from tarifs;";
                    cmd.CommandType = CommandType.Text;
                    var reader = cmd.ExecuteReader();

                    if( reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            tarifClassList.Add(new TarifClass
                            {
                                Cost = Convert.ToDouble(reader["Cost"]),
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(), 
                            });
                        }
                        tarifCmb.ItemsSource = tarifClassList;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private void fillClients()
        {
            try
            {
                using(var connection =  new SQLiteConnection(m_dbConnection))
                {
                    if(clientsList.Count > 0) { clientsList.Clear(); }
                    var cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "Select Id as Id, Firstname || ' ' || Lastname as Fullname, Phone as Phone, Address_Id as AddressId from clients;";
                    cmd.CommandType = CommandType.Text;
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    if( reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            clientsList.Add(new Client
                            {
                                AddressId = Convert.ToInt32(reader["AddressId"]),
                                Fullname = reader["Fullname"].ToString(),
                                Id = Convert.ToInt32(reader["Id"]),
                                PhoneNumber = reader["Phone"].ToString()
                            });
                        }
                        Recipientscmb.ItemsSource = clientsList;
                        Recipientscmb.Items.Refresh();
                        Senderscmb.ItemsSource = clientsList;
                        Senderscmb.Items.Refresh();
                    }    
                    

                    //autocomplete.SelectedItem
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void ClientsTabLoaded(object sender, RoutedEventArgs e)
        {
            ClientList();
            FillAddressCmb("MR");

        }

        private void DeleteClientClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if((MessageBox.Show("Are you sure!!","Confirming!", MessageBoxButton.YesNo)==MessageBoxResult.Yes))
                {
                    DataRowView row = dgClientsDataGrid.SelectedItems[0] as DataRowView;
                    int id = Convert.ToInt32(row["Id"]);
                    using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                    {
                        SQLiteCommand cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "Delete from Clients where id = @param1;";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@param1", id));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Deleted!");
                        ClientList();
                        fillClients();

                    }
                }
                
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void AddClientButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ClientFirstName.Text != string.Empty && ClientLastName.Text != string.Empty && ClientsAdressCmb.SelectedIndex > -1 && ClientsPhone.Text != string.Empty)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                    {
                        SQLiteCommand cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "INSERT INTO CLIENTS(FIRStNAME, LASTNAME, PHONE, ADDRESS_ID, ADDDATE) VALUES(@PARAM1,@PARAM2, @PARAM3,@PARAM4,@PARAM5)";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM1", ClientFirstName.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM2", ClientLastName.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM3", ClientsPhone.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM4", ClientsAdressCmb.SelectedValue));
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM5", System.DateTime.Now.ToShortDateString()));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Added!!");
                        ClientList();
                        fillClients();
                        ClientFirstName.Text = string.Empty;
                        ClientLastName.Text = string.Empty;
                        ClientsPhone.Text = string.Empty;
                        ClientsAdressCmb.SelectedIndex = -1;
                        
                    }
                }
                else
                    MessageBox.Show("All fields must be filled!!!", "FILL");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private void FillAddressCmb(string country)
        {
            try
            {
                MRCityNames.Clear();
                DECityNames.Clear();
                using(SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "Select id as Id, City as City from Addresses where country=@param1;";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@param1", country));
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            if(country=="DE")
                            {
                                DECityNames.Add(new CitiesClass { CityName = reader["City"].ToString(), Id = Convert.ToInt16(reader["Id"]) });
                            }
                            else
                            {

                                MRCityNames.Add(new CitiesClass { CityName = reader["City"].ToString(), Id = Convert.ToInt16(reader["Id"]) });
                            }
                        }
                        if (country == "DE")
                            ClientsAdressCmb.ItemsSource = DECityNames;
                        else
                            ClientsAdressCmb.ItemsSource = MRCityNames;
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private void ClientList()
        {
            try
            {
                using(SQLiteConnection connection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(connection);
                    command.CommandText = "Select id as Id, Firstname || ' '|| Lastname as Fullname, (Select City from Addresses where id = clients.Address_Id) as Address, Phone as Phone  from clients;";
                    command.CommandType = CommandType.Text;

                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(dataSet, "ClientsList");
                    dgClientsDataGrid.DataContext = dataSet;
                    dgClientsDataGrid.Items.Refresh();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }


        private void CheckboxClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if(addressChckB.IsChecked==true)
                {
                    FillAddressCmb("DE");
                }
                else
                {
                    FillAddressCmb("MR");
                }
            }
            catch(Exception  ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        
     

        

        private void SearchRecipienttxbEvent(object sender, TextChangedEventArgs e)
        {
            try
            {
                string typedText = SearchRecipienttxb.Text.Trim().ToLower();
                if(string.IsNullOrEmpty(typedText))
                {
                    Recipientscmb.ItemsSource = clientsList;
                }
                else
                {
                    List<Client> tempRlist = new List<Client>();
                    for(int i = 0; i < clientsList.Count; i++)
                    {
                        if (clientsList[i].Fullname.ToLower().StartsWith(typedText))
                            tempRlist.Add(clientsList[i]);
                        else if(clientsList[i].PhoneNumber.ToLower().StartsWith(typedText))
                            tempRlist.Add(clientsList[i]);
                    }
                    if (tempRlist.Count > 0)
                        Recipientscmb.ItemsSource = tempRlist;
                    else
                        Recipientscmb.ItemsSource = clientsList;
                    
                }
                Recipientscmb.Items.Refresh();
                Recipientscmb.IsDropDownOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        

        private void SearchSendertxbEvent(object sender, TextChangedEventArgs e)
        {
            try
            {
                string typedText = SearchSendertxb.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(typedText))
                {
                    Senderscmb.ItemsSource = clientsList;
                }
                else
                {
                    List<Client> tempRlist = new List<Client>();
                    for (int i = 0; i < clientsList.Count; i++)
                    {
                        if (clientsList[i].Fullname.ToLower().StartsWith(typedText))
                            tempRlist.Add(clientsList[i]);
                        else if (clientsList[i].PhoneNumber.ToLower().StartsWith(typedText))
                            tempRlist.Add(clientsList[i]);
                    }
                    if (tempRlist.Count > 0)
                        Senderscmb.ItemsSource = tempRlist;
                    else
                        Senderscmb.ItemsSource = clientsList;

                }
                Senderscmb.Items.Refresh();
                Senderscmb.IsDropDownOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void SenderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SenderId = Convert.ToInt32(Senderscmb.SelectedValue);
            //MessageBox.Show(SenderId.ToString());
            if (Recipientscmb.SelectedIndex > -1 && productsAddDate.Text != string.Empty)
            {
                string sendcode = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
                ProductsListDg(sendcode);
                CountDeliverCost(sendcode);
            }
        }

        private void RecipientsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecipientsId = Convert.ToInt32(Recipientscmb.SelectedValue);
            //MessageBox.Show(RecipientsId.ToString());
            if(Senderscmb.SelectedIndex>-1 && productsAddDate.Text!=string.Empty)
            {
                string sendcode = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
                ProductsListDg(sendcode);
                CountDeliverCost(sendcode);
                if (IsPaid(sendcode))
                {
                    paidSymbol.Kind = MaterialDesignThemes.Wpf.PackIconKind.CashCheck;
                    paidSymbol.Visibility = Visibility.Visible;
                }
                else
                {
                    paidSymbol.Kind = MaterialDesignThemes.Wpf.PackIconKind.CashRemove;
                    paidSymbol.Visibility = Visibility.Visible;
                }
                    
            }
        }

        private void AddProductButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (productsAddDate.Text.Trim() != string.Empty && ProductsType.Text.Trim() != string.Empty && ProductsWeight.Text.Trim() != string.Empty && Senderscmb.SelectedIndex>-1 && Recipientscmb.SelectedIndex > -1 )
                {
                    using (var connection = new SQLiteConnection(m_dbConnection))
                    {
                        string sendcode = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
                        var cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "INSERT INTO PRODUCTS(WEIGHT, TYPE, SENDER_ID, RECIPIENT_ID, SEND_CODE, ISSENDED, DATE) VALUES(@P1,@P2,@P3,@P4,@P5,@P6,@P7)";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@P1", ProductsWeight.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@P2", ProductsType.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@P3", SenderId));
                        cmd.Parameters.Add(new SQLiteParameter("@P4", RecipientsId));
                        cmd.Parameters.Add(new SQLiteParameter("@P5", sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month)));
                        cmd.Parameters.Add(new SQLiteParameter("@P6","0"));
                        cmd.Parameters.Add(new SQLiteParameter("@P7", productsAddDate.Text.Trim()));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Added!!");
                        ProductsListDg(sendcode);
                        CountDeliverCost(sendcode);

                    }
                }
                else
                    MessageBox.Show("Fill all fields!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private string sendCodeGenerator(int sId, int rId, int month)
        {
            try
            {
                using(var connection =  new SQLiteConnection(m_dbConnection))
                {
                    var cmd = new SQLiteCommand(connection);
                    cmd.CommandText = $"select address_id from clients where id = {rId}";
                    cmd.CommandType = CommandType.Text;
                    int Raddressid = Convert.ToInt32(cmd.ExecuteScalar());
                    return ($"{sId.ToString("D4")}{Raddressid.ToString("D3")}{month.ToString("D2")}");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
                return string.Empty;
            }
        }

        private void SendCodeFlippedChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if(Senderscmb.SelectedIndex>-1&& Recipientscmb.SelectedIndex>-1 && productsAddDate.Text!=string.Empty)
            {
                SendCodeFlipperButton.Content = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
            }
            else
            {
                SendCodeFlipperButton.Content = "Please select sender, recipient and date\n to generate sendcode.";
            }
        }

        private void ProductDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var row = dgProducts.SelectedItems[0] as DataRowView;
                int id = Convert.ToInt32(row["id"]);
                if(MessageBox.Show("Are you sure?!", "Confirming", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using(var connection = new SQLiteConnection(m_dbConnection))
                    {
                        var cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "delete from products where id = @p;";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@p", id));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Deleted!!");
                        ProductsListDg(row["SendCode"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void ProductEditClick(object sender, RoutedEventArgs e)
        {

        }
        private void ProductsListDg(string SendCode)
        {
            try
            {
                using (var connection = new SQLiteConnection(m_dbConnection))
                {
                    var command = new SQLiteCommand(connection);
                    command.CommandText = $"Select id as Id, weight as Weight , type as Type, send_code as SendCode, date as Date from products where  send_code = @param1;";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@param1", SendCode));
                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(dataSet, "ProductsDgList");
                    dgProducts.DataContext = dataSet;
                    dgProducts.Items.Refresh();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void PayProductsButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Senderscmb.SelectedIndex > -1 && Recipientscmb.SelectedIndex > -1 && productsAddDate.Text!=string.Empty)
                {
                    string sendcode = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
                    var payment = new Payment(sendcode, CountDeliverCost(sendcode));
                    payment.Show();
                    
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        

        private void TarifselectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(MessageBox.Show("Are you want you edit the tarif for this clients", "Confrming", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using(var connection = new SQLiteConnection(m_dbConnection))
                    {
                        string sendcode = sendCodeGenerator(SenderId, RecipientsId, Convert.ToDateTime(productsAddDate.Text).Month);
                        var cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "UPDATE PRODUcTS SET TARIF_ID = @PARAM1 WHERE Send_Code = @PARAM2;";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM1", tarifCmb.SelectedValue.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@PARAM2", sendcode));
                        cmd.ExecuteNonQuery();
                        CountDeliverCost(sendcode);
                        MessageBox.Show("Changed!!");
                    }
                }
                catch(Exception ex )
                {
                    MessageBox.Show(ex.Message, "error"); 
                }
            }
        }
        private string CountDeliverCost(string sendcode)
        {
            try
            {
                using(var connection = new SQLiteConnection(m_dbConnection))
                {
                    var cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "select (sum(products.Weight)*(select tarifs.Cost from tarifs where id = products.tarif_id)) as totalCost from products where send_code = @param1;";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@param1", sendcode));
                    string? totalCost = (string.IsNullOrEmpty( cmd.ExecuteScalar().ToString()))? "0" : cmd.ExecuteScalar().ToString();
                    totalCostTxb.Text = "Total cost:" + totalCost + "€";
                    return totalCost;
                }
            }
            catch(Exception ex)
            {

                MessageBox.Show(ex.Message, "error");
                return "0";
            }
        }
        private bool IsPaid(string sendcode)
        {
            try
            {
                using(var connection = new SQLiteConnection(m_dbConnection)) 
                { 
                    var cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "Select count(id) from payments where sendcode = @p;";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@p", sendcode));
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
                return false;
                
            }
        }

        private void SettingsTabLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void ValueChangedSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TextSizeSlider.Value = (double)(int)e.NewValue;
        }
    }
    
}
