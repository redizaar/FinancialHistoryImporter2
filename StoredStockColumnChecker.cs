using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class StoredStockColumnChecker
    {
        private DataRow mostMatchingRow;
        private Application excel;
        private Workbook workbook;
        private Worksheet analyseWorksheet;
        public System.Data.DataTable dtb;
        private MainWindow mainWindow;
        public StoredStockColumnChecker() { }
        public void addDistinctBanksToCB()
        {
            foreach (var item in SpecifiedImportStock.getInstance(null, mainWindow).bankChoices.ToList())
            {
                if (item != "Add new Type")
                    SpecifiedImportStock.getInstance(null, mainWindow).bankChoices.Remove(item);
            }
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select distinct BankName From [StoredColumns_Stock]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            if (datatable.Rows.Count > 0)
            {
                foreach (DataRow row in dtb.Rows)
                {
                    SpecifiedImportStock.getInstance(null, mainWindow).bankChoices.Add(row["BankName"].ToString());
                }
            }
        }
        public void getDataTableFromSql(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select * From [StoredColumns_Stock]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            dtb = datatable;
            SpecifiedImportStock.getInstance(null, mainWindow).setDataTableFromSql(datatable);
        }
        public void setAnalyseWorksheet(string filePath)
        {
            excel = new Application();
            workbook = excel.Workbooks.Open(filePath);
            Worksheet worksheet = workbook.Worksheets[1];
            analyseWorksheet = worksheet;
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
        public DataRow findMostMatchingRow()
        {
            if (dtb.Rows.Count > 0)
            {
                DataRow mostMatches = null;
                int matchingColumns = 0;
                foreach (DataRow row in dtb.Rows)
                {
                    int tempCounter = 0;
                    int transactionsRow = int.Parse(row["TransStartRow"].ToString());
                    string stockNameColumnString = row["StockName"].ToString();
                    string priceColumnString = row["PriceColumn"].ToString();
                    string quantityColumnString = row["QuantityColumn"].ToString();
                    string dateColumnString = row["DateColumn"].ToString();
                    string typeColumnColumnString = row["TypeColumn"].ToString();
                    int stockNameColumn;
                    try
                    {
                        stockNameColumn = int.Parse(stockNameColumnString);
                    }
                    catch(Exception e)
                    {
                        stockNameColumn = ExcelColumnNameToNumber(stockNameColumnString);
                    }
                    int dateColumn;
                    try
                    {
                        dateColumn = int.Parse(dateColumnString);
                    }
                    catch (Exception e)
                    {
                        dateColumn = ExcelColumnNameToNumber(dateColumnString);
                    }
                    int priceColumn;
                    try
                    {
                        priceColumn = int.Parse(priceColumnString);
                    }
                    catch(Exception e)
                    {
                        priceColumn = ExcelColumnNameToNumber(priceColumnString);
                    }
                    int quantityColumn;
                    try
                    {
                        quantityColumn = int.Parse(quantityColumnString);
                    }
                    catch(Exception e)
                    {
                        quantityColumn = ExcelColumnNameToNumber(quantityColumnString);
                    }
                    int typeColumn;
                    try
                    {
                        typeColumn = int.Parse(typeColumnColumnString);
                    }
                    catch(Exception e)
                    {
                        typeColumn = ExcelColumnNameToNumber(typeColumnColumnString);
                    }
                    if(analyseWorksheet.Cells[transactionsRow,stockNameColumn].Value!=null)
                        tempCounter++;
                    if(analyseWorksheet.Cells[transactionsRow,priceColumn].Value!=null)
                        tempCounter++;
                    if(analyseWorksheet.Cells[transactionsRow,quantityColumn].Value!=null)
                        tempCounter++;
                    if (analyseWorksheet.Cells[transactionsRow, dateColumn].Value != null)
                        tempCounter++;
                    if (analyseWorksheet.Cells[transactionsRow, typeColumn].Value != null)
                        tempCounter++;
                    if (tempCounter > matchingColumns)
                    {
                        matchingColumns = tempCounter;
                        mostMatches = row;
                    }
                }
                return mostMatches;
            }
            return null;
        }
        public void setSpecifiedImportPageTextBoxes()
        {
            if (mostMatchingRow != null)
            {
                SpecifiedImportStock.getInstance(null, mainWindow).bankChoice = mostMatchingRow["BankName"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).transactionsRowTextBox.Text = mostMatchingRow["TransStartRow"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).stockNameColumnTextBox.Text = mostMatchingRow["StockName"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).priceColumnTextBox.Text = mostMatchingRow["PriceColumn"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).quantityColumnTextBox.Text = mostMatchingRow["QuantityColumn"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).dateColumnTextBox.Text = mostMatchingRow["DateColumn"].ToString();
                SpecifiedImportStock.getInstance(null, mainWindow).transactionTypeTextBox.Text = mostMatchingRow["TypeColumn"].ToString();
            }
            else//no data in sql
            {
                SpecifiedImportBank.getInstance(null, mainWindow).bankChoice = "Add new Type";
            }
        }
        public void setMostMatchesRow(DataRow value)
        {
            mostMatchingRow = value;
        }
        ~StoredStockColumnChecker()
        {
            workbook.Close();
            excel.Quit();
        }
    }
}
