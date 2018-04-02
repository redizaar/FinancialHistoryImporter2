using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Data.SqlClient;

namespace WpfApp1
{
    public class SavedTransactions
    {
        _Application excel = new _Excel.Application();
        Workbook ReadWorkbook;
        Worksheet ReadWorksheet;
        public static List<Transaction> savedTransactionsBank;
        public static List<Stock> savedTransactionsStock;
        private static SavedTransactions instance;
        private SavedTransactions()
        {
            savedTransactionsBank = new List<Transaction>();
            savedTransactionsStock = new List<Stock>();
            ReadWorkbook = excel.Workbooks.Open(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
        }
        public void readOutSavedBankTransactions()
        {
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string query = "Select * From [importedBankTransactions]";
            SqlDataAdapter sda = new SqlDataAdapter(query, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count != 0)
            {
                string writeoutDate;
                string tempTransactionDate;
                string transactionDate="";
                int accountBalance;
                int transactionPrice;
                string accountNumber;
                string description;
                string bankName;
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
                        for (int j = 0; j < splittedDate.Length - 1; j++)
                        {
                            if (j < 3)
                                transactionDate += splittedDate[j];
                        }
                    }
                    accountBalance = int.Parse(row["AccountBalance"].ToString());
                    transactionPrice = int.Parse(row["Difference"].ToString());
                    accountNumber = row["AccountNumber"].ToString();
                    description = row["Comment"].ToString();
                    bankName = row["BankName"].ToString();
                    Console.WriteLine(writeoutDate + " - " + transactionDate + " - " + accountBalance + " - " + transactionPrice + " - " + description + " - " + bankName);
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
                    string earningMethod = "-";
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
            }
        public void readOutStockSavedTransactions()
        {
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
            excel.Application.Quit();
            excel.Quit();
        }
    }
}
