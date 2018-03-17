using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for SpecifiedImport.xaml
    /// </summary>
    public partial class SpecifiedImportBank : Page, INotifyPropertyChanged
    {
        private static SpecifiedImportBank instance;
        public MainWindow mainWindow;
        public static List<string> folderPath;
        public int numberofFile;
        public System.Data.DataTable dataTable;
        //binding
        private ButtonCommands btnCommand;
        public List<string> accountNumberChoices { get; set; }
        public string _accountNumberChoice;
        public string accountNumberChoice
        {
            get
            {
                return _accountNumberChoice;
            }
            set
            {
                _accountNumberChoice = value;
                OnPropertyChanged("accountNumberChoice");
            }
        }
        public List<string> priceColumnChoices { get; set; }
        public string _priceColumnChoice;
        public string priceColumnChoice
        {
            get
            {
                return _priceColumnChoice;
            }
            set
            {
                _priceColumnChoice = value;
                OnPropertyChanged("priceColumnChoice");
            }
        }
        public List<string> balanceColumnChoices { get; set; }
        public string _balanceColumnChoice;
        public string balanceColumnChoice
        {
            get
            {
                return _balanceColumnChoice;
            }
            set
            {
                _balanceColumnChoice = value;
                OnPropertyChanged("balanceColumnChoice");
            }
        }
        public string commentColumnHelp { get; set; }
        public List<string> bankChoices { get; set; }
        public string _bankChoice;
        public string bankChoice
        {
            get
            {
                return _bankChoice;
            }
            set
            {
                _bankChoice = value;
                OnPropertyChanged("bankChoice");
            }
        }
        public ButtonCommands importPushed
        {
            get
            {
                btnCommand = new ButtonCommands(this, folderPath[numberofFile]);
                return btnCommand;
            }
        }
        public void setDataTableFromSql(System.Data.DataTable _datatable)
        {
            dataTable = _datatable;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged!=null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //flags
        private bool accNum_Column;
        private bool accNum_Cell;
        private bool accNum_SheetName;

        private bool priceSingleColumn;
        private bool priceMultipleColumn;

        private bool balanceColumn;
        private bool noBalanceColumn;
        private SpecifiedImportBank(MainWindow mainWindow)
        {
            numberofFile = 0;
            this.mainWindow = mainWindow;
            InitializeComponent();
            DataContext = this;

            commentColumnHelp = "Multiple comment columns can be separated by commas (i.e. A,B,C)!";
            string[] splitedFileName = folderPath[numberofFile].Split('\\');
            int lastSplitIndex = splitedFileName.Length - 1;
            currentFileLabel.Content = "File: " + splitedFileName[lastSplitIndex];
            accountNumberChoices = new List<string>();
            accountNumberChoices.Add("Column");
            accountNumberChoices.Add("Cell");
            accountNumberChoices.Add("Sheet name");
            priceColumnChoices = new List<string>();
            priceColumnChoices.Add("One column");
            priceColumnChoices.Add("Income,Spending");
            balanceColumnChoices = new List<string>();
            balanceColumnChoices.Add("Column");
            balanceColumnChoices.Add("None");
            bankChoices = new List<string>();
            bankChoices.Add("Add new Bank");
            accountNumberTextBox.Visibility = Visibility.Hidden;
            priceColumnTextBox_1.Visibility = Visibility.Hidden;
            priceColumnTextBox_2.Visibility = Visibility.Hidden;
            balanceColumnTextBox.Visibility = Visibility.Hidden;
        }
        public static SpecifiedImportBank getInstance(List<string> newfoldetPath, MainWindow mainWindow)
        {
            if (newfoldetPath != null)
            {
                folderPath = newfoldetPath;
            }
            if (instance == null)
            {
                instance = new SpecifiedImportBank(mainWindow);
            }
            return instance;
        }
        private void accountNumberCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            accountNumberTextBox.Visibility = Visibility.Hidden;
            accNum_Column = false;
            accNum_Cell = false;
            accNum_SheetName = false;
            if (accountNumberChoice == "Column")
            {
                accountNumberTextBox.Visibility = Visibility.Visible;
                accNum_Column = true;
            }
            else if (accountNumberChoice == "Cell")
            {
                accountNumberTextBox.Visibility = Visibility.Visible;
                accNum_Cell = true;
            }
            else if (accountNumberChoice == "Sheet name")
            {
                accNum_SheetName = true;
            }
        }
        public void setCurrentFileLabel(string currentFile)
        {
            currentFileLabel.Content = "File: " + currentFile;
        }
        private void priceColumnCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            priceSingleColumn = false;
            priceMultipleColumn = false;
            if (priceColumnChoice == "One column")
            {
                priceSingleColumn = true;
                priceColumnTextBox_1.Visibility = Visibility.Visible;
                priceColumnTextBox_2.Visibility = Visibility.Hidden;
            }
            else if (priceColumnChoice == "Income,Spending")
            {
                priceMultipleColumn = false;
                priceColumnTextBox_1.Visibility = Visibility.Visible;
                priceColumnTextBox_2.Visibility = Visibility.Visible;
            }
        }

        private void balanceColumnCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            balanceColumn = false;
            noBalanceColumn = false;
            if (balanceColumnChoice == "Column")
            {
                balanceColumn = true;
                balanceColumnTextBox.Visibility = Visibility.Visible;
            }
            else if (balanceColumnChoice == "None")
            {
                noBalanceColumn = true;
                balanceColumnTextBox.Visibility = Visibility.Hidden;
            }
        }
        public void incrementNumberofFile()
        {
            numberofFile++;
        }
        public int getCurrentFileIndex()
        {
            return numberofFile;
        }
        public bool getAccNum_Column()
        {
            return accNum_Column;
        }
        public bool getAccNum_Cell()
        {
            return accNum_Cell;
        }
        public bool getAccNum_SheetName()
        {
            return accNum_SheetName;
        }
        public bool getPriceSingleColumn()
        {
            return priceSingleColumn;
        }
        public bool getPriceMultipleColumn()
        {
            return priceMultipleColumn;
        }
        public bool getBalanceColumn()
        {
            return balanceColumn;
        }
        public bool getNoBalanceColumn()
        {
            return noBalanceColumn;
        }
        public class ButtonCommands : ICommand
        {
            private SpecifiedImportBank specifiedImport;
            private string currentFileName;
            public ButtonCommands(SpecifiedImportBank specifiedImport,string fileName)
            {
                this.specifiedImport = specifiedImport;
                currentFileName = fileName;
                specifiedImport.PropertyChanged += new PropertyChangedEventHandler(test_PropertyChanged);
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
            public void Execute(object parameter)
            {
                if (specifiedImport.newBankTextbox.Text.ToString() != "")
                {
                    List<string> currentFile = new List<string>();
                    currentFile.Add(currentFileName);
                    new ImportReadIn("Bank", currentFile, specifiedImport.mainWindow, true);
                    if (SpecifiedImportBank.folderPath.Count < specifiedImport.getCurrentFileIndex())
                    {
                        specifiedImport.incrementNumberofFile();
                        string nextFileName = SpecifiedImportBank.folderPath[specifiedImport.getCurrentFileIndex()];
                        string[] splittedFileName = nextFileName.Split('\\');
                        int lastSplitIndex = nextFileName.Length - 1;
                        specifiedImport.currentFileLabel.Content = "File: " + splittedFileName[lastSplitIndex];
                        StoredColumnChecker columnChecker = new StoredColumnChecker();
                        columnChecker.getDataTableFromSql(specifiedImport.mainWindow);
                        columnChecker.addDistinctBanksToCB();
                        columnChecker.setAnalyseWorksheet(nextFileName);
                        columnChecker.setMostMatchesRow(columnChecker.findMostMatchingRow());
                        columnChecker.setSpecifiedImportPageTextBoxes();
                    }
                }
                else//didn't typed in the new banks name
                {
                    MessageBox.Show("Type in the new Bank name first, to the TextBox under the Type ComboBox!");
                    specifiedImport.newBankTextbox.Focus();
                }
            }
        }

        private void storedTypesCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bankChoice == "Add new Bank")
                newBankTextbox.Visibility = Visibility.Visible;
            else
            {
                newBankTextbox.Visibility = Visibility.Hidden;
                foreach(System.Data.DataRow record in dataTable.Rows)
                {
                    if(record["BankName"].ToString()==bankChoice)
                    {
                        transactionsRowTextBox.Text = record["TransStartRow"].ToString();
                        string accountNumber = record["AccountNumberPos"].ToString();
                        if (accountNumber != "Sheet name")
                        {
                            long size = sizeof(char) * accountNumber.Length;
                            if (size > 1)
                            {
                                accountNumberChoice = "Cell";
                            }
                            else if (size == 1)
                            {
                                accountNumberChoice = "Column";
                            }
                            accountNumberTextBox.Text = accountNumber;
                        }
                        else
                        {
                            accountNumberChoice = "Sheet name";
                        }
                        dateColumnTextBox.Text = record["DateColumn"].ToString();
                        string price = record["PriceColumn"].ToString();
                        string[] splittedPrice = price.Split(',');
                        if(splittedPrice.Length==1)
                        {
                            priceColumnChoice = "One column";
                            priceColumnTextBox_1.Text = splittedPrice[0];
                        }
                        else
                        {
                            priceColumnChoice = "Income,Spending";
                            priceColumnTextBox_1.Text = splittedPrice[0];
                            priceColumnTextBox_2.Text = splittedPrice[1];
                        }
                        string balance = record["BalanceColumn"].ToString();
                        if(balance=="None")
                        {
                            balanceColumnChoice = "None";
                        }
                        else
                        {
                            balanceColumnChoice = "Column";
                            balanceColumnTextBox.Text = balance;
                        }
                        commentColumnTextBox.Text = record["CommentColumn"].ToString();
                    }
                }
            }
        }

        internal void setBoxValuesToZero()
        {
            accountNumberCB.SelectedIndex = -1;
            priceColumnCB.SelectedIndex = -1;
            balanceColumnCB.SelectedIndex = -1;
            storedTypesCB.SelectedIndex = -1;

            transactionsRowTextBox.Text = "";
            accountNumberTextBox.Text = "";
            dateColumnTextBox.Text = "";
            priceColumnTextBox_1.Text = "";
            priceColumnTextBox_2.Text = "";
            balanceColumnTextBox.Text = "";
            commentColumnTextBox.Text = "";
            newBankTextbox.Text = "";
        }
    }
}
