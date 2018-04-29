using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for StockDataGrid.xaml
    /// </summary>
    public partial class StockDataGrid : Page
    {
        private MainWindow mainWindow;
        public List<string> datesFromSql { get; set; }
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        public StockDataGrid(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            DataContext = this;
            addItemsToSymbolCB();
        }
        public void addItemsToSymbolCB()
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StockData] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT,'Name' TEXT, 'Date' TEXT, 'openPrice' TEXT, " +
                        "'highPrice' TEXT, 'lowPrice' TEXT, 'closePrice' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string storedQuery = "select distinct Name from [StockData]";
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if(dtb.Rows.Count>0)
            {
                foreach (DataRow row in dtb.Rows)
                {
                    string nameFromSql = row["Name"].ToString();
                    symbolComboBox.Items.Add(nameFromSql);
                }
            }
            mConn.Close();
            /*
            string distinctNameQuery = "Select distinct Name From [Stock_WebData]";
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StockData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(distinctNameQuery, sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count > 0)
            {
                foreach (DataRow row in dtb.Rows)
                {
                    string nameFromSql = row["Name"].ToString();
                    symbolComboBox.Items.Add(nameFromSql);
                }
            }
            */
        }

        private void symbolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string symbol = symbolComboBox.SelectedItem.ToString();
            mConn.Open();
            string storedQuery = "select *  from [StockData] where Name='"+symbol+"'";
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count > 0)
            {
                ThreadStart threadStart = delegate
                {
                    sortDatesInOrder(dtb, symbol);
                };
                Thread sqlThread = new Thread(threadStart);
                sqlThread.IsBackground = true;
                sqlThread.Start();
                sqlThread.Join();
            }
            mConn.Close();
            /*
            string selectedItemQuery = "Select * From [Stock_WebData] Where Name='"+symbol+"'";
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StockData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(selectedItemQuery, sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count > 0)
            {
                ThreadStart threadStart = delegate
                {
                    sortDatesInOrder(dtb, symbol);
                };
                Thread sqlThread = new Thread(threadStart);
                sqlThread.IsBackground = true;
                sqlThread.Start();
                sqlThread.Join();
            }
            */
        }
        private void sortDatesInOrder(DataTable dtb,string symbol)
        {
            List<DateTime> dates = new List<DateTime>();
            foreach (DataRow row in dtb.Rows)
            {
                string dateFromSql = row["Date"].ToString();
                DateTime dt=new DateTime();
                try
                {
                    dt = DateTime.ParseExact(dateFromSql, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {

                }
                dates.Add(dt);
            }
            dates.Sort();
            dates.Reverse();
            List<Stock> tableAttributes = new List<Stock>();
            DateTime latestDate;
            var rows=dtb.Select();
            while (dates.Count != 0)
            {
                for (int i = dtb.Rows.Count - 1; i >= 0; i--)
                {
                    if (dates.Count != 0)
                    {
                        latestDate = dates[0];
                        string dateFromSql = dtb.Rows[i]["Date"].ToString();
                        DateTime dt=new DateTime();
                        try
                        {
                            dt = DateTime.ParseExact(dateFromSql, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {
                           
                        }
                        if (dt == latestDate)
                        {
                            dates.Remove(latestDate);
                            string openPrice = dtb.Rows[i]["openPrice"].ToString();
                            string HighPrice = dtb.Rows[i]["highPrice"].ToString();
                            string LowPrice = dtb.Rows[i]["lowPrice"].ToString();
                            string closePrice = dtb.Rows[i]["closePrice"].ToString();
                            Stock stock = new Stock(symbol, dt.ToString().Substring(0, 12), openPrice, HighPrice, LowPrice, closePrice);
                            tableAttributes.Add(stock);
                            dtb.Rows[i].Delete();
                            break;
                        }
                    }
                }
                dtb.AcceptChanges();
            }
            addAtributesToTable(tableAttributes);
        }
        private void addAtributesToTable(List<Stock> tableAttributes)
        {
            //because sortDates in order runs from a different thread
            //a different thread owns it
            Dispatcher.BeginInvoke(new Action(() =>
            {
                storedStockDataGrid.Items.Clear();
                foreach (var attribute in tableAttributes)
                    storedStockDataGrid.Items.Add(attribute);
            }));
        }
    }
}
