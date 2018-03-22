using System;
using System.Collections.Generic;
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
    public partial class DatabaseDataBank : Page
    {
        public List<string> categoryName { get; set; }
        private static DatabaseDataBank instance;
        private List<Transaction> tableAttributes;
        private MainWindow mainWindow;
        private DatabaseDataBank(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            DataContext = this;
            InitializeComponent();
        }
        public void setTableAttributes()
        {
            if (TransactionTableXAML != null)
            {
                TransactionTableXAML.Items.Clear();
            }
            tableAttributes = SavedTransactions.getSavedTransactionsBank();
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
                addAtribuesToTable();
            }
        }
        private void addAtribuesToTable()
        {
            //valami baj van itt az ERSTE-s importal, automatikus importálás esetén
            //közleményt kiválasztva

            //nem adja hozzá a frissen importált részvényeket a táblázathoz
            //lehet hogy a bankhoz sem....

            //banki interface hogy müködik stb.
            foreach (var attribute in tableAttributes)
            {
                string[] splittedAccountNumbers = mainWindow.getCurrentUser().getAccountNumber().Split(',');
                for (int i = 0; i < splittedAccountNumbers.Length; i++)
                {
                    if (attribute.getAccountNumber() == splittedAccountNumbers[i])
                    {
                        TransactionTableXAML.Items.Add(attribute);
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
