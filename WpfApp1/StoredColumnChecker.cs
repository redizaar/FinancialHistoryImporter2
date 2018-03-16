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
    class StoredColumnChecker
    {
        private string accountNumberComboBox;
        private string priceComboBox;
        private string balanceComboBox;
        private DataRow mostMatchingRow;
        private Application excel;
        private Workbook workbook;
        private Worksheet analyseWorksheet;
        public System.Data.DataTable dtb;
        private MainWindow mainWindow;
        public StoredColumnChecker() { }
        public void addDistinctBanksToCB()
        {
            foreach(var item in SpecifiedImportBank.getInstance(null,mainWindow).bankChoices.ToList())
            {
                if (item != "Add new Bank")
                    SpecifiedImportBank.getInstance(null, mainWindow).bankChoices.Remove(item);
            }
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select distinct BankName From [StoredColumns]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            if (datatable.Rows.Count > 0)
            {
                foreach (DataRow row in dtb.Rows)
                {
                   SpecifiedImportBank.getInstance(null,mainWindow).bankChoices.Add(row["BankName"].ToString());
                }
            }
        }
        public void getDataTableFromSql(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            SqlConnection sqlConn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ImportFileData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            sqlConn.Open();
            string getEveryRow = "Select * From [StoredColumns]";
            SqlDataAdapter sda = new SqlDataAdapter(getEveryRow, sqlConn);
            System.Data.DataTable datatable = new System.Data.DataTable();
            sda.Fill(datatable);
            dtb = datatable;
            SpecifiedImportBank.getInstance(null, mainWindow).setDataTableFromSql(datatable);
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
                    string accountNumberPosString = row["AccountNumberPos"].ToString();
                    string dateColumnString = row["DateColumn"].ToString();
                    string priceColumnString = row["PriceColumn"].ToString();
                    string balanceColumnString = row["BalanceColumn"].ToString();
                    string commentColumnString = row["CommentColumn"].ToString();
                    int dateColumn;
                    try
                    {
                        dateColumn = int.Parse(dateColumnString);
                    }
                    catch (Exception e)
                    {
                        dateColumn = ExcelColumnNameToNumber(dateColumnString);
                    }
                    int balanceColumn = -1;
                    if (balanceColumnString != "None")
                    {
                        try
                        {
                            balanceColumn = int.Parse(balanceColumnString);
                        }
                        catch (Exception e)
                        {
                            balanceColumn = ExcelColumnNameToNumber(balanceColumnString);
                        }
                    }
                    List<int> accountNumberPos = new List<int>();
                    // if it has 2 elements its in a cell
                    // if it has 1 element it is a column
                    if (accountNumberPosString != "Sheet name")
                    {
                        int tempValue1 = 0;
                        long size = sizeof(char) * accountNumberPosString.Length;
                        //todo
                        if (size > 1)//its a cell 
                        {
                            int tempValue2 = 0;
                            try
                            {
                                tempValue1 = int.Parse(accountNumberPosString[1].ToString());
                            }
                            catch (Exception e)
                            {
                                tempValue1 = ExcelColumnNameToNumber(accountNumberPosString[1].ToString());
                            }
                            try
                            {
                                tempValue2 = int.Parse(accountNumberPosString[0].ToString());
                            }
                            catch (Exception e)
                            {
                                tempValue2 = ExcelColumnNameToNumber(accountNumberPosString[0].ToString());
                            }
                            accountNumberPos.Add(tempValue1);
                            accountNumberPos.Add(tempValue2);
                        }
                        else if (size == 1)
                        {
                            try
                            {
                                tempValue1 = int.Parse(accountNumberPosString);
                            }
                            catch (Exception e)
                            {
                                tempValue1 = ExcelColumnNameToNumber(accountNumberPosString);
                            }
                            accountNumberPos.Add(tempValue1);
                        }
                    }
                    else
                    {
                        accountNumberPos = null;
                    }
                    List<int> commentColumns = new List<int>();
                    string[] commentColumnsSplitted = commentColumnString.Split(',');
                    for (int i = 0; i < commentColumnsSplitted.Length; i++)
                    {
                        int tempValue;
                        try
                        {
                            tempValue = int.Parse(commentColumnsSplitted[i]);
                        }
                        catch (Exception e)
                        {
                            tempValue = ExcelColumnNameToNumber(commentColumnsSplitted[i]);
                        }
                        commentColumns.Add(tempValue);
                    }
                    List<int> priceColumns = new List<int>();
                    string[] priceColumnsSplitted = priceColumnString.Split(',');
                    bool isMultiplePriceColumns = false;
                    if (priceColumnsSplitted.Length > 1)
                    {
                        isMultiplePriceColumns = true;
                        for (int i = 0; i < priceColumnsSplitted.Length; i++)
                        {
                            int tempValue;
                            try
                            {
                                tempValue = int.Parse(priceColumnsSplitted[i]);
                            }
                            catch (Exception e)
                            {
                                tempValue = ExcelColumnNameToNumber(priceColumnsSplitted[i]);
                            }
                            priceColumns.Add(tempValue);
                        }
                    }
                    else
                    {
                        int tempValue;
                        try
                        {
                            tempValue = int.Parse(priceColumnsSplitted[0]);
                        }
                        catch (Exception e)
                        {
                            tempValue = ExcelColumnNameToNumber(priceColumnsSplitted[0]);
                        }
                        priceColumns.Add(tempValue);
                    }
                    if (analyseWorksheet.Cells[transactionsRow, dateColumn].Value != null)
                    {
                        if (accountNumberPos == null)
                        {
                            if (analyseWorksheet.Name != null)
                            {
                                accountNumberComboBox = "Sheet name";
                                tempCounter++;
                            }
                        }
                        else
                        {
                            if (accountNumberPos.Count > 1)
                            {
                                if (analyseWorksheet.Cells[accountNumberPos[0], accountNumberPos[1]].Value != null)
                                {
                                    accountNumberComboBox = "Cell";
                                    tempCounter++;
                                }
                            }
                            else if (accountNumberPos.Count == 1)
                            {
                                if (analyseWorksheet.Cells[transactionsRow, accountNumberPos[0]].Value != null)
                                {
                                    accountNumberComboBox = "Column";
                                    tempCounter++;
                                }
                            }
                        }
                        if (isMultiplePriceColumns)
                        {
                            if ((analyseWorksheet.Cells[transactionsRow, priceColumns[0]].Value != null) ||
                                (analyseWorksheet.Cells[transactionsRow, priceColumns[1]].Value != null))
                            {
                                priceComboBox = "Income,Spending";
                                tempCounter++;
                            }
                        }
                        else
                        {
                            if (analyseWorksheet.Cells[transactionsRow, priceColumns[0]] != null)
                            {
                                priceComboBox = "One column";
                                tempCounter++;
                            }
                        }
                        for (int i = 0; i < commentColumns.Count; i++)
                        {
                            if (analyseWorksheet.Cells[transactionsRow, commentColumns[i]].Value != null)
                            {
                                tempCounter++;
                            }
                        }
                        if(balanceColumn==-1)
                        {
                            balanceComboBox = "None";
                        }
                        else
                        {
                            balanceComboBox = "Column";
                        }
                    }
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
                SpecifiedImportBank.getInstance(null, mainWindow).bankChoice = mostMatchingRow["BankName"].ToString();
                SpecifiedImportBank.getInstance(null, mainWindow).transactionsRowTextBox.Text = mostMatchingRow["TransStartRow"].ToString();
                SpecifiedImportBank.getInstance(null, mainWindow).accountNumberChoice = accountNumberComboBox;
                SpecifiedImportBank.getInstance(null, mainWindow).accountNumberTextBox.Text = mostMatchingRow["AccountNumberPos"].ToString();
                SpecifiedImportBank.getInstance(null, mainWindow).dateColumnTextBox.Text = mostMatchingRow["DateColumn"].ToString();
                SpecifiedImportBank.getInstance(null, mainWindow).priceColumnChoice = priceComboBox;
                if (priceComboBox == "One column")
                    SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text = mostMatchingRow["PriceColumn"].ToString();
                else
                {
                    string[] splittedPriceColumns = mostMatchingRow["PriceColumn"].ToString().Split(',');
                    SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text = splittedPriceColumns[0];
                    SpecifiedImportBank.getInstance(null, mainWindow).priceColumnTextBox_1.Text = splittedPriceColumns[1];
                }
                SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnChoice = balanceComboBox;
                if (balanceComboBox != "None")
                {
                    SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnTextBox.Text = mostMatchingRow["BalanceColumn"].ToString();
                }
                SpecifiedImportBank.getInstance(null, mainWindow).commentColumnTextBox.Text = mostMatchingRow["CommentColumn"].ToString();
            }
            else//no data in sql
            {
                SpecifiedImportBank.getInstance(null, mainWindow).bankChoice = "Add new Bank";
            }
        }
        public void setMostMatchesRow(DataRow value)
        {
            mostMatchingRow = value;
        }
        ~StoredColumnChecker()
        {
            workbook.Close();
            excel.Quit();
        }
    }
}
