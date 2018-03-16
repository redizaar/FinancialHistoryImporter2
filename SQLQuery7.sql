CREATE PROC insertStockDataToSql
@name varchar(50),
@date varchar(50),
@openprice float,
@highprice float,
@lowprice float,
@closeprice float
AS
	INSERT INTO StockData.dbo.[Stock_WebData](Name,Date,openPrice,HighPrice,LowPrice,closePrice)
	VALUES (@name,@date,@openprice,@highprice,@lowprice,@closeprice)