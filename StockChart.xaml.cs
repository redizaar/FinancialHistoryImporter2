using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for StockChart.xaml
    /// </summary>
    public partial class StockChart : Page,INotifyPropertyChanged
    {
        private ButtonCommands btnCommand;
        public ChartValues<double> ValuesA { get; set; }
        public List<string> Labels { get; set; }
        private SeriesCollection _Series;
        private MainWindow mainWindow;
        public SeriesCollection Series
        {
            get
            {
                return _Series;
            }
            set
            {
                _Series = value;
                OnPropertyChanged("Series");
            }
        }
        public List<string> months { get; set; }
        public WebStockData webStockData;
        //public ChartValues<double> ValuesB { get; set; }
        //public ChartValues<double> ValuesC { get; set; }
        public StockChart(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            DataContext = this;
            months = new List<string>();
            webStockData = new WebStockData();
            addValuesToDateVariables();
        }
        //not in use
        public void getNewStockData()
        {
            WebStockData refreshStockData = new WebStockData();
            //refreshStockData.GetDataFromWeb();
            //refreshStockData.writeStocksToSQL();
        }
        //not in use
        public void refreshChartAttributes()
        {
            ValuesA = new ChartValues<double>();
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StockData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string stockName = "Select * From [Stock_WebData] where Name = 'AAPL'";
            List<double> sharePrices = new List<double>();
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand(stockName,sqlConn);
            {
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                {
                    while(sqlDataReader.Read())
                    {
                        //sharePrices.Add(sqlDataReader.GetDouble(1));
                        sharePrices.Add((double)Math.Round((Decimal)sqlDataReader.GetDouble(1), 3, MidpointRounding.AwayFromZero));
                    }
                }
            }
            //https://stackoverflow.com/questions/33881503/convert-strings-in-datarow-to-double
            for (int i = 0; i < sharePrices.Count; i++)
            {
                ValuesA.Add(sharePrices[i]);
            }
            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Apple",
                    Values = ValuesA,
                }
            };
            Labels = new List<string>() { "2017.02.02", "2017.02.02", "2017.03.30" };
        }
        public void refreshCSVChartAttribues()
        {
            ValuesA = new ChartValues<double>();
            int i = webStockData.getPrices().Count-1;
            while(i>0)
            {
                ValuesA.Add(webStockData.getPrices()[i]);
                i--;
            }
            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = tickerTextBox.Text.ToString(),
                    Values = ValuesA,
                }
            };
            Labels = new List<string>();
            int j = webStockData.getDates().Count - 1;

            while(j>0)
            {
                Labels.Add(webStockData.getDates()[j]);
                j--;
            }
        }
        private void addValuesToDateVariables()
        {
            int currentYear = DateTime.Now.Year;
            for(int i=currentYear;i!=currentYear-5;i--)
            {
                yearComboBox.Items.Add(i);
            }
            months.Add("Jan");
            months.Add("Feb");
            months.Add("Mar");
            months.Add("Apr");
            months.Add("May");
            months.Add("Jun");
            months.Add("Jul");
            months.Add("Aug");
            months.Add("Sep");
            months.Add("Oct");
            months.Add("Nov");
            months.Add("Dec");
        }
        public ButtonCommands getStockData
        {
            get
            {
                btnCommand = new ButtonCommands(this,"DownloadData",mainWindow);
                return btnCommand;
            }
        }
        public ButtonCommands switchPage
        {
            get
            {
                btnCommand = new ButtonCommands(this, "SQLData",mainWindow);
                return btnCommand;
            }
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private void monthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dayComboBox.Items.Clear();
            if (monthComboBox.SelectedIndex >= 0)
            {
                if (int.Parse(yearComboBox.SelectedItem.ToString()) == DateTime.Now.Year)
                {
                    if (monthComboBox.SelectedIndex == (DateTime.Now.Month) - 1)
                    {
                        int passedDaysThisMonth = DateTime.Now.Day;
                        for (int i = 1; i < passedDaysThisMonth + 1; i++)
                        {
                            dayComboBox.Items.Add(i);
                        }
                    }
                    else
                    {
                        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, (monthComboBox.SelectedIndex) + 1);
                        for (int i = 1; i < daysInMonth + 1; i++)
                        {
                            dayComboBox.Items.Add(i);
                        }
                    }
                }
                else
                {
                    int year = int.Parse(yearComboBox.SelectedItem.ToString());
                    int month = (monthComboBox.SelectedIndex) + 1;
                    int days = DateTime.DaysInMonth(year, month);
                    for (int i = 1; i != days + 1; i++)
                    {
                        dayComboBox.Items.Add(i);
                    }
                }
            }
        }
        private void yearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            monthComboBox.Items.Clear();
            if (int.Parse(yearComboBox.SelectedItem.ToString()) == DateTime.Now.Year)
            {
                int currentMonth = DateTime.Now.Month;
                for (int i = 0; i < currentMonth; i++)
                {
                    monthComboBox.Items.Add(months[i]);
                }
            }
            else
            {
                for (int i = 0; i != months.Count; i++)
                {
                    monthComboBox.Items.Add(months[i]);
                }
            }
        }
        public class ButtonCommands : ICommand
        {
            private StockChart stockChart;
            private DispatcherTimer timer1;
            private static int tik;
            private string action;
            private MainWindow mainWindow;
            public ButtonCommands(StockChart stockChart,string _action,MainWindow mainWindow)
            {
                action = _action;
                this.mainWindow = mainWindow;
                this.stockChart = stockChart;
                this.stockChart.PropertyChanged += new PropertyChangedEventHandler(test_PropertyChanged);
                timer1 = new DispatcherTimer();
                tik = 20;
            }
            private void test_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                //todo
                return true;
            }

            public void Execute(object parameter)
            {
                if (action == "DownloadData")
                {
                    if (tik == 20)
                    {
                        string ticker = stockChart.tickerTextBox.Text.ToString();
                        string year = stockChart.yearComboBox.SelectedItem.ToString();
                        string month = stockChart.monthComboBox.SelectedItem.ToString();
                        string day = stockChart.dayComboBox.SelectedItem.ToString();
                        if (int.Parse(day) < 10)
                            stockChart.webStockData.getCSVDataFromGoogle(ticker, "0" + day, month, year);
                        else
                            stockChart.webStockData.getCSVDataFromGoogle(ticker, day, month, year);
                        stockChart.refreshCSVChartAttribues();
                        timer1.Interval = new TimeSpan(0, 0, 0, 1);
                        timer1.Tick += new EventHandler(timer1_Tick);
                        timer1.Start();
                        stockChart.downloadButton.IsEnabled = false;
                    }
                }
                else if(action=="SQLData")
                {
                    mainWindow.MainFrame.Content = new StockDataGrid(mainWindow);
                }
            }
            void timer1_Tick(object sender, EventArgs e)
            {
                stockChart.downloadButton.Content = tik + " Secs Remaining";
                if (tik > 0)
                    tik--;
                else
                {
                    stockChart.downloadButton.IsEnabled = true;
                    stockChart.downloadButton.Content = "Download Data";
                }
            }
        }
    }
}
