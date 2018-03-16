using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using System.Globalization;

namespace WpfApp1
{
    public class TemplateStockReadIn
    {
        private string folderAddresses;
        private ImportReadIn stockHandler;
        private Workbook workbook;
        private Worksheet stockWorksheet;
        private _Application excel = new _Excel.Application();
        private string temporaryExcel="";
        private Dictionary<string, string> companyToCSV;
        private Dictionary<string, string> companyToTicker;
        private List<Stock> importedStocks;
        private int companyNameColumn;
        private int transactionDateColumn;
        private int priceColumn;
        private int quantityColumn;
        private int transactionTypeColumn;
        public TemplateStockReadIn(ImportReadIn _stockHandler,string filePath)
        {
            stockHandler = _stockHandler;
            folderAddresses = filePath;
            workbook = excel.Workbooks.Open(folderAddresses);
            stockWorksheet = workbook.Worksheets[1];
        }
        public void analyzeStockTransactionFile()
        {
            companyNameColumn = getCompanyColumn();
            transactionDateColumn = getDateColumn();
            getHistoricalStockPrice(companyNameColumn, transactionDateColumn);
            priceColumn=getPricesToDatesFromCSV(companyNameColumn,transactionDateColumn);
            quantityColumn = getQuantityColumn();
            transactionTypeColumn = getTransactionType();
        }

        private int getTransactionType()
        {
            Regex quantityRegex1 = new Regex(@"Vásárolt");
            Regex quantityRegex2 = new Regex(@"Eladott");
            Regex quantityRegex3 = new Regex(@"Bought");
            Regex quantityRegex4 = new Regex(@"Sold");
            Regex quantityRegex5 = new Regex(@"Buy");
            Regex quantityRegex6 = new Regex(@"Sell");
            int blank_cell_counter = 0;
            int row = 1;
            int column = 1;
            while (row <= 4)
            {
                column=1;
                blank_cell_counter = 0;
                while (blank_cell_counter < 2)
                {
                    if (stockWorksheet.Cells[row, column].Value != null)
                    {
                        blank_cell_counter = 0;
                        string cellValue = stockWorksheet.Cells[row, column].Value.ToString();
                        if (quantityRegex1.IsMatch(cellValue) ||
                            quantityRegex2.IsMatch(cellValue) ||
                            quantityRegex3.IsMatch(cellValue) ||
                            quantityRegex4.IsMatch(cellValue))
                        {
                            return column;
                        }
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    column++;
                }
                row++;
            }
            return 0;
        }

        private int getQuantityColumn()
        {
            Regex quantityRegex1 = new Regex(@"Quantity");
            Regex quantityRegex2 = new Regex(@"Mennyiség");
            Regex quantityRegex3 = new Regex(@"quantity");
            Regex quantityRegex4 = new Regex(@"mennyiség");
            int blank_cell_counter = 0;
            int row = 1;
            int column = 1;
            while (row <= 2)
            {
                blank_cell_counter = 0;
                while (blank_cell_counter < 2)
                {
                    if (stockWorksheet.Cells[row, column].Value != null)
                    {
                        blank_cell_counter = 0;
                        string cellValue = stockWorksheet.Cells[row, column].Value.ToString();
                        if (quantityRegex1.IsMatch(cellValue) ||
                            quantityRegex2.IsMatch(cellValue) ||
                            quantityRegex3.IsMatch(cellValue) ||
                            quantityRegex4.IsMatch(cellValue))
                        {
                            return column;
                        }
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    column++;
                }
                row++;
            }
            return 0;
        }

        public void readOutTransactions()
        {
            if ((companyNameColumn != 0) && (transactionDateColumn != 0) && (priceColumn != 0))
            {
                importedStocks = new List<Stock>();
                int blank_cell_counter = 0;
                int row = 2;
                while (blank_cell_counter < 2)
                {
                    if ((stockWorksheet.Cells[row, companyNameColumn].Value != null) &&
                            (stockWorksheet.Cells[row, transactionDateColumn].Value != null) &&
                            (stockWorksheet.Cells[row, priceColumn].Value != null))
                    {
                        blank_cell_counter = 0;
                        string companyName = stockWorksheet.Cells[row, companyNameColumn].Value.ToString();
                        string transactionDate = stockWorksheet.Cells[row, transactionDateColumn].Value.ToString();
                        string transactionPriceString = stockWorksheet.Cells[row, priceColumn].Value.ToString().Replace(',', '.');
                        double transactionPrice = 0;
                        try
                        {
                            transactionPrice = double.Parse(transactionPriceString, CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {

                        }
                        string transactionType = "-";
                        string quantityString = "";
                        int quantity = 0;
                        if (stockWorksheet.Cells[row, transactionTypeColumn].Value != null)
                        {
                            transactionType = stockWorksheet.Cells[row, transactionTypeColumn].Value.ToString();
                        }
                        if (stockWorksheet.Cells[row, quantityColumn].Value != null)
                        {
                            quantityString = stockWorksheet.Cells[row, quantityColumn].Value.ToString();
                            try
                            {
                                quantity = int.Parse(quantityString);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        Stock stock = new Stock(companyName, transactionPrice, quantity, transactionDate, transactionType);
                        importedStocks.Add(stock);
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    row++;
                }
                stockHandler.addTransactions(importedStocks);
            }
        }
        private int getPricesToDatesFromCSV(int companyNameColumn,int transactionDateColumn)
        {
            int blank_cell_counter = 0;
            int row = 2;
            string companyName;
            string transactionDate;
            while(blank_cell_counter<2)
            {
                if ((stockWorksheet.Cells[row, companyNameColumn].Value != null) && (stockWorksheet.Cells[row, transactionDateColumn].Value!=null))
                {
                    companyName = stockWorksheet.Cells[row, companyNameColumn].Value.ToString();
                    transactionDate = stockWorksheet.Cells[row, transactionDateColumn].Value.ToString();
                    foreach(var cToCSV in companyToCSV)
                    {
                        if(companyName==cToCSV.Key)
                        {
                            double dayHighest = 0;
                            double dayLowest = 0;
                            getDayHighAndDayLowPrice(cToCSV.Value, transactionDate, ref dayHighest, ref dayLowest);
                            int blank_column_counter = 0;
                            //we go through the columns
                            int column = 1;
                            while (blank_column_counter<2)
                            {
                                if(stockWorksheet.Cells[row,column].Value!=null)
                                {
                                    blank_column_counter = 0;
                                    try
                                    {
                                        string cellValue = stockWorksheet.Cells[row, column].Value.ToString().Replace(',','.');
                                        double transactionPrice = double.Parse(cellValue, CultureInfo.InvariantCulture);
                                        if((transactionPrice>= dayLowest && transactionPrice<=dayHighest) ||
                                                (transactionPrice>=(dayLowest-(dayLowest*0.03)) && (transactionPrice<=(dayHighest*1.03))))
                                        {
                                            return column;
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        //cellvalue is not a number
                                    }
                                }
                                else
                                {
                                    blank_column_counter++;
                                }
                                column++;
                            }
                        }
                    }
                }
                else
                {
                    blank_cell_counter++;
                }
                row++;
            }
            return 0;
        }
        private void getDayHighAndDayLowPrice(string csvString,string transactionDate,ref double highPrice,ref double lowPrice)
        {
            string month = "";
            Regex dateRegex1 = new Regex(@"^20\d{2}.\s\d{2}.\s\d{2}");
            Regex dateRegex2 = new Regex(@"^20\d{2}.\d{2}.\d{2}");
            Regex dateRegex3 = new Regex(@"^\d{2}-[\u0000-\u00FF]{3}.-\d{4}$");
            Regex dateRegex4 = new Regex(@"^\d{2}-[\u0000-\u00FF]{4}.-\d{4}$");
            if (dateRegex1.IsMatch(transactionDate))
            {
                string[] splitted = transactionDate.Split(' ');
                string dateCellValueFormatted = "";
                for (int k = 0; k < splitted.Length; k++)
                {
                    dateCellValueFormatted += splitted[k];
                }
                dateCellValueFormatted=dateCellValueFormatted.Replace('.', '-');
                //2018-02-28
                transactionDate = convertDateToCompare(dateCellValueFormatted);
            }
            else if (dateRegex2.IsMatch(transactionDate))
            {
                //2018-02-28
                transactionDate=transactionDate.Replace('.', '-');
                transactionDate = convertDateToCompare(transactionDate);
            }
            else if (dateRegex3.IsMatch(transactionDate))
            {
                switch (transactionDate.Substring(3, 3))
                {
                    case "maj":
                        month = "may";
                        break;
                    case "okt":
                        month = "oct";
                        break;
                }
                //05-may.-2018
                transactionDate=transactionDate.Replace(transactionDate.Substring(3, 3), month);
                transactionDate = transactionDate.Remove(6, 1);
                //05-may-2018
                string temp = "";
                string[] splitted = transactionDate.Split('-');
                splitted[2] = splitted[2].Remove(0, 2);
                for (int i = 0; i < splitted.Length; i++)
                {
                    if (i != splitted.Length - 1)
                        temp += splitted[i] + '-';
                    else
                        temp += splitted[i];
                }
                transactionDate = temp;
            }
            else if (dateRegex4.IsMatch(transactionDate))
            {
                string temp = "";
                //05-febr.-2018
                transactionDate=transactionDate.Remove(6, 2);
                //05-feb-2018
                string[] splitted = transactionDate.Split('-');
                splitted[2] = splitted[2].Remove(0, 2);
                for (int i = 0; i < splitted.Length; i++)
                {
                    if (i != splitted.Length - 1)
                        temp += splitted[i] + '-';
                    else
                        temp += splitted[i];
                }
                transactionDate = temp;
            }
            string[] words = csvString.Split(',');
            int j = 0;;
            Regex regex1 = new Regex(@"[0-9]{2}-[a-zA-Z]{3}-[0-9]{2}", RegexOptions.IgnoreCase);
            Regex regex2 = new Regex(@"[0-9]-[a-zA-Z]{3}-[0-9]{2}", RegexOptions.IgnoreCase);
            for (int i = 0; i < words.Length; i++)
            {
                if ((regex1.IsMatch(words[i])) || (regex2.IsMatch(words[i])))
                {
                    string[] date = words[i].Split('\n');
                    if(string.Equals(date[1],transactionDate, StringComparison.CurrentCultureIgnoreCase))
                    {
                        highPrice = double.Parse(words[i+2].Replace('.', ','));
                        lowPrice = double.Parse(words[i+3].Replace('.', ','));
                    }
                }
            }
        }
        //right know olny works for NASDAQ,NYSE
        private void getHistoricalStockPrice(int companyColumn, int dateColumn)
        {
            List<string> companyNames = collectCompanyNames(companyColumn);
            List<string> copyOfCompanyNames = new List<string>(companyNames);
            Dictionary<string, string> companyToDate = collectOldestShareDates(copyOfCompanyNames, companyColumn,dateColumn);
            companyToTicker = new Dictionary<string, string>();
            string companyNamesCSV;
            using (var web = new WebClient())
            {
                var url = $"http://www.nasdaq.com/screening/companies-by-industry.aspx?render=download";
                companyNamesCSV = web.DownloadString(url);
            }
            Regex reg = new Regex("\"([^\"]*?)\"");
            var matches = reg.Matches(companyNamesCSV).
                Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
            for (int i = 9; i < matches.Length; i += 9)
            {
                for(int j=0;j<companyNames.Count;j++)
                {
                    string[] splitted1 = matches[i+1].Split('"');
                    string companyName = "";
                    for (int k = 0; k < splitted1.Length; k++)
                        companyName += splitted1[k];
                    if (matches[i+1].Contains(companyNames[j]) || levenshteinDistance(companyNames[j],companyName)==1)
                    {
                        string [] splitted = matches[i].Split('"');
                        string ticker = "";
                        for (int k = 0; k < splitted.Length; k++)
                            ticker += splitted[k];
                        companyToTicker.Add(companyNames[j], ticker);
                    }
                }
                //Console.WriteLine("Ticker: {0} -> Company name :{1} ", matches[i], matches[i + 1]);
            }
            getCSVdataFromGoogle(companyToDate,companyToTicker);
        }

        private void getCSVdataFromGoogle(Dictionary<string, string> companyToDate, Dictionary<string, string> companyToTicker)
        {
            companyToCSV = new Dictionary<string, string>();
            foreach (var cToTicker in companyToTicker)
            {
                foreach(var cToDate in companyToDate)
                {
                    if(cToTicker.Key==cToDate.Key)
                    {
                        string csv;
                        using (var web = new WebClient())
                        {
                            string url = "https://finance.google.com/finance/historical?q=" + cToTicker.Value + "&startdate=" + cToDate.Value + "&output=csv";
                            //$"https://finance.google.com/finance/historical?q=AAPL&startdate=01-Jan-2016&output=csv";
                            csv = web.DownloadString(url);
                            string companyName = cToTicker.Key;
                            companyToCSV.Add(companyName, csv);
                        }
                    }
                }
            }
        }

        private Dictionary<string, string> collectOldestShareDates(List<string> companyNames,int companyColumn, int dateColumn)
        {
            int blank_cell_counter = 0;
            int row=1;
            Dictionary<string, string> companyToOldestDate = new Dictionary<string, string>();
            while(blank_cell_counter<2)
            {
                if(stockWorksheet.Cells[row,1].Value!=null)
                {
                    blank_cell_counter = 0;
                }
                else
                {
                    blank_cell_counter++;
                }
                row++;
            }
            for(int i=row-2;i>1;i--)
            {
                if (companyNames.Count == 0)
                {
                    //ha elfogyott a "begyűjtendő" cégnevek száma (megvan az összeshez a legrégebbi dátum)
                    break;
                }
                else
                {
                    if (stockWorksheet.Cells[i, companyColumn].Value != null)
                    {
                        for (int j = 0; j < companyNames.Count; j++)
                        {
                            if (stockWorksheet.Cells[i, companyColumn].Value.ToString() == companyNames[j])
                            {
                                //$"https://finance.google.com/finance/historical?q=AAPL&startdate=01-Jan-2016&output=csv"; 2016 január 1
                                //$"https://finance.google.com/finance/historical?q=AAPL&startdate=10-02-2016&output=csv"; 2016 october 10
                                if (stockWorksheet.Cells[i, dateColumn].Value != null)
                                {
                                    //in case of Márc,Áprl,Máj
                                    string dateCellValue = removeDiacritics(stockWorksheet.Cells[i, dateColumn].Value.ToString());
                                    string month = "";
                                    Regex dateRegex1 = new Regex(@"^20\d{2}.\s\d{2}.\s\d{2}");
                                    Regex dateRegex2 = new Regex(@"^20\d{2}.\d{2}.\d{2}");
                                    Regex dateRegex3 = new Regex(@"^\d{2}-[\u0000-\u00FF]{3}.-\d{4}$");
                                    Regex dateRegex4 = new Regex(@"^\d{2}-[\u0000-\u00FF]{4}.-\d{4}$");
                                    if (dateRegex1.IsMatch(dateCellValue))
                                    {
                                        string[] splitted = dateCellValue.Split(' ');
                                        string dateCellValueFormatted = "";
                                        for (int k = 0; k < splitted.Length; k++)
                                        {
                                            dateCellValueFormatted += splitted[k];
                                        }
                                        dateCellValueFormatted=dateCellValueFormatted.Replace('.', '-');
                                        //2018-02-28
                                        dateCellValue = dateCellValueFormatted;
                                    }
                                    else if (dateRegex2.IsMatch(dateCellValue))
                                    {
                                        //2018-02-28
                                        dateCellValue=dateCellValue.Replace('.', '-');
                                    }
                                    else if (dateRegex3.IsMatch(dateCellValue))
                                    {
                                        switch (dateCellValue.Substring(3, 3))
                                        {
                                            case "maj":
                                                month = "may";
                                                break;
                                            case "okt":
                                                month = "oct";
                                                break;
                                        }
                                        //05-maj.-2018
                                        dateCellValue=dateCellValue.Replace(dateCellValue.Substring(3, 3), month);
                                        //05-may.-2018
                                        dateCellValue = dateCellValue.Remove(6, 1);
                                        //05-may-2018
                                    }
                                    else if (dateRegex4.IsMatch(dateCellValue))
                                    {
                                        //05-febr.-2018
                                        dateCellValue=dateCellValue.Remove(6, 2);
                                        //05-feb-2018
                                    }
                                    companyToOldestDate.Add(companyNames[j], dateCellValue);

                                }
                                companyNames.Remove(companyNames[j]);
                            }
                        }
                    }
                }
            }
            return companyToOldestDate;
        }

        private string convertDateToCompare(string dateCellValueFormatted)
        {
            string CSVform = "";
            string[] splitted = dateCellValueFormatted.Split('-');
            CSVform = splitted[2].Remove(0,2) + "-";
            switch (splitted[1])
            {
                case "01":
                    CSVform += "Jan";
                    break;
                case "02":
                    CSVform += "Feb";
                    break;
                case "03":
                    CSVform += "Mar";
                    break;
                case "04":
                    CSVform += "Apr";
                    break;
                case "05":
                    CSVform += "May";
                    break;
                case "06":
                    CSVform += "Jun";
                    break;
                case "07":
                    CSVform += "Jul";
                    break;
                case "08":
                    CSVform += "Aug";
                    break;
                case "09":
                    CSVform += "Sep";
                    break;
                case "10":
                    CSVform += "Oct";
                    break;
                case "11":
                    CSVform += "Nov";
                    break;
                case "12":
                    CSVform += "Dec";
                    break;
            }
            CSVform += "-" + splitted[0];
            return CSVform;
        }

        public static int levenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
        private List<string> collectCompanyNames(int companyColumn)
        {
            List<string> returnValue = new List<string>();
            int blank_cell_counter = 0;
            int row = 2;
            while(blank_cell_counter<2)
            {
                if(stockWorksheet.Cells[row,companyColumn].Value!=null)
                {
                    blank_cell_counter = 0;
                    if(!returnValue.Contains(stockWorksheet.Cells[row, companyColumn].Value.ToString()))
                        returnValue.Add(stockWorksheet.Cells[row, companyColumn].Value.ToString());
                }
                else
                {
                    blank_cell_counter++;
                }
                row++;
            }
            return returnValue;
        }

        public int getDateColumn()
        {
            Regex dateRegex1 = new Regex(@"^20\d{2}.\d{2}.\d{2}");
            Regex dateRegex2 = new Regex(@"^20\d{2}-\d{2}-\d{2}");
            Regex dateRegex3 = new Regex(@"^20\d{2}.\s\d{2}.\s\d{2}");
            Regex dateRegex4 = new Regex(@"^\d{2}-[\u0000-\u00FF]{3}.-\d{4}$"); // pl. 28-ápr-2018
            Regex dateRegex5 = new Regex(@"^\d{2}-[\u0000-\u00FF]{4}.-\d{4}$"); // pl. 28-márc-2018
            Regex dateRegex6 = new Regex(@"^\d{4}-[\u0000-\u00FF]{4}.-\d{2}$");
            Regex dateRegex7 = new Regex(@"^\d{4}-[\u0000-\u00FF]{3}.-\d{2}$");
            int blank_cell_counter = 0;
            int row = 2;
            int column = 1;
            while (true)
            {
                while (blank_cell_counter < 2)
                {
                    if (stockWorksheet.Cells[row, column].Value != null)
                    {
                        blank_cell_counter=0;
                        if (dateRegex1.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex2.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex3.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex4.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex5.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex6.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()) ||
                            dateRegex7.IsMatch(stockWorksheet.Cells[row, column].Value.ToString()))
                        {
                            /*
                             * It can be in the first column, but it got to have other values in that row
                             * It can happen that it is the last value un the row
                             * but in that case it isn't in the first column
                             */
                             //itt valami baj van mikor csak 1 adat van, egy dátumnál a portfoliosba
                             //todo
                            if((stockWorksheet.Cells[row,column+1].Value!=null) || column!=1)
                            {
                                return column;
                            }
                        }
                        else
                        {
                            column++;
                        }
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    row++;
                }
                column = 1;
                if (stockWorksheet.Cells[row++, column].Value != null)
                {
                    blank_cell_counter = 0;
                    row++;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int getCompanyColumn()
        {
            int blank_cell_counter = 0;
            int row = 2;
            int column = 1;
            string companyRegex1 = "Co.";
            string companyRegex2 = "AG";
            string companyRegex3 = "Inc.";
            string companyRegex4 = "Corp.";
            string companyRegex5 = "Ltd.";
            string companyRegex6 = "Nyrt.";
            while (true)
            {
                while (blank_cell_counter < 2)
                {
                    if (stockWorksheet.Cells[row, column].Value != null)
                    {
                        blank_cell_counter = 0;
                        if (stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex1) ||
                            stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex2) ||
                            stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex3) ||
                            stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex4) ||
                            stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex5) ||
                            stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex6))
                        {
                            int matchingCells = 1;
                            for(int i=row;i<row+3;i++)
                            {
                                if (stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex1) ||
                                    stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex2) ||
                                    stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex3) ||
                                    stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex4) ||
                                    stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex5) ||
                                    stockWorksheet.Cells[row, column].Value.ToString().Contains(companyRegex6))
                                {
                                    matchingCells++;
                                }
                            }
                            if(matchingCells>1)
                            {
                                return column;
                            }
                        }
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    column++;
                }
                column = 1;
                if(stockWorksheet.Cells[row++,column].Value!=null)
                {
                    blank_cell_counter = 0;
                    row++;
                }
                else
                {
                    return 0;
                }
            }
        }
        public void readOutUserspecifiedTransactions(string startingRowString,string nameColumnString,string priceColumnString,
            string quantityColumnString,string dateColumnString,string transactionTypeColumnString)
        {
            int startingRow = 0;
            try
            {
                startingRow = int.Parse(startingRowString);
            }
            catch(Exception e)
            {
                startingRow = ExcelColumnNameToNumber(startingRowString);
            }
            int nameColumn = 0;
            try
            {
                nameColumn = int.Parse(nameColumnString);
            }
            catch (Exception e)
            {
                nameColumn = ExcelColumnNameToNumber(nameColumnString);
            }
            int priceColumn = 0;
            try
            {
                priceColumn = int.Parse(priceColumnString);
            }
            catch (Exception e)
            {
                priceColumn = ExcelColumnNameToNumber(priceColumnString);
            }
            int quantityColumn = 0;
            try
            {
                quantityColumn = int.Parse(quantityColumnString);
            }
            catch (Exception e)
            {
                quantityColumn = ExcelColumnNameToNumber(quantityColumnString);
            }
            int dateColumn = 0;
            try
            {
                dateColumn = int.Parse(dateColumnString);
            }
            catch (Exception e)
            {
                dateColumn = ExcelColumnNameToNumber(dateColumnString);
            }
            int transactionTypeColumn = 0;
            try
            {
                transactionTypeColumn = int.Parse(transactionTypeColumnString);
            }
            catch (Exception e)
            {
                transactionTypeColumn = ExcelColumnNameToNumber(transactionTypeColumnString);
            }
            if ((nameColumn != 0) && (dateColumn != 0) && (priceColumn != 0))
            {
                importedStocks = new List<Stock>();
                int blank_cell_counter = 0;
                while (blank_cell_counter < 2)
                {
                    if ((stockWorksheet.Cells[startingRow, nameColumn].Value != null) &&
                            (stockWorksheet.Cells[startingRow, dateColumn].Value != null) &&
                            (stockWorksheet.Cells[startingRow, priceColumn].Value != null))
                    {
                        blank_cell_counter = 0;
                        string companyName = stockWorksheet.Cells[startingRow, nameColumn].Value.ToString();
                        string transactionDate = stockWorksheet.Cells[startingRow, dateColumn].Value.ToString();
                        string transactionPriceString = stockWorksheet.Cells[startingRow, priceColumn].Value.ToString().Replace(',', '.');
                        double transactionPrice = 0;
                        try
                        {
                            transactionPrice = double.Parse(transactionPriceString, CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {

                        }
                        string transactionType = "-";
                        string quantityString = "";
                        int quantity = 1;
                        if (stockWorksheet.Cells[startingRow, transactionTypeColumn].Value != null)
                        {
                            transactionType = stockWorksheet.Cells[startingRow, transactionTypeColumn].Value.ToString();
                        }
                        if (stockWorksheet.Cells[startingRow, quantityColumn].Value != null)
                        {
                            quantityString = stockWorksheet.Cells[startingRow, quantityColumn].Value.ToString();
                            try
                            {
                                quantity = int.Parse(quantityString);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        Stock stock = new Stock(companyName, transactionPrice, quantity, transactionDate, transactionType);
                        importedStocks.Add(stock);
                    }
                    else
                    {
                        blank_cell_counter++;
                    }
                    startingRow++;
                }
                stockHandler.addTransactions(importedStocks);
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
        public void deleteTemporaryExcel()
        {
            if (File.Exists(temporaryExcel))
            {
                File.Delete(temporaryExcel);
            }
        }
        public string[] WriteSafeReadAllLines(string path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(csv))
            {
                List<string> file = new List<string>();
                while (!sr.EndOfStream)
                {
                    file.Add(sr.ReadLine());
                }
                return file.ToArray();
            }
        }
        static string removeDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public Dictionary<string,string> getCompanyToTickerMap()
        {
            return companyToTicker;
        }
        public void setCompanyNameColumn(int value)
        {
            companyNameColumn = value;
        }
        public void setTransactionDateColumn(int value)
        {
            transactionDateColumn = value;
        }
        public void setPriceColumn(int value)
        {
            priceColumn = value;
        }
        public void setQuantityColumn(int value)
        {
            quantityColumn = value;
        }
        public void setTransactionTypeColumn(int value)
        {
            transactionTypeColumn = value;
        }
        ~TemplateStockReadIn()
        {
            /*
            if(temporaryExcel!="")
            {
                deleteTemporaryExcel();
            }
            */
            workbook.Close();
            excel.Quit();
        }
    }
}