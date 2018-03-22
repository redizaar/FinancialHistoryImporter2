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
    /// <summary>
    /// Interaction logic for DatabaseDataStock.xaml
    /// </summary>
    public partial class DatabaseDataStock : Page
    {
        private static DatabaseDataStock instance;
        private List<Stock> tableAttributes;
        private MainWindow mainWindow;
        private DatabaseDataStock(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            DataContext = this;
            InitializeComponent();
        }
        public void setTableAttributes()
        {
            if (TransactionTableXAML != null)
            {
                TransactionTableXAML.Items.Clear();
            }
            tableAttributes = SavedTransactions.getSavedTransactionsStock();
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
        
            foreach (var attribute in tableAttributes)
            {
                if (attribute.getImporter() == mainWindow.getCurrentUser().getUsername())
                    TransactionTableXAML.Items.Add(attribute);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DatabaseDataBank.getInstance(mainWindow).setTableAttributes();
            mainWindow.MainFrame.Content = DatabaseDataBank.getInstance(mainWindow);
        }
    }
}
