CREATE PROC insertStockDataNew3
@name varchar(50),
@price float,
@date varchar(50),
@time_id varchar(50)
AS
	INSERT INTO StockData.dbo.[Stock_WebData](Name,Price,Date,TIME_ID)
	VALUES (@name,@price,@date,@time_id)

