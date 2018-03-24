using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private ButtonCommands btnCommand;
        //private List<Transaction> tableAttributes=null;
        private bool newImport = false;
        public User currentUser;
        private string accountNumber= " ";
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            LoginFrame.Content = new Login_Page(this);
            tableMenuTop.Visibility = System.Windows.Visibility.Hidden; //importmenu is default
            portfolioMenuTop.Visibility = System.Windows.Visibility.Hidden;
            startUpReadIn();
        }
        public void setCurrentUser(User user)
        {
            currentUser = user;
        }
        public bool getNewImport()
        {
            return newImport;
        }
        public String getAccounNumber()
        {
            return accountNumber;
        }
        public User getCurrentUser()
        {
            return currentUser;
        }
        public void setAccountNumber(string _accountNumber)
        {
            accountNumber = _accountNumber;
        }
        public ButtonCommands ImportPushed
        {
            get
            {
                btnCommand = new ButtonCommands(ImportButton.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands TablePushed
        {
            get
            {
                btnCommand = new ButtonCommands(TableButton.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands PortfolioPushed
        {
            get
            {
                btnCommand = new ButtonCommands(StockChartButton.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands ExitPushed
        {
            get
            {
                btnCommand = new ButtonCommands(ExitButton.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands BankImportPushed
        {
            get
            {
                btnCommand = new ButtonCommands(bankImport.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands StockImportPushed
        {
            get
            {
                btnCommand = new ButtonCommands(stockImport.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands BankDatabasePushed
        {
            get
            {
                btnCommand = new ButtonCommands(bankDatabase.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands stockDatabasePushed
        {
            get
            {
                btnCommand = new ButtonCommands(stockDatabase.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands stockChartPushed
        {
            get
            {
                btnCommand = new ButtonCommands(stockChart.Content.ToString(), this);
                return btnCommand;
            }
        }
        public ButtonCommands stockDatagridPushed
        {
            get
            {
                btnCommand = new ButtonCommands(stockDatagrid.Content.ToString(), this);
                return btnCommand;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void startUpReadIn()
        {
            //reading in saved transactions
            SavedTransactions.getInstance().readOutSavedBankTransactions();
            SavedTransactions.getInstance().readOutStockSavedTransactions();
        }
        public void getTransactions(string bankName,List<string> folderAddress)
        {
            new ImportReadIn(bankName, folderAddress,this,false);
        }
    }
    public class ButtonCommands : ICommand
    {
        private string buttonContent;
        private MainWindow mainWindow;
        public ButtonCommands(string buttonContent,MainWindow mainWindow)
        {
            this.buttonContent = buttonContent;
            this.mainWindow = mainWindow;

            this.mainWindow.PropertyChanged += new PropertyChangedEventHandler(test_PropertyChanged);
        }
        private void test_PropertyChanged(object sender,PropertyChangedEventArgs e)
        {
            if(CanExecuteChanged!=null)
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
            mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(217, 133, 59));
            mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(217, 133, 59));
            mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(217, 133, 59));
            mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.bankImport.Background = Brushes.Transparent;
            mainWindow.stockImport.Background = Brushes.Transparent;
            mainWindow.bankDatabase.Background = Brushes.Transparent;
            mainWindow.stockDatabase.Background = Brushes.Transparent;
            mainWindow.stockChart.Background = Brushes.Transparent;
            mainWindow.stockDatagrid.Background = Brushes.Transparent;
            /*
            if (buttonContent.Equals("Import"))
            {
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
            }
            */
            if (buttonContent == "Bank Import")
            {
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.bankImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (buttonContent == "Stock Import")
            {
                ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageStock.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            /*
            else if(buttonContent.Equals("Database"))
            {
                DatabaseDataBank.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content=DatabaseDataBank.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
            }
            */
            else if (buttonContent == "Bank Data")
            {
                DatabaseDataBank.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content = DatabaseDataBank.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.bankDatabase.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if(buttonContent == "Stock Data")
            {
                DatabaseDataStock.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content = DatabaseDataStock.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockDatabase.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if(buttonContent=="Chart")
            {
                StockChart stockChart = new StockChart(mainWindow);
                mainWindow.MainFrame.Content = stockChart;
                mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockChart.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if(buttonContent=="DataGrid")
            {
                mainWindow.MainFrame.Content = new StockDataGrid(mainWindow);
                mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockChart.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if(buttonContent.Equals("Exit"))
            {
                mainWindow.Close();
            }
        }
    }
}
