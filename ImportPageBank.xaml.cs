﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Animation;
using WPFCustomMessageBox;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ImportMainPage.xaml
    /// </summary>
    public partial class ImportPageBank : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        private ButtonCommands btnCommand;
        private MainWindow mainWindow;
        private static ImportPageBank instance;
        public PageAnimation pageLoadAnimation { get; set; } = PageAnimation.SlideAndFadeInFromRight;
        public PageAnimation pageUnloadAnimation { get; set; } = PageAnimation.SlideAndFadeOutToLeft;
        public string _selectedBank;
        public string selectedBank
        {
            get
            {
                return _selectedBank;
            }
            set
            {
                if(_selectedBank!=value)
                {
                    int counter = 0;
                    foreach(var transaction in SavedTransactions.getSavedTransactionsBank())
                    {
                        if (transaction.getBankname() == value)
                            counter++;
                    }
                    dividedForBanks.Content = counter.ToString();
                    dividedForBanks.Visibility = Visibility.Visible;
                    _selectedBank = value;
                    OnPropertyChanged("selectedBank");
                }
            }
        }
        public List<string> _banks;
        public List<string> banks
        {
            get
            {
                return _banks;
            }
            set
            {
                if(_banks!=value)
                {
                    _banks = value;
                    OnPropertyChanged("banks");
                }
            }
        }
        public string _selectedAccountNumber;
        public string selectedAccountNumber
        {
            get
            {
                return _selectedAccountNumber;
            }
            set
            {
                if (_selectedAccountNumber != value)
                {
                    int counter = 0;
                    foreach (var transactions in SavedTransactions.getSavedTransactionsBank())
                    {
                        if (value == transactions.getAccountNumber())
                            counter++;
                    }
                    dividedForAccountNumber.Content = counter.ToString();
                    dividedForAccountNumber.Visibility = Visibility.Visible;
                    _selectedAccountNumber = value;
                    OnPropertyChanged("selectedAccountNumber");
                }
            }
        }
        public List<string> _accountNumbers;
        public List<string> accountNumbers
        {
            get
            {
                return _accountNumbers;
            }
            set
            {
                if (_accountNumbers != value)
                {
                    _accountNumbers = value;
                    OnPropertyChanged("accountNumbers");
                }
            }
        }
        public double slideSenconds { get; set; } = 0.5;
        public bool alwaysAsk
        {
            get
            {
                if(alwaysAskCB.IsChecked.Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if(value)
                {
                    neverAskCB.SetCurrentValue(RadioButton.IsCheckedProperty, false);
                }
            }
        }
        public bool neverAsk
        {
            get
            {
                if (neverAskCB.IsChecked.Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    alwaysAskCB.SetCurrentValue(RadioButton.IsCheckedProperty, false);
                }
            }
        }
        private ImportPageBank(MainWindow mainWindow)
        {
            DataContext = this;
            InitializeComponent();
            neverAskCB.IsChecked = true;
            this.mainWindow = mainWindow;
            dividedForAccountNumber.Visibility = Visibility.Hidden;
            dividedForBanks.Visibility = Visibility.Hidden;
        }
        public void setUserStatistics(User currentUser)
        {
            int numberOfTransactions = 0;
            int totalIncome = 0;
            int totalSpendings = 0;
            string latestImportDate = "";
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime todayDate = Convert.ToDateTime(todaysDate);
            usernameLabel.Content = currentUser.getUsername();
            string accountNumber = currentUser.getAccountNumber();
            string[] splittedAccountNumber = accountNumber.Split(',');
            accountNumbers = new List<string>();
            banks = new List<string>();
            for(int i=0;i<splittedAccountNumber.Length;i++)
            {
                accountNumbers.Add(splittedAccountNumber[i]);
            }
            foreach (var transactions in SavedTransactions.getSavedTransactionsBank())
            {
                if (!banks.Contains(transactions.getBankname()))
                    banks.Add(transactions.getBankname());
                //it the user has more than 1 account number it separated by commas
                for (int i = 0; i < splittedAccountNumber.Length; i++)
                {
                    if (transactions.getAccountNumber()==splittedAccountNumber[i])
                    {
                        numberOfTransactions++;
                        latestImportDate = transactions.getWriteDate();
                        if (transactions.getTransactionPrice() > 0)
                        {
                            totalIncome += transactions.getTransactionPrice();
                        }
                        else
                        {
                            totalSpendings += transactions.getTransactionPrice();
                        }
                    }
                }
            }
            if (latestImportDate.Length > 12)
            {
                lastImportDateLabel.Content = latestImportDate.Substring(0, 12);
            }
            else
            {
                lastImportDateLabel.Content = latestImportDate;
            }
            noTransactionsLabel.Content = numberOfTransactions;
            DateTime importDate;
            if (latestImportDate.Length > 0)
            {
                importDate = Convert.ToDateTime(latestImportDate);
                float diffTicks = (todayDate - importDate).Days;
                if (diffTicks >= 30)
                {
                    urgencyLabel.Content = "Recommended!";
                    urgencyLabel.Foreground = new SolidColorBrush(Color.FromRgb(217, 30, 24));
                    mainWindow.exclamImage.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    urgencyLabel.Content = "Not urgent";
                    urgencyLabel.Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    if (mainWindow.exclamImage.Visibility != System.Windows.Visibility.Visible)
                    {
                        mainWindow.exclamImage.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
            else
            {
                urgencyLabel.Content = "You haven't imported yet!";
                lastImportDateLabel.Content = "You haven't imported yet!";
            }
            if (SavedTransactions.getSavedTransactionsBank().Count > 0)
                importHistoryButton.Visibility = Visibility.Visible;
            else
                importHistoryButton.Visibility = Visibility.Hidden;
        }
        private void getTransactions(string importType, List<string> folderAddress)
        {
            new ImportReadIn(importType, folderAddress, mainWindow,false);
        }
        public ButtonCommands OpenFilePushed
        {
            get
            {
                btnCommand = new ButtonCommands(FileBrowser.Content.ToString(),this);
                return btnCommand;
            }
        }
        public ButtonCommands ImportHistoryPushed
        {
            get
            {
                btnCommand = new ButtonCommands(importHistoryButton.Name.ToString(), this);
                return btnCommand;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static ImportPageBank getInstance(MainWindow mainWindow)
        {
            if(instance==null)
            {
                instance = new ImportPageBank(mainWindow);
            }
            instance.Loaded += instance.Instance_Loaded;
            return instance;
        }
        private void Instance_Loaded(object sender, RoutedEventArgs e)
        {
            switch (instance.pageLoadAnimation)
            {
                case PageAnimation.SlideAndFadeInFromRight:
                    var sb = new Storyboard();
                    var slideAnimation = new ThicknessAnimation
                    {
                        Duration = new Duration(TimeSpan.FromSeconds(instance.slideSenconds)),
                        From = new Thickness(instance.WindowWidth, 0, -instance.WindowWidth, 0),
                        To = new Thickness(0),
                        DecelerationRatio = 0.9f
                    };
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("Margin"));
                    sb.Children.Add(slideAnimation);
                    sb.Begin(instance);
                    break;
                case PageAnimation.SlideAndFadeOutToLeft:
                    break;
            }
        }
        public class ButtonCommands : ICommand
        {
            private string buttonContent;
            private ImportPageBank importPageBank;
            public ButtonCommands(string buttonContent,ImportPageBank importPageBank)
            {
                this.buttonContent = buttonContent;
                this.importPageBank = importPageBank;

                this.importPageBank.PropertyChanged += new PropertyChangedEventHandler(test_PropertyChanged);
            }
            private void test_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (CanExecuteChanged != null)
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
            private void check_if_csv(int fileIndex,ref List<string> fileAddresses)
            {
                string[] fileName = fileAddresses[fileIndex].Split('\\');
                int lastPartIndex = fileName.Length - 1;
                Regex csvPattern = new Regex(@".csv$");
                if (csvPattern.IsMatch(fileName[lastPartIndex]))
                {
                    string newExcelPath = fileAddresses[fileIndex].Substring(0, fileAddresses[fileIndex].Length - 4);
                    string xls = newExcelPath + ".xls";

                    List<List<string>> allWords = new List<List<string>>();
                    IEnumerable<String> all_lines = System.IO.File.ReadLines(fileAddresses[fileIndex], Encoding.Default);
                    foreach (var lines in all_lines)
                    {
                        List<string> words = lines.Split(';').ToList();
                        allWords.Add(words);
                    }
                    Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                    Workbook wb = app.Workbooks.Open(fileAddresses[fileIndex], Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    Worksheet sheet = wb.Worksheets[1];
                    int row = 1;
                    foreach (List<string> lines in allWords)
                    {
                        int column = 1;
                        for (int itr = 0; itr < lines.Count; itr++)
                        {
                            sheet.Cells[row, column].Value = lines[itr];
                            column++;
                        }
                        row++;
                    }
                    wb.SaveAs(newExcelPath, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    wb.Close();
                    app.Quit();
                    fileAddresses[fileIndex] = newExcelPath; //overwriting the old string
                }
            }
            public void Execute(object parameter)
            {
                if (buttonContent.Equals("Import Bank Transactions"))
                {
                    MessageBoxResult messageBoxResult = CustomMessageBox.ShowYesNo(
                        "\tPlease choose an import type!",
                        "Import type alert!",
                        "Automatized",
                        "User specified");
                    if (messageBoxResult == MessageBoxResult.Yes || messageBoxResult==MessageBoxResult.No)
                    {
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.DefaultExt = ".xls,.csv";
                        dlg.Filter = "Excel files (*.xls)|*.xls|Excel Files (*.xlsx)|*.xlsx|Excel Files (*.xlsm)|*.xlsm|CSV Files (*.csv)|*.csv";
                        dlg.Multiselect = true;
                        Nullable<bool> result = dlg.ShowDialog();
                        if (result == true)
                        {
                            List<string> fileAdresses = dlg.FileNames.ToList();
                            for (int i = 0; i < dlg.FileNames.ToList().Count; i++)
                            {
                                check_if_csv(i,ref fileAdresses);
                            }
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                importPageBank.getTransactions("Bank", fileAdresses);
                            }
                            else if (messageBoxResult == MessageBoxResult.No)
                            {
                                string[] fileName = dlg.FileNames.ToList()[0].Split('\\');
                                int lastPartIndex = fileName.Length - 1; // to see which file the user immporting first
                                SpecifiedImportBank.getInstance(fileAdresses, importPageBank.mainWindow).setCurrentFileLabel(fileName[lastPartIndex]);
                                //fájl felismerés
                                SpecifiedImportBank.getInstance(null, importPageBank.mainWindow).setBoxValuesToZero();
                                StoredColumnChecker columnChecker = new StoredColumnChecker();
                                columnChecker.getDataTableFromSql(importPageBank.mainWindow);
                                columnChecker.addDistinctBanksToCB();
                                columnChecker.setAnalyseWorksheet(dlg.FileNames.ToList()[0]);
                                columnChecker.setMostMatchesRow(columnChecker.findMostMatchingRow());
                                columnChecker.setSpecifiedImportPageTextBoxes();
                                importPageBank.mainWindow.MainFrame.Content = SpecifiedImportBank.getInstance(dlg.FileNames.ToList(), importPageBank.mainWindow);
                            }
                        }
                    }
                }
                else if(buttonContent == "importHistoryButton")
                {
                    ImportStatsChartBank importStatsChart = new ImportStatsChartBank(importPageBank.banks);
                    importStatsChart.selectedBank = "All";
                    importPageBank.mainWindow.MainFrame.Content = importStatsChart;
                }
            }
        }
    }
}
