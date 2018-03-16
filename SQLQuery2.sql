CREATE PROC insertStockDataNew
@name varchar(50),
@price float,
@time_id varchar(50)
AS
	INSERT INTO StockData.dbo.[Stock_WebData](Name,Price,TIME_ID)
	VALUES (@name,@price,@time_id)
