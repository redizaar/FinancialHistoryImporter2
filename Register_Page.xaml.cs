using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Register_Page.xaml
    /// </summary>
    public partial class Register_Page : Page
    {
        private MainWindow mainWindow;
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        public Register_Page(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            #region SQLite
            if (checkPassword() && checkUsername())
            {
                try
                {
                    SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
                    mConn.Open();
                    string insertQuery = "insert into [UserInfo](Username, Password, AccountNumber) " +
                        "values('" + registerUsernameTextbox.Text.ToString() + "','" + encryptString(RegisterPasswordTextbox.Password.ToString()) + "','" + "-" + "')";
                    SQLiteCommand insercommand = new SQLiteCommand(insertQuery, mConn);
                    insercommand.CommandType = CommandType.Text;
                    try
                    {
                        insercommand.ExecuteNonQuery();
                        MessageBox.Show("You can log in now!", "Successfull registartion!");
                        mainWindow.LoginFrame.Content = new Login_Page(mainWindow);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageBox.Show(MainWindow.dbPath);
                }
            }
            #endregion
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("registrationQuery4", sqlConn);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            if (checkUsername() && checkPassword())
            {
                sqlCommand.Parameters.AddWithValue("@username", registerUsernameTextbox.Text.ToString());
                sqlCommand.Parameters.AddWithValue("@password", encryptString(RegisterPasswordTextbox.Password.ToString()));
                sqlCommand.Parameters.AddWithValue("@accountNumber", "-");
                sqlCommand.ExecuteNonQuery();
                if (MessageBox.Show("You can log in now!", "Successfull registartion!",
                         MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    mainWindow.LoginFrame.Content = new Login_Page(mainWindow);
                }
            }
            */
        }
        public string encryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = Encoding.UTF8.GetBytes(inputString);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateEncryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
            }
            return Convert.ToBase64String(memStream.ToArray());
        }
        private bool checkPassword()
        {
            if (RegisterPasswordTextbox.Password.ToString() == RegisterPasswordTextbox2.Password.ToString())
            {
                if (RegisterPasswordTextbox.Password.Length >= 4)
                    return true;
                else
                {
                    MessageBox.Show("Password has to be atleast 4 characters!");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Passwords doesn't match");
                return false;
            }
        }

        private bool checkUsername()
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [UserInfo] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'Username' TEXT, 'Password' TEXT, 'AccountNumber' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string usernameInUseQuery = "select * from [UserInfo] where Username= '"+registerUsernameTextbox.Text.ToString()+"'";
            SQLiteCommand command = new SQLiteCommand(usernameInUseQuery, mConn);
            DataTable DT = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(DT);
            mConn.Close();
            if (DT.Rows.Count == 0)
                return true;
            else
            {
                MessageBox.Show("Username is already in use!");
                return false;
            }
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string loginQuery = "Select * From [UserDatas] where username = '" + registerUsernameTextbox.Text.ToString()+"'";
            SqlDataAdapter sda = new SqlDataAdapter(loginQuery, sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count == 0)
                return true;
            else
            {
                MessageBox.Show("This username is already in use!");
                return false;
            }
            */
        }
    }
}
