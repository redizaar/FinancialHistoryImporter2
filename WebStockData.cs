using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace WpfApp1
{
    public class WebStockData
    {
        private List<string> dates;
        private List<double> highestPrices;
        private List<double> lowestPrices;
        private List<Stock> stocksForSql;
        private SQLiteConnection mConn = new SQLiteConnection("Data Source=" + MainWindow.dbPath, true);
        public WebStockData()
        {
            stocksForSql = new List<Stock>();
        }
        public void getCSVDataFromIEX(string ticker,string date)
        {
            string[] dateSplitted = date.Split(' ');
            if (dateSplitted[1] == "month")
            {
                date = dateSplitted[0] + "M";
            }
            else if (dateSplitted[1] == "year")
            {
                date = dateSplitted[0] + "y";
            }
            dates = new List<string>();
            highestPrices = new List<double>();
            lowestPrices = new List<double>();
            string csv;
            using (var web = new WebClient())
            {
                var url = $"https://api.iextrading.com/1.0/stock/"+ticker+"/chart/"+date+"?format=csv";
                //var url = $"https://finance.google.com/finance/historical?q="+ticker+"&startdate="+day+"-"+month+"-"+year+"&output=csv";
                //nem müködik 2018.03.17 ......................................
                //$"https://finance.google.com/finance/historical?q=AAPL&startdate=01-Jan-2016&output=csv";
                csv = web.DownloadString(url);
            }
            string[] lines = csv.Split(',');
            string regex = "[0-9]{4}-[0-9]{2}-[0-9]{2}";
            string tempDate="";
            for (int i = 11; i < lines.Length-1; i++)
            {
                if ((Regex.IsMatch(lines[i], regex)))
                {
                    string[] _date = lines[i].Split('\n');
                    dates.Add(_date[1]);
                    tempDate = _date[1];
                    double highPrice = double.Parse(lines[i + 2], CultureInfo.InvariantCulture);
                    double lowPrice = double.Parse(lines[i + 3], CultureInfo.InvariantCulture);
                    string openPriceString = lines[i + 1];
                    string highPriceString = lines[i + 2];
                    string lowPriceString = lines[i + 3];
                    string closePriceString = lines[i + 4];
                    Stock stock = new Stock(ticker, tempDate, openPriceString, highPriceString, lowPriceString, closePriceString);
                    stocksForSql.Add(stock);
                    highestPrices.Add(highPrice);
                    lowestPrices.Add(lowPrice);
                }
            }
            ThreadStart threadStart = delegate
            {
                writeStocksToSQL(stocksForSql);
            };
            Thread sqlThread = new Thread(threadStart);
            sqlThread.IsBackground = true;
            sqlThread.Start();
            sqlThread.Join();
        }
        private void writeStocksToSQL(List<Stock> stocksFromCSV)
        {
            //elől vannak a friss dátumok, árak
            //atatbázusba nyilván fordítva

            //ticker is the same for all
            string ticker = stocksForSql[0].getSymbolToSql();
            mConn.Open();
            using (SQLiteCommand mCmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [StockData] " +
                        "(id INTEGER PRIMARY KEY AUTOINCREMENT,'Name' TEXT, 'Date' TEXT, 'openPrice' TEXT, " +
                        "'highPrice' TEXT, 'lowPrice' TEXT, 'closePrice' TEXT);", mConn))
            {
                mCmd.ExecuteNonQuery();
            }
            string storedQuery = "select * from [StockData] where Name= '"+ticker+"'";
            SQLiteCommand command = new SQLiteCommand(storedQuery, mConn);
            DataTable dtb = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dtb);
            if (dtb.Rows.Count == 0)
            {
                for (int i = stocksForSql.Count - 1; i > 0; i--)
                {
                    string insertQuery = "insert into [StockData]" +
                   "(Name,Date,openPrice,highPrice,lowPrice,closePrice)" +
                   " values('" + stocksForSql[i].getSymbolToSql() + "','" + stocksForSql[i].getDateToSql() + "','" + stocksForSql[i].getOpenPriceForSql() + "','" 
                    + stocksForSql[i].getHighPriceForSql() + "','"+ stocksForSql[i].getLowPriceForSql()+"','"+ stocksForSql[i].getClosePriceForSql()+"')";
                    SQLiteCommand insertcommand = new SQLiteCommand(insertQuery, mConn);
                    insertcommand.CommandType = CommandType.Text;
                    insertcommand.ExecuteNonQuery();
                }
            }
            else
            {
                bool storedinSql;
                List<int> notStoredIndexes = new List<int>();
                for (int i = 0; i < stocksForSql.Count; i++)
                {
                    storedinSql = false;
                    foreach (DataRow row in dtb.Rows)
                    {
                        string dateFromSql = row["Date"].ToString();
                        if (stocksForSql[i].getDateToSql() == dateFromSql)
                        {
                            storedinSql = true;
                            break;
                        }
                        //DateTime dt1 = DateTime.ParseExact(dateFromSql, "dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                        //converts a string to a date fromat for example : 27-feb-18
                    }
                    if (!storedinSql)
                        notStoredIndexes.Add(i);
                }
                if (notStoredIndexes.Count > 0)
                {
                    for (int i = 0; i < notStoredIndexes.Count; i++)
                    {
                        string insertQuery = "insert into [StockData]" +
                   "(Name,Date,openPrice,highPrice,lowPrice,closePrice)" +
                   " values('" + stocksForSql[notStoredIndexes[i]].getSymbolToSql() + "','" + stocksForSql[notStoredIndexes[i]].getDateToSql() + "','" + stocksForSql[notStoredIndexes[i]].getOpenPriceForSql() + "','"
                    + stocksForSql[notStoredIndexes[i]].getHighPriceForSql() + "','" + stocksForSql[notStoredIndexes[i]].getLowPriceForSql() + "','" + stocksForSql[notStoredIndexes[i]].getClosePriceForSql() + "')";
                        SQLiteCommand insertcommand = new SQLiteCommand(insertQuery, mConn);
                        insertcommand.CommandType = CommandType.Text;
                        insertcommand.ExecuteNonQuery();
                    }
                }
            }
            mConn.Close();
            /*
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StockData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string query = "Select * From [Stock_WebData] where Name = '"+ticker+"'";
            SqlDataAdapter sda = new SqlDataAdapter(query, sqlConn);
            DataTable dtb = new DataTable();
            sda.Fill(dtb);
            if (dtb.Rows.Count == 0)
            {
                SqlCommand sqlCommand = new SqlCommand("insertStockDataToSql", sqlConn);//SQLQuery 7
                sqlCommand.CommandType = CommandType.StoredProcedure;
                for (int i = stocksForSql.Count-1; i > 0; i--)
                {
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@name", stocksForSql[i].getSymbolToSql());
                    sqlCommand.Parameters.AddWithValue("@date", stocksForSql[i].getDateToSql());
                    sqlCommand.Parameters.AddWithValue("@openprice", stocksForSql[i].getOpenPriceForSql());
                    sqlCommand.Parameters.AddWithValue("@highprice", stocksForSql[i].getHighPriceForSql());
                    sqlCommand.Parameters.AddWithValue("@lowprice", stocksForSql[i].getLowPriceForSql());
                    sqlCommand.Parameters.AddWithValue("@closeprice", stocksForSql[i].getClosePriceForSql());
                    sqlCommand.ExecuteNonQuery();
                }
            }
            else
            {
                bool storedinSql;
                List<int> notStoredIndexes = new List<int>();
                for (int i = 0; i < stocksForSql.Count; i++)
                {
                    storedinSql = false;
                    foreach (DataRow row in dtb.Rows)
                    {
                        string dateFromSql = row["Date"].ToString();
                        if(stocksForSql[i].getDateToSql()==dateFromSql)
                        {
                            storedinSql = true;
                            break;
                        }
                        //DateTime dt1 = DateTime.ParseExact(dateFromSql, "dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                        //converts a string to a date fromat for example : 27-feb-18
                    }
                    if (!storedinSql)
                        notStoredIndexes.Add(i);
                }
                if (notStoredIndexes.Count > 0)
                {
                    SqlCommand sqlCommand = new SqlCommand("insertStockDataToSql", sqlConn);//SQLQuery 7
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    for (int i = 0; i < notStoredIndexes.Count; i++)
                    {
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.AddWithValue("@name", stocksForSql[notStoredIndexes[i]].getSymbolToSql());
                        sqlCommand.Parameters.AddWithValue("@date", stocksForSql[notStoredIndexes[i]].getDateToSql());
                        sqlCommand.Parameters.AddWithValue("@openprice", stocksForSql[notStoredIndexes[i]].getOpenPriceForSql());
                        sqlCommand.Parameters.AddWithValue("@highprice", stocksForSql[notStoredIndexes[i]].getHighPriceForSql());
                        sqlCommand.Parameters.AddWithValue("@lowprice", stocksForSql[notStoredIndexes[i]].getLowPriceForSql());
                        sqlCommand.Parameters.AddWithValue("@closeprice", stocksForSql[notStoredIndexes[i]].getClosePriceForSql());
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            */
        }
        public List<double> getHighestPrices()
        {
            return highestPrices;
        }
        public List<double> getLowestPrices()
        {
            return lowestPrices;
        }
        public List<string> getDates()
        {
            return dates;
        }
    }
}
