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
        public Stopwatch webStockStopwatch=new Stopwatch();
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
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void startUpReadIn()
        {
            //reading in saved transactions
            SavedTransactions.getInstance().readOutSavedBankTransactions();
            //SavedTransactions.getInstance().readOutStockSavedTransactions();
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
            if (buttonContent.Equals("Import"))
            {
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
            }
           else if(buttonContent.Equals("Database"))
           {
                TransactionMain.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content=TransactionMain.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
           }
           else if(buttonContent.Equals("stockMarketData"))
            {
                if (mainWindow.webStockStopwatch.Elapsed == TimeSpan.FromMilliseconds(0))
                {
                    mainWindow.webStockStopwatch.Start();
                    StockChart stockChart = new StockChart(mainWindow);
                    mainWindow.MainFrame.Content = stockChart;
                    mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Visible;
                    mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                }
                else
                {
                    if (mainWindow.webStockStopwatch.Elapsed <= TimeSpan.FromMinutes(0.2))
                    {
                        MessageBox.Show("Please wait for " + (TimeSpan.FromMinutes(0.2) - mainWindow.webStockStopwatch.Elapsed) + " seconds!");
                    }
                    else
                    {
                        mainWindow.webStockStopwatch.Stop();
                        mainWindow.webStockStopwatch.Reset();
                    }
                }
            }
           else if(buttonContent.Equals("Exit"))
           {
                mainWindow.Close();
           }
        }
    }
}
