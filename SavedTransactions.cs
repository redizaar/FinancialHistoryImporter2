﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Globalization;

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

                savedTransactionsBank.Add(new Transaction(writeoutDate, transactionDate, balance, transactionPrice, accountNumber, description));
                i++;
            }
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
                int quantity = 0;
                string transactionType = "";
                string importer = "";

                writeoutDate = ReadWorksheet.Cells[i, 1].Value.ToString();
                transactionDate = ReadWorksheet.Cells[i, 2].Value.ToString();
                stockName = ReadWorksheet.Cells[i, 3].Value.ToString();
                stockPriceString = ReadWorksheet.Cells[i, 4].Value.ToString().Replace(',','.');
                stockPrice = double.Parse(stockPriceString, CultureInfo.InvariantCulture);
                string quantityString = "";
                if (ReadWorksheet.Cells[i,5].Value!=null)//eladott
                {
                    quantityString = ReadWorksheet.Cells[i, 5].Value.ToString();
                    quantity=int.Parse(quantityString);
                    transactionType = "Sell";
                }
                else if(ReadWorksheet.Cells[i,6].Value!=null)//vásárolt
                {
                    quantityString = ReadWorksheet.Cells[i, 6].Value.ToString();
                    quantity = int.Parse(quantityString);
                    transactionType = "Buy";
                }
                if(ReadWorksheet.Cells[i,10].Value!=null)
                {
                    importer=ReadWorksheet.Cells[i, 10].Value.ToString();
                }
                Stock stock = new Stock(writeoutDate, transactionDate, stockName, stockPrice, quantity, transactionType,importer);
                if(stock.getTransactionType()=="Sell")
                {
                    string earningString = "";
                    double earning = 0;
                    if (ReadWorksheet.Cells[i,8].Value!=null)
                    {
                        earningString = ReadWorksheet.Cells[i, 8].Value.ToString().Replace(',', '.');
                        earning = double.Parse(earningString, CultureInfo.InvariantCulture);
                        stock.setProfit(earning);
                    }
                }
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
            for(int i=0;i<newImported.Count;i++)
            {
                savedTransactionsBank.Add(newImported[i]);
            }
        }
        public static void addToSavedTransactionsStock(List<Stock> newImported)
        {
            for(int i=0;i<newImported.Count;i++)
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
