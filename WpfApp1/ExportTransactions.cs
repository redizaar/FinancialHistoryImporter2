using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Windows;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace WpfApp1
{
    class ExportTransactions
    {
        Workbook WriteWorkbook;
        Worksheet WriteWorksheet;
        _Application excel = new _Excel.Application();
        private MainWindow mainWindow;
        private string importerAccountNumber;
        public ExportTransactions(List<Transaction> transactions,MainWindow mainWindow,string currentFileName)
        {
            this.mainWindow = mainWindow;
            for (int i = 0; i < transactions.Count; i++)
            {
                string [] spaceSplitted=transactions[i].getTransactionDate().Split(' ');
                string dateString="";
                for (int j = 0; j < spaceSplitted.Length; j++)
                    dateString += spaceSplitted[j];
                Console.WriteLine(dateString.Substring(0,11));
            }
            MessageBox.Show("Exporting data from: " + currentFileName, "", MessageBoxButton.OK);
                                                    //BUT FIRST - check if the transaction is already exported or not 
            List<Transaction> neededTransactions = newTransactions(transactions);
            SavedTransactions.addToSavedTransactionsBank(neededTransactions);//adding the freshyl imported transactions to the saved 
            WriteWorkbook = excel.Workbooks.Open(@"C:\Users\Tocki\Desktop\Kimutatas.xlsx");
            WriteWorksheet = WriteWorkbook.Worksheets[1];
            if (neededTransactions != null)
            {
                string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
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
            string storedAccountNumber="-"; //alapértelmezett érték ha valaki regisztrált és még nem importált
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            string accountNumberQuery = "Select * From [UserDatas] where Username = '" + mainWindow.getCurrentUser().getUsername() + "'";
            SqlDataAdapter sda = new SqlDataAdapter(accountNumberQuery, sqlConn);
            System.Data.DataTable dtb = new System.Data.DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count == 1)
            {
                foreach(System.Data.DataRow row in dtb.Rows)
                {
                    storedAccountNumber = row["AccountNumber"].ToString();
                }
            }
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
                using (SqlCommand command = sqlConn.CreateCommand())
                {
                    command.CommandText = "UPDATE UserDatas SET AccountNumber = '" + storedAccountNumber + "' Where Username = '" + mainWindow.getCurrentUser().getUsername() + "'";
                    sqlConn.Open();
                    command.ExecuteNonQuery();
                    sqlConn.Close();
                }
                mainWindow.currentUser.setAccountNumber(storedAccountNumber += "," + accountNumber);
            }
            else if(storedAccountNumber=="-")
            {
                storedAccountNumber = accountNumber;
                using (SqlCommand command = sqlConn.CreateCommand())
                {
                    command.CommandText = "UPDATE UserDatas SET AccountNumber = '" + storedAccountNumber + "' Where Username = '" + mainWindow.getCurrentUser().getUsername() + "'";
                    sqlConn.Open();
                    command.ExecuteNonQuery();
                    sqlConn.Close();
                }
                mainWindow.currentUser.setAccountNumber(accountNumber);
            }
        }
        public ExportTransactions(List<Stock> transactions, MainWindow mainWindow,string currentFileName)
        {
            string earningMethod = ImportPageStock.getInstance(mainWindow).getMethod();
            switch(earningMethod)
            {
                case "FIFO":
                    stockExportFIFO(transactions);
                    break;
                case "LIFO":
                    stockExportLIFO(transactions);
                    break;
                case "CUSTOM":
                    break;
            }
        }
        private void stockExportFIFO(List<Stock> allCompany)
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
                            if ((soldStock != null) && (soldStock.getQuantity()>0))
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
                                if ((boughtStock != null) && (boughtStock.getQuantity()>0))
                                {
                                    double profit = 0;
                                    if ((boughtStock.getQuantity() - soldStock.getQuantity()) == 0)
                                    {
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                        totalCount = boughtIndex--;
                                        boughtStock.setQuantity(0);
                                        soldStock.setQuantity(0);
                                    }
                                    else if (soldStock.getQuantity() > boughtStock.getQuantity())
                                    {
                                        //it's important to multiple it by the boughtStock,
                                        //because the soldStock quantity is higher than the bought
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtStock.getQuantity();
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
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtStock.getQuantity();
                                                        totalCount = boughtIndex--;
                                                    }
                                                    else if (boughtStock.getQuantity() > soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                                        int leftBoughtQuantity = boughtStock.getQuantity() - soldStock.getQuantity();
                                                        boughtStock.setQuantity(leftBoughtQuantity);
                                                    }
                                                    else if (boughtStock.getQuantity() == soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                                        boughtStock.setQuantity(0);
                                                        totalCount = boughtIndex--;
                                                    }
                                                    soldStock.setQuantity(leftQuantity);
                                                }
                                                else
                                                {
                                                    finished = true;
                                                }
                                            }
                                            else//we reached the sell transaction but the quantity is still not zero, ? to do in that case
                                            {
                                                soldStock.setQuantity(0);
                                            }
                                        }
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                    }
                                    else if ((boughtStock.getQuantity() - soldStock.getQuantity()) > 0)
                                    {
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                        int leftBoughtStock = boughtStock.getQuantity() - soldStock.getQuantity();
                                        company.Find(i => i == boughtStock).setQuantity(leftBoughtStock);
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                    }
                                }
                                else
                                {
                                    finished = true;
                                    distinctCompanyNames.RemoveAt(0);
                                }
                            }
                            else
                            {
                                finished = true;
                                distinctCompanyNames.RemoveAt(0);
                            }
                        }
                        else
                        {
                            finished = true;
                            distinctCompanyNames.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    allFinished = true;
                }
            }
            foreach(var test in allCompany)
            {
                Console.WriteLine("Share name:" +test.getStockName() + " Transaction Price: " +test.getStockPrice()+" Transaction type: "+test.getTransactionType());
                Console.WriteLine("Profit: " + test.getProfit());
            }
        }
        private void stockExportLIFO(List<Stock> allCompany)
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
                                for (int i = soldIndex + 1; i<=totalCount; i++)
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
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                        boughtStock.setQuantity(0);
                                        soldStock.setQuantity(0);
                                    }
                                    else if (soldStock.getQuantity() > boughtStock.getQuantity())
                                    {
                                        //it's important to multiple it by the boughtStock,
                                        //because the soldStock quantity is higher than the bought
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtStock.getQuantity();
                                        int leftQuantity = soldStock.getQuantity() - boughtStock.getQuantity();
                                        soldStock.setQuantity(leftQuantity);
                                        boughtStock.setQuantity(0);
                                        while (soldStock.getQuantity() != 0)
                                        {
                                            /*this if means that, we "run" out of bought quantity, and the next Stock would be the SoldStock, but we still have quantity to sell*/
                                            if (boughtIndex - 1 != soldIndex)
                                            {
                                                for (int i = soldIndex; i < boughtIndex+1; i++)
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
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * boughtStock.getQuantity();
                                                    }
                                                    else if (boughtStock.getQuantity() > soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                                        int leftBoughtQuantity = boughtStock.getQuantity() - soldStock.getQuantity();
                                                        boughtStock.setQuantity(leftBoughtQuantity);
                                                    }
                                                    else if (boughtStock.getQuantity() == soldStock.getQuantity())
                                                    {
                                                        leftQuantity = 0;
                                                        profit += (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                                        boughtStock.setQuantity(0);
                                                    }
                                                    soldStock.setQuantity(leftQuantity);
                                                }
                                                else
                                                {
                                                    finished = true;
                                                }
                                            }
                                            else//we reached the sell transaction but the quantity is still not zero, ? to do in that case
                                            {
                                                soldStock.setQuantity(0);
                                            }
                                        }
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                    }
                                    else if ((boughtStock.getQuantity() - soldStock.getQuantity()) > 0)
                                    {
                                        profit = (soldStock.getStockPrice() - boughtStock.getStockPrice()) * soldStock.getQuantity();
                                        int leftBoughtStock = boughtStock.getQuantity() - soldStock.getQuantity();
                                        company.Find(i => i == boughtStock).setQuantity(leftBoughtStock);
                                        int index = transactionMap[soldStock];
                                        allCompany[index].setProfit(profit);
                                    }
                                }
                                else
                                {
                                    finished = true;
                                    distinctCompanyNames.RemoveAt(0);
                                }
                            }
                            else
                            {
                                finished = true;
                                distinctCompanyNames.RemoveAt(0);
                            }
                        }
                        else
                        {
                            finished = true;
                            distinctCompanyNames.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    allFinished = true;
                }
            }
            foreach (var test in allCompany)
            {
                Console.WriteLine("Share name:" + test.getStockName() + " Transaction Price: " + test.getStockPrice() + " Transaction type: " + test.getTransactionType());
                Console.WriteLine("Profit: " + test.getProfit());
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
            excel.Application.Quit();
            excel.Quit();
        }
    }
}
