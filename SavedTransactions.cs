using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Data;

namespace WpfApp1
{
    public class SavedTransactions
    {
        public static List<Transaction> savedTransactionsBank;
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        public static List<Stock> savedTransactionsStock;
        private static SavedTransactions instance;
        private SqlConnection sqlConn;
        private SavedTransactions()
        {
            savedTransactionsBank = new List<Transaction>();
            savedTransactionsStock = new List<Stock>();
            //sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
        public void readOutSavedBankTransactions()
        {
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [importedBankTransactions] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'ExportDate' TEXT, 'TransactionDate' TEXT, " +
                        "'AccountBalance' INTEGER, 'Difference' INTEGER, 'Income' INTEGER, 'Spending' INTEGER, " +
                        "'Comment' TEXT, 'AccountNumber' TEXT, 'BankName' TEXT  );", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string getAllTransactionsQuery = "select * from [importedBankTransactions]";
            SQLiteCommand command = new SQLiteCommand(getAllTransactionsQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count != 0)
            {
                string writeoutDate = "";
                string tempTransactionDate = "";
                string transactionDate = "";
                int accountBalance;
                int transactionPrice;
                string accountNumber = "";
                string description = "";
                string bankName = "";
                foreach (DataRow row in dtb.Rows)
                {
                    writeoutDate = row["ExportDate"].ToString();
                    tempTransactionDate = row["TransactionDate"].ToString();
                    string[] splittedDate = tempTransactionDate.Split(' ');
                    if (splittedDate.Length == 1)
                    {
                        transactionDate = tempTransactionDate;
                    }
                    else
                    {
                        transactionDate = splittedDate[0] + splittedDate[1] + splittedDate[2];
                    }
                    accountBalance = int.Parse(row["AccountBalance"].ToString());
                    transactionPrice = int.Parse(row["Difference"].ToString());
                    accountNumber = row["AccountNumber"].ToString();
                    description = row["Comment"].ToString();
                    bankName = row["BankName"].ToString();
                    Transaction transaction = new Transaction(writeoutDate, transactionDate, accountBalance, transactionPrice, accountNumber, description);
                    transaction.setBankname(bankName);
                    savedTransactionsBank.Add(transaction);
                }
            }
            /*
            sqlConn.Open();
            string query = "Select * From [importedBankTransactions]";
            SqlDataAdapter sda = new SqlDataAdapter(query, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count != 0)
            {
                string writeoutDate="";
                string tempTransactionDate="";
                string transactionDate="";
                int accountBalance;
                int transactionPrice;
                string accountNumber="";
                string description="";
                string bankName="";
                foreach (System.Data.DataRow row in dtb.Rows)
                {
                    writeoutDate = row["ExportDate"].ToString();
                    tempTransactionDate = row["TransactionDate"].ToString();
                    string[] splittedDate = tempTransactionDate.Split(' ');
                    if (splittedDate.Length == 1)
                    {
                        transactionDate = tempTransactionDate;
                    }
                    else
                    {
                        transactionDate = splittedDate[0] + splittedDate[1] + splittedDate[2];
                    }
                    accountBalance = int.Parse(row["AccountBalance"].ToString());
                    transactionPrice = int.Parse(row["Difference"].ToString());
                    accountNumber = row["AccountNumber"].ToString();
                    description = row["Comment"].ToString();
                    bankName = row["BankName"].ToString();
                    Transaction transaction = new Transaction(writeoutDate, transactionDate, accountBalance, transactionPrice, accountNumber, description);
                    transaction.setBankname(bankName);
                    savedTransactionsBank.Add(transaction);
                }
            }
            /*
            ReadWorksheet = ReadWorkbook.Worksheets[1];
            int i = 2;
            while (ReadWorksheet.Cells[i, 1].Value != null)
            {
                string writeoutDate = "";
                string tempTransactionDate = "";
                string transactionDate = "";
                string balanceString = "";
                int balance = 0;
                string transactionPriceString = "";
                int transactionPrice = 0;
                string accountNumber = "";
                string description = "";
                string bankname = "";
                writeoutDate = ReadWorksheet.Cells[i, 1].Value.ToString();
                tempTransactionDate = ReadWorksheet.Cells[i, 2].Value.ToString();
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
                balanceString = ReadWorksheet.Cells[i, 3].Value.ToString();
                balance = int.Parse(balanceString);
                if (ReadWorksheet.Cells[i, 7].Value != null)
                {
                    transactionPriceString = ReadWorksheet.Cells[i, 7].Value.ToString();
                    transactionPrice = int.Parse(transactionPriceString);
                }
                else if (ReadWorksheet.Cells[i, 9].Value != null)
                {
                    transactionPriceString = ReadWorksheet.Cells[i, 9].Value.ToString();
                    transactionPrice = int.Parse(transactionPriceString);
                }
                accountNumber = ReadWorksheet.Cells[i, 16].Value.ToString();
                if (ReadWorksheet.Cells[i, 14].Value != null)
                {
                    description = ReadWorksheet.Cells[i, 14].Value.ToString();
                }
                if(ReadWorksheet.Cells[i,17].Value!=null)
                {
                    bankname = ReadWorksheet.Cells[i, 17].Value.ToString();
                }
                Transaction transaction = new Transaction(writeoutDate, transactionDate, balance, transactionPrice, accountNumber, description);
                transaction.setBankname(bankname);
                savedTransactionsBank.Add(transaction);
                i++;
            }
            */
            //sqlConn.Close();
        }
        public void readOutStockSavedTransactions()
        {
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [importedStockTransactions] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT, 'ExportDate' TEXT, 'TransactionDate' TEXT, " +
                        "'StockName' TEXT, 'StockPrice' TEXT, 'SoldQuantity' INTEGER, 'BoughtQuantity' INTEGER, " +
                        "'CurrentQuantity' INTEGER, 'Spending' TEXT, 'Profit' TEXT," +
                        " 'EarningMethod' TEXT, 'ImporterName' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string getAllTransactionsQuery = "select * from [importedStockTransactions]";
            SQLiteCommand command = new SQLiteCommand(getAllTransactionsQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count != 0)
            {
                string writeoutDate = "";
                string stockName = "";
                string transactionDate = "";
                string stockPrice = "";
                int originalQuantity = 0;
                string transactionType = "";
                string importer = "";
                int currentQuantity=0;
                string originalAndCurrentQuantity = "";
                string profit = "";
                string earningMethod = "";
                foreach (DataRow row in dtb.Rows)
                {
                    writeoutDate = row["ExportDate"].ToString();
                    transactionDate = row["TransactionDate"].ToString();
                    stockName = row["StockName"].ToString();
                    stockPrice = row["StockPrice"].ToString();
                    if (int.Parse(row["SoldQuantity"].ToString()) != 0)
                    {
                        transactionType = "Sell";
                        originalQuantity = int.Parse(row["SoldQuantity"].ToString());
                        originalAndCurrentQuantity = originalQuantity.ToString();
                        profit = row["Profit"].ToString();
                        earningMethod = row["EarningMethod"].ToString();
                    }
                    else if (int.Parse(row["BoughtQuantity"].ToString()) != 0)
                    {
                        transactionType = "Buy";
                        originalQuantity = int.Parse(row["BoughtQuantity"].ToString());
                        currentQuantity = int.Parse(row["CurrentQuantity"].ToString());
                        originalAndCurrentQuantity = originalQuantity.ToString() + " (" + currentQuantity + ")";
                    }
                    importer = row["ImporterName"].ToString();
                    Stock stock = new Stock(writeoutDate, transactionDate, stockName, stockPrice, originalQuantity, transactionType, importer);
                    if (stock.getTransactionType() == "Sell")
                    {
                        stock.setProfit(profit);
                        stock.setEarningMethod(earningMethod);
                    }
                    else
                    {
                        stock.setCurrentQuantity(currentQuantity);
                    }
                    stock.setOriginalAndSellQuantity(originalAndCurrentQuantity);
                    savedTransactionsStock.Add(stock);
                }
            }
            mConn.Close();
            /*
            sqlConn.Open();
            string query = "Select * From [importedStockTransactions]";
            SqlDataAdapter sda = new SqlDataAdapter(query, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count != 0)
            {
                string writeoutDate = "";
                string stockName = "";
                string transactionDate = "";
                double stockPrice = 0;
                int originalQuantity = 0;
                string transactionType = "";
                string importer = "";
                string currentQuantity = "";
                string originalAndCurrentQuantity = "";
                double profit = 0;
                string earningMethod="";
                foreach (System.Data.DataRow row in dtb.Rows)
                {
                    writeoutDate = row["ExportDate"].ToString();
                    transactionDate = row["TransactionDate"].ToString();
                    stockName = row["StockName"].ToString();
                    stockPrice = double.Parse(row["StockPrice"].ToString());
                    if (row["SoldQuantity"] != DBNull.Value)
                    {
                        transactionType = "Sell";
                        originalQuantity = int.Parse(row["SoldQuantity"].ToString());
                        originalAndCurrentQuantity = originalQuantity.ToString();
                        profit = double.Parse(row["Profit"].ToString());
                        earningMethod = row["EarningMethod"].ToString();
                    }
                    else if (row["BoughtQuantity"] != DBNull.Value)
                    {
                        transactionType = "Buy";
                        originalQuantity = int.Parse(row["BoughtQuantity"].ToString());
                        currentQuantity = row["CurrentQuantity"].ToString();
                        originalAndCurrentQuantity = originalQuantity.ToString() + " (" + currentQuantity + ")";
                    }
                    importer = row["ImporterName"].ToString();
                    Stock stock = new Stock(writeoutDate, transactionDate, stockName, stockPrice, originalQuantity, transactionType, importer);
                    if(stock.getTransactionType()=="Sell")
                    {
                        stock.setProfit(profit);
                        stock.setEarningMethod(earningMethod);
                    }
                    else
                    {
                        stock.setCurrentQuantity(int.Parse(currentQuantity));
                    }
                    stock.setOriginalAndSellQuantity(originalAndCurrentQuantity);
                    savedTransactionsStock.Add(stock);
                }
            }

            /*
            ReadWorksheet = ReadWorkbook.Worksheets[2];
            int i = 2;
            while (ReadWorksheet.Cells[i, 1].Value != null)
            {
                string writeoutDate = "";
                string stockName = "";
                string transactionDate = "";
                string stockPriceString = "";
                double stockPrice = 0;
                int originalQuantity = 0;
                string transactionType = "";
                string importer = "";
                string currentQuantity = "";
                string currentAndOriginal = "";
                writeoutDate = ReadWorksheet.Cells[i, 1].Value.ToString();
                transactionDate = ReadWorksheet.Cells[i, 2].Value.ToString();
                stockName = ReadWorksheet.Cells[i, 3].Value.ToString();
                stockPriceString = ReadWorksheet.Cells[i, 4].Value.ToString().Replace(',','.');
                stockPrice = double.Parse(stockPriceString, CultureInfo.InvariantCulture);
                string quantityString = "";
                if (ReadWorksheet.Cells[i,5].Value!=null)//eladott
                {
                    quantityString = ReadWorksheet.Cells[i, 5].Value.ToString();
                    originalQuantity=int.Parse(quantityString);
                    transactionType = "Sell";
                }
                else if(ReadWorksheet.Cells[i,6].Value!=null)//vásárolt
                {
                    quantityString = ReadWorksheet.Cells[i, 6].Value.ToString();
                    originalQuantity = int.Parse(quantityString);
                    transactionType = "Buy";
                }
                if (ReadWorksheet.Cells[i, 7].Value != null) //jelenlegi darab
                {
                    currentQuantity = ReadWorksheet.Cells[i, 7].Value.ToString();
                }
                currentAndOriginal = originalQuantity.ToString() + " (" + currentQuantity + ")";
                if (ReadWorksheet.Cells[i,11].Value!=null)
                {
                    importer=ReadWorksheet.Cells[i, 11].Value.ToString();
                }
                Stock stock = new Stock(writeoutDate, transactionDate, stockName, stockPrice, originalQuantity, transactionType,importer);
                if(stock.getTransactionType()=="Sell")
                {
                    string profitString = "";
                    string earningMethod = "-";
                    double profit = 0;
                    if (ReadWorksheet.Cells[i,9].Value!=null)
                    {
                        profitString = ReadWorksheet.Cells[i, 9].Value.ToString().Replace(',', '.');
                        profit = double.Parse(profitString, CultureInfo.InvariantCulture);
                        stock.setProfit(profit);
                    }
                    if(ReadWorksheet.Cells[i,9].Value!=null)
                    {
                        earningMethod = ReadWorksheet.Cells[i, 10].Value.ToString();
                    }
                    stock.setEarningMethod(earningMethod);
                }
                stock.setOriginalAndSellQuantity(currentAndOriginal);
                savedTransactionsStock.Add(stock);
                i++;
            }
            excel.Workbooks.Close();
            excel.Quit();
            */
            //sqlConn.Close();
        }
        public static List<Transaction> getSavedTransactionsBank()
        {
             return savedTransactionsBank;
        }
        public static List<Stock> getSavedTransactionsStock()
        {
            return savedTransactionsStock;
        }
        public static void addToSavedTransactionsBank(List<Transaction> newImported)
        {
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            for (int i = 0; i < newImported.Count; i++)
            {
                newImported[i].setWriteDate(todaysDate);
            }
            for (int i=0;i<newImported.Count;i++)
            {
                savedTransactionsBank.Add(newImported[i]);
            }
        }
        public static void addToSavedTransactionsStock(List<Stock> newImported)
        {
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            for (int i = 0; i < newImported.Count; i++)
            {
                newImported[i].setWriteDate(todaysDate);
            }
            for (int i=0;i<newImported.Count;i++)
            {
                savedTransactionsStock.Add(newImported[i]);
            }
        }
        public static SavedTransactions getInstance()
        {
            if(instance==null)
            {
                instance = new SavedTransactions();
            }
            return instance;
        }
        ~SavedTransactions()
        {
        }
    }
}
