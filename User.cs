using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class User
    {
        private string accountNumber;
        private string username;
        private int netWorth;
        private string latestImportDate;
        private int numberOfTransactions;
        public User()
        {

        }
        public User(string _accountNumber,string _username,int _networth,string _latestimportdate,int _numberoftransactions)
        {
            accountNumber = _accountNumber;
            username = _username;
            netWorth = _networth;
            latestImportDate = _latestimportdate;
            numberOfTransactions = _numberoftransactions;
        }
        public void setAccountNumber(string value)
        {
            accountNumber = value;
        }
        public void setUsername(string value)
        {
            username = value;
        }
        public void setNetWorth(int value)
        {
            netWorth = value;
        }
        public void setLatestImportDate(string value)
        {
            latestImportDate = value;
        }
        public void setNumberOfTransactions(int value)
        {
            numberOfTransactions = value;
        }
        public string getAccountNumber()
        {
            return accountNumber;
        }
        public string getUsername()
        {
            return username;
        }
        public int getNetWorth()
        {
            return netWorth;
        }
        public string getLatestImportDate()
        {
            return latestImportDate;
        }
        public int getNumberOfTransactions()
        {
            return numberOfTransactions;
        }
    }
}
