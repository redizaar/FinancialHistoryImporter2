using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public class MainWindowViewModel
    {
        public Object SelectedExpander { get; set; }
        public MainWindow mainWindow;
        public MainWindowViewModel(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
        }
        public ICommand bankImportPushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        public ICommand stockImportPushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        public ICommand bankDatabasePushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        public ICommand stockDatabasePushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        public ICommand stockChartPushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        public ICommand stockDatagridPushed
        {
            get
            {
                var v = check_sender();
                return new CommandHandler(() => MyAction(v), _canExecute);
            }
        }
        private string check_sender([CallerMemberName]string memberName = "")
        {
            return memberName;
        }
        private bool _canExecute = true;
        public void MyAction(string commandName)
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
            if (commandName == "bankImportPushed")
            {
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageBank.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.bankImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName == "stockImportPushed")
            {
                ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                mainWindow.MainFrame.Content = ImportPageStock.getInstance(mainWindow);
                mainWindow.importMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.importDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockImport.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName == "bankDatabasePushed")
            {
                DatabaseDataBank.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content = DatabaseDataBank.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.bankDatabase.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName == "stockDatabasePushed")
            {
                DatabaseDataStock.getInstance(mainWindow).setTableAttributes();
                mainWindow.MainFrame.Content = DatabaseDataStock.getInstance(mainWindow);
                mainWindow.tableMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.tableDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockDatabase.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName == "stockChartPushed")
            {
                StockChart stockChart = new StockChart(mainWindow);
                mainWindow.MainFrame.Content = stockChart;
                mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockChart.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName == "stockDatagridPushed")
            {
                mainWindow.MainFrame.Content = new StockDataGrid(mainWindow);
                mainWindow.portfolioMenuTop.Visibility = System.Windows.Visibility.Visible;
                mainWindow.stockChartDock.Background = new SolidColorBrush(Color.FromRgb(198, 61, 15));
                mainWindow.stockChart.Background = new SolidColorBrush(Color.FromRgb(255, 140, 105));
            }
            else if (commandName=="Exit")
            {
                mainWindow.Close();
            }
        }
    }
    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
