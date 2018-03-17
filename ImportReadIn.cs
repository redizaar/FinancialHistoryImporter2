using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
namespace WpfApp1
{
    public class ImportReadIn
    {
        private List<string> path;
        private string importType = "";
        private string currentFileName;
        private MainWindow mainWindow;
        private List<Transaction> bankTransactions;
        private List<Stock> stockTransactions;
        _Application excel = new _Excel.Application();
        Workbook ReadWorkbook;
        Worksheet ReadWorksheet;
        public ImportReadIn(string _importType, List<string> _path,MainWindow _mainWindow,bool specifiedByUser)
        {
            path = _path;
            importType = _importType;
            mainWindow = _mainWindow;
            if (path[0] != "FolderAdress")//a path wasn't choosen, useless ( not in use )
            {
                for (int i = 0; i < path.Count; i++)
                {
                    string [] splittedFileName=path[i].Split('\\');
                    int lastSplitIndex = splittedFileName.Length-1;
                    currentFileName = splittedFileName[lastSplitIndex];
                    if (importType=="Bank")
                    {
                        ReadWorkbook = excel.Workbooks.Open(path[i]);
                        ReadWorksheet = ReadWorkbook.Worksheets[1];
                        if (!specifiedByUser)
                        {
                            TemplateBankReadIn templateBank = new TemplateBankReadIn(this, ReadWorkbook, ReadWorksheet, mainWindow, false);
                            //so far we got the Starting Row(of the transactions),Number of Columns, account number
                            templateBank.readOutTransactionColumns(templateBank.getStartingRow(), templateBank.getNumberOfColumns());
                        }
                        else //userSpecified==true
                        {
                            TemplateBankReadIn templateBank = new TemplateBankReadIn(this, ReadWorkbook, ReadWorksheet, mainWindow, true);
                            string startingRow = SpecifiedImportBank.getInstance(null,mainWindow).transactionsRowTextBox.Text.ToString();
                            string dateColumn = SpecifiedImportBank.getInstance(null,mainWindow).dateColumnTextBox.Text.ToString();
                            string commentColumn = SpecifiedImportBank.getInstance(null,mainWindow).commentColumnTextBox.Text.ToString();
                            string accountNumberCB = SpecifiedImportBank.getInstance(null,mainWindow).accountNumberCB.SelectedItem.ToString();
                            string transactionPriceCB = SpecifiedImportBank.getInstance(null,mainWindow).priceColumnCB.SelectedItem.ToString();
                            string balanceCB = SpecifiedImportBank.getInstance(null,mainWindow).balanceColumnCB.SelectedItem.ToString();
                            string balanceComboBocString = SpecifiedImportBank.getInstance(null, mainWindow).balanceColumnTextBox.Text.ToString();
                            templateBank.readOutUserspecifiedTransactions(startingRow, dateColumn, commentColumn, accountNumberCB, transactionPriceCB, balanceCB, balanceComboBocString);
                        }
                    }
                    else if(importType=="Stock")
                    {
                        if (!specifiedByUser)
                        {
                            TemplateStockReadIn templateStock = new TemplateStockReadIn(this, path[i]);
                            templateStock.analyzeStockTransactionFile();
                            templateStock.readOutTransactions();
                        }
                        else//userSpecified==true
                        {
                            TemplateStockReadIn templateStock = new TemplateStockReadIn(this, path[i]);
                            string startingRowString = SpecifiedImportStock.getInstance(null, mainWindow).transactionsRowTextBox.ToString();
                            string nameColumnString = SpecifiedImportStock.getInstance(null, mainWindow).stockNameColumnTextBox.ToString();
                            string priceColumnString = SpecifiedImportStock.getInstance(null, mainWindow).priceColumnTextBox.ToString();
                            string quantityColumnString = SpecifiedImportStock.getInstance(null, mainWindow).quantityColumnTextBox.ToString();
                            string dateColumnString = SpecifiedImportStock.getInstance(null, mainWindow).dateColumnTextBox.ToString();
                            string transactionTypeString = SpecifiedImportStock.getInstance(null, mainWindow).transactionTypeTextBox.ToString();
                            templateStock.readOutUserspecifiedTransactions(startingRowString, nameColumnString, priceColumnString, quantityColumnString, dateColumnString, transactionTypeString);
                        }
                    }
                }
                excel.Application.Quit();
                excel.Quit();
            }
        }
        ~ImportReadIn()
        {
            excel.Application.Quit();
            excel.Quit();
        }
        public void addTransactions(List<Transaction> newTransactions)
        {
            bankTransactions = newTransactions;
            writeOutBankTransactions();
        }
        public void addTransactions(List<Stock> newTransactions)
        {
            stockTransactions = newTransactions;
            writeOutStockTransactions();
        }

        private void writeOutStockTransactions()
        {
            new ExportTransactions(stockTransactions,mainWindow,currentFileName);
        }

        public void writeOutBankTransactions()
        {
            new ExportTransactions(bankTransactions,mainWindow,currentFileName);
        }
    }
}
