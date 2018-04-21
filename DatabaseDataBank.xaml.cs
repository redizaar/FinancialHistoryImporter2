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
        private static DatabaseDataBank instance;
        public string _accountNumberChoice;
        public string accountNumberChoice
        {
            get
            {
                return _accountNumberChoice;
            }
            set
            {
                if(_accountNumberChoice!=value)
                {
                    _accountNumberChoice = value;
                    OnPropertyChanged("accountNumberChoice");
                }
            }
        }
        public List<string> _accountNumberChoices;
        public List<string> accountNumberChoices
        {
            get
            {
                return _accountNumberChoices;
            }
            set
            {
                if(_accountNumberChoices!=value)
                {
                    _accountNumberChoices = value;
                    OnPropertyChanged("accountNumberChoices");
                }
            }
        }
        public string _bankChoice;
        public string bankChoice
        {
            get
            {
                return _bankChoice;
            }
            set
            {
                if(_bankChoice!=value)
                {
                    _bankChoice = value;
                    OnPropertyChanged("bankChoice");
                }
            }
        }
        public List<string> _bankChoices;
        public List<string> bankChoices
        {
            get
            {
                return _bankChoices;
            }
            set
            {
                if(_bankChoices!=value)
                {
                    _bankChoices = value;
                    OnPropertyChanged("bankChoices");
                }
            }
        }
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
            bankChoices = new List<string>();
            List<Transaction> allTransactions = SavedTransactions.getSavedTransactionsBank();
            List<Transaction> reference = new List<Transaction>();
            string[] splittedAccountNumbers = mainWindow.getCurrentUser().getAccountNumber().Split(',');
            foreach (var tableAttribute in allTransactions)
            {
                for (int i = 0; i < splittedAccountNumbers.Length; i++)
                {
                    if (tableAttribute.getAccountNumber() == splittedAccountNumbers[i])
                    {
                        reference.Add(tableAttribute);
                        break;
                    }
                }
            }
            if (tableAttributes != null)
            {
                foreach (var transaction in reference)
                {
                    if (transaction.getWriteDate() != null && transaction.getWriteDate().Length >= 12)
                    {
                        transaction.setWriteDate(transaction.getWriteDate().Substring(0, 12));
                    }
                    else
                    {
                        transaction.setWriteDate(DateTime.Now.ToString("yyyy/MM/dd").Substring(0, 12));
                    }
                }
            }
            tableAttributes = reference;
            foreach(var x in tableAttributes)
            {
                if (!bankChoices.Contains(x.getBankname()))
                    bankChoices.Add(x.getBankname());
            }
        }
        public void setTableAttributesToBankName(string bankName)
        {
            List<Transaction> allTransactions = SavedTransactions.getSavedTransactionsBank();
            List<Transaction> reference = new List<Transaction>();
            string[] splittedAccountNumbers = mainWindow.getCurrentUser().getAccountNumber().Split(',');
            foreach (var tableAttribute in allTransactions)
            {
                for (int i = 0; i < splittedAccountNumbers.Length; i++)
                {
                    if ((tableAttribute.getAccountNumber() == splittedAccountNumbers[i]) && (tableAttribute.getBankname()==bankName))
                    {
                        reference.Add(tableAttribute);
                        break;
                    }
                }
            }
            if (tableAttributes != null)
            {
                foreach (var transaction in reference)
                {
                    if (transaction.getWriteDate() != null && transaction.getWriteDate().Length >= 12)
                    {
                        transaction.setWriteDate(transaction.getWriteDate().Substring(0, 12));
                    }
                    else
                    {
                        transaction.setWriteDate(DateTime.Now.ToString("yyyy/MM/dd").Substring(0, 12));
                    }
                }
            }
            tableAttributes = reference;
        }
        public static DatabaseDataBank getInstance(MainWindow mainWindow)
        {
            if(instance==null)
            {
                instance = new DatabaseDataBank(mainWindow);
            }
            return instance;
        }

        private void bankNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setTableAttributesToBankName(bankChoice);
        }
    }
}
