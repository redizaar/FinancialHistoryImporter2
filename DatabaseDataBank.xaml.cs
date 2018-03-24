using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class DatabaseDataBank : Page, INotifyPropertyChanged
    {
        public List<string> categoryName { get; set; }
        private static DatabaseDataBank instance;
        public List<Transaction> _tableAttributes;
        public List<Transaction> tableAttributes
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private MainWindow mainWindow;
        private DatabaseDataBank(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            DataContext = this;
            InitializeComponent();
        }
        public void setTableAttributes()
        {
            List<Transaction> allTransactions = SavedTransactions.getSavedTransactionsBank();
            List<Transaction> reference = new List<Transaction>();
            foreach (var tableAttribute in allTransactions)
            {
                string[] splittedAccountNumbers = mainWindow.getCurrentUser().getAccountNumber().Split(',');
                for (int i = 0; i < splittedAccountNumbers.Length; i++)
                {
                    if (tableAttribute.getAccountNumber() == splittedAccountNumbers[i])
                    {
                        reference.Add(tableAttribute);
                        break;
                    }
                }
            }
            tableAttributes = new List<Transaction>(reference);
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
        public static DatabaseDataBank getInstance(MainWindow mainWindow)
        {
            if(instance==null)
            {
                instance = new DatabaseDataBank(mainWindow);
            }
            return instance;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DatabaseDataStock.getInstance(mainWindow).setTableAttributes();
            mainWindow.MainFrame.Content = DatabaseDataStock.getInstance(mainWindow);
        }
    }
}
