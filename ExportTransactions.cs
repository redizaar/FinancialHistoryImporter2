﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace WpfApp1
{
    class ExportTransactions
    {
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        private MainWindow mainWindow;
        private string importerAccountNumber;
        public ExportTransactions(List<Transaction> transactions,MainWindow mainWindow,string currentFileName)
        {
            this.mainWindow = mainWindow;
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            for (int i = 0; i < transactions.Count; i++)
            {
                string [] spaceSplitted=transactions[i].getTransactionDate().Split(' ');
                string dateString="";
                for (int j = 0; j < spaceSplitted.Length; j++)
                    dateString += spaceSplitted[j];
            }
            MessageBox.Show("Exporting data from: " + currentFileName, "", MessageBoxButton.OK);
            //BUT FIRST - check if the transaction is already exported or not

            List<Transaction> neededTransactions = newTransactions(transactions);
            SavedTransactions.addToSavedTransactionsBank(neededTransactions);//adding the freshyl imported transactions to the saved
            if (neededTransactions != null)
            {
                mConn.Open();
                for (int i = 0; i < neededTransactions.Count; i++)
                {
                    string insertQuery = "insert into [importedBankTransactions]" +
                    "(ExportDate,TransactionDate,AccountBalance,Difference,Income,Spending,Comment,AccountNumber,BankName)" +
                    " values('" + todaysDate + "','" + neededTransactions[i].getTransactionDate() + "','" + neededTransactions[i].getBalance_rn() + "','" + neededTransactions[i].getTransactionPrice() + "'";
                    if (neededTransactions[i].getTransactionPrice() > 0)
                    {
                        insertQuery += ",'" + neededTransactions[i].getTransactionPrice() + "','" + DBNull.Value + "'";
                    }
                    else
                    {
                        insertQuery += ",'" + DBNull.Value + "','" + neededTransactions[i].getTransactionPrice() + "'";
                    }
                    insertQuery += ",'" + neededTransactions[i].getTransactionDescription() + "','" + neededTransactions[i].getAccountNumber() + "','" + neededTransactions[i].getBankname() + "')";
                    SQLiteCommand insertcommand = new SQLiteCommand(insertQuery, mConn);
                    insertcommand.CommandType = CommandType.Text;
                    insertcommand.ExecuteNonQuery();
                }
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            }
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("insertBankTransaction", sqlConn);//SQLQuery 7
            sqlCommand.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < neededTransactions.Count; i++)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@exportDate", todaysDate);
                sqlCommand.Parameters.AddWithValue("@transactionDate", neededTransactions[i].getTransactionDate());
                sqlCommand.Parameters.AddWithValue("@accountBalance", neededTransactions[i].getBalance_rn());
                sqlCommand.Parameters.AddWithValue("@difference", neededTransactions[i].getTransactionPrice());
                if (neededTransactions[i].getTransactionPrice() > 0)
                {
                    sqlCommand.Parameters.AddWithValue("@income", neededTransactions[i].getTransactionPrice());
                    sqlCommand.Parameters.AddWithValue("@spending", DBNull.Value);
                }
                else
                {
                    sqlCommand.Parameters.AddWithValue("@spending", neededTransactions[i].getTransactionPrice());
                    sqlCommand.Parameters.AddWithValue("@income", DBNull.Value);
                }
                sqlCommand.Parameters.AddWithValue("@comment", neededTransactions[i].getTransactionDescription());
                sqlCommand.Parameters.AddWithValue("@accountNumber", neededTransactions[i].getAccountNumber());
                sqlCommand.Parameters.AddWithValue("@bankName", neededTransactions[i].getBankname());
                sqlCommand.ExecuteNonQuery();
            }
            ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            /*
            WriteWorkbook = excel.Workbooks.Open(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
            WriteWorksheet = WriteWorkbook.Worksheets[1];
            if (neededTransactions != null)
            {
                int row_number = 1;
                while (WriteWorksheet.Cells[row_number, 1].Value != null)
                {
                    row_number++; // get the current last row
                }
                foreach (var transctn in neededTransactions)
                {

                    WriteWorksheet.Cells[row_number, 1].Value = todaysDate;
                    WriteWorksheet.Cells[row_number, 2].Value = transctn.getTransactionDate();
                    WriteWorksheet.Cells[row_number, 3].Value = transctn.getBalance_rn();
                    WriteWorksheet.Cells[row_number, 7].Value = transctn.getTransactionPrice();
                    if (transctn.getTransactionPrice() < 0)
                    {
                        WriteWorksheet.Cells[row_number, 9].Value = transctn.getTransactionPrice();
                        WriteWorksheet.Cells[row_number, 11].Value = transctn.getTransactionPrice();
                        WriteWorksheet.Cells[row_number, 15].Value = "havi";
                    }
                    else
                    {
                        WriteWorksheet.Cells[row_number, 8].Value = transctn.getTransactionPrice();
                        WriteWorksheet.Cells[row_number, 10].Value = transctn.getTransactionPrice();
                        WriteWorksheet.Cells[row_number, 11].Value = transctn.getTransactionPrice();
                        WriteWorksheet.Cells[row_number, 15].Value = "havi";
                    }
                    WriteWorksheet.Cells[row_number, 14].Value = transctn.getTransactionDescription();
                    WriteWorksheet.Cells[row_number, 16].Value = transctn.getAccountNumber();
                    WriteWorksheet.Cells[row_number, 17].Value = transctn.getBankname();
                    row_number++;
                    Range line = (Range)WriteWorksheet.Rows[row_number];
                    line.Insert();
                }
                try
                {
                    //WriteWorkbook.SaveAs(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
                    excel.ActiveWorkbook.Save();
                    excel.Workbooks.Close();
                    excel.Quit();
                }
                catch(Exception e)
                {

                }
                ImportPageBank.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            }
            else
            {
                return;
            }
            */
        }
        private List<Transaction> newTransactions(List<Transaction> importedTransactions) //check if the transaction is already exported or not
        {
            List<Transaction> savedTransactions = SavedTransactions.getSavedTransactionsBank();
            List<Transaction> neededTransactions=new List<Transaction>();
            importerAccountNumber = importedTransactions[0].getAccountNumber();//account number is the same for all
            ThreadStart threadStart = delegate
            {
                writeAccountNumberToSql(importerAccountNumber);
            };
            Thread sqlThread = new Thread(threadStart);
            sqlThread.IsBackground = true;
            sqlThread.Start();
            sqlThread.Join();
            mainWindow.setAccountNumber(importerAccountNumber);
            if (savedTransactions.Count != 0)//if the export file was not empty we scan the list
            {
                List<Transaction> tempTransactions = new List<Transaction>();
                foreach (var saved in savedTransactions)
                {
                   //egy külön listába tesszük azokat az elemeket a már elmentet tranzakciókból ahol a bankszámlaszám
                   //megegyezik az importálandó bankszámlaszámmal
                   if(saved.getAccountNumber().Equals(importerAccountNumber))
                    {
                        tempTransactions.Add(saved);
                    }
                }
                if (tempTransactions.Count != 0)//ha van olyan már elmentett tranzakció aminek az  a bankszámlaszáma mint amit importálni akarunk
                {
                    int explicitImported=0;
                    StreamWriter logFile =new StreamWriter("C:\\Users\\Tocki\\Desktop\\transactionsLog.txt", append:true);
                    foreach (var imported in importedTransactions)
                    {
                        bool redundant = false;
                        foreach (var saved in tempTransactions)
                        {
                            if (saved.getTransactionDate().Equals(imported.getTransactionDate()) &&
                                    saved.getTransactionPrice().Equals(imported.getTransactionPrice()) &&
                                    saved.getBalance_rn() == imported.getBalance_rn())
                            {
                                redundant = true;
                                if (ImportPageBank.getInstance(mainWindow).alwaysAsk==true)
                                {
                                    if (MessageBox.Show("This transaction is most likely to be in the Databse already!\n -- Transaction date: " + imported.getTransactionDate() + "\n-- Transaction price: " + imported.getTransactionPrice()
                                        + "\n-- Imported on: " + saved.getWriteDate().Substring(0,12)+"\nWould you like to import it anyways?",
                                     "Imprt alert!",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        neededTransactions.Add(imported);
                                        explicitImported++;
                                        logFile.WriteLine("AccountNumber: " + imported.getAccountNumber() +
                                            "\n ImportDate: " + imported.getTransactionDate() +
                                            "\n TransactionPrice: " 
                                            + imported.getTransactionPrice()+" *");
                                    }
                                }
                                break;
                            }
                        }
                        if (redundant == false)
                        {
                            neededTransactions.Add(imported);;
                        }
                    }
                    logFile.Close();
                    if (MessageBox.Show("You have imported "+neededTransactions.Count+" new transaction(s)!\n" +
                        "("+(tempTransactions.Count-explicitImported)+" was already imported)", "Import alert!",
                         MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        return neededTransactions;
                    }
                    return neededTransactions;
                }
                else //nincs olyan elmentett tranzakció aminek az lenne a bankszámlaszáma mint amit importálni akarunk
                    //tehát az összeset importáljuk
                {
                    //mainWindow.setTableAttributes(importedTransactions,"empty");
                    if (MessageBox.Show("You have imported " + importedTransactions.Count + " new transaction(s)!\n", "Import alert!",
                         MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        return importedTransactions;
                    }
                    return importedTransactions;
                }
            }
            else // még nincs elmentett tranzakció
                 // tehát az összeset importáljuk
            {
                //mainWindow.setTableAttributes(importedTransactions,"empty");
                if (MessageBox.Show("You have imported " + importedTransactions.Count + " new transaction(s)!\n", "Import alert!",
                         MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    return importedTransactions;
                }
                return importedTransactions;
            }
        }
        private void writeAccountNumberToSql(string accountNumber)
        {
            mConn.Open();
            string storedAccountNumber = mainWindow.getCurrentUser().getAccountNumber();
            string []splittedAccountNumber = storedAccountNumber.Split(',');
            bool stored = false;
            for (int i = 0; i < splittedAccountNumber.Length; i++)
            {
                if (splittedAccountNumber[i]==accountNumber)
                {
                    stored = true;
                    break;
                }
            }
            if (!stored && storedAccountNumber!="-")
            {
                storedAccountNumber += "," + accountNumber;
                string updateAcountNumberQuery = "UPDATE [UserInfo] SET AccountNumber = '" + storedAccountNumber + "' Where Username = '" + mainWindow.getCurrentUser().getUsername() + "'";
                SQLiteCommand insercommand = new SQLiteCommand(updateAcountNumberQuery, mConn);
                insercommand.CommandType = CommandType.Text;
                insercommand.ExecuteNonQuery();
                mainWindow.currentUser.setAccountNumber(storedAccountNumber += "," + accountNumber);
            }
            else if(storedAccountNumber=="-")
            {
                string updateAcountNumberQuery = "UPDATE [UserInfo] SET AccountNumber = '" + accountNumber + "' Where Username = '" + mainWindow.getCurrentUser().getUsername() + "'";
                SQLiteCommand insercommand = new SQLiteCommand(updateAcountNumberQuery, mConn);
                insercommand.CommandType = CommandType.Text;
                insercommand.ExecuteNonQuery();
                mainWindow.currentUser.setAccountNumber(accountNumber);
            }
            mConn.Close();
        }
        public ExportTransactions(List<Stock> transactions, MainWindow mainWindow,string currentFileName)
        {
            string earningMethod = ImportPageStock.getInstance(mainWindow).getMethod();
            //we need the original quantity that's we going to get it from the original transactions List 
            //(after the switch it is zero for every object)

            List<int> quantities = new List<int>();
            for (int i = 0; i < transactions.Count; i++)
                quantities.Add(transactions[i].getQuantity());
            //but we need the profits too , which we going to get from the tempTransactions

            switch(earningMethod)
            {
                case "FIFO":
                    stockExportFIFO(ref transactions);
                    break;
                case "LIFO":
                    stockExportLIFO(ref transactions);
                    break;
                case "CUSTOM":
                    mainWindow.MainFrame.Content = new CustomStockExportPage(mainWindow, transactions,currentFileName);
                    break;
            }
            if (earningMethod == "FIFO" || earningMethod == "LIFO")
            {
                //for DataBaseDataStock
                for (int i = 0; i < transactions.Count; i++)
                {
                    string value = quantities[i] + " (" + transactions[i].getQuantity() + ")";
                    transactions[i].setOriginalAndSellQuantity(value);
                    transactions[i].setImporter(mainWindow.getCurrentUser().getUsername());
                    transactions[i].setEarningMethod(earningMethod);
                }
                MessageBox.Show("Exporting data from: " + currentFileName, "", MessageBoxButton.OK);

                string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
                Regex typeRegex1 = new Regex(@"Eladott");
                Regex typeRegex2 = new Regex(@"Sold");
                Regex typeRegex3 = new Regex(@"Sell");
                Regex typeRegex4 = new Regex(@"Vásárolt");
                Regex typeRegex5 = new Regex(@"Bought");
                Regex typeRegex6 = new Regex(@"Buy");
                mConn.Open();
                for (int i = 0; i < transactions.Count; i++)
                {
                    string insertQuery = "insert into [importedStockTransactions]" +
                    "(ExportDate,TransactionDate,StockName," +
                    "StockPrice,SoldQuantity,BoughtQuantity" +
                    ",CurrentQuantity,Spending,Profit" +
                    ",EarningMethod,ImporterName)" +
                    " values('" + todaysDate + "','" + transactions[i].getTransactionDate() + "','" +
                    transactions[i].getStockName() + "','" + transactions[i].getStockPrice() + "'";
                    if (typeRegex1.IsMatch(transactions[i].getTransactionType()) ||
                       typeRegex2.IsMatch(transactions[i].getTransactionType()) ||
                       typeRegex3.IsMatch(transactions[i].getTransactionType())) //Eladott
                    {
                        insertQuery += ",'" + quantities[i] + "','" +
                            DBNull.Value + "','" +
                            DBNull.Value + "','" +
                            DBNull.Value + "','" + 
                            transactions[i].getProfit() +
                            "','" + transactions[i].getEarningMethod() + "'";
                    }
                    else if (typeRegex4.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex5.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex6.IsMatch(transactions[i].getTransactionType()))//Vásárolt
                    {
                        double stockPrice = double.Parse(transactions[i].getStockPrice(), CultureInfo.InvariantCulture);
                        insertQuery += ",'" + DBNull.Value +
                            "','" + quantities[i] +
                            "','" + transactions[i].getQuantity() +
                            "','" + stockPrice * quantities[i] +
                            "','" + DBNull.Value +
                            "','" + DBNull.Value + "'";
                    }
                    insertQuery += ",'" + transactions[i].getImporter() + "')";
                    SQLiteCommand insertcommand = new SQLiteCommand(insertQuery, mConn);
                    insertcommand.CommandType = CommandType.Text;
                    insertcommand.ExecuteNonQuery();
                }
                SavedTransactions.addToSavedTransactionsStock(transactions);//adding the freshyl imported transactions to the saved 
                ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                /*
                SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                sqlConn.Open();
                SqlCommand sqlCommand = new SqlCommand("insertStockTransaction", sqlConn);//SQLQuery 7
                sqlCommand.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < transactions.Count; i++)
                {
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@exportDate", todaysDate);
                    sqlCommand.Parameters.AddWithValue("@transactionDate", transactions[i].getTransactionDate());
                    sqlCommand.Parameters.AddWithValue("@stockName", transactions[i].getStockName());
                    sqlCommand.Parameters.AddWithValue("@stockPrice", transactions[i].getStockPrice());
                    if (typeRegex1.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex2.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex3.IsMatch(transactions[i].getTransactionType())) //Eladott
                    {
                        sqlCommand.Parameters.AddWithValue("@soldQuantity", quantities[i]);
                        sqlCommand.Parameters.AddWithValue("@profit", transactions[i].getProfit());
                        sqlCommand.Parameters.AddWithValue("@earningMethod", transactions[i].getEarningMethod());
                        sqlCommand.Parameters.AddWithValue("@boughtQuantity", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@spending", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@currentQuantity", DBNull.Value);
                    }
                    else if (typeRegex4.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex5.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex6.IsMatch(transactions[i].getTransactionType()))//Vásárolt
                    {
                        sqlCommand.Parameters.AddWithValue("@boughtQuantity", quantities[i]);
                        sqlCommand.Parameters.AddWithValue("@spending", transactions[i].getStockPrice() * quantities[i]);
                        sqlCommand.Parameters.AddWithValue("@currentQuantity", transactions[i].getQuantity());
                        sqlCommand.Parameters.AddWithValue("@soldQuantity", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@profit", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("@earningMethod", DBNull.Value);
                    }
                    sqlCommand.Parameters.AddWithValue("@importerName", transactions[i].getImporter());
                    sqlCommand.ExecuteNonQuery();
                }
                SavedTransactions.addToSavedTransactionsStock(transactions);//adding the freshyl imported transactions to the saved 
                ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                /*
                WriteWorkbook = excel.Workbooks.Open(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
                WriteWorksheet = WriteWorkbook.Worksheets[2];
                int row_number = 1;
                while (WriteWorksheet.Cells[row_number, 1].Value != null)
                {
                    row_number++; // get the current last row
                }
                for (int i = 0; i < transactions.Count; i++)
                {

                    WriteWorksheet.Cells[row_number, 1].Value = todaysDate;
                    WriteWorksheet.Cells[row_number, 2].Value = transactions[i].getTransactionDate();
                    WriteWorksheet.Cells[row_number, 3].Value = transactions[i].getStockName();
                    WriteWorksheet.Cells[row_number, 4].Value = transactions[i].getStockPrice();
                    if (typeRegex1.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex2.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex3.IsMatch(transactions[i].getTransactionType())) //Eladott
                    {
                        WriteWorksheet.Cells[row_number, 5].Value = quantities[i];                                    //!! eredeti quantity
                        WriteWorksheet.Cells[row_number, 9].Value = transactions[i].getProfit();
                        WriteWorksheet.Cells[row_number, 10].Value = earningMethod;
                    }
                    else if (typeRegex4.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex5.IsMatch(transactions[i].getTransactionType()) ||
                        typeRegex6.IsMatch(transactions[i].getTransactionType()))//Vásárolt
                    {
                        WriteWorksheet.Cells[row_number, 6].Value = quantities[i];                                    //! eredeti quantity
                        WriteWorksheet.Cells[row_number, 8].Value = quantities[i] * transactions[i].getStockPrice();     //!! eredeti quantity
                        WriteWorksheet.Cells[row_number, 7].Value = transactions[i].getQuantity();                       //!! mostani quantity
                    }
                    WriteWorksheet.Cells[row_number, 11].Value = mainWindow.getCurrentUser().getUsername();
                    row_number++;
                    Range line = (Range)WriteWorksheet.Rows[row_number];
                    line.Insert();
                }
                try
                {
                    excel.ActiveWorkbook.Save();
                    excel.Workbooks.Close();
                    excel.Quit();
                }
                catch (Exception e)
                {

                }
                ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
                */
            }
        }
        public ExportTransactions(List<Stock> customTransactions, MainWindow mainWindow, string currentFileName,string customEarning)
        {
            for (int i = 0; i < customTransactions.Count; i++)
            {
                customTransactions[i].setImporter(mainWindow.getCurrentUser().getUsername());
                customTransactions[i].setEarningMethod(customEarning);
            }
            MessageBox.Show("Exporting data from: " + currentFileName, "", MessageBoxButton.OK);
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            Regex typeRegex1 = new Regex(@"Eladott");
            Regex typeRegex2 = new Regex(@"Sold");
            Regex typeRegex3 = new Regex(@"Sell");
            Regex typeRegex4 = new Regex(@"Vásárolt");
            Regex typeRegex5 = new Regex(@"Bought");
            Regex typeRegex6 = new Regex(@"Buy");
            mConn.Open();
            for (int i = 0; i < customTransactions.Count; i++)
            {
                string insertQuery = "insert into [importedStockTransactions]" +
                "(ExportDate,TransactionDate,StockName,StockPrice,SoldQuantity,BoughtQuantity,CurrentQuantity,Spending,Profit,EarningMethod,ImporterName)" +
                " values('" + todaysDate + "','" + customTransactions[i].getTransactionDate() + "','" + customTransactions[i].getStockName() + "','" + customTransactions[i].getStockPrice() + "'";
                if (typeRegex1.IsMatch(customTransactions[i].getTransactionType()) ||
                   typeRegex2.IsMatch(customTransactions[i].getTransactionType()) ||
                   typeRegex3.IsMatch(customTransactions[i].getTransactionType())) //Eladott
                {
                    insertQuery += ",'" + customTransactions[i].getOriginalQuantityForCustomEarning() + "','" + DBNull.Value + "','" + DBNull.Value + "','" + DBNull.Value + "','" + customTransactions[i].getProfit() + "','" + customTransactions[i].getEarningMethod() + "'";
                }
                else if (typeRegex4.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex5.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex6.IsMatch(customTransactions[i].getTransactionType()))//Vásárolt
                {
                    double stockPrice = double.Parse(customTransactions[i].getStockPrice(), CultureInfo.InvariantCulture);
                    insertQuery += ",'" + DBNull.Value + "','" + customTransactions[i].getOriginalQuantityForCustomEarning() + "','" + customTransactions[i].getQuantity() + "','" + stockPrice * customTransactions[i].getOriginalQuantityForCustomEarning() + "','" + DBNull.Value + "','" + DBNull.Value + "'";
                }
                insertQuery += ",'" + customTransactions[i].getImporter() + "')";
                SQLiteCommand insertcommand = new SQLiteCommand(insertQuery, mConn);
                insertcommand.CommandType = CommandType.Text;
                insertcommand.ExecuteNonQuery();
            }
            SavedTransactions.addToSavedTransactionsStock(customTransactions);//adding the freshyl imported transactions to the saved 
            ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            SqlCommand sqlCommand = new SqlCommand("insertStockTransaction", sqlConn);//SQLQuery 7
            sqlCommand.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < customTransactions.Count; i++)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@exportDate", todaysDate);
                sqlCommand.Parameters.AddWithValue("@transactionDate", customTransactions[i].getTransactionDate());
                sqlCommand.Parameters.AddWithValue("@stockName", customTransactions[i].getStockName());
                sqlCommand.Parameters.AddWithValue("@stockPrice", customTransactions[i].getStockPrice());
                if (typeRegex1.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex2.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex3.IsMatch(customTransactions[i].getTransactionType())) //Eladott
                {
                    sqlCommand.Parameters.AddWithValue("@soldQuantity", customTransactions[i].getOriginalQuantityForCustomEarning());
                    sqlCommand.Parameters.AddWithValue("@profit", customTransactions[i].getProfit());
                    sqlCommand.Parameters.AddWithValue("@earningMethod", customTransactions[i].getEarningMethod());
                    sqlCommand.Parameters.AddWithValue("@boughtQuantity", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@spending", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@currentQuantity", DBNull.Value);
                }
                else if (typeRegex4.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex5.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex6.IsMatch(customTransactions[i].getTransactionType()))//Vásárolt
                {
                    sqlCommand.Parameters.AddWithValue("@boughtQuantity", customTransactions[i].getOriginalQuantityForCustomEarning());
                    sqlCommand.Parameters.AddWithValue("@spending", customTransactions[i].getStockPrice() * customTransactions[i].getOriginalQuantityForCustomEarning());
                    sqlCommand.Parameters.AddWithValue("@currentQuantity", customTransactions[i].getQuantity());
                    sqlCommand.Parameters.AddWithValue("@soldQuantity", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@profit", DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@earningMethod", DBNull.Value);
                }
                sqlCommand.Parameters.AddWithValue("@importerName", customTransactions[i].getImporter());
                sqlCommand.ExecuteNonQuery();
            }
            SavedTransactions.addToSavedTransactionsStock(customTransactions);//adding the freshyl imported transactions to the saved 
            ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            /*
            WriteWorkbook = excel.Workbooks.Open(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
            WriteWorksheet = WriteWorkbook.Worksheets[2];
            int row_number = 1;
            while (WriteWorksheet.Cells[row_number, 1].Value != null)
            {
                row_number++; // get the current last row
            }
            for (int i = 0; i < customTransactions.Count; i++)
            {

                WriteWorksheet.Cells[row_number, 1].Value = todaysDate;
                WriteWorksheet.Cells[row_number, 2].Value = customTransactions[i].getTransactionDate();
                WriteWorksheet.Cells[row_number, 3].Value = customTransactions[i].getStockName();
                WriteWorksheet.Cells[row_number, 4].Value = customTransactions[i].getStockPrice();
                if (typeRegex1.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex2.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex3.IsMatch(customTransactions[i].getTransactionType())) //Eladott
                {
                    WriteWorksheet.Cells[row_number, 5].Value = customTransactions[i].getOriginalQuantityForCustomEarning();            //!! eredeti quantity
                    WriteWorksheet.Cells[row_number, 9].Value = customTransactions[i].getProfit();
                    WriteWorksheet.Cells[row_number, 10].Value = customEarning;
                }
                else if (typeRegex4.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex5.IsMatch(customTransactions[i].getTransactionType()) ||
                    typeRegex6.IsMatch(customTransactions[i].getTransactionType()))//Vásárolt
                {
                    WriteWorksheet.Cells[row_number, 6].Value = customTransactions[i].getOriginalQuantityForCustomEarning();                                 //! eredeti quantity
                    WriteWorksheet.Cells[row_number, 8].Value = customTransactions[i].getOriginalQuantityForCustomEarning() * customTransactions[i].getStockPrice();     //!! eredeti quantity
                    WriteWorksheet.Cells[row_number, 7].Value = customTransactions[i].getQuantity();
                }                     //!! mostani quantity
                WriteWorksheet.Cells[row_number, 11].Value = mainWindow.getCurrentUser().getUsername();
                row_number++;
                Range line = (Range)WriteWorksheet.Rows[row_number];
                line.Insert();
            }
            try
            {
                excel.ActiveWorkbook.Save();
                excel.Workbooks.Close();
                excel.Quit();
            }
            catch (Exception e)
            {

            }
            ImportPageStock.getInstance(mainWindow).setUserStatistics(mainWindow.getCurrentUser());
            */
        }
        private void stockExportFIFO(ref List<Stock> allCompany)
        {
            /*Need to add the SavedStocks to this List*/
            /*Should save the left quantity in the Kimutatás excel to make it less time consuming*/
            /*getting the company names*/
            List<string> distinctCompanyNames = new List<string>();
            foreach (var transaction in allCompany)
            {
                if (!distinctCompanyNames.Contains(transaction.getStockName()))
                    distinctCompanyNames.Add(transaction.getStockName());
            }
            /*getting the company names*/

            /*To keep the original order of Stocks*/
            Dictionary<Stock, int> transactionMap = new Dictionary<Stock, int>();

            List<Stock> company;
            bool allFinished = false;
            while (!allFinished)
            {
                if (distinctCompanyNames.Count != 0)
                {
                    company = new List<Stock>();
                    //removing the companies we done calculating
                    string companyName = distinctCompanyNames[0];

                    /*Separating the Stocks based on CompanyNames*/
                    /*If we add it to the separate list we also save the original index,to set the profit,and keep the same order*/
                    /*and also make a help Quantity value for Bought stocks to the export file*/
                    for (int i = 0; i < allCompany.Count; i++)
                    {
                        if (allCompany[i].getStockName() == companyName)
                        {
                            company.Add(allCompany[i]);
                            transactionMap.Add(allCompany[i], i);
                        }
                    }
                    /*Separating the Stocks based on CompanyNames*/
                    /*If we add it to the separate list we also save the original index,to set the proft,and keep the same order*/

                    //Megtaláljuk Hátulról a legelső eladást
                    Stock soldStock = null;
                    Stock boughtStock = null;
                    int totalCount = company.Count - 1;
                    int soldIndex = -1;
                    int boughtIndex = -1;
                    bool finished = false;
                    while (!finished)
                    {
                        if (totalCount > 0)
                        {
                            for (int i = totalCount; i >= 0; i--)
                            {
                                Regex quantityRegex1 = new Regex(@"Eladott");
                                Regex quantityRegex2 = new Regex(@"Sold");
                                Regex quantityRegex3 = new Regex(@"Sell");
                                if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                {
                                    if (company[i].getQuantity() > 0)
                                    {
                                        soldStock = company[i];
                                        soldIndex = i;
                                        break;
                                    }
                                }
                            }
                            if ((soldStock != null) && (soldStock.getQuantity() > 0))
                            {
                                for (int i = totalCount; i >= soldIndex + 1; i--)
                                {
                                    Regex quantityRegex1 = new Regex(@"Vásárolt");
                                    Regex quantityRegex2 = new Regex(@"Bought");
                                    Regex quantityRegex3 = new Regex(@"Buy");
                                    if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                    {
                                        if (company[i].getQuantity() > 0)
                                        {
                                            boughtStock = company[i];
                                            boughtIndex = i;
                                            break;
                                        }
                                    }
                                }
                                if ((boughtStock != null) && (boughtStock.getQuantity() > 0))
                                {
                                    double profit = 0;
                                    if ((boughtStock.getQuantity() - soldStock.getQuantity()) == 0)
                                    {
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                        totalCount = boughtIndex--;
                                        boughtStock.setQuantity(0);
                                        soldStock.setQuantity(0);
                                    }
                                    else if (soldStock.getQuantity() > boughtStock.getQuantity())
                                    {
                                        //it's important to multiple it by the boughtStock,
                                        //because the soldStock quantity is higher than the bought
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * boughtStock.getQuantity();
                                        int leftQuantity = soldStock.getQuantity() - boughtStock.getQuantity();
                                        soldStock.setQuantity(leftQuantity);
                                        boughtStock.setQuantity(0);
                                        while (soldStock.getQuantity() != 0)
                                        {
                                            /*this if means that, we "run" out of bought quantity, and the next Stock would be the SoldStock, but we still have quantity to sell*/
                                            if (boughtIndex - 1 != soldIndex)
                                            {
                                                for (int i = boughtIndex - 1; i > soldIndex; i--)
                                                {
                                                    Regex quantityRegex1 = new Regex(@"Vásárolt");
                                                    Regex quantityRegex2 = new Regex(@"Bought");
                                                    Regex quantityRegex3 = new Regex(@"Buy");
                                                    if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                                    {
                                                        boughtStock = company[i];
                                                        boughtIndex = i;
                                                        break;
                                                    }
                                                }
                                                /**
                                                 * We change the bought Stocks quantity because if we have other SOLD stocks we dont want it to count
                                                 * with the full quantity (There would be a mistake)
                                                 **/
                                                if (boughtStock.getQuantity() > 0)
                                                {
                                                    if (soldStock.getQuantity() > boughtStock.getQuantity())
                                                    {
                                                        leftQuantity = soldStock.getQuantity() - boughtStock.getQuantity();
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * boughtStock.getQuantity();
                                                        totalCount = boughtIndex--;
                                                    }
                                                    else if (boughtStock.getQuantity() > soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                                        int leftBoughtQuantity = boughtStock.getQuantity() - soldStock.getQuantity();
                                                        boughtStock.setQuantity(leftBoughtQuantity);
                                                    }
                                                    else if (boughtStock.getQuantity() == soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                                        boughtStock.setQuantity(0);
                                                        totalCount = boughtIndex--;
                                                    }
                                                    soldStock.setQuantity(leftQuantity);
                                                }
                                                else
                                                {
                                                    boughtIndex--;
                                                }
                                            }
                                            else//we reached the sell transaction but the quantity is still not zero, ? to do in that case
                                            {
                                                soldStock.setQuantity(0);
                                            }
                                        }
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                    }
                                    else if ((boughtStock.getQuantity() - soldStock.getQuantity()) > 0)
                                    {
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                        int leftBoughtStock = boughtStock.getQuantity() - soldStock.getQuantity();
                                        company.Find(i => i == boughtStock).setQuantity(leftBoughtStock);
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                        soldStock.setQuantity(0);
                                        boughtStock.setQuantity(leftBoughtStock);
                                    }
                                }
                                else
                                {
                                    finished = true;
                                    distinctCompanyNames.RemoveAt(0);
                                }
                                if (boughtStock.getQuantity() > 0)
                                    allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                            }
                            else
                            {
                                finished = true;
                                distinctCompanyNames.RemoveAt(0);
                            }
                            if (boughtStock.getQuantity() > 0)
                                allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                        }
                        else
                        {
                            finished = true;
                            distinctCompanyNames.RemoveAt(0);
                        }
                        if (boughtStock.getQuantity() > 0)
                            allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                    }
                }
                else
                {
                    allFinished = true;
                }
            }
        }
        private void stockExportLIFO(ref List<Stock> allCompany)
        {
            /*Need to add the SavedStocks to this List*/
            /*Should save the left quantity in the Kimutatás excel to make it less time consuming*/
            /*getting the company names*/
            List<string> distinctCompanyNames = new List<string>();
            foreach (var transaction in allCompany)
            {
                if (!distinctCompanyNames.Contains(transaction.getStockName()))
                    distinctCompanyNames.Add(transaction.getStockName());
            }
            /*getting the company names*/

            /*To keep the original order of Stocks*/
            Dictionary<Stock, int> transactionMap = new Dictionary<Stock, int>();

            List<Stock> company;
            bool allFinished = false;
            while (!allFinished)
            {
                if (distinctCompanyNames.Count != 0)
                {
                    company = new List<Stock>();
                    //removing the companies we done calculating
                    string companyName = distinctCompanyNames[0];

                    /*Separating the Stocks based on CompanyNames*/
                    /*If we add it to the separate list we also save the original index,to set the profit,and keep the same order*/
                    /*and also make a help Quantity value for Bought stocks to the export file*/
                    for (int i = 0; i < allCompany.Count; i++)
                    {
                        if (allCompany[i].getStockName() == companyName)
                        {
                            company.Add(allCompany[i]);
                            transactionMap.Add(allCompany[i], i);
                        }
                    }
                    /*Separating the Stocks based on CompanyNames*/
                    /*If we add it to the separate list we also save the original index,to set the proft,and keep the same order*/

                    //Megtaláljuk Hátulról a legelső eladást
                    Stock soldStock = null;
                    Stock boughtStock = null;
                    int totalCount = company.Count - 1;
                    int soldIndex = -1;
                    int boughtIndex = -1;
                    bool finished = false;
                    while (!finished)
                    {
                        if (totalCount > 0)
                        {
                            for (int i = totalCount; i >= 0; i--)
                            {
                                Regex quantityRegex1 = new Regex(@"Eladott");
                                Regex quantityRegex2 = new Regex(@"Sold");
                                Regex quantityRegex3 = new Regex(@"Sell");
                                if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                {
                                    if (company[i].getQuantity() > 0)
                                    {
                                        soldStock = company[i];
                                        soldIndex = i;
                                        break;
                                    }
                                }
                            }
                            if ((soldStock != null) && (soldStock.getQuantity() > 0))
                            {
                                for (int i = soldIndex + 1; i <= totalCount; i++)
                                {
                                    Regex quantityRegex1 = new Regex(@"Vásárolt");
                                    Regex quantityRegex2 = new Regex(@"Bought");
                                    Regex quantityRegex3 = new Regex(@"Buy");
                                    if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                    {
                                        if (company[i].getQuantity() > 0)
                                        {
                                            boughtStock = company[i];
                                            boughtIndex = i;
                                            break;
                                        }
                                    }
                                }
                                if ((boughtStock != null) && (boughtStock.getQuantity() > 0))
                                {
                                    double profit = 0;
                                    if ((boughtStock.getQuantity() == soldStock.getQuantity()))
                                    {
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                        boughtStock.setQuantity(0);
                                        soldStock.setQuantity(0);
                                    }
                                    else if (soldStock.getQuantity() > boughtStock.getQuantity())
                                    {
                                        //it's important to multiple it by the boughtStock,
                                        //because the soldStock quantity is higher than the bought
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * boughtStock.getQuantity();
                                        int leftQuantity = soldStock.getQuantity() - boughtStock.getQuantity();
                                        soldStock.setQuantity(leftQuantity);
                                        boughtStock.setQuantity(0);
                                        while (soldStock.getQuantity() != 0)
                                        {
                                            /*this if means that, we "run" out of bought quantity, and the next Stock would be the SoldStock, but we still have quantity to sell*/
                                            if (boughtIndex - 1 != soldIndex)
                                            {
                                                for (int i = soldIndex; i < boughtIndex + 1; i++)
                                                {
                                                    Regex quantityRegex1 = new Regex(@"Vásárolt");
                                                    Regex quantityRegex2 = new Regex(@"Bought");
                                                    Regex quantityRegex3 = new Regex(@"Buy");
                                                    if (quantityRegex1.IsMatch(company[i].getTransactionType()) ||
                                                        quantityRegex2.IsMatch(company[i].getTransactionType()) ||
                                                        quantityRegex3.IsMatch(company[i].getTransactionType()))
                                                    {
                                                        if (company[i].getQuantity() > 0)
                                                        {
                                                            boughtStock = company[i];
                                                            boughtIndex = i;
                                                            break;
                                                        }
                                                    }
                                                }
                                                /**
                                                 * We change the bought Stocks quantity because if we have other SOLD stocks we dont want it to count
                                                 * with the full quantity (There would be a mistake)
                                                 **/
                                                if (boughtStock.getQuantity() > 0)
                                                {
                                                    if (soldStock.getQuantity() > boughtStock.getQuantity())
                                                    {
                                                        leftQuantity = soldStock.getQuantity() - boughtStock.getQuantity();
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * boughtStock.getQuantity();
                                                        boughtStock.setQuantity(0);
                                                    }
                                                    else if (boughtStock.getQuantity() > soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                                        int leftBoughtQuantity = boughtStock.getQuantity() - soldStock.getQuantity();
                                                        boughtStock.setQuantity(leftBoughtQuantity);
                                                    }
                                                    else if (boughtStock.getQuantity() == soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                                        boughtStock.setQuantity(0);
                                                    }
                                                    soldStock.setQuantity(leftQuantity);
                                                }
                                                else
                                                {
                                                    boughtIndex++;
                                                }
                                            }
                                            else//we reached the sell transaction but the quantity is still not zero, ? to do in that case
                                            {
                                                soldStock.setQuantity(0);
                                            }
                                        }
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                    }
                                    else if ((boughtStock.getQuantity() > soldStock.getQuantity()))
                                    {
                                        profit = (double.Parse(soldStock.getStockPrice()) - double.Parse(boughtStock.getStockPrice())) * soldStock.getQuantity();
                                        int leftBoughtStock = boughtStock.getQuantity() - soldStock.getQuantity();
                                        company.Find(i => i == boughtStock).setQuantity(leftBoughtStock);
                                        soldStock.setQuantity(0);
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit.ToString());
                                    }
                                }
                                else
                                {
                                    finished = true;
                                    distinctCompanyNames.RemoveAt(0);
                                    if (boughtStock.getQuantity() > 0)
                                        allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                                }
                            }
                            else
                            {
                                finished = true;
                                distinctCompanyNames.RemoveAt(0);
                                if (boughtStock.getQuantity() > 0)
                                    allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                            }
                        }
                        else
                        {
                            finished = true;
                            distinctCompanyNames.RemoveAt(0);
                            if (boughtStock.getQuantity() > 0)
                                allCompany.Find(i => i == boughtStock).setQuantity(boughtStock.getQuantity());
                        }
                    }
                }
                else
                {
                    allFinished = true;
                }
            }
        }

        public string geImporterAccountNumber()
        {
            return importerAccountNumber;
        }
        public void setimporterAccountNumber(string value)
        {
            importerAccountNumber = value;
        }
        ~ExportTransactions()
        {
            try
            {
                mConn.Close();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
