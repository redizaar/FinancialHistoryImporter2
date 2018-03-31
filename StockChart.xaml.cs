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
        private SeriesCollection _Series;
        private MainWindow mainWindow;
        public List<string> _Labels;
        public List<string> Labels
        {
            get
            {
                return _Labels;
            }
            set
            {
                _Labels = value;
                OnPropertyChanged("Labels");
            }
        }
        public string _dateChoice;
        public string dateChoice
        {
            get
            {
                return _dateChoice;
            }
            set
            {
                _dateChoice = value;
                OnPropertyChanged("dateChoice");
            }
        }
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
        public List<string> dateChoices { get; set; }
        public WebStockData webStockData;
        //public ChartValues<double> ValuesB { get; set; }
        //public ChartValues<double> ValuesC { get; set; }
        public StockChart(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            DataContext = this;
            dateChoices = new List<string>();
            webStockData = new WebStockData();
            addValuesToDateVariables();
        }
        public void refreshCSVChartAttribues()
        {
            ValuesA = new ChartValues<double>();
            int i = 0;
            while(i<webStockData.getPrices().Count)
            {
                ValuesA.Add(webStockData.getPrices()[i]);
                i++;
            }
            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = tickerTextBox.Text.ToString(),
                    Values = ValuesA,
                }
            };
            List<string> tempLabels = new List<string>();
            int j = 0;

            while(j<webStockData.getDates().Count)
            {
                tempLabels.Add(webStockData.getDates()[j]);
                j++;
            }
            Labels = tempLabels;
        }
        private void addValuesToDateVariables()
        {
            dateChoices.Add("1 month");
            dateChoices.Add("3 month");
            dateChoices.Add("6 month");
            dateChoices.Add("1 year");
        }
        public ButtonCommands getStockData
        {
            get
            {
                btnCommand = new ButtonCommands(this,"DownloadData",mainWindow);
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
                        string date = stockChart._dateChoice;
                            stockChart.webStockData.getCSVDataFromIEX(ticker, date);
                        stockChart.refreshCSVChartAttribues();
                        timer1.Interval = new TimeSpan(0, 0, 0, 1);
                        timer1.Tick += new EventHandler(timer1_Tick);
                        timer1.Start();
                        stockChart.downloadButton.IsEnabled = false;
                    }
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
