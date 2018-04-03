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
    /// Interaction logic for SpecifiedImportStock.xaml
    /// </summary>
    public partial class SpecifiedImportStock : Page,INotifyPropertyChanged
    {
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
        public System.Data.DataTable dataTable;
        private static SpecifiedImportStock instance;
        public static List<string> folderPath;
        public MainWindow mainWindow;
        private ButtonCommands btnCommand;
        public int numberofFile;
        private SpecifiedImportStock(MainWindow mainWindow)
        {
            numberofFile = 0;
            this.mainWindow = mainWindow;
            DataContext = this;
            InitializeComponent();
            bankChoices = new List<string>();
            bankChoices.Add("Add new Type");
        }
        public static SpecifiedImportStock getInstance(List<string> newfoldetPath,MainWindow mainWindow)
        {
            if (newfoldetPath != null)
            {
                folderPath = newfoldetPath;
            }
            if (instance == null)
            {
                instance = new SpecifiedImportStock(mainWindow);
            }
            return instance;
        }
        public ButtonCommands importPushed
        {
            get
            {
                btnCommand = new ButtonCommands(this, folderPath[numberofFile]);
                return btnCommand;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void setCurrentFileLabel(string currentFile)
        {
            currentFileLabel.Content = "File: " + currentFile;
        }
        public void incrementNumberofFile()
        {
            numberofFile++;
        }
        public int getCurrentFileIndex()
        {
            return numberofFile;
        }
        public void setDataTableFromSql(System.Data.DataTable _datatable)
        {
            dataTable = _datatable;
        }
        public class ButtonCommands : ICommand
        {
            private SpecifiedImportStock specifiedImport;
            private string currentFileName;
            public ButtonCommands(SpecifiedImportStock specifiedImport, string fileName)
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
            private void set_box_values_to_zero()
            {
                specifiedImport.transactionsRowTextBox.Text = null;
                specifiedImport.stockNameColumnTextBox.Text = null;
                specifiedImport.priceColumnTextBox.Text = null;
                specifiedImport.quantityColumnTextBox.Text = null;
                specifiedImport.dateColumnTextBox.Text = null;
                specifiedImport.transactionTypeTextBox.Text = null;
            }
            public void Execute(object parameter)
            {
                if (specifiedImport.storedTypesCB.SelectedItem.ToString() != "Add new Type" || specifiedImport.newBankTextbox.Text.ToString() != "")
                {
                    List<string> currentFile = new List<string>();
                    currentFile.Add(currentFileName);
                    new ImportReadIn("Stock", currentFile, specifiedImport.mainWindow, true);
                    if (SpecifiedImportStock.folderPath.Count < specifiedImport.getCurrentFileIndex())
                    {
                        specifiedImport.incrementNumberofFile();
                        string nextFileName = SpecifiedImportStock.folderPath[specifiedImport.getCurrentFileIndex()];
                        string[] splittedFileName = nextFileName.Split('\\');
                        int lastSplitIndex = nextFileName.Length - 1;
                        specifiedImport.currentFileLabel.Content = "File: " + splittedFileName[lastSplitIndex];
                        set_box_values_to_zero();
                        /*
                        StoredColumnChecker columnChecker = new StoredColumnChecker();
                        columnChecker.getDataTableFromSql(specifiedImport.mainWindow);
                        columnChecker.setAnalyseWorksheet(nextFileName);
                        columnChecker.setMostMatchesRow(columnChecker.findMostMatchingRow());
                        columnChecker.setSpecifiedImportPageTextBoxes();
                        */
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
            if (bankChoice == "Add new Type")
                newBankTextbox.Visibility = Visibility.Visible;
            else if(dataTable.Rows.Count>0)
            {
                newBankTextbox.Visibility = Visibility.Hidden;
                foreach (System.Data.DataRow record in dataTable.Rows)
                {
                    if (record["BankName"].ToString() == bankChoice)
                    {
                        transactionsRowTextBox.Text = record["TransStartRow"].ToString();
                        stockNameColumnTextBox.Text = record["StockName"].ToString();
                        priceColumnTextBox.Text = record["PriceColumn"].ToString();
                        quantityColumnTextBox.Text = record["QuantityColumn"].ToString();
                        dateColumnTextBox.Text = record["DateColumn"].ToString();
                        transactionTypeTextBox.Text = record["TypeColumn"].ToString();
                    }
                }
            }
        }
    }
}
