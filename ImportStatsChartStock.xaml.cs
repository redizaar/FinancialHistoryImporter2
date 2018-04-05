using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for ImportStatsChartStock.xaml
    /// </summary>
    public partial class ImportStatsChartStock : Page, INotifyPropertyChanged
    {
        public string _selectedStockName;
        public string selectedStockName
        {
            get
            {
                return _selectedStockName;
            }
            set
            {
                if (_selectedStockName != value)
                {
                    if (value == "All")
                        displayAllData();
                    else
                        displayData(value);
                    _selectedStockName = value;
                    OnPropertyChanged("selectedStockName");
                }
            }
        }

        public void displayAllData()
        {
            SeriesCollection = new SeriesCollection();
            Labels = new string[SavedTransactions.getSavedTransactionsStock().Count];
            LineSeries lineSeries = new LineSeries();
            lineSeries.Title = "All";
            ChartValues<int> stats = new ChartValues<int>();
            List<DateTime> distinctDays = new List<DateTime>();
            List<string> distinctDaysString = new List<string>();
            int counter = 0;
            foreach (var transactions in SavedTransactions.getSavedTransactionsStock())
            {
                DateTime dt = DateTime.ParseExact(transactions.getWriteDate(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (!distinctDays.Contains(dt))
                    distinctDays.Add(dt);
            }
            distinctDays.Sort();

            Dictionary<string, int> importedTransactionsToDays = new Dictionary<string, int>();
            foreach (var x in distinctDays)
            {
                counter = 0;
                string[] splittedDate = x.ToString().Split(' ');
                string date = splittedDate[0] + splittedDate[1] + splittedDate[2];
                date = date.Remove(date.Length - 1);
                date = date.Replace('.', '-');
                foreach (var y in SavedTransactions.getSavedTransactionsStock())
                {
                    if (date == y.getWriteDate())
                        counter++;
                }
                importedTransactionsToDays.Add(date, counter);
            }
            int i = 0;
            foreach (var x in importedTransactionsToDays)
            {
                stats.Add(x.Value);
                Labels[i] = x.Key.ToString();
                i++;
            }
            lineSeries.Values = stats;
            SeriesCollection.Add(lineSeries);
        }

        public List<string> _stocks;
        public List<string> stocks
        {
            get
            {
                return _stocks;
            }
            set
            {
                if (_stocks != value)
                {
                    _stocks = value;
                    OnPropertyChanged("stocks");
                }
            }
        }
        public ImportStatsChartStock(List<string> _value)
        {
            DataContext = this;
            InitializeComponent();
            _value.Add("All");
            stocks = _value;
        }
        public void displayData(string stockName)
        {
            SeriesCollection = new SeriesCollection();
            Labels = new string[SavedTransactions.getSavedTransactionsStock().Count];
            LineSeries lineSeries = new LineSeries();
            lineSeries.Title = stockName;
            ChartValues<int> stats = new ChartValues<int>();
            List<DateTime> distinctDays = new List<DateTime>();
            List<string> distinctDaysString = new List<string>();
            int counter = 0;
            foreach (var transactions in SavedTransactions.getSavedTransactionsStock())
            {
                if (transactions.getStockName() == stockName)
                {
                    DateTime dt = DateTime.ParseExact(transactions.getWriteDate(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (!distinctDays.Contains(dt))
                        distinctDays.Add(dt);
                }
            }
            distinctDays.Sort();
            distinctDays.Reverse();

            Dictionary<string, int> importedTransactionsToDays = new Dictionary<string, int>();
            foreach (var x in distinctDays)
            {
                counter = 0;
                string[] splittedDate = x.ToString().Split(' ');
                string date = splittedDate[0] + splittedDate[1] + splittedDate[2];
                date = date.Remove(date.Length - 1);
                date = date.Replace('.', '-');
                foreach (var y in SavedTransactions.getSavedTransactionsStock())
                {
                    if (date == y.getWriteDate() && stockName==y.getStockName())
                        counter++;
                }
                importedTransactionsToDays.Add(date, counter);
            }
            int i = 0;
            foreach (var x in importedTransactionsToDays)
            {
                stats.Add(x.Value);
                Labels[i] = x.Key.ToString();
                i++;
            }
            lineSeries.Values = stats;
            SeriesCollection.Add(lineSeries);
            /*
            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);
            */
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public SeriesCollection _SeriesCollection;
        public SeriesCollection SeriesCollection
        {
            get
            {
                return _SeriesCollection;
            }
            set
            {
                if (_SeriesCollection != value)
                {
                    _SeriesCollection = value;
                    OnPropertyChanged("SeriesCollection");
                }
            }
        }
        public string[] _Labels;
        public string[] Labels
        {
            get
            {
                return _Labels;
            }
            set
            {
                if (_Labels != value)
                {
                    _Labels = value;
                    OnPropertyChanged("Labels");
                }
            }
        }
    }
}
