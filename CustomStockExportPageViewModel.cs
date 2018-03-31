using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public class CustomStockExportPageViewModel : INotifyPropertyChanged
    {
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
                if(_tableAttributes!=value)
                {
                    _tableAttributes = value;
                    OnPropertyChanged("tableAttributes");
                }
            }
        }
        public List<string> companies { get; set; }
        public string _selectedCompany;
        public string selectedCompany
        {
            get
            {
                return _selectedCompany;
            }
            set
            {
                if(_selectedCompany!=value)
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
        public bool _calculateEnabled;
        public bool calculateEnabled
        {
            get
            {
                return _calculateEnabled;
            }
            set
            {
                if(_calculateEnabled!=value)
                {
                    _calculateEnabled = value;
                    OnPropertyChanged("calculateEnabled");
                }
            }
        }
        public CustomStockExportPage stockExportPage;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public CustomStockExportPageViewModel(CustomStockExportPage _stockExportPage,List<Stock> transactions)
        {
            stockExportPage = _stockExportPage;
            stocks = setTransactions(transactions);
            companies = addCompaniesToComboBox(stocks);
        }

        private List<string> addCompaniesToComboBox(Dictionary<int, Stock> stocks)
        {
            List<string> _companies = new List<string>();
            foreach (var x in stocks.Values)
            {
                if (!_companies.Contains(x.getStockName()))
                {
                    _companies.Add(x.getStockName());
                }
            }
            return _companies;
        }

        public Dictionary<int,Stock> setTransactions(List<Stock> transactions)
        {
            Dictionary<int, Stock> _stocks = new Dictionary<int, Stock>();
            for(int i=0;i<transactions.Count;i++)
            {
                _stocks.Add(i, transactions[i]);
            }
            return _stocks;
        }

        private void ImportedTransactions_Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("asd");
            List<Stock> tempStock = new List<Stock>(tableAttributes);
            if (stockExportPage.ImportedTransactions_Grid.SelectedItem != null)
            {
                if (tableAttributes != null)
                {
                    if (stockExportPage.ImportedTransactions_Grid.SelectedItems.Count > 2 || (stockExportPage.ImportedTransactions_Grid.SelectedItems.Count < 2))
                    {
                        calculateEnabled = false;
                    }
                    else
                    {
                        bool bought = false;
                        bool sold = false;
                        foreach (var selected in stockExportPage.ImportedTransactions_Grid.SelectedItems)
                        {
                            if (selected is Stock)
                            {
                                var stock = (Stock)selected;
                                if (stock.getTransactionType() == "SELL") //todo
                                {
                                    sold = true;
                                }
                                else if (stock.getTransactionType() == "BUY") //todo
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
    }
}
