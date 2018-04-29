using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public static string dbPath= AppDomain.CurrentDomain.BaseDirectory + "FHI_database.db";
        private bool newImport = false;
        public User currentUser;
        private string accountNumber = " ";
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            menuStackPanel.DataContext = new MainWindowViewModel(this);
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
        private void startUpReadIn()
        {
            //reading in saved transactions
            SavedTransactions.getInstance().readOutSavedBankTransactions();
            SavedTransactions.getInstance().readOutStockSavedTransactions();
        }
        public void getTransactions(string bankName, List<string> folderAddress)
        {
            new ImportReadIn(bankName, folderAddress, this, false);
        }
    }
    public class ExpanderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value)) return parameter;
            return null;
        }
    }
}
