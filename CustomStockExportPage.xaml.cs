using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CustomStockExportPage.xaml
    /// </summary>
    public partial class CustomStockExportPage : Page, INotifyPropertyChanged
    {
        private static List<Stock> stockTransactions;
        private MainWindow mainWindow;
        public Dictionary<int, Stock> stocks;
        public List<Stock> _tableAttributes;
        public List<Stock> tableAttributes
        {
            get
            {
                return _tableAttributes;
            }
            set
            {
                if (_tableAttributes != value)
                {
                    _tableAttributes = value;
                    OnPropertyChanged("tableAttributes");
                }
            }
        }
        public List<string> _companies;
        public List<string> companies
        {
            get
            {
                return _companies;
            }
            set
            {
                if(_companies!=value)
                {
                    _companies = value;
                    OnPropertyChanged("companies");
                }
            }
        }
        public string _selectedCompany;
        public string selectedCompany
        {
            get
            {
                return _selectedCompany;
            }
            set
            {
                if (_selectedCompany != value)
                {
                    _selectedCompany = value;
                    OnPropertyChanged("selectedCompany");
                    List<Stock> temp = new List<Stock>();
                    foreach (var x in stocks.Values)
                    {
                        if (x.getStockName() == value)
                        {
                            temp.Add(x);
                        }
                    }
                    tableAttributes = temp;
                }
            }
        }
        public bool _calculateEnabled = false;
        public bool calculateEnabled
        {
            get
            {
                return _calculateEnabled;
            }
            set
            {
                if (_calculateEnabled != value)
                {
                    _calculateEnabled = value;
                    OnPropertyChanged("calculateEnabled");
                }
            }
        }
        public List<Stock> _exportAttributes;
        public List<Stock> exportAttributes
        {
            get
            {
                return _exportAttributes;
            }
            set
            {
                _exportAttributes = value;
                OnPropertyChanged("exportAttributes");
            }
        }
        public CustomStockExportPage(MainWindow _mainWindow, List<Stock> transactions)
        {
            mainWindow = _mainWindow;
            stockTransactions = transactions;
            this.DataContext = this;
            InitializeComponent();
            exportAttributes = new List<Stock>();
            stocks = setTransactions(transactions);
            companies = addCompaniesToComboBox(stocks);

        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> addCompaniesToComboBox(Dictionary<int, Stock> stocks)
        {
            List<string> _companies = new List<string>();
            foreach (var x in stocks.Values)
            {
                if (!_companies.Contains(x.getStockName()))
                {
                    _companies.Add(x.getStockName());
                    Console.WriteLine("?");
                }
            }
            return _companies;
        }

        public Dictionary<int, Stock> setTransactions(List<Stock> transactions)
        {
            Dictionary<int, Stock> _stocks = new Dictionary<int, Stock>();
            for (int i = 0; i < transactions.Count; i++)
            {
                _stocks.Add(i, transactions[i]);
            }
            return _stocks;
        }

        private void ImportedTransactions_Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Stock> tempStock = new List<Stock>(tableAttributes);
            if (ImportedTransactions_Grid.SelectedItem != null)
            {
                if (tableAttributes != null)
                {
                    if (ImportedTransactions_Grid.SelectedItems.Count > 2 || (ImportedTransactions_Grid.SelectedItems.Count < 2))
                    {
                        calculateEnabled = false;
                    }
                    else
                    {
                        bool bought = false;
                        bool sold = false;
                        Regex typeRegex1 = new Regex(@"Vásárolt");
                        Regex typeRegex2 = new Regex(@"Eladott");
                        Regex typeRegex3 = new Regex(@"Bought");
                        Regex typeRegex4 = new Regex(@"Sold");
                        Regex typeRegex5 = new Regex(@"Buy");
                        Regex typeRegex6 = new Regex(@"Sell");
                        foreach (var selected in ImportedTransactions_Grid.SelectedItems)
                        {
                            if (selected is Stock)
                            {
                                var stock = (Stock)selected;
                                if (typeRegex2.IsMatch(stock.getTransactionType()) || 
                                    typeRegex4.IsMatch(stock.getTransactionType()) ||
                                     typeRegex6.IsMatch(stock.getTransactionType())) //todo
                                {
                                    sold = true;
                                }
                                else if(typeRegex1.IsMatch(stock.getTransactionType()) ||
                                        typeRegex3.IsMatch(stock.getTransactionType()) ||
                                        typeRegex5.IsMatch(stock.getTransactionType())) //todo
                                {
                                    bought = true;
                                }
                            }
                        }
                        if (bought && sold)
                            calculateEnabled = true;
                    }
                }
            }
        }

        private void dataGridChanger_Click(object sender, RoutedEventArgs e)
        {
            exportPreviewGrid.Visibility = Visibility.Hidden;
            ImportedTransactions_Grid.Visibility = Visibility.Hidden;
            if (dataGridChanger.Content.ToString()=="Export Preview")
            {
                exportPreviewGrid.Visibility = Visibility.Visible;
                dataGridChanger.Content = "Back to Joining";
            }
            else
            {
                ImportedTransactions_Grid.Visibility = Visibility.Visible;
                dataGridChanger.Content = "Export Preview";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int boughtQuantity = 0;
            int soldQuantity = 0;
            List<Stock> selectedItems = new List<Stock>();
            foreach (var x in ImportedTransactions_Grid.SelectedItems)
            {
                if (x is Stock)
                {
                    var stock = (Stock)x;
                    selectedItems.Add(stock);
                }
            }
            List<Stock> tempTableAttributes = new List<Stock>(tableAttributes);
            double earning = 0;
            Stock boughtStock;
            Stock soldStock;
            Regex typeRegex1 = new Regex(@"Vásárolt");
            Regex typeRegex2 = new Regex(@"Eladott");
            Regex typeRegex3 = new Regex(@"Bought");
            Regex typeRegex4 = new Regex(@"Sold");
            Regex typeRegex5 = new Regex(@"Buy");
            Regex typeRegex6 = new Regex(@"Sell");
            if (typeRegex1.IsMatch(selectedItems[0].getTransactionType()) ||
                typeRegex3.IsMatch(selectedItems[0].getTransactionType()) ||
                typeRegex5.IsMatch(selectedItems[0].getTransactionType())) //todo
            { 
                boughtStock = selectedItems[0];
                boughtQuantity = boughtStock.getQuantity();
                soldStock = selectedItems[1];
                soldQuantity = soldStock.getQuantity();
                if (boughtQuantity == soldQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtQuantity;
                    soldStock.setQuantity(0);
                    boughtStock.setQuantity(0);
                }
                else if (boughtQuantity > soldQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldQuantity;
                    soldStock.setQuantity(0);
                    boughtStock.setQuantity(boughtQuantity - soldQuantity);
                }
                else if (soldQuantity > boughtQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtQuantity;
                    boughtStock.setQuantity(0);
                    soldStock.setQuantity(soldQuantity - boughtQuantity);
                }
                soldStock.setProfit(soldStock.getProfit() + earning);
                selectedItems[0] = boughtStock;
                selectedItems[1] = soldStock;
            }
            else
            {
                boughtStock = selectedItems[1];
                boughtQuantity = boughtStock.getQuantity();
                soldStock = selectedItems[0];
                soldQuantity = soldStock.getQuantity();
                if (boughtQuantity == soldQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtQuantity;
                }
                else if (boughtQuantity > soldQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldQuantity;
                    soldStock.setQuantity(0);
                    boughtStock.setQuantity(boughtQuantity - soldQuantity);
                }
                else if (soldQuantity > boughtQuantity)
                {
                    earning = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtQuantity;
                    boughtStock.setQuantity(0);
                    soldStock.setQuantity(soldQuantity - boughtQuantity);
                }
                soldStock.setProfit(soldStock.getProfit() + earning);
                selectedItems[1] = boughtStock;
                selectedItems[0] = soldStock;
            }
            for (int i = 0; i < tempTableAttributes.Count; i++)
            {
                if (tempTableAttributes[i] == boughtStock)
                {
                    tempTableAttributes[i] = boughtStock;
                }
                else if (tempTableAttributes[i] == soldStock)
                {
                    tempTableAttributes[i] = soldStock;
                }
            }
            tableAttributes = tempTableAttributes;
            List<Stock> tempExportAttributes = new List<Stock>(exportAttributes);
            if (!tempExportAttributes.Contains(boughtStock))
                tempExportAttributes.Add(boughtStock);
            else//if it's in there we update it
            {
                for (int i = 0; i < tempExportAttributes.Count; i++)
                {
                    if (tempExportAttributes[i] == boughtStock)
                    {
                        tempExportAttributes[i] = boughtStock;
                    }
                }
            }
            if (!tempExportAttributes.Contains(soldStock))
                tempExportAttributes.Add(soldStock);
            else//if it's in there we update it
            {
                for (int i = 0; i < tempExportAttributes.Count; i++)
                {
                    if (tempExportAttributes[i] == soldStock)
                    {
                        tempExportAttributes[i] = soldStock;
                    }
                }
            }

            exportAttributes = tempExportAttributes;
            List<int> indexes = new List<int>();
            for (int i = 0; i < tempExportAttributes.Count; i++)
            {
                int currentIndex = stocks.FirstOrDefault(x => x.Value == tempExportAttributes[i]).Key;
                indexes.Add(currentIndex);//adding the dictionary indexes
            }
            for (int i = 0; i < indexes.Count; i++)
            {
                int minIdx = indexes.Count - 1;
                for (int j = i + 1; j < indexes.Count; j++)
                {
                    if (indexes[minIdx] > indexes[j])
                    {
                        minIdx = j;
                    }
                }
                if (indexes[minIdx] < indexes[i])
                {
                    int copy = indexes[i];
                    indexes[i] = indexes[minIdx];
                    indexes[minIdx] = copy;
                }
            }
            List<Stock> tempExportAttributes2 = new List<Stock>();
            for (int i = 0; i < indexes.Count; i++)
            {
                for (int j = 0; j < tableAttributes.Count; j++)
                {
                    if (tableAttributes[j] == stocks[indexes[i]])
                    {
                        tempExportAttributes2.Add(tableAttributes[j]);
                        break;
                    }
                }
            }
            exportAttributes = tempExportAttributes2;
        }
    }
}

