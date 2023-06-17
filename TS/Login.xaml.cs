using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;

namespace TS
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        SQLiteConnection? m_dbConnection;
        public Window1()
        {
            InitializeComponent();
            try
            {
                m_dbConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["connection"].ConnectionString);
                m_dbConnection.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void UserClickedOkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if(UsernameTextBox.Text != string.Empty && PasswordtextBox.Password != string.Empty)
                {
                    if (Logining())
                    {
                        MainWindow welcome = new MainWindow();
                        welcome.Show();
                        this.Hide();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect Username or Password");
                    }

                }
                else
                {
                    MessageBox.Show("Type Username and password!!", "Info");
                }
                
            }
            catch (Exception ex)  
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
        private bool Logining()
        {
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(m_dbConnection))
                {
                    SQLiteCommand command = new SQLiteCommand(dbConnection);
                    command.CommandText = "Select count(id) from Users where username = @param1 and password = @param2;";
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@param1", UsernameTextBox.Text.Trim()));
                    command.Parameters.Add(new SQLiteParameter("@param2", PasswordtextBox.Password.Trim()));
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
                return false;
            }
        }
        private void UserClickedCancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
