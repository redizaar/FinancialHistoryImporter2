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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Login_Page.xaml
    /// </summary>
    public partial class Login_Page : Page
    {
        MainWindow mainWindow;
        private DispatcherTimer timer1;
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        private static int tik;
        private int failedLogins=0;
        public Login_Page(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            /*
            SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
            mConn.Open();
            string dropTable = "Drop Table [importedBankTransactions]";
            SQLiteCommand command = new SQLiteCommand(dropTable, mConn);
            command.ExecuteNonQuery();
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [UserInfo] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'Username' TEXT, 'Password' TEXT, 'AccountNumber' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string usernameInUseQuery = "select * from [UserInfo] where Username= '" + usernameTextbox.Text.ToString() + "'";
            SQLiteCommand command = new SQLiteCommand(usernameInUseQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count == 1)
            {
                string decryptedPassword = decryptString(dtb.Rows[0][2].ToString());
                if (decryptedPassword == passwordTextbox.Password.ToString())
                {
                    failedLogins = 0;
                    User currentUser = new User();
                    currentUser.setUsername(usernameTextbox.Text.ToString());
                    currentUser.setAccountNumber(dtb.Rows[0][3].ToString());
                    mainWindow.currentUserLabel.Content = currentUser.getUsername(); //notification label
                    mainWindow.setCurrentUser(currentUser);
                    this.Visibility = Visibility.Hidden;
                    ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                    mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                    mainWindow.importMenuTop.Visibility = Visibility.Visible;
                    mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                    mainWindow.bankImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
                }
                else
                {
                    failedLogins++;
                    if (failedLogins >= 3)
                    {
                        timer1 = new DispatcherTimer();
                        tik = 30;
                        timer1.Interval = new TimeSpan(0, 0, 0, 1);
                        timer1.Tick += new EventHandler(timer1_Tick);
                        timer1.Start();
                    }
                    MessageBox.Show("Wrong username or password!");
                }
            }
            else
            {
                failedLogins++;
                if (failedLogins >= 3)
                {
                    timer1 = new DispatcherTimer();
                    tik = 30;
                    timer1.Interval = new TimeSpan(0, 0, 0, 1);
                    timer1.Tick += new EventHandler(timer1_Tick);
                    timer1.Start();
                }
                MessageBox.Show("Wrong username or password!");
            }
            mConn.Close();
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string loginQuery = "Select * From [UserDatas] where username = '" + usernameTextbox.Text.ToString()+"'";
            SqlDataAdapter sda = new SqlDataAdapter(loginQuery,sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if(dtb.Rows.Count==1)
            {
                string decryptedPassword = decryptString(dtb.Rows[0][1].ToString());
                if (decryptedPassword == passwordTextbox.Password.ToString())
                {
                    failedLogins = 0;
                    User currentUser = new User();
                    currentUser.setUsername(usernameTextbox.Text.ToString());
                    currentUser.setAccountNumber(dtb.Rows[0][2].ToString());
                    mainWindow.currentUserLabel.Content = currentUser.getUsername(); //notification label
                    mainWindow.setCurrentUser(currentUser);
                    Visibility = System.Windows.Visibility.Hidden;
                    ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                    mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                    mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                    mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                    mainWindow.bankImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
                }
                else
                {
                    MessageBox.Show("Wrong username or password!");
                    failedLogins++;
                    if (failedLogins > 3)
                    {
                        timer1 = new DispatcherTimer();
                        tik = 30;
                        timer1.Interval = new TimeSpan(0, 0, 0, 1);
                        timer1.Tick += new EventHandler(timer1_Tick);
                        timer1.Start();
                    }
                }
            }
            else
            {
                MessageBox.Show("Wrong username or password!");
                failedLogins++;
                if (failedLogins > 3)
                {
                    timer1 = new DispatcherTimer();
                    tik = 30;
                    timer1.Interval = new TimeSpan(0, 0, 0, 1);
                    timer1.Tick += new EventHandler(timer1_Tick);
                    timer1.Start();
                }
            }
            */
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            loginButton.IsEnabled = false;
            loginButton.Content = tik;
            if (tik > 0)
                tik--;
            else
            {
                loginButton.IsEnabled = true;
                loginButton.Content = "Login";
                failedLogins = 0;
            }
        }
        public string decryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "aXb2uy4z"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = new byte[inputString.Length];
                byteInput = Convert.FromBase64String(inputString);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateDecryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
            }
            Encoding encoding1 = Encoding.UTF8;
            return encoding1.GetString(memStream.ToArray());
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mainWindow.LoginFrame.Content = new Register_Page(mainWindow);
        }
    }
}
