CREATE PROC insertBankTransaction
@exportDate varchar(50),
@transactionDate varchar(50),
@accountBalance int,
@difference int,
@income int,
@spending int,
@comment varchar(500),
@accountNumber varchar(50),
@bankName varchar(100)
AS
	INSERT INTO ImportFileData.dbo.[importedBankTransactions](ExportDate,TransactionDate,AccountBalance,Difference,Income,Spending,Comment,AccountNumber,BankName)
	VALUES (@exportDate,@transactionDate,@accountBalance,@difference,@income,@spending,@comment,@accountNumber,@bankName)