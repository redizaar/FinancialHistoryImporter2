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

namespace WpfApp1
{
    class TemplateBankReadIn
    {
        private Worksheet TransactionSheet;

        private List<Transaction> transactions;
        private ImportReadIn bankHanlder = null;
        private int startingRow;
        private int nofColumns;
        private int pastTransactionPrice;//in case of missing Balance column..
        private bool isFirstTransaction;//in case of missing Balance column..
        private string accountNumber;
        private MainWindow mainWindow;
        private bool multipleColumn;
        private bool calculatedBalance;//in case of having a balance column , but it is null in some of the rows..........

        public TemplateBankReadIn(ImportReadIn importReadin, Workbook workbook, Worksheet worksheet,MainWindow mainWindow,bool userSpecified)
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
            Regex accoutNumberRegex1 = new Regex(@"^Számlaszám$");
            Regex accountNumberRegex2 = new Regex(@"^Könyvelési számla$");
            Regex accoutNumberRegex3 = new Regex(@"^Számlaszám:$");
            int blank_row=0;
            int blank_cells = 0;
            int i = 1;

            int maxColumns=1;
            int transactionsStartRow = 1;
            while (blank_row<5)
            {
                int column = 1;
                if(TransactionSheet.Cells[i,column].Value!=null)
                {
                    if (this.accountNumber.Equals(""))
                    {
                        if ((column == 1) || (column==2 ))
                        {
                            string cellValue = TransactionSheet.Cells[i, column].Value.ToString();
                            if (accoutNumberRegex1.IsMatch(cellValue) || accountNumberRegex2.IsMatch(cellValue) || accoutNumberRegex3.IsMatch(cellValue))
                            {
                                string accountNumberValue = TransactionSheet.Cells[i, column + 1].Value.ToString();//the cell next to it
                                setAccountNumber(accountNumberValue);
                            }
                        }
                    }
                    blank_cells=0;
                    while(blank_cells<3)
                    {
                        if(TransactionSheet.Cells[i, column].Value != null)
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
                    blank_row=0;
                }
                else
                {
                    blank_row++;
                }
                if(column>maxColumns)
                {
                    maxColumns = column;
                    transactionsStartRow = i;
                    if(this.accountNumber.Equals(""))
                    {
                        for(int j=1;j<column;j++)
                        {
                            if(TransactionSheet.Cells[i, j].Value!=null)
                            {
                                string cellValue = TransactionSheet.Cells[i, j].Value.ToString();
                                if(accoutNumberRegex1.IsMatch(cellValue) || accountNumberRegex2.IsMatch(cellValue) || accoutNumberRegex3.IsMatch(cellValue))
                                {
                                    string accountNumberValue = TransactionSheet.Cells[i+1, j].Value.ToString();//the cell below it
                                    setAccountNumber(accountNumberValue);
                                }
                            }
                        }
                    }
                }
                i++;
            }
            if (this.accountNumber.Equals(""))
            {
                accountNumber = TransactionSheet.Name;
            }
            setStartingRow(transactionsStartRow);
            setNofColumns(maxColumns-blank_cells);
        }

        public void readOutTransactionColumns(int row, int maxColumn)
        {
            int descriptionColumn = getDescriptionColumn(row, maxColumn);
            int dateColumn=getDateColumn(row,maxColumn);
            string pricecolumnType = isMultiplePriceColumn(row,maxColumn);
            int singlepriceColumn = -1;
            try
            {
                singlepriceColumn=int.Parse(pricecolumnType);
            }
            catch(Exception e)
            {

            }
            if(singlepriceColumn==-1)
            {
                this.multipleColumn = true;
            }
            int balaceColumn=getAccountBalanceColumn(row,maxColumn);
            readOutTransactions(row,maxColumn,dateColumn,singlepriceColumn,balaceColumn,descriptionColumn);
        }

        private int getDescriptionColumn(int row, int maxColumn)
        {
            Regex descrRegex1 = new Regex(@"^Közlemény$");
            Regex descrRegex2 = new Regex(@"típusa$");
            Regex descrRegex3 = new Regex(@"^Típus$");
            Regex descrRegex4 = new Regex(@"Leírás$");

            List<int> descrColumns=new List<int>();
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
            if (descrColumns.Count != 0)
            {
                if (descrColumns.Count == 2)
                {
                    MessageBoxResult messageBoxResult = CustomMessageBox.ShowYesNo(
                        "\tPlease choose a Comment column!",
                        "Alert!",
                        descrColumnNames[0],
                        descrColumnNames[1]);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        return descrColumns[0];
                    }
                    else if(messageBoxResult == MessageBoxResult.No)
                    {
                        return descrColumns[1];
                    }
                }
                else if(descrColumns.Count==1)
                {
                    return descrColumns[0];
                }
                else if(descrColumns.Count>2)
                {
                    return descrColumns[0];
                }
            }
            return 0;
        }

        private void readOutTransactions(int row, int maxColumn,int dateColumn, int singlepriceColumn, int balaceColumn,int descriptionColumn)
        {
            if(row==1)
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
                if(titleRow)
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

                            string transactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                            string accountNumber = getAccountNumber();
                            string transactionPriceString = TransactionSheet.Cells[row, singlepriceColumn].Value.ToString();
                            string transactionBalanceString = TransactionSheet.Cells[row, balaceColumn].Value.ToString();

                            string transactionDescription = "-";
                            if(TransactionSheet.Cells[row, descriptionColumn].Value!=null)
                            {
                                transactionDescription = TransactionSheet.Cells[row, descriptionColumn].Value.ToString(); 
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

                            string transactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
                            string accountNumber = getAccountNumber();
                            string transactionPriceString = TransactionSheet.Cells[row, singlepriceColumn].Value.ToString();
                            string transactionDescription="-";
                            if (TransactionSheet.Cells[row, descriptionColumn].Value != null)
                            {
                                transactionDescription = TransactionSheet.Cells[row, descriptionColumn].Value.ToString();
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

                                string transactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
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
                                if (TransactionSheet.Cells[row, descriptionColumn].Value != null)
                                {
                                    transactionDescription = TransactionSheet.Cells[row, descriptionColumn].Value.ToString();
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

                                string transactionDate = TransactionSheet.Cells[row, dateColumn].Value.ToString();
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
                                if (TransactionSheet.Cells[row, descriptionColumn].Value != null)
                                {
                                    transactionDescription = TransactionSheet.Cells[row, descriptionColumn].Value.ToString();
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
                    bankHanlder.addTransactions(transaction);
                }
                else
                {
                    Console.WriteLine("Couldn't locate the price columns");
                }
            }
        }
        /**
         * 1. az utolsó balance cella értéke ami nem volt null
         * 2. az aktuális sor ahol tartunk(ahol null a balance cella)
         * 3.az utolsó sor ahol volt értéke a balance cellának
         * 4.a terhelés cella
         * 5.a jövedelem cella
         * 
         * return value : the right balance value
         * */
        private int calculatePastBalance(int transactionBalance,int row,int tempRow,int costPriceColumn,int incomePriceColumn)
        {
            tempRow--;//we are currently at a cell where we have a balance value
            //so we go up
            while (tempRow!=row-1)
            {
                if(TransactionSheet.Cells[tempRow, costPriceColumn].Value!=null)
                {
                    string costPriceString = TransactionSheet.Cells[tempRow, costPriceColumn].Value.ToString();
                    int costPrice = int.Parse(costPriceString)*(-1);
                    transactionBalance += costPrice ;
                }
                else if(TransactionSheet.Cells[tempRow, incomePriceColumn].Value!=null)
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
            Regex priceRegex1 = new Regex(@"Összeg");
            Regex priceRegex2 = new Regex(@"összeg");
            Regex priceRegex3 = new Regex(@"Terhelés$");
            Regex priceRegex4 = new Regex(@"Jóváírás$");
            if (row != 1)
            {
                for (int i = row-1; i <= row+2; i++)
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
                            else if (priceRegex3.IsMatch(inputData) || priceRegex4.IsMatch(inputData))
                            {
                                return "multiple";
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
                        else if (priceRegex3.IsMatch(inputData) || priceRegex4.IsMatch(inputData))
                        {
                            return "multiple";
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
        public void readOutUserspecifiedTransactions(string startingRow,string dateColumnString,string commentColumnString
            ,string accounNumberCB,string transactionPriceCB,string balanceCB,string balanceColumnString)
        {
            //getting the account number fist
            string accountNumber="";
            int accountNumberColumn;
            string accountNumberResult = SpecifiedImportBank.getInstance(null,mainWindow).accountNumberTextBox.Text.ToString();
            if (accounNumberCB=="Column")
            {
                try
                {
                    //check if it is a number
                    accountNumberColumn = int.Parse(accountNumberResult);
                }
                catch(Exception e)
                {
                    //it isn't a number its a letter like A,B,E,
                    //so we convert it to a number
                    accountNumberColumn = ExcelColumnNameToNumber(accountNumberResult);
                }
            }
            else if(accounNumberCB=="Cell")
            {
                string firstChar= accountNumberResult.Substring(0,1);
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
                accountNumber = TransactionSheet.Cells[accountNumberResult.Substring(1),accountNumberColumn].Value.ToString();
            }
            else if(accounNumberCB=="Sheet name")
            {
                accountNumber = TransactionSheet.Name;
            }

            int balanceColumn=0;
            if(balanceCB=="Column")
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
            else if(balanceCB=="None")
            {
                balanceColumn = -1;
            }
            int transactionDescriptionColumn=0;
            List<string> commentColumnStrings;//C,B,E,G  1,3,5,2
            commentColumnStrings = commentColumnString.Split(',').ToList();
            List<int> transactionDescriptionColumns=new List<int>();
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
            catch(Exception e)
            {
                dateColumn = ExcelColumnNameToNumber(dateColumnString);
            }
            int transactionRow = int.Parse(startingRow);
            //we have the account number,desription column , date column , balance column(or we no there isn't)
            //the price column(s) left
            bool isOneColumn = true;
            int priceColumn=0;
            int incomeColumn=0;
            int spendingColumn=0;
            if (transactionPriceCB == "One column")
            {
                string priceColumnString = SpecifiedImportBank.getInstance(null,mainWindow).priceColumnTextBox_1.Text.ToString();
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
                string incomeColumnString = SpecifiedImportBank.getInstance(null,mainWindow).priceColumnTextBox_1.Text.ToString();
                try
                {
                    incomeColumn = int.Parse(incomeColumnString);
                }
                catch (Exception e)
                {
                    incomeColumn = ExcelColumnNameToNumber(incomeColumnString);
                }
                string spendingColumnString = SpecifiedImportBank.getInstance(null,mainWindow).priceColumnTextBox_2.Text.ToString();
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
                                    transactionDescription = TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString()+", ";
                                else if(i== transactionDescriptionColumns.Count-1)
                                    transactionDescription += TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString();
                                else
                                    transactionDescription += TransactionSheet.Cells[transactionRow, transactionDescriptionColumns[i]].Value.ToString() + ", ";
                            }
                        }
                    }
                    int transactionPrice = 0;
                    if (balanceColumn != -1)
                    {
                        if (TransactionSheet.Cells[transactionRow, balanceColumn].Value != null)//check if the balance column has a value (fhb of course)
                        {
                            string balanceRnString = TransactionSheet.Cells[transactionRow, balanceColumn].Value.ToString();
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
                            //az utolsó olyan sor ahol van értéke a balance cellának
                            string lastKownBalanceString = TransactionSheet.Cells[tempRow, balanceColumn].Value.ToString();
                            int lastKownBalance = int.Parse(lastKownBalanceString);
                            int calcuatedBalance = calculatePastBalance(lastKownBalance, transactionRow, tempRow, spendingColumn, incomeColumn);
                            if (TransactionSheet.Cells[transactionRow, incomeColumn].Value != null)
                            {
                                string incomeString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
                                int income = int.Parse(incomeString);
                                transactions.Add(new Transaction(calcuatedBalance, transactionDate, income, transactionDescription, accountNumber));
                            }
                            else//it is a spending transaction
                            {
                                string spendingString = TransactionSheet.Cells[transactionRow, incomeColumn].Value.ToString();
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
                            transactionPrice = int.Parse(transactionPriceString);
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
                bankHanlder.addTransactions(transactions);
                addImportFileDataToDB(int.Parse(startingRow),accountNumberResult, 
                    dateColumnString,transactionPriceCB  ,balanceColumnString, commentColumnString);
            }
        }
        private void addImportFileDataToDB(int startingRow,string accountNumberTextBox,
            string dateColumnTextBox,string priceCheckBox,string balanceColumnTextBox,string commentColumnTextbox)
        {
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();

            string storedQuery="";
            string firstColumn = ""; //price
            string secondColumn = ""; //price
            bool accountTextBoxSheetName = false;
            bool isMultiplePriceColumns = false;
            bool haveBalanceColumn = true;
            if(SpecifiedImportBank.getInstance(null,mainWindow).accountNumberCB.SelectedItem.ToString()=="Sheet name")
            {
                accountTextBoxSheetName = true;
                 storedQuery= "Select * From [StoredColumns] where TransStartRow = '" + startingRow + "'" +
                " AND AccountNumberPos = '" + "Sheet name" + "'" +
                " AND DateColumn = '" + dateColumnTextBox + "'";
                firstColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text.ToString();
                if (priceCheckBox=="One column")
                {
                    storedQuery += " AND PriceColumn = '" + firstColumn + "'";
                }
                else if(priceCheckBox=="Income,Spending")
                {
                    isMultiplePriceColumns = true;
                    secondColumn = SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_2.Text.ToString();
                    storedQuery += " AND PriceColumn = '" + firstColumn + "," + secondColumn + "'";
                }
                string balanceColumnCB=SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnCB.SelectedItem.ToString();
                if(balanceColumnCB=="Column")
                {
                    storedQuery += " AND BalanceColumn = '" + balanceColumnTextBox + "'";
                }
                else if(balanceColumnCB=="None")
                {
                    haveBalanceColumn = false;
                    storedQuery += " AND BalanceColumn = '" + "None" + "'";
                }

                storedQuery += " AND CommentColumn = '" + commentColumnTextbox + "'";
            }
            else
            {
                storedQuery = "Select * From [StoredColumns] where TransStartRow = '" + startingRow + "'" +
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
            SqlDataAdapter sda = new SqlDataAdapter(storedQuery, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count==0)
            {
                SqlCommand sqlCommand = new SqlCommand("insertNewColumns3", sqlConn);//SQLQuery 8
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (SpecifiedImportBank.getInstance(null, mainWindow).storedTypesCB.SelectedItem.ToString() == "Add new Bank")
                {
                    string newBankName= SpecifiedImportBank.getInstance(null, mainWindow).newBankTextbox.Text.ToString();
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
                if(haveBalanceColumn)
                    sqlCommand.Parameters.AddWithValue("@balanceColumn", balanceColumnTextBox);
                else
                    sqlCommand.Parameters.AddWithValue("@balanceColumn", "None");
                sqlCommand.Parameters.AddWithValue("@commentColumn", commentColumnTextbox);
                sqlCommand.ExecuteNonQuery();
            }
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
