CREATE PROC insertNewColumns3
@bankName varchar(50),
@transStartRow int,
@accountNumberPos varchar(50),
@dateColumn varchar(50),
@priceColumn varchar(50),
@balanceColumn varchar(50),
@commentColumn varchar(50)
AS
	INSERT INTO ImportFileData.dbo.[StoredColumns](BankName,TransStartRow,AccountNumberPos,DateColumn,PriceColumn,BalanceColumn,CommentColumn)
	VALUES (@bankName,@transStartRow,@accountNumberPos,@dateColumn,@priceColumn,@balanceColumn,@commentColumn)