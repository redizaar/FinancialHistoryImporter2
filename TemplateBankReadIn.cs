using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Data.SqlClient;
using System.Data;
using WPFCustomMessageBox;
using Microsoft.VisualBasic;
using System.Data.SQLite;

namespace WpfApp1
{
    class TemplateBankReadIn
    {
        private Worksheet TransactionSheet;
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        private List<Transaction> transactions;
        private ImportReadIn bankHanlder = null;
        private int startingRow;
        private int nofColumns;
        private int pastTransactionPrice;//in case of missing Balance column..
        private bool isFirstTransaction;//in case of missing Balance column..
        private string accountNumber;
        private string accountNumberPos;
        private MainWindow mainWindow;
        private bool multipleColumn;
        private string priceColumns;
        private bool calculatedBalance;//in case of having a balance column , but it is null in some of the rows..........

        public TemplateBankReadIn(ImportReadIn importReadin, Workbook workbook, Worksheet worksheet, MainWindow mainWindow, bool userSpecified)
        {
            worksheet = workbook.Worksheets[1];
            this.bankHanlder = importReadin;
            this.mainWindow = mainWindow;
            transactions = new List<Transaction>();
            this.TransactionSheet = worksheet;
            //kiolvasás milyen banktól van
            if (!userSpecified)
            {
                this.multipleColumn = false;
                this.isFirstTransaction = false;
                this.calculatedBalance = false;
                getTransactionRows();
            }
        }
        private void getTransactionRows()
        {
            this.accountNumber = "";
            Regex accountNumberRegex1 = new Regex(@"^Számlaszám$");
            Regex accountNumberRegex2 = new Regex(@"^Könyvelési számla$");
            Regex accountNumberRegex3 = new Regex(@"^Számlaszám:$");
            Regex accountNumberRegex4 = new Regex(@"^\d{8}-\d{8}");
            Regex accountNumberRegex5 = new Regex(@"\d{8}-\d{8}-\d{8}");
            int blank_row = 0;
            int blank_cells = 0;
            int i = 1;


            int maxColumns = 1;
            int transactionsStartRow = 1;
            while (blank_row < 5)
            {
                int column = 1;
                if (TransactionSheet.Cells[i, column].Value != null)
                {
                    if (this.accountNumber == "")
                    {
                        if ((column == 1) || (column == 2) || (column==3))
                        {
                            string cellValue = TransactionSheet.Cells[i, column].Value.ToString();
                            if (accountNumberRegex1.IsMatch(cellValue) ||
                                accountNumberRegex2.IsMatch(cellValue) ||
                                accountNumberRegex3.IsMatch(cellValue))
                            {
                                string accountNumberValue = TransactionSheet.Cells[i, column + 1].Value.ToString();//the cell next to it
                                setAccountNumber(accountNumberValue);
                                //only in one cell i.e. B3
                                char c1 = 'A';
                                for (int k = 1; k < column+1; k++)
                                    c1++;
                                accountNumberPos = c1 + i.ToString();
                            }
                            else if(accountNumberRegex4.IsMatch(cellValue) ||
                                    accountNumberRegex5.IsMatch(cellValue))
                            {
                                char c1 = 'A';
                                for (int k = 1; k < column; k++)
                                    c1++;
                                accountNumberPos = c1 + i.ToString();
                            }
                        }
                    }
                    blank_cells = 0;
                    while (blank_cells < 3)
                    {
                        if (TransactionSheet.Cells[i, column].Value != null)
                        {
                            column++;
                            blank_cells = 0;
                        }
                        else
                        {
                            column++;
                            blank_cells++;
                        }
                    }
                    blank_row = 0;
                }
                else
                {
                    blank_row++;
                }
                if (column > maxColumns)
                {
                    maxColumns = column;
                    transactionsStartRow = i;
                    if (this.accountNumber == "")
                    {
                        for (int j = 1; j < column; j++)
                        {
                            if (TransactionSheet.Cells[i, j].Value != null)
                            {
                                string cellValue = TransactionSheet.Cells[i, j].Value.ToString();
                                if (accountNumberRegex1.IsMatch(cellValue) ||
                                    accountNumberRegex2.IsMatch(cellValue) ||
                                    accountNumberRegex3.IsMatch(cellValue))
                                {
                                    string accountNumberValue = TransactionSheet.Cells[i + 1, j].Value.ToString();//the cell below it
                                    setAccountNumber(accountNumberValue);
                                    //it's in every transaction in a column
                                        accountNumberPos = j.ToString();
                                }
                                else if(accountNumberRegex4.IsMatch(cellValue) ||
                                        accountNumberRegex5.IsMatch(cellValue))
                                {
                                    string accountNumberValue = TransactionSheet.Cells[i, j].Value.ToString();
                                        accountNumberPos = j.ToString();
                                }
                            }
                        }
                    }
                }
                i++;
            }
            if (this.accountNumber == "")
            {
                string sheetname = TransactionSheet.Name;
                if (accountNumberRegex4.IsMatch(sheetname) || accountNumberRegex5.IsMatch(sheetname))
                {
                    accountNumber = TransactionSheet.Name;
                    accountNumberPos = "Sheet name";
                }
                else
                {
                    accountNumber = "?";
                }
            }
            setStartingRow(transactionsStartRow);
            setNofColumns(maxColumns - blank_cells);
        }

        public void readOutTransactionColumns(int row, int maxColumn)
        {
            List<int> descriptionColumn = getDescriptionColumn(row, maxColumn);
            int dateColumn = getDateColumn(row, maxColumn);
            string pricecolumn = isMultiplePriceColumn(row, maxColumn);
            if (pricecolumn != null)
            {
                int singlepriceColumn = -1;
                try
                {
                    singlepriceColumn = int.Parse(pricecolumn);
                }
                catch (Exception e)
                {

                }
                if (singlepriceColumn == -1)
                {
                    this.multipleColumn = true;
                    priceColumns = pricecolumn;
                }
                int balaceColumn = getAccountBalanceColumn(row, maxColumn);
                readOutTransactions(row, maxColumn, dateColumn, singlepriceColumn, balaceColumn, descriptionColumn);
            }
            else
            {
                MessageBox.Show("Couldn't find the price columns, please use Specified Import on this file: !"+bankHanlder.getCurrentFileName());
            }
        }

        private List<int> getDescriptionColumn(int row, int maxColumn)
        {
            Regex descrRegex1 = new Regex(@"^Közlemény$");
            Regex descrRegex2 = new Regex(@"típusa$");
            Regex descrRegex3 = new Regex(@"^Típus$");
            Regex descrRegex4 = new Regex(@"Leírás$");

            List<int> descrColumns = new List<int>();
            List<string> descrColumnNames = new List<string>();
            if (row != 1)
            {
                for (int i = row - 1; i <= row + 2; i++)
                {
                    for (int j = 1; j < maxColumn; j++)
                    {
                        if (TransactionSheet.Cells[i, j].Value != null)
                        {
                            string inputData = TransactionSheet.Cells[i, j].Value.ToString();
                            if (descrRegex1.IsMatch(inputData) || descrRegex2.IsMatch(inputData) ||
                                descrRegex3.IsMatch(inputData) || descrRegex4.IsMatch(inputData))
                            {
                                descrColumns.Add(j);
                                descrColumnNames.Add(inputData);
                            }
                        }
                    }
                }
            }
            else//colum titles first row
            {
                for (int j = 1; j < maxColumn; j++)
                {
                    if (TransactionSheet.Cells[row, j].Value != null)
                    {
                        string inputData = TransactionSheet.Cells[row, j].Value.ToString();
                        if (descrRegex1.IsMatch(inputData) || descrRegex2.IsMatch(inputData) ||
                                descrRegex3.IsMatch(inputData) || descrRegex4.IsMatch(inputData))
                        {
                            descrColumns.Add(j);
                            descrColumnNames.Add(inputData);
                        }
                    }
                }
            }
            return descrColumns;
        }

        private void readOutTransactions(int row, int maxColumn, int dateColumn, int singlepriceColumn, int balaceColumn, List<int> descriptionColumns)
        {
            int startingRowReference = row;
            if (row == 1)
            {
                row++;
            }
            else
            {
                Regex dateRegex1 = new Regex(@"^20\d{2}.\d{2}.\d{2}");
                Regex dateRegex2 = new Regex(@"^20\d{2}-\d{2}-\d{2}");
                Regex dateRegex3 = new Regex(@"^20\d{2}.\s\d{2}.\s\d{2}");
                bool titleRow = true;
                for (int j = 1; j < maxColumn; j++)
                {
                    if (TransactionSheet.Cells[row, j].Value != null)
                    {
                        string inputdata = TransactionSheet.Cells[row, j].Value.ToString();
                        if ((dateRegex1.IsMatch(inputdata) || dateRegex2.IsMatch(inputdata) || dateRegex3.IsMatch(inputdata)))
                        {
                            titleRow = false;
                            break;
                        }
                    }
                }
                if (titleRow)
                {
                    row++;
                }
            }
            if (singlepriceColumn != -1)//single column
            {
                int blank_counter = 0;
                List<Transaction> transaction = new List<Transaction>();
                while (blank_counter < 2)
                {
                    if (balaceColumn != -1)//have balance column
                    {
                        if (TransactionSheet.Cells[row, dateColumn].Value != null && TransactionSheet.Cells[row, singlepriceColumn].Value != null)
                        {
                            blank_counter = 0;

                            string transactionDate = "";
                            string tempTransactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                            string[] splittedDate = tempTransactionDate.Split(' ');
                            if (splittedDate.Length == 1)
                            {
                                transactionDate = tempTransactionDate;
                            }
                            else
                            {
                                for (int j = 0; j < splittedDate.Length - 1; j++)
                                {
                                    if (j < 3)
                                        transactionDate += splittedDate[j];
                                }
                            }
                            string accountNumber = getAccountNumber();
                            string transactionPriceString = TransactionSheet.Cells[row, singlepriceColumn].Value.ToString();
                            string transactionBalanceString = TransactionSheet.Cells[row, balaceColumn].Value.ToString();

                            string transactionDescription = "-";
                            for (int i = 0; i < descriptionColumns.Count; i++)
                            {
                                if (TransactionSheet.Cells[row, descriptionColumns[i]].Value != null)
                                {
                                    if (transactionDescription == "-")
                                        transactionDescription = TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                    else
                                        transactionDescription += " , " + TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                }
                            }
                            int transactionPrice = 0;
                            int transactionBalance = 0;
                            try
                            {
                                transactionPrice = int.Parse(transactionPriceString);
                                transactionBalance = int.Parse(transactionBalanceString);
                            }
                            catch (Exception e)
                            {

                            }
                            transaction.Add(new Transaction(transactionBalance, transactionDate, transactionPrice, transactionDescription, accountNumber));
                        }
                        else
                        {
                            blank_counter++;
                        }
                    }
                    else//don't have balance column
                    {
                        if (TransactionSheet.Cells[row, dateColumn].Value != null && TransactionSheet.Cells[row, singlepriceColumn].Value != null)
                        {
                            blank_counter = 0;

                            string transactionDate = "";
                            string tempTransactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                            string[] splittedDate = tempTransactionDate.Split(' ');
                            if (splittedDate.Length == 1)
                            {
                                transactionDate = tempTransactionDate;
                            }
                            else
                            {
                                for (int j = 0; j < splittedDate.Length - 1; j++)
                                {
                                    if (j < 3)
                                        transactionDate += splittedDate[j];
                                }
                            }
                            string accountNumber = getAccountNumber();
                            string transactionPriceString = TransactionSheet.Cells[row, singlepriceColumn].Value.ToString();
                            string transactionDescription = "-";
                            for (int i = 0; i < descriptionColumns.Count; i++)
                            {
                                if (TransactionSheet.Cells[row, descriptionColumns[i]].Value != null)
                                {
                                    if (transactionDescription == "-")
                                        transactionDescription = TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                    else
                                        transactionDescription += " , " + TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                }
                            }
                            int transactionPrice = 0;
                            try
                            {
                                transactionPrice = int.Parse(transactionPriceString);
                            }
                            catch (Exception e)
                            {

                            }
                            transaction.Add(new Transaction("-", transactionDate, transactionPrice, transactionDescription, accountNumber));
                        }
                        else
                        {
                            blank_counter++;
                        }
                    }
                    row++;
                }
                //singlePricecolumn
                string bankName = getBankNameFromStoredData(startingRowReference, accountNumberPos, dateColumn, singlepriceColumn, balaceColumn, descriptionColumns);
                foreach (var tempTransaction in transaction)
                    tempTransaction.setBankname(bankName);
                bankHanlder.addTransactions(transaction);
            }
            else//multiple price columns
            {
                Regex priceRegex1 = new Regex(@"^Terhelés$");
                Regex priceRegex2 = new Regex(@"^Jóváírás$");
                int costPriceColumn = 0;
                int incomePriceColumn = 0;
                for (int i = row - 1; i < row + 1; i++)//a row!=1 azt már lekezeltük
                {
                    for (int j = 1; j < maxColumn; j++)
                    {
                        if (TransactionSheet.Cells[i, j].Value != null)
                        {
                            string cellValue = TransactionSheet.Cells[i, j].Value.ToString();
                            if (priceRegex1.IsMatch(cellValue))
                            {
                                costPriceColumn = j;
                            }
                            if (priceRegex2.IsMatch(cellValue))
                            {
                                incomePriceColumn = j;
                            }
                        }
                    }
                }
                if ((costPriceColumn != 0) && (incomePriceColumn != 0))
                {
                    int blank_counter = 0;
                    List<Transaction> transaction = new List<Transaction>();
                    while (blank_counter < 2)
                    {
                        if (balaceColumn != -1)//have balance column
                        {
                            if ((TransactionSheet.Cells[row, dateColumn].Value != null) &&
                                TransactionSheet.Cells[row, costPriceColumn].Value != null ||
                                TransactionSheet.Cells[row, incomePriceColumn].Value != null)
                            {
                                blank_counter = 0;

                                string transactionDate = "";
                                string tempTransactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                                string[] splittedDate = tempTransactionDate.Split(' ');
                                if (splittedDate.Length == 1)
                                {
                                    transactionDate = tempTransactionDate;
                                }
                                else
                                {
                                    for (int j = 0; j < splittedDate.Length - 1; j++)
                                    {
                                        if (j < 3)
                                            transactionDate += splittedDate[j];
                                    }
                                }
                                string accountNumber = getAccountNumber();

                                string incomePriceString = "";
                                string costPriceString = "";
                                int tempRow = 0;
                                int incomePrice = 0;
                                int costPrice = 0;
                                if (TransactionSheet.Cells[row, incomePriceColumn].Value != null)
                                {
                                    incomePriceString = TransactionSheet.Cells[row, incomePriceColumn].Value.ToString();
                                    incomePrice = int.Parse(incomePriceString);
                                }
                                else if (TransactionSheet.Cells[row, costPriceColumn].Value != null)
                                {
                                    costPriceString = TransactionSheet.Cells[row, costPriceColumn].Value.ToString();
                                    costPrice = int.Parse(costPriceString);
                                }
                                string transactionDescription = "-";
                                for (int i = 0; i < descriptionColumns.Count; i++)
                                {
                                    if (TransactionSheet.Cells[row, descriptionColumns[i]].Value != null)
                                    {
                                        if (transactionDescription == "-")
                                            transactionDescription = TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                        else
                                            transactionDescription += " , " + TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                    }
                                }
                                string transactionBalanceString = "";
                                int transactionBalance = 0;
                                int calcuatedBalance = 0;
                                if (TransactionSheet.Cells[row, balaceColumn].Value != null)
                                {
                                    setCalculatedBalance(false);
                                    transactionBalanceString = TransactionSheet.Cells[row, balaceColumn].Value.ToString();
                                    transactionBalance = int.Parse(transactionBalanceString);
                                }
                                else
                                {

                                    setCalculatedBalance(true);
                                    tempRow = row;
                                    while (TransactionSheet.Cells[tempRow, balaceColumn].Value == null)
                                    {
                                        tempRow++;
                                    }
                                    transactionBalanceString = TransactionSheet.Cells[tempRow, balaceColumn].Value.ToString();
                                    transactionBalance = int.Parse(transactionBalanceString);
                                    calcuatedBalance = calculatePastBalance(transactionBalance, row, tempRow, costPriceColumn, incomePriceColumn);
                                }
                                if (getCalculatedBalance())
                                {
                                    if (incomePrice != 0)
                                    {
                                        transaction.Add(new Transaction(calcuatedBalance, transactionDate, incomePrice, transactionDescription, accountNumber));
                                    }
                                    else if (costPrice != 0)
                                    {
                                        transaction.Add(new Transaction(calcuatedBalance, transactionDate, costPrice, transactionDescription, accountNumber));
                                    }
                                }
                                else
                                {
                                    if (incomePrice != 0)
                                    {
                                        transaction.Add(new Transaction(transactionBalance, transactionDate, incomePrice, transactionDescription, accountNumber));
                                    }
                                    else if (costPrice != 0)
                                    {
                                        transaction.Add(new Transaction(transactionBalance, transactionDate, costPrice, transactionDescription, accountNumber));
                                    }
                                }
                            }
                            else
                            {
                                blank_counter++;
                            }
                        }
                        else//don't have balance column
                        {
                            if ((TransactionSheet.Cells[row, dateColumn].Value != null) &&
                                TransactionSheet.Cells[row, costPriceColumn].Value != null ||
                                TransactionSheet.Cells[row, incomePriceColumn].Value != null)
                            {
                                blank_counter = 0;

                                string transactionDate = "";
                                string tempTransactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                                string[] splittedDate = tempTransactionDate.Split(' ');
                                if (splittedDate.Length == 1)
                                {
                                    transactionDate = tempTransactionDate;
                                }
                                else
                                {
                                    for (int j = 0; j < splittedDate.Length - 1; j++)
                                    {
                                        if (j < 3)
                                            transactionDate += splittedDate[j];
                                    }
                                }
                                string accountNumber = getAccountNumber();

                                string incomePriceString = "";
                                string costPriceString = "";
                                if (TransactionSheet.Cells[row, incomePriceColumn].Value != null)
                                {
                                    incomePriceString = TransactionSheet.Cells[row, incomePriceColumn].Value.ToString();
                                }
                                else if (TransactionSheet.Cells[row, costPriceColumn].Value != null)
                                {
                                    costPriceString = TransactionSheet.Cells[row, costPriceColumn].Value.ToString();

                                }

                                int incomePrice = 0;
                                int costPrice = 0;
                                try
                                {
                                    incomePrice = int.Parse(incomePriceString);
                                }
                                catch (Exception e)
                                {

                                }
                                try
                                {
                                    costPrice = int.Parse(costPriceString) * (-1);
                                }
                                catch (Exception e)
                                {

                                }
                                string transactionDescription = "-";
                                for (int i = 0; i < descriptionColumns.Count; i++)
                                {
                                    if (TransactionSheet.Cells[row, descriptionColumns[i]].Value != null)
                                    {
                                        if (transactionDescription == "-")
                                            transactionDescription = TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                        else
                                            transactionDescription += " , " + TransactionSheet.Cells[row, descriptionColumns[i]].Value.ToString();
                                    }
                                }
                                transaction.Add(new Transaction("-", transactionDate, incomePrice, transactionDescription, accountNumber));
                            }
                            else
                            {
                                blank_counter++;
                            }
                        }
                        row++;
                    }
                    //multiple priceColumns
                    string bankName = getBankNameFromStoredData(row, accountNumberPos, dateColumn, priceColumns, balaceColumn, descriptionColumns);
                    foreach (var tempTransaction in transaction)
                        tempTransaction.setBankname(bankName);
                    bankHanlder.addTransactions(transaction);
                }
                else
                {
                    Console.WriteLine("Couldn't locate the price columns");
                }
            }
        }
        //singlePriceColumn
        private string getBankNameFromStoredData(int startingRow, string accountNumberPos, int dateColumn, int singlepriceColumn, int balanceColumn, List<int> descriptionColumns)
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StoredColumnsBank] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'BankName' TEXT, 'TransStartRow' INTEGER, " +
                        "'AccountNumberPos' TEXT, 'DateColumn' TEXT, 'PriceColumn' TEXT, 'BalanceColumn' TEXT, " +
                        "'CommentColumn' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string storedQuery = "select * from [StoredColumnsBank]";
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            mConn.Close();
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select * From [StoredColumns]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            */
            int maxMatch = 0;
            DataRow mostMatchingrow = null;
            foreach (DataRow row in dtb.Rows)
            {
                int matchCounter = 0;
                int transactionsRow = int.Parse(row["TransStartRow"].ToString());
                string accountNumberPosString = row["AccountNumberPos"].ToString();
                string dateColumnString = row["DateColumn"].ToString();
                string priceColumnString = row["PriceColumn"].ToString();
                string balanceColumnString = row["BalanceColumn"].ToString();
                string commentColumnString = row["CommentColumn"].ToString();
                if (transactionsRow == startingRow)
                    matchCounter++;
                if (accountNumberPosString == accountNumberPos)
                    matchCounter++;
                if (ExcelColumnNameToNumber(dateColumnString) == dateColumn)
                    matchCounter++;
                if (balanceColumn != -1)
                {
                    if (ExcelColumnNameToNumber(balanceColumnString) == balanceColumn)
                        matchCounter++;
                }
                else
                {
                    if (balanceColumnString == "None")
                        matchCounter++;
                }
                if (ExcelColumnNameToNumber(priceColumnString) == singlepriceColumn)
                {
                    matchCounter++;
                }
                string[] splittedComment = commentColumnString.Split(',');
                for (int i = 0; i < descriptionColumns.Count; i++)
                {
                    for (int j = 0; j < splittedComment.Length; j++)
                    {
                        if (descriptionColumns[i] == ExcelColumnNameToNumber(splittedComment[j]))
                        {
                            matchCounter++;
                            break;
                        }
                    }
                }
                if (matchCounter > maxMatch)
                {
                    maxMatch = matchCounter;
                    mostMatchingrow = row;
                }
            }
            //Columns-1 because we don't count the BankName column, (we are looking for that)
            if (dtb.Columns.Count-1 == maxMatch)
                return mostMatchingrow["BankName"].ToString();
            else if (mostMatchingrow != null)
            {
                if (((((dtb.Columns.Count) - 2) == maxMatch) || ((dtb.Columns.Count) - 3) == maxMatch))
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Is this import file from " + mostMatchingrow["BankName"].ToString() + " ?",
                        "Filename: "+bankHanlder.getCurrentFileName(), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        return mostMatchingrow["BankName"].ToString();
                    }
                    else
                    {
                        string input = "";
                        while (input == "")
                        {
                            input = Interaction.InputBox("Please type in the Bank name!", "", "");
                        }
                        string commentColumns = "";
                        for (int j = 0; j < descriptionColumns.Count; j++)
                        {
                            if (j == 0)
                                commentColumns = descriptionColumns[j].ToString();
                            else
                                commentColumns += "," + descriptionColumns[j].ToString();
                        }
                        writeNewRecordToSql(input, startingRow, accountNumberPos, dateColumn, singlepriceColumn, balanceColumn, commentColumns);
                        return input;
                    }
                }
                else
                {
                    string input = "";
                    while (input == "")
                    {
                        input = Interaction.InputBox("Please type in the Bank name!", "", "");
                    }
                    string commentColumns = "";
                    for (int j = 0; j < descriptionColumns.Count; j++)
                    {
                        if (j == 0)
                            commentColumns = ExcelColumnFromNumber(descriptionColumns[j]);
                        else
                            commentColumns += "," + ExcelColumnFromNumber(descriptionColumns[j]);
                    }
                    writeNewRecordToSql(input, startingRow, accountNumberPos, dateColumn, singlepriceColumn, balanceColumn, commentColumns);
                    return input;
                }
            }
            return "unkown";
        }
        //multiplePricecolumn
        private string getBankNameFromStoredData(int startingRow, string accountNumberPos, int dateColumn, string priceColumns, int balanceColumn, List<int> descriptionColumns)
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StoredColumnsBank] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'BankName' TEXT, 'TransStartRow' INTEGER, " +
                        "'AccountNumberPos' TEXT, 'DateColumn' TEXT, 'PriceColumn' TEXT, 'BalanceColumn' TEXT, " +
                        "'CommentColumn' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string storedQuery = "select * from [StoredColumnsBank]";
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            mConn.Close();
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select * From [StoredColumns]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            */
            int maxMatch = 0;
            DataRow mostMatchingrow = null;
            string[] splittedPriceColumns = priceColumns.Split(',');
            foreach (DataRow row in dtb.Rows)
            {
                int matchCounter = 0;
                int transactionsRow = int.Parse(row["TransStartRow"].ToString());
                string accountNumberPosString = row["AccountNumberPos"].ToString();
                string dateColumnString = row["DateColumn"].ToString();
                string priceColumnString = row["PriceColumn"].ToString();
                string balanceColumnString = row["BalanceColumn"].ToString();
                string commentColumnString = row["CommentColumn"].ToString();
                if (transactionsRow == startingRow)
                    matchCounter++;
                if (accountNumberPosString == accountNumberPos)
                    matchCounter++;
                if (ExcelColumnNameToNumber(dateColumnString) == dateColumn)
                    matchCounter++;
                if (balanceColumn != -1)
                {
                    if (ExcelColumnNameToNumber(balanceColumnString) == balanceColumn)
                        matchCounter++;
                }
                else
                {
                    if (balanceColumnString == "None")
                        matchCounter++;
                }
                for (int i = 0; i < splittedPriceColumns.Length; i++)
                {
                    char c1 = 'A';
                    for (int j = 1; j < int.Parse(splittedPriceColumns[i]); j++)
                        c1++;
                    string[] splittedStoredPriceColumns = priceColumnString.Split(',');
                    for (int j = 0; j < splittedStoredPriceColumns.Length; j++)
                    {
                        if (splittedStoredPriceColumns[j] == c1.ToString())
                        {
                            matchCounter++;
                            break;
                        }
                    }
                }
                string[] splittedComment = commentColumnString.Split(',');
                for (int i = 0; i < descriptionColumns.Count; i++)
                {
                    for (int j = 0; j < splittedComment.Length; j++)
                    {
                        if (descriptionColumns[i] == ExcelColumnNameToNumber(splittedComment[j]))
                        {
                            matchCounter++;
                            break;
                        }
                    }
                }
                if (matchCounter > maxMatch)
                {
                    maxMatch = matchCounter;
                    mostMatchingrow = row;
                }
            }
            if (dtb.Columns.Count-1 == maxMatch)
                return mostMatchingrow["BankName"].ToString();
            else if (((((dtb.Columns.Count) - 2) == maxMatch) || ((dtb.Columns.Count) - 3) == maxMatch))
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Is this import file from " + mostMatchingrow["BankName"].ToString() + " ?",
                    "Filename: "+bankHanlder.getCurrentFileName(), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    return mostMatchingrow["BankName"].ToString();
                }
                else
                {
                    string input = "";
                    while (input == "")
                    {
                        input=Interaction.InputBox("Please type in the Bank name!", "", "");
                    }
                    string commentColumns = "";
                    for (int j = 0; j < descriptionColumns.Count; j++)
                    {
                        if (j == 0)
                            commentColumns = descriptionColumns[j].ToString();
                        else
                            commentColumns += "," + descriptionColumns[j].ToString();
                    }
                    writeNewRecordToSql(input, startingRow, accountNumberPos, dateColumn, priceColumns, balanceColumn, commentColumns);
                    return input;
                }
            }
            else
            {
                string input = "";
                while (input == "")
                {
                    input=Interaction.InputBox("Please type in the Bank name!", "", "");
                }
                string commentColumns = "";
                for (int j = 0; j < descriptionColumns.Count; j++)
                {
                    if (j == 0)
                        commentColumns = ExcelColumnFromNumber(descriptionColumns[j]);
                    else
                        commentColumns += "," + ExcelColumnFromNumber(descriptionColumns[j]);
                }
                writeNewRecordToSql(input, startingRow, accountNumberPos, dateColumn, priceColumns, balanceColumn, commentColumns);
                return input;
            }
        }
        private void writeNewRecordToSql(string input, int startingRow, string accountNumberPos, int dateColumn, int singlepriceColumn, int balanceColumn, string commentColumns)
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StoredColumnsBank] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'BankName' TEXT, 'TransStartRow' INTEGER, " +
                        "'AccountNumberPos' TEXT, 'DateColumn' TEXT, 'PriceColumn' TEXT, 'BalanceColumn' TEXT, " +
                        "'CommentColumn' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("insertNewColumns3", sqlConn);//SQLQuery 8
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@bankName", input);
            sqlCommand.Parameters.AddWithValue("@transStartRow", startingRow);
            try
            {
                int priceColumn = int.Parse(accountNumberPos);
                sqlCommand.Parameters.AddWithValue("@accountNumberPos", ExcelColumnFromNumber(priceColumn));
            }
            catch(Exception e)
            {
                //it's a cell
                sqlCommand.Parameters.AddWithValue("@accountNumberPos", accountNumberPos);
            }
            sqlCommand.Parameters.AddWithValue("@dateColumn", ExcelColumnFromNumber(dateColumn));
            sqlCommand.Parameters.AddWithValue("@priceColumn", ExcelColumnFromNumber(singlepriceColumn));
            if (balanceColumn!=-1)
                sqlCommand.Parameters.AddWithValue("@balanceColumn", ExcelColumnFromNumber(balanceColumn));
            else
                sqlCommand.Parameters.AddWithValue("@balanceColumn", "None");
            sqlCommand.Parameters.AddWithValue("@commentColumn", commentColumns);
            sqlCommand.ExecuteNonQuery();
            */
            string insertQuery = "insert into [StoredColumnsBank](BankName, TransStartRow, AccountNumberPos,DateColumn,PriceColumn,BalanceColumn,CommentColumn) " +
                        "values('" + input + "','" + startingRow +"'";
            try
            {
                int priceColumn = int.Parse(accountNumberPos);
                insertQuery+=",'"+ExcelColumnFromNumber(priceColumn)+"'";
            }
            catch (Exception e)
            {
                //it's a cell
                insertQuery+=",'"+accountNumberPos+"'";
            }
            insertQuery += ",'" + ExcelColumnFromNumber(dateColumn) + "','" + ExcelColumnFromNumber(singlepriceColumn) + "'";
            if (balanceColumn != -1)
                insertQuery+=",'"+ExcelColumnFromNumber(balanceColumn)+"'";
            else
                insertQuery+=",'None'";
            insertQuery+=",'"+commentColumns+"')";
            SQLiteCommand insercommand = new SQLiteCommand(insertQuery, mConn);
            insercommand.CommandType = CommandType.Text;
            insercommand.ExecuteNonQuery();
            mConn.Close();
        }
        private void writeNewRecordToSql(string input, int startingRow, string accountNumberPos, int dateColumn, string priceColumns, int balanceColumn, string commentColumns)
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StoredColumnsBank] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'BankName' TEXT, 'TransStartRow' INTEGER, " +
                        "'AccountNumberPos' TEXT, 'DateColumn' TEXT, 'PriceColumn' TEXT, 'BalanceColumn' TEXT, " +
                        "'CommentColumn' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("insertNewColumns3", sqlConn);//SQLQuery 8
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@bankName", input);
            sqlCommand.Parameters.AddWithValue("@transStartRow", startingRow);
            try
            {
                int priceColumn = int.Parse(accountNumberPos);
                sqlCommand.Parameters.AddWithValue("@accountNumberPos", ExcelColumnFromNumber(priceColumn));
            }
            catch (Exception e)
            {
                //it's a cell
                sqlCommand.Parameters.AddWithValue("@accountNumberPos", accountNumberPos);
            }
            sqlCommand.Parameters.AddWithValue("@dateColumn", ExcelColumnFromNumber(dateColumn));
            sqlCommand.Parameters.AddWithValue("@priceColumn", priceColumns);
            if (balanceColumn != -1)
                sqlCommand.Parameters.AddWithValue("@balanceColumn", ExcelColumnFromNumber(balanceColumn));
            else
                sqlCommand.Parameters.AddWithValue("@balanceColumn", "None");
            sqlCommand.Parameters.AddWithValue("@commentColumn", commentColumns);
            sqlCommand.ExecuteNonQuery();
            */
            string insertQuery = "insert into [StoredColumnsBank](BankName, TransStartRow, AccountNumberPos,DateColumn,PriceColumn,BalanceColumn,CommentColumn) " +
                        "values('" + input + "','" + startingRow + "'";
            try
            {
                int priceColumn = int.Parse(accountNumberPos);
                insertQuery += ",'" + ExcelColumnFromNumber(priceColumn) + "'";
            }
            catch (Exception e)
            {
                //it's a cell
                insertQuery += ",'" + accountNumberPos + "'";
            }
            insertQuery += ",'" + ExcelColumnFromNumber(dateColumn) + "','" + priceColumns + "'";
            if (balanceColumn != -1)
                insertQuery += ",'" + ExcelColumnFromNumber(balanceColumn) + "'";
            else
                insertQuery += ",'None'";
            insertQuery += ",'" + commentColumns + "')";
            SQLiteCommand insercommand = new SQLiteCommand(insertQuery, mConn);
            insercommand.CommandType = CommandType.Text;
            insercommand.ExecuteNonQuery();
            mConn.Close();
        }
        /**
        * 1. az új utolsó balance cella értéke ami már nem volt null
        * 2. az aktuális sor ahol tartunk(ahol null a balance cella)
        * 3.az utolsó sor ahol volt értéke a balance cellának
        * 4.a terhelés cella
        * 5.a jövedelem cella
        * 
        * return value : the right balance value
        * */
        private int calculatePastBalance(int transactionBalance, int row, int tempRow, int costPriceColumn, int incomePriceColumn)
        {
            tempRow--;//we are currently at a cell where we have a balance value
            //so we go up
            while (tempRow != row - 1)
            {
                if (TransactionSheet.Cells[tempRow, costPriceColumn].Value != null)
                {
                    string costPriceString = TransactionSheet.Cells[tempRow, costPriceColumn].Value.ToString();
                    int costPrice = int.Parse(costPriceString) * (-1);
                    transactionBalance += costPrice;
                }
                else if (TransactionSheet.Cells[tempRow, incomePriceColumn].Value != null)
                {
                    string incomePriceString = TransactionSheet.Cells[tempRow, incomePriceColumn].Value.ToString();
                    int incomePrice = int.Parse(incomePriceString);
                    transactionBalance += incomePrice;
                }
                tempRow--;
            }
            return transactionBalance;
        }

        private int getAccountBalanceColumn(int row, int maxColumn)
        {
            Regex balanceRegex1 = new Regex(@"^Egyenleg$");
            Regex balanceRegex2 = new Regex(@"könyvelt egyenleg$");
            Regex balanceRegex3 = new Regex(@"^Számlaegyenleg$");

            if (row != 1)
            {
                for (int i = row - 1; i <= row + 2; i++)
                {
                    for (int j = 1; j < maxColumn; j++)
                    {
                        if (TransactionSheet.Cells[i, j].Value != null)
                        {
                            string inputData = TransactionSheet.Cells[i, j].Value.ToString();
                            if (balanceRegex1.IsMatch(inputData) || balanceRegex2.IsMatch(inputData) ||
                                balanceRegex3.IsMatch(inputData))
                            {
                                return j;
                            }
                        }
                    }
                }
            }
            else//colum titles first row
            {
                for (int j = 1; j < maxColumn; j++)
                {
                    if (TransactionSheet.Cells[row, j].Value != null)
                    {
                        string inputData = TransactionSheet.Cells[row, j].Value.ToString();
                        if (balanceRegex1.IsMatch(inputData) || balanceRegex2.IsMatch(inputData) ||
                            balanceRegex3.IsMatch(inputData))
                        {
                            return j;
                        }
                    }
                }
            }
            return -1;
        }

        private string isMultiplePriceColumn(int row, int maxColumn)
        {
            Console.WriteLine(row+" és "+maxColumn);
            Regex priceRegex1 = new Regex(@"Összeg");
            Regex priceRegex2 = new Regex(@"összeg");
            Regex priceRegex3 = new Regex(@"Terhelés$");
            Regex priceRegex4 = new Regex(@"Jóváírás$");
            if (row != 1)
            {
                for (int i = row - 1; i <= row + 2; i++)
                {
                    for (int j = 1; j < maxColumn; j++)
                    {
                        if (TransactionSheet.Cells[i, j].Value != null)
                        {
                            string inputData = TransactionSheet.Cells[i, j].Value.ToString();
                            if (priceRegex1.IsMatch(inputData) || priceRegex2.IsMatch(inputData))
                            {
                                return j.ToString();
                            }
                            else if (priceRegex3.IsMatch(inputData))
                            {
                                if (TransactionSheet.Cells[i, j + 2].Value != null)
                                {
                                    string inputData2 = TransactionSheet.Cells[i, j + 2].Value.ToString();
                                    if (priceRegex4.IsMatch(inputData2))
                                    {
                                        int column2 = j + 2;
                                        return j + "," + column2;
                                    }
                                }
                                if (TransactionSheet.Cells[i, j - 2].Value != null)
                                {
                                    string inputData2 = TransactionSheet.Cells[i, j - 2].Value.ToString();
                                    if (priceRegex4.IsMatch(inputData2))
                                    {
                                        int column2 = j - 2;
                                        return column2 + "," + j;
                                    }
                                }
                                if (TransactionSheet.Cells[i, j + 1].Value != null)
                                {
                                    string inputData2 = TransactionSheet.Cells[i, j + 1].Value.ToString();
                                    if (priceRegex4.IsMatch(inputData2))
                                    {
                                        int column2 = j + 1;
                                        return j + "," + column2;
                                    }
                                }
                                if (TransactionSheet.Cells[i, j - 1].Value != null)
                                {
                                    string inputData2 = TransactionSheet.Cells[i, j - 1].Value.ToString();
                                    if (priceRegex4.IsMatch(inputData2))
                                    {
                                        int column2 = j - 1;
                                        return column2 + "," + j;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else//colum titles first row
            {
                for (int j = 1; j < maxColumn; j++)
                {
                    if (TransactionSheet.Cells[row, j].Value != null)
                    {
                        string inputData = TransactionSheet.Cells[row, j].Value.ToString();
                        if (priceRegex1.IsMatch(inputData) || priceRegex2.IsMatch(inputData))
                        {
                            return j.ToString();
                        }
                        else if (priceRegex3.IsMatch(inputData))
                        {
                            if (TransactionSheet.Cells[row, j + 1].Value != null)
                            {
                                string inputData2 = TransactionSheet.Cells[row, j + 1].Value.ToString();
                                if (priceRegex4.IsMatch(inputData2))
                                {
                                    return inputData + "," + inputData2;
                                }
                            }
                            else if (TransactionSheet.Cells[row, j - 1].Value != null)
                            {
                                string inputData2 = TransactionSheet.Cells[row, j - 1].Value.ToString();
                                if (priceRegex4.IsMatch(inputData2))
                                {
                                    return inputData2 + "," + inputData2;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private int getDateColumn(int row, int maxColumn)
        {
            Regex dateRegex1 = new Regex(@"^20\d{2}.\d{2}.\d{2}");
            Regex dateRegex2 = new Regex(@"^20\d{2}-\d{2}-\d{2}");
            Regex dateRegex3 = new Regex(@"^20\d{2}.\s\d{2}.\s\d{2}");
            if (row != 1)
            {
                for (int i = row; i < row + 3; i++)
                {
                    for (int column = 1; column < maxColumn; column++)
                    {
                        if (TransactionSheet.Cells[i, column].Value != null)
                        {
                            string inputData = TransactionSheet.Cells[i, column].Value.ToString();
                            if (dateRegex1.IsMatch(inputData) || dateRegex2.IsMatch(inputData) || dateRegex3.IsMatch(inputData))
                            {
                                return column;
                            }
                        }
                    }
                }
            }
            else//colum titles first row
            {
                for (int i = row + 1; i < row + 3; i++)
                {
                    for (int column = 1; column < maxColumn; column++)
                    {
                        if (TransactionSheet.Cells[i, column].Value != null)
                        {
                            string inputData = TransactionSheet.Cells[i, column].Value.ToString();
                            if (dateRegex1.IsMatch(inputData) || dateRegex2.IsMatch(inputData) || dateRegex3.IsMatch(inputData))
                            {
                                return column;
                            }
                        }
                    }
                }
            }
            return -1;
        }
        public void readOutUserspecifiedTransactions(string startingRow, string dateColumnString, string commentColumnString
            , string accounNumberCB, string transactionPriceCB, string balanceCB, string balanceColumnString)
        {
            int transactionRow = int.Parse(startingRow);
            //getting the account number fist
            string accountNumber = "";
            int accountNumberColumn;
            string accountNumberResult = SpecifiedImportBank.getInstance(null, mainWindow).accountNumberTextBox.Text.ToString();
            if (accounNumberCB == "Column")
            {
                try
                {
                    //check if it is a number
                    accountNumberColumn = int.Parse(accountNumberResult);
                }
                catch (Exception e)
                {
                    //it isn't a number its a letter like A,B,E,
                    //so we convert it to a number
                    accountNumberColumn = ExcelColumnNameToNumber(accountNumberResult);
                }
                accountNumber = TransactionSheet.Cells[transactionRow, accountNumberColumn].Value.ToString();
            }
            else if (accounNumberCB == "Cell")
            {
                string firstChar = accountNumberResult.Substring(0, 1);
                try
                {
                    //check if it is a number
                    accountNumberColumn = int.Parse(firstChar);
                }
                catch (Exception e)
                {
                    //it isn't a number its a letter like A,B,E,
                    //so we convert it to a number
                    accountNumberColumn = ExcelColumnNameToNumber(firstChar);
                }
                accountNumber = TransactionSheet.Cells[accountNumberResult.Substring(1), accountNumberColumn].Value.ToString();
            }
            else if (accounNumberCB == "Sheet name")
            {
                accountNumber = TransactionSheet.Name;
            }

            int balanceColumn = 0;
            if (balanceCB == "Column")
            {
                try
                {
                    //check if it is a number
                    balanceColumn = int.Parse(balanceColumnString);
                }
                catch (Exception e)
                {
                    //it isn't a number its a letter like A,B,E,
                    //so we convert it to a number
                    balanceColumn = ExcelColumnNameToNumber(balanceColumnString);
                }
            }
            else if (balanceCB == "None")
            {
                balanceColumn = -1;
            }
            int transactionDescriptionColumn = 0;
            List<string> commentColumnStrings;//C,B,E,G  1,3,5,2
            commentColumnStrings = commentColumnString.Split(',').ToList();
            List<int> transactionDescriptionColumns = new List<int>();
            if (commentColumnStrings.Count > 1)//if it cannot be splitted it returns the whole string
            {
                for (int i = 0; i < commentColumnStrings.Count; i++)
                {
                    try
                    {
                        transactionDescriptionColumn = int.Parse(commentColumnStrings[i]);
                    }
                    catch (Exception e)
                    {
                        transactionDescriptionColumn = ExcelColumnNameToNumber(commentColumnStrings[i]);
                    }
                    transactionDescriptionColumns.Add(transactionDescriptionColumn);
                }
            }
            else
            {
                try
                {
                    transactionDescriptionColumn = int.Parse(commentColumnString);
                }
                catch (Exception e)
                {
                    transactionDescriptionColumn = ExcelColumnNameToNumber(commentColumnString);
                }
            }
            int dateColumn;
            try
            {
                dateColumn = int.Parse(dateColumnString);
            }
            catch (Exception e)
            {
                dateColumn = ExcelColumnNameToNumber(dateColumnString);
            }
            //we have the account number,desription column , date column , balance column(or we no there isn't)
            //the price column(s) left
            bool isOneColumn = true;
            int priceColumn = 0;
            int incomeColumn = 0;
            int spendingColumn = 0;
            if (transactionPriceCB == "One column")
            {
                string priceColumnString = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text.ToString();
                try
                {
                    priceColumn = int.Parse(priceColumnString);
                }
                catch (Exception e)
                {
                    priceColumn = ExcelColumnNameToNumber(priceColumnString);
                }
            }
            else if (transactionPriceCB == "Income,Spending")
            {
                isOneColumn = false;
                string incomeColumnString = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text.ToString();
                try
                {
                    incomeColumn = int.Parse(incomeColumnString);
                }
                catch (Exception e)
                {
                    incomeColumn = ExcelColumnNameToNumber(incomeColumnString);
                }
                string spendingColumnString = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_2.Text.ToString();
                try
                {
                    spendingColumn = int.Parse(spendingColumnString);
                }
                catch (Exception e)
                {
                    spendingColumn = ExcelColumnNameToNumber(spendingColumnString);
                }
            }
            //we have every info
            int blank_counter = 0;
            while (blank_counter < 2)
            {
                if (TransactionSheet.Cells[transactionRow, dateColumn].Value != null)
                {
                    blank_counter = 0;
                    string transactionDate = TransactionSheet.Cells[transactionRow, dateColumn].Value.ToString();
                    string transactionDescription = "-";
                    if (transactionDescriptionColumns.Count > 0)
                    {
                        for (int i = 0; i < transactionDescriptionColumns.Count; i++)
                        {
                            if (TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value != null)
                            {
                                if (i == 0)//transactionDescription initalization
                                    transactionDescription = TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString() + ", ";
                                else if (i == transactionDescriptionColumns.Count - 1)
                                    transactionDescription += TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString();
                                else
                                    transactionDescription += TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString() + ", ";
                            }
                        }
                    }
                    else
                    {
                        if (TransactionSheet.Cells[transactionRow, transactionDescriptionColumn].Value != null)
                            transactionDescription = TransactionSheet.Cells[transactionRow, transactionDescriptionColumn].Value.ToString();
                    }
                    int transactionPrice = 0;
                    if (balanceColumn != -1)
                    {
                        if (TransactionSheet.Cells[transactionRow, balanceColumn].Value != null)//check if the balance column has a value (FHB bankfile of course)
                        {
                            string balanceRnString = TransactionSheet.Cells[transactionRow, balanceColumn].Value.ToString();
                            Console.WriteLine(balanceRnString);
                            int balanceRn = int.Parse(balanceRnString);
                            if (isOneColumn) // single column , have balance column
                            {
                                string transactionPriceString = TransactionSheet.Cells[transactionRow, priceColumn].Value.ToString();
                                transactionPrice = int.Parse(transactionPriceString);
                                transactions.Add(new Transaction(balanceRn, transactionDate, transactionPrice, transactionDescription, accountNumber));
                            }
                            else //multiple column , have balance column
                            {
                                if (TransactionSheet.Cells[transactionRow, incomeColumn].Value != null)
                                {
                                    string incomeString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                    int income = int.Parse(incomeString);
                                    transactions.Add(new Transaction(balanceRn, transactionDate, income, transactionDescription, accountNumber));
                                }
                                else//it is a spending transaction
                                {
                                    string spendingString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                    int spending = int.Parse(spendingString) * (-1);
                                    transactions.Add(new Transaction(balanceRn, transactionDate, spending, transactionDescription, accountNumber));
                                }
                            }
                        }
                        else
                        {
                            int tempRow = transactionRow;
                            while (TransactionSheet.Cells[tempRow, balanceColumn].Value == null)
                            {
                                tempRow++;
                            }
                            //az első olyan sor ahol újra van értéke a balance cellának
                            string newKownBalanceString = TransactionSheet.Cells[tempRow, balanceColumn].Value.ToString();
                            int newKownBalance = int.Parse(newKownBalanceString);
                            int calcuatedBalance = calculatePastBalance(newKownBalance, transactionRow, tempRow, spendingColumn, incomeColumn);
                            if (TransactionSheet.Cells[transactionRow, incomeColumn].Value != null)
                            {
                                string incomeString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                int income = int.Parse(incomeString);
                                transactions.Add(new Transaction(calcuatedBalance, transactionDate, income, transactionDescription, accountNumber));
                            }
                            else
                            {
                                string spendingString = TransactionSheet.Cells[transactionRow, spendingColumn].Value.ToString();
                                int spending = int.Parse(spendingString) * (-1);
                                transactions.Add(new Transaction(calcuatedBalance, transactionDate, spending, transactionDescription, accountNumber));
                            }
                        }
                    }
                    else//no balance column
                    {
                        string noBalance = "-";
                        if (isOneColumn) // single price column , no balance column
                        {
                            string transactionPriceString = TransactionSheet.Cells[transactionRow, priceColumn].Value.ToString();
                            string[] splittedPrice = transactionPriceString.Split(' ');
                            if (splittedPrice.Length == 1)
                                transactionPrice = int.Parse(transactionPriceString);
                            else
                                transactionPrice = int.Parse(splittedPrice[0]);
                            transactions.Add(new Transaction(noBalance, transactionDate, transactionPrice, transactionDescription, accountNumber));
                        }
                        else //multiple price column ,  doesnt have balance column
                        {
                            if (TransactionSheet.Cells[transactionRow, incomeColumn].Value != null)
                            {
                                string incomeString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                int income = int.Parse(incomeString);
                                transactions.Add(new Transaction(noBalance, transactionDate, income, transactionDescription, accountNumber));
                            }
                            else//it is a spending transaction
                            {
                                string spendingString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                int spending = int.Parse(spendingString) * (-1);
                                transactions.Add(new Transaction(noBalance, transactionDate, spending, transactionDescription, accountNumber));
                            }
                        }
                    }
                }
                else
                {
                    blank_counter++;
                }
                transactionRow++;
            }
            if (transactions.Count > 0)
            {
                string bankName = "";
                if (SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString() != "Add new Bank")
                    bankName = SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString();
                else
                    bankName = SpecifiedImportBank.getInstance(null, mainWindow).newBankTextbox.Text.ToString();
                for (int i = 0; i < transactions.Count; i++)
                    transactions[i].setBankname(bankName);
                bankHanlder.addTransactions(transactions);
                //todo another thread
                addImportFileDataToDB(int.Parse(startingRow), accountNumberResult,
                    dateColumnString, transactionPriceCB, balanceColumnString, commentColumnString);
            }
        }
        private void addImportFileDataToDB(int startingRow, string accountNumberTextBox,
            string dateColumnTextBox, string priceCheckBox, string balanceColumnTextBox, string commentColumnTextbox)
        {
            //SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            //sqlConn.Open();
            string storedQuery = "";
            string firstColumn = ""; //price
            string secondColumn = ""; //price
            bool accountTextBoxSheetName = false;
            bool isMultiplePriceColumns = false;
            bool haveBalanceColumn = true;
            if (SpecifiedImportBank.getInstance(null, mainWindow).accountNumberCB.SelectedItem.ToString() == "Sheet name")
            {
                accountTextBoxSheetName = true;
                storedQuery = "Select * From [StoredColumnsBank] where TransStartRow = '" + startingRow + "'" +
               " AND AccountNumberPos = '" + "Sheet name" + "'" +
               " AND DateColumn = '" + dateColumnTextBox + "'";
                firstColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text.ToString();
                if (priceCheckBox == "One column")
                {
                    storedQuery += " AND PriceColumn = '" + firstColumn + "'";
                }
                else if (priceCheckBox == "Income,Spending")
                {
                    isMultiplePriceColumns = true;
                    secondColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_2.Text.ToString();
                    storedQuery += " AND PriceColumn = '" + firstColumn + "," + secondColumn + "'";
                }
                string balanceColumnCB = SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnCB.SelectedItem.ToString();
                if (balanceColumnCB == "Column")
                {
                    storedQuery += " AND BalanceColumn = '" + balanceColumnTextBox + "'";
                }
                else if (balanceColumnCB == "None")
                {
                    haveBalanceColumn = false;
                    storedQuery += " AND BalanceColumn = '" + "None" + "'";
                }

                storedQuery += " AND CommentColumn = '" + commentColumnTextbox + "'";
            }
            else
            {
                storedQuery = "Select * From [StoredColumnsBank] where TransStartRow = '" + startingRow + "'" +
               " AND AccountNumberPos = '" + accountNumberTextBox + "'" +
               " AND DateColumn = '" + dateColumnTextBox + "'";
                firstColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text.ToString();
                if (priceCheckBox == "One column")
                {
                    storedQuery += " AND PriceColumn = '" + firstColumn + "'";
                }
                else if (priceCheckBox == "Income,Spending")
                {
                    isMultiplePriceColumns = true;
                    secondColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_2.Text.ToString();
                    storedQuery += " AND PriceColumn = '" + firstColumn + "," + secondColumn + "'";
                }
                string balanceColumnCB = SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnCB.SelectedItem.ToString();
                if (balanceColumnCB == "Column")
                {
                    storedQuery += " AND BalanceColumn = '" + balanceColumnTextBox + "'";
                }
                else if (balanceColumnCB == "None")
                {
                    haveBalanceColumn = false;
                    storedQuery += " AND BalanceColumn = '" + "None" + "'";
                }

                storedQuery += " AND CommentColumn = '" + commentColumnTextbox + "'";
            }
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count == 0)
            {
                string insertQuery = "insert into [StoredColumnsBank](BankName, TransStartRow, AccountNumberPos,DateColumn,PriceColumn,BalanceColumn,CommentColumn) " +
                        "values(";
                if (SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString() == "Add new Bank")
                {
                    string newBankName = SpecifiedImportBank.getInstance(null, mainWindow).newBankTextbox.Text.ToString();
                    insertQuery += "'" + newBankName + "'";
                }
                else
                {
                    string bankName = SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString();
                    insertQuery += "'" + bankName + "'";
                }
                insertQuery += ",'" + startingRow + "'";
                if (accountTextBoxSheetName)
                    insertQuery += ",'Sheet name'";
                else
                    insertQuery += ",'" + accountNumberTextBox + "'";
                insertQuery += ",'" + dateColumnTextBox + "'";
                if (isMultiplePriceColumns)
                {
                    string columns = firstColumn + "," + secondColumn;
                    insertQuery += ",'" + columns + "'";
                }
                else
                    insertQuery += ",'" + firstColumn + "'";
                if (haveBalanceColumn)
                    insertQuery += ",'" + balanceColumnTextBox + "'";
                else
                    insertQuery += ",'None'";
                insertQuery += ",'" + commentColumnTextbox + "')";
                SQLiteCommand insercommand = new SQLiteCommand(insertQuery, mConn);
                insercommand.CommandType = CommandType.Text;
                insercommand.ExecuteNonQuery();
            }
            /*
            SqlDataAdapter sda = new SqlDataAdapter(storedQuery, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count == 0)
            {
                SqlCommand sqlCommand = new SqlCommand("insertNewColumns3", sqlConn);//SQLQuery 8
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString() == "Add new Bank")
                {
                    string newBankName = SpecifiedImportBank.getInstance(null, mainWindow).newBankTextbox.Text.ToString();
                    sqlCommand.Parameters.AddWithValue("@bankName", newBankName);
                }
                else
                {
                    string bankName = SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString();
                    sqlCommand.Parameters.AddWithValue("@bankName", bankName);
                }
                sqlCommand.Parameters.AddWithValue("@transStartRow", startingRow);
                if (accountTextBoxSheetName)
                    sqlCommand.Parameters.AddWithValue("@accountNumberPos", "Sheet name");
                else
                    sqlCommand.Parameters.AddWithValue("@accountNumberPos", accountNumberTextBox);
                sqlCommand.Parameters.AddWithValue("@dateColumn", dateColumnTextBox);
                if (isMultiplePriceColumns)
                    sqlCommand.Parameters.AddWithValue("@priceColumn", firstColumn + "," + secondColumn);
                else
                    sqlCommand.Parameters.AddWithValue("@priceColumn", firstColumn);
                if (haveBalanceColumn)
                    sqlCommand.Parameters.AddWithValue("@balanceColumn", balanceColumnTextBox);
                else
                    sqlCommand.Parameters.AddWithValue("@balanceColumn", "None");
                sqlCommand.Parameters.AddWithValue("@commentColumn", commentColumnTextbox);
                sqlCommand.ExecuteNonQuery();
            }
            */
        }
        public static int ExcelColumnNameToNumber(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName");

            columnName = columnName.ToUpperInvariant();

            int sum = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A' + 1);
            }

            return sum;
        }
        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }
        private void setStartingRow(int value)
        {
            startingRow = value;
        }
        private void setNofColumns(int value)
        {
            nofColumns = value;
        }
        private void setAccountNumber(string value)
        {
            accountNumber = value;
        }
        private void setPastTransactionPrice(int value)
        {
            pastTransactionPrice = value;
        }
        private void setIsFirstTransaction(bool value)
        {
            isFirstTransaction = value;
        }
        public void setCalculatedBalance(bool value)
        {
            calculatedBalance = value;
        }

        public int getStartingRow()
        {
            return startingRow;
        }
        public int getNumberOfColumns()
        {
            return nofColumns;
        }
        public string getAccountNumber()
        {
            return accountNumber;
        }
        public int getPastTransactionPrice()
        {
            return pastTransactionPrice;
        }
        public bool getIsFirstTransaction()
        {
            return isFirstTransaction;
        }
        public bool getCalculatedBalance()
        {
            return calculatedBalance;
        }
    }
}
