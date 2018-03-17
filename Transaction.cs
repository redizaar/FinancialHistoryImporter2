using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Transaction
    {
        private int balance_rn ;
        public string transactionDescription { get; set; }
        public string transactionDate { get; set; }
        public int transactionPrice { get; set; }
        public string accountNumber { get; set; }
        public string writeDate { get; set; }
        private string noBalance;

        public Transaction(int balance_rn,string date,int price,string description, string accountNumber)
        {
            this.balance_rn = balance_rn;
            this.transactionDate = date;
            this.transactionPrice = price;
            this.transactionDescription = description;
            this.accountNumber = accountNumber;
        }
        public Transaction(string noBalance,string date, int price, string description, string accountNumber)
        {
            this.noBalance = noBalance;
            this.transactionDate = date;
            this.transactionPrice = price;
            this.transactionDescription = description;
            this.accountNumber = accountNumber;
        }
        public Transaction(string writeDate,string transactionDate,int balance,int price,string accountNumber,string transactionDescription)
        {
            this.writeDate = writeDate;
            this.transactionDate = transactionDate;
            this.balance_rn = balance;
            this.transactionPrice = price;
            this.accountNumber = accountNumber;
            this.transactionDescription = transactionDescription;
        }

        public void setWriteDate(String todaysDate)
        {
            this.writeDate = todaysDate;
        }
        public void setTransactionDate(String value)
        {
            this.transactionDate = value;
        }
        public string getWriteDate()
        {
            return writeDate;
        }
        public string getAccountNumber()
        {
            return accountNumber;
        }
        public int getBalance_rn()
        {
            return balance_rn;
        }
        public string getTransactionDescription()
        {
            return transactionDescription;
        }
        public string getTransactionDate()
        {
            return transactionDate;
        }
        public int getTransactionPrice()
        {
            return transactionPrice;
        }
    }
}
