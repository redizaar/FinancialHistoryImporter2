CREATE PROC registrationQuery3
@username varchar(50),
@password varchar(50),
@accountnumber varchar(50)
AS
	INSERT INTO LoginDB.dbo.[UserDatas](Username,Password,AccountNumber)
	VALUES (@username,@password,@accountnumber)