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
using WpfApp1;
using WPFCustomMessageBox;
namespace ImportProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ImportMain : Page
    {
        private ButtonCommands btnCommand;
        private MainWindow mainWindow;
        private User currentUser;
        private static ImportMain instance;
        public bool alwaysAsk
        {
            get
            {
                if (alwaysAskCB.IsChecked.Equals(true))
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
        public ImportMain(MainWindow mainWindow)
        {
            DataContext = this;
            InitializeComponent();
            neverAskCB.IsChecked = true;
            descriptionComboBox.Visibility = System.Windows.Visibility.Hidden;

            this.mainWindow = mainWindow;
            this.currentUser = mainWindow.getCurrentUser();
            if (currentUser.getAccountNumber().Equals(mainWindow.getAccounNumber()))
            {
                getUserStatistics(currentUser);
            }
        }
        private void getUserStatistics(User currentUser)
        {
            int numberOfTransactions = 0;
            int totalIncome = 0;
            int totalSpendings = 0;
            string latestImportDate = "";
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime todayDate = Convert.ToDateTime(todaysDate);
            usernameLabel.Content = currentUser.getUsername();
            foreach (var transactions in SavedTransactions.getSavedTransactionsBank())
            {
                if (transactions.getAccountNumber().Equals(currentUser.getAccountNumber()))
                {
                    numberOfTransactions++;
                    latestImportDate = transactions.getWriteDate();//always overwrites it --- todo (more logic needed lulz)
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
                }
                else
                {
                    urgencyLabel.Content = "Not urgent";
                    urgencyLabel.Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                }
            }
            else
            {
                urgencyLabel.Content = "You haven't imported yet!";
                lastImportDateLabel.Content = "You haven't imported yet!";
            }
        }
        private void getTransactions(string bankName, List<string> folderAddress)
        {
            new ImportReadIn(bankName, folderAddress, mainWindow, false);
        }
        public ButtonCommands OpenFilePushed
        {
            get
            {
                btnCommand = new ButtonCommands(FileBrowser.Content.ToString(), this);
                return btnCommand;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static ImportMain getInstance(MainWindow mainWindow)
        {
            if (instance == null)
            {
                instance = new ImportMain(mainWindow);
            }
            return instance;
        }
        public class ButtonCommands : ICommand
        {
            private string buttonContent;
            private ImportMain importPage;
            public ButtonCommands(string buttonContent, ImportMain importPage)
            {
                this.buttonContent = buttonContent;
                this.importPage = importPage;

                this.importPage.PropertyChanged += new PropertyChangedEventHandler(test_PropertyChanged);
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
                if (buttonContent.Equals("Import Transactions"))
                {
                    MessageBoxResult messageBoxResult = CustomMessageBox.ShowYesNo(
                        "\tPlease choose an import type!",
                        "Import type alert!",
                        "Automatized",
                        "User specified");

                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.DefaultExt = ".xls";
                    dlg.Filter = "Excel files (*.xls)|*.xls|Excel Files (*.xlsx)|*.xlsx|Excel Files (*.xlsm)|*.xlsm";
                    dlg.Multiselect = true;
                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                    {
                        //importPage.FolderAddressLabel.Content = dlg.FileName.;
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            importPage.getTransactions("All", dlg.FileNames.ToList());
                        }
                        else if (messageBoxResult == MessageBoxResult.No)
                        {
                            string[] fileName = dlg.FileNames.ToList()[0].Split('\\');
                            int lastPartIndex = fileName.Length - 1;
                            SpecifiedImport.getInstance(dlg.FileNames.ToList(), importPage.mainWindow).setCurrentFileLabel(fileName[lastPartIndex]);
                            //importPage.mainWindow.MainFrame.Content = SpecifiedImport.getInstance(dlg.FileNames.ToList(), importPage.mainWindow);
                        }
                    }
                }
            }
        }
    }
}
