using System;
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
using System.Windows.Shapes;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using System.Data.Common;

namespace TS
{
    /// <summary>
    /// Interaction logic for Payment.xaml
    /// </summary>
    public partial class Payment : Window
    {
        #region Declaration
        SQLiteConnection? m_dbConnection;
        int Tcost;
        string sendcode;
        #endregion
        public Payment(string SendCode, string totalCost)
        {
            InitializeComponent();
            try
            {
                // DescribeTxb.te
                m_dbConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["connection"].ConnectionString);
                m_dbConnection.Open();
                using(var connection = new SQLiteConnection(m_dbConnection))
                {
                    var cmd = new SQLiteCommand(connection);
                    cmd.CommandText = "select (Select firstname || ' ' || lastname  from clients where id = products.sender_id) as fullname from products where send_code = @p";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@p", SendCode));
                    string? sendersName = Convert.ToString(cmd.ExecuteScalar());
                    cmd.Parameters.Clear();
                    cmd.CommandText = "select (Select firstname || ' ' || lastname  from clients where id = products.recipient_id) as fullname from products where send_code = @p";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SQLiteParameter("@p", SendCode));
                    string? recipientsName = Convert.ToString(cmd.ExecuteScalar());
                    DescribeTxb.Text = $"This payment is for transaction with  {SendCode} sendcode\n from {sendersName} to {recipientsName}. The total price is {totalCost}€";
                    Tcost = Convert.ToInt32(totalCost);
                    sendcode = SendCode;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void paidInGermany_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(paidInGermany.Text))
                    paidInMarocco.Text = Convert.ToString(Convert.ToInt32(Tcost) - Convert.ToInt32(paidInGermany.Text));
                else
                    paidInMarocco.Text = string.Empty;
            }
            catch( Exception ex )
            {
                MessageBox.Show(ex.Message, "error");
            }
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (paidInGermany.Text.Trim() != string.Empty && paidInMarocco.Text.Trim() != string.Empty)
                {
                    using (var connection = new SQLiteConnection(m_dbConnection))
                    {
                        var cmd = new SQLiteCommand(connection);
                        cmd.CommandText = "INSERT INTO PAYMENTS(PAIDINMAROCCO, PAIDINGERMANY, SENDCODE) VALUES(@P1,@P2,@P3);";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SQLiteParameter("@P1", paidInMarocco.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@P2", paidInGermany.Text.Trim()));
                        cmd.Parameters.Add(new SQLiteParameter("@P3", sendcode));
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Payment is successful. Type Ok to continue!!");
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("fill the fields");
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show(ex.Message, "error");
            }
        }
    }
}
