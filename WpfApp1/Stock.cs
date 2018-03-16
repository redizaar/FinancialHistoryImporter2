using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Stock
    {
        public string stockName { get;  set; }
        public double stockPrice { get;  set; }
        public int quantity { get;  set; }
        public string transactionDate { get;  set; }
        public string transactionType { get;  set; }
        public string writeDate { get;  set; }
        public double profit { get; set; }
        //for StockDataGrid
        public string symbol { get;  set; }
        public string date { get;  set; }
        public double openPrice { get;  set; }
        public double highPrice { get;  set; }
        public double lowPrice { get;  set; }
        public double closePrice { get;  set; }
        //reading out from file Constructor
        public Stock(string _stockName,double _stockPrice,int _quantity,string _transactionDate,string _transactionType)
        {
            stockName = _stockName;
            stockPrice = _stockPrice;
            quantity = _quantity;
            transactionDate = _transactionDate;
            transactionType = _transactionType;
        }
        //writing to file Constructor
        public Stock(string _writeDate, string _transactionDate,string _stockName,double _stockPrice, int _quantity, string _transactionType,double _profit)
        {
            writeDate = _writeDate;
            stockName = _stockName;
            stockPrice = _stockPrice;
            quantity = _quantity;
            transactionDate = _transactionDate;
            transactionType = _transactionType;
            profit = _profit;
        }
        //sql Stock constructor
        public Stock(string _smybol,string _date,double _openPrice,double _highPrice,double _lowPrice,double _closePrice)
        {
            symbol = _smybol;
            date = _date;
            openPrice = _openPrice;
            highPrice = _highPrice;
            lowPrice = _lowPrice;
            closePrice = _closePrice;
        }
        public string getSymbolToSql()
        {
            return symbol;
        }
        public double getProfit()
        {
            return profit;
        }
        public string getDateToSql()
        {
            return date;
        }
        public double getOpenPriceForSql()
        {
            return openPrice;
        }
        public double getHighPriceForSql()
        {
            return highPrice;
        }
        public double getLowPriceForSql()
        {
            return lowPrice;
        }
        public double getClosePriceForSql()
        {
            return closePrice;
        }
        public string getStockName()
        {
            return stockName;
        }
        public double getStockPrice()
        {
            return stockPrice;
        }
        public string getTransactionDate()
        {
            return transactionDate;
        }
        public string getTransactionType()
        {
            return transactionType;
        }
        public int getQuantity()
        {
            return quantity;
        }
        public void setQuantity(int value)
        {
            quantity = value;
        }
        public void setProfit(double value)
        {
            profit = value;
        }
    }
}
