using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Login_Page.xaml
    /// </summary>
    public partial class Login_Page : Page
    {
        MainWindow mainWindow;
        private DispatcherTimer timer1;
        private static int tik;
        private int failedLogins=0;
        public Login_Page(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string loginQuery = "Select * From [UserDatas] where username = '" + usernameTextbox.Text.ToString()+"' and password = '"+passwordTextbox.Password.ToString()+"'";
            SqlDataAdapter sda = new SqlDataAdapter(loginQuery,sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if(dtb.Rows.Count==1)
            {
                failedLogins = 0;
                User currentUser = new User();
                currentUser.setUsername(usernameTextbox.Text.ToString());
                currentUser.setAccountNumber(dtb.Rows[0][2].ToString());
                mainWindow.currentUserLabel.Content = currentUser.getUsername(); //notification label
                mainWindow.setCurrentUser(currentUser);
                Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Wrong username or password!");
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
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mainWindow.LoginFrame.Content = new Register_Page(mainWindow);
        }
    }
}
