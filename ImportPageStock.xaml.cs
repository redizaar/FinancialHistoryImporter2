﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfApp1.Animation;
using WPFCustomMessageBox;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ImportPageStock.xaml
    /// </summary>
    public partial class ImportPageStock : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        public bool _lifoMethod  = true;
        public bool _fifoMethod  = false;
        public bool _customMethod  = false;
        public bool lifoMethod
        {
            get
            {
                return _lifoMethod;
            }
            set
            {
                _lifoMethod = value;
            }
        }
        public bool fifoMethod
        {
            get
            {
                return _fifoMethod;
            }
            set
            {
                _fifoMethod = value;
            }
        }
        public bool customMethod
        {
            get
            {
                return _customMethod;
            }
            set
            {
                _customMethod = value;
            }
        }
        public PageAnimation pageLoadAnimation { get; set; } = PageAnimation.SlideAndFadeInFromRight;
        public PageAnimation pageUnloadAnimation { get; set; } = PageAnimation.SlideAndFadeOutToLeft;
        public double slideSenconds { get; set; } = 0.5;
        public string _selectedStock;
        public string selectedStock
        {
            get
            {
                return _selectedStock;
            }
            set
            {
                if(_selectedStock!=value)
                {
                    int counter = 0;
                    foreach(var transaction in SavedTransactions.getSavedTransactionsStock())
                    {
                        if(transaction.getStockName()==value && transaction.getCurrentQuantity()>0)
                        {
                            counter++;
                        }
                    }
                    specifiedOwned.Content = counter.ToString();
                    specifiedOwned.Visibility = Visibility.Visible;
                    _selectedStock = value;
                    OnPropertyChanged("selectedStock");
                }
            }
        }
        public List<string> _currentlyOwnedStocks;
        public List<string> currentlyOwnedStocks
        {
            get
            {
                return _currentlyOwnedStocks;
            }
            set
            {
                if(_currentlyOwnedStocks!=value)
                {
                    _currentlyOwnedStocks = value;
                    OnPropertyChanged("currentlyOwnedStocks");
                }
            }
        }
        public MainWindow mainWindow;
        private static ImportPageStock instance; 
        private ImportPageStock(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            DataContext = this;
            InitializeComponent();
            specifiedOwned.Visibility = Visibility.Hidden;
        }
        public static ImportPageStock getInstance(MainWindow mainWindow)
        {
            if(instance==null)
            {
                instance = new ImportPageStock(mainWindow);
            }
            instance.Loaded += instance.ImportPageStock_Loaded;
            return instance;
        }
        private  void ImportPageStock_Loaded(object sender, RoutedEventArgs e)
        {
            switch (pageLoadAnimation)
            {
                case PageAnimation.SlideAndFadeInFromRight:
                    var sb = new Storyboard();
                    var slideAnimation = new ThicknessAnimation
                    {
                        Duration = new Duration(TimeSpan.FromSeconds(this.slideSenconds)),
                        From = new Thickness(-this.WindowWidth, 0, this.WindowWidth, 0),
                        To = new Thickness(0),
                        DecelerationRatio = 0.9f
                    };
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("Margin"));
                    sb.Children.Add(slideAnimation);
                    sb.Begin(this);
                    break;
                case PageAnimation.SlideAndFadeOutToLeft:
                    break;
            }
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
            currentlyOwnedStocks = new List<string>();
            int counter=0;
            foreach (var transactions in SavedTransactions.getSavedTransactionsStock())
            {
                string username = currentUser.getUsername();
                if (transactions.getImporter() == username)
                {
                    numberOfTransactions++;
                    latestImportDate = transactions.getWriteDate();
                }
                if (transactions.getCurrentQuantity() > 0)
                {
                    counter++;
                    if(!currentlyOwnedStocks.Contains(transactions.getStockName()))
                        currentlyOwnedStocks.Add(transactions.getStockName());
                }
            }
            allOwned.Content = counter.ToString();
            noTransactionsLabel.Content = numberOfTransactions;
            if (latestImportDate.Length >= 12)
            {
                lastImportDateLabel.Content = latestImportDate.Substring(0, 12);
            }
            else
            {
                lastImportDateLabel.Content = latestImportDate;
            }
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
        }
        private void FileBrowser_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = CustomMessageBox.ShowYesNo(
                        "\tPlease choose an import type!",
                        "Import type alert!",
                        "Automatized",
                        "User specified");
            if (messageBoxResult == MessageBoxResult.Yes || messageBoxResult == MessageBoxResult.No)
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
                        check_if_csv(i, ref fileAdresses);
                    }
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        new ImportReadIn("Stock", fileAdresses, mainWindow, false);
                    }
                    else if(messageBoxResult == MessageBoxResult.No)
                    {
                        string[] fileName = dlg.FileNames.ToList()[0].Split('\\');
                        int lastPartIndex = fileName.Length - 1; // to see which file the user immporting first
                        SpecifiedImportStock.getInstance(fileAdresses, mainWindow).setCurrentFileLabel(fileName[lastPartIndex]);
                        StoredStockColumnChecker columnChecker = new StoredStockColumnChecker();
                        columnChecker.getDataTableFromSql(mainWindow);
                        columnChecker.addDistinctBanksToCB();
                        columnChecker.setAnalyseWorksheet(dlg.FileNames.ToList()[0]);
                        columnChecker.setMostMatchesRow(columnChecker.findMostMatchingRow());
                        columnChecker.setSpecifiedImportPageTextBoxes();
                        mainWindow.MainFrame.Content = SpecifiedImportStock.getInstance(fileAdresses, mainWindow);
                    }
                }
            }
        }
        /**
        * in case if it's a csv we have to overwrite the existing filepath to the new converted excel file string
        * so we need the original list of strings
        * that's the reason why it is a reference
        */
        public void check_if_csv(int fileIndex, ref List<string> fileAddresses)
        {
            string[] fileName = fileAddresses[fileIndex].Split('\\');
            int lastPartIndex = fileName.Length - 1;
            Regex csvPattern = new Regex(@".csv$");
            if (csvPattern.IsMatch(fileName[lastPartIndex]))
            {
                string newExcelPath = fileAddresses[fileIndex].Substring(0, fileAddresses[fileIndex].Length - 4) + ".xls";

                List<List<string>> allWords = new List<List<string>>();
                List<string> all_lines = System.IO.File.ReadAllLines(fileAddresses[fileIndex], Encoding.Default).ToList();
                foreach (var lines in all_lines)
                {
                    List<string> words = lines.Split(';').ToList();
                    Regex reg = new Regex("\"([^\"]*?)\"");
                    for (int i = 0; i < words.Count; i++)
                    {
                        /**
                         * For some reason if there is a Value -> 10,1
                         * it automatically puts it in quotes  ->"10,1"
                         * we have to convert it back..
                         */
                        if (reg.IsMatch(words[i]))
                        {
                            string[] splitted = words[i].Split('"');
                            string word = "";
                            for (int j = 0; j < splitted.Length; j++)
                            {
                                word += splitted[j];
                            }
                            words[i] = word;
                        }
                    }
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
                fileAddresses[fileIndex] = newExcelPath; //overwriting the old path string
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string getMethod()
        {
            if (_lifoMethod)
                return "LIFO";
            else if (_fifoMethod)
                return "FIFO";
            else if (_customMethod)
                return "CUSTOM";
            return "LIFO";
        }
    }
}
