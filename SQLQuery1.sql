CREATE PROC registrationQuery
@username varchar(50),
@password varchar(50),
@failedLogins int
AS
	INSERT INTO LoginDB.dbo.[Table](username,password,failedlogins)
	VALUES (@username,@password,@failedlogins)