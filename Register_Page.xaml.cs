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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Register_Page.xaml
    /// </summary>
    public partial class Register_Page : Page
    {
        private MainWindow mainWindow;
        public Register_Page(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("registrationQuery3", sqlConn);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            if (checkUsername() && checkPassword())
            {
                sqlCommand.Parameters.AddWithValue("@username", registerUsernameTextbox.Text.ToString());
                sqlCommand.Parameters.AddWithValue("@password", RegisterPasswordTextbox.Password.ToString());
                sqlCommand.Parameters.AddWithValue("@failedlogins", 0);
                sqlCommand.Parameters.AddWithValue("@accountnumber", "-");
                sqlCommand.ExecuteNonQuery();
                if (MessageBox.Show("You can log in now!", "Successfull registartion!",
                         MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    mainWindow.LoginFrame.Content = new Login_Page(mainWindow);
                }
            }

        }

        private bool checkPassword()
        {
            if (RegisterPasswordTextbox.Password.ToString() == RegisterPasswordTextbox2.Password.ToString())
                return true;
            else
            {
                MessageBox.Show("Passwords doesn't match");
                return false;
            }
        }

        private bool checkUsername()
        {
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
        }
    }
}
