CREATE PROC insertStockTransaction
@exportDate varchar(50),
@transactionDate varchar(50),
@stockName varchar(50),
@stockPrice int,
@soldQuantity int,
@boughtQuantity int,
@currentQuantity int,
@spending int,
@profit int,
@earningMethod varchar(50),
@importerName varchar(50)
AS
	INSERT INTO ImportFileData.dbo.[importedStockTransactions](ExportDate,TransactionDate,StockName,StockPrice,SoldQuantity,BoughtQuantity,CurrentQuantity,Spending,Profit,EarningMethod,ImporterName)
	VALUES (@exportDate,@transactionDate,@stockName,@stockPrice,@soldQuantity,@boughtQuantity,@currentQuantity,@spending,@profit,@earningMethod,@importerName)