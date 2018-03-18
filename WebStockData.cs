using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace WpfApp1
{
    public class WebStockData
    {
        private List<string> dates;
        private List<double> prices;
        private List<Stock> stocksForSql;
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
            prices = new List<double>();
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
            int j = 0;
            string regex = "[0-9]{4}-[0-9]{2}-[0-9]{2}";
            string tempDate="";
            for (int i = 11; i < lines.Length-1; i+=11)
            {
                if ((Regex.IsMatch(lines[i], regex)))
                {
                    string[] _date = lines[i].Split('\n');
                    dates.Add(_date[1]);
                    tempDate = _date[1];
                }
                double openPrice = double.Parse(lines[i+1].Replace('.', ','));
                double highPrice = double.Parse(lines[i+2].Replace('.', ','));
                double lowPrice = double.Parse(lines[i+3].Replace('.', ','));
                double closePrice = double.Parse(lines[i+4].Replace('.', ','));
                Stock stock = new Stock(ticker, tempDate,openPrice,highPrice,lowPrice,closePrice);
                stocksForSql.Add(stock);
                prices.Add(closePrice);
                j = 0;
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
        }
        public List<double> getPrices()
        {
            return prices;
        }
        public List<string> getDates()
        {
            return dates;
        }
    }
}
