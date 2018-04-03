CREATE PROC insertNewStockColumns
@bankName varchar(50),
@transStartRow int,
@stockName varchar(100),
@priceColumn varchar(50),
@quantityColumn varchar(50),
@dateColumn varchar(50),
@typeColumn varchar(50)
AS
	INSERT INTO ImportFileData.dbo.[StoredColumns_Stock](BankName,TransStartRow,StockName,PriceColumn,QuantityColumn,DateColumn,TypeColumn)
	VALUES (@bankName,@transStartRow,@stockName,@priceColumn,@quantityColumn,@dateColumn,@typeColumn)