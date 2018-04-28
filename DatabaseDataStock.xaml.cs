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
    /// Interaction logic for DatabaseDataStock.xaml
    /// </summary>
    public partial class DatabaseDataStock : Page, INotifyPropertyChanged
    {
        private static DatabaseDataStock instance;
        public string _stockChoice;
        public string stockChoice
        {
            get
            {
                return _stockChoice;
            }
            set
            {
                if(_stockChoice!=value)
                {
                    _stockChoice = value;
                    List<Stock> allTransactions = SavedTransactions.getSavedTransactionsStock();
                    List<Stock> reference = new List<Stock>();
                    foreach (var tableAttribute in allTransactions)
                    {
                        if ((tableAttribute.getImporter() == mainWindow.getCurrentUser().getUsername()) && (tableAttribute.getStockName()==stockChoice))
                        {
                            reference.Add(tableAttribute);
                        }
                    }
                    tableAttributes = new List<Stock>(reference);
                    if (tableAttributes != null)
                    {
                        foreach (var transaction in tableAttributes)
                        {
                            if (transaction.getWriteDate() != null && transaction.getWriteDate().Length >= 12)
                            {
                                transaction.setWriteDate(transaction.getWriteDate().Substring(0, 12));
                            }
                            else
                            {
                                transaction.setWriteDate(DateTime.Now.ToString("yyyy/MM/dd"));
                            }
                        }
                    }
                    OnPropertyChanged("stockChoice");
                }
            }
        }
        public List<string> _stockChoices;
        public List<string> stockChoices
        {
            get
            {
                return _stockChoices;
            }
            set
            {
                if(_stockChoices!=value)
                {
                    _stockChoices = value;
                    OnPropertyChanged("stockChoices");
                }
            }
        }
        public List<Stock> _tableAttributes;
        public List<Stock> tableAttributes
        {
            get
            {
                return _tableAttributes;
            }
            set
            {
                _tableAttributes = value;
                OnPropertyChanged("tableAttributes");
            }
        }
        private MainWindow mainWindow;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private DatabaseDataStock(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            DataContext = this;
            InitializeComponent();
        }
        public void setTableAttributes()
        {
            //Binding is the reason for .ToList() , the count changed - it freezes it is not there
            List<Stock> allTransactions = SavedTransactions.getSavedTransactionsStock();
            List<Stock> reference = new List<Stock>();
            stockChoices = new List<string>();
            foreach (var tableAttribute in allTransactions)
            {
                if (tableAttribute.getImporter() == mainWindow.getCurrentUser().getUsername())
                {
                    reference.Add(tableAttribute);
                }
                if (!stockChoices.Contains(tableAttribute.getStockName()))
                    stockChoices.Add(tableAttribute.getStockName());
            }
            tableAttributes = new List<Stock>(reference);
            if (tableAttributes != null)
            {
                foreach (var transaction in tableAttributes)
                {
                    if (transaction.getWriteDate() != null && transaction.getWriteDate().Length >= 12)
                    {
                        transaction.setWriteDate(transaction.getWriteDate().Substring(0, 12));
                    }
                    else
                    {
                        transaction.setWriteDate(DateTime.Now.ToString("yyyy/MM/dd"));
                    }
                    if (transaction.getProfit() != null)
                    {
                        double profit = double.Parse(transaction.getProfit());
                        var f = new NumberFormatInfo { NumberGroupSeparator = "," };
                        var s = profit.ToString("n", f);
                        transaction.setProfit(s);
                    }
                    else
                    {
                        transaction.setProfit("-");
                    }
                }
            }
        }
        public static DatabaseDataStock getInstance(MainWindow mainWindow)
        {
            if(instance==null)
            {
                instance = new DatabaseDataStock(mainWindow);
            }
            return instance;
        }
    }
    public class CellColoringClass : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string profitString = value.ToString();
                if (profitString != "-")
                {
                    decimal profit = 0;
                    var allowedStyles = (NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands);

                    if (Decimal.TryParse(profitString, allowedStyles, CultureInfo.GetCultureInfo("DE-de"), out profit))
                    {
                    }
                    else if (Decimal.TryParse(profitString, allowedStyles, CultureInfo.GetCultureInfo("EN-us"), out profit))
                    {
                    }
                    if (profit > 0)
                        return Brushes.LightGreen;
                    else
                        return Brushes.LightSalmon;
                }
                return DependencyProperty.UnsetValue;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
