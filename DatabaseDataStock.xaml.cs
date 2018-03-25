using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            foreach (var tableAttribute in allTransactions)
            {
                if (tableAttribute.getImporter() == mainWindow.getCurrentUser().getUsername())
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
}
