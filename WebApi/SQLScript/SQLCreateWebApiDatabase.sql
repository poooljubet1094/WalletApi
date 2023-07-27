CREATE DATABASE WalletApi;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionHistory](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [decimal](18, 0) NOT NULL,
	[FromAccountNumber] [bigint] NOT NULL,
	[ToAccountNumber] [bigint] NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
	[EndBalance] [decimal](18, 0) NOT NULL,
	[TransactionType] [int] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[TransactionHistory] ADD PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[LoginName] [varchar](255) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[Balance] [decimal](18, 0) NOT NULL,
	[RegisterDate] [datetime] NOT NULL,
	[AccountNumber] [bigint] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[LoginName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [Balance]
GO
CREATE FUNCTION GetEndBalance (
	@Balance DECIMAL,
    @Amount DECIMAL,
    @TransactionType INT
)
RETURNS BIGINT AS
BEGIN
	DECLARE @return_value DECIMAL;

    IF (@TransactionType = 0)
    BEGIN
        SET @return_value = @Balance + @Amount;
    END

    IF (@TransactionType = 1 OR @TransactionType = 2)
    BEGIN
        SET @return_value =@Balance - @Amount;
    END
 
    RETURN @return_value
END;
CREATE PROCEDURE [dbo].[RegisterUser]
    @LoginName varchar(255),
    @Password varchar(255)
AS

    INSERT INTO dbo.Users (
        LoginName,
        [Password],
        RegisterDate
    ) VALUES (
        @LoginName,
        @Password,
        (SELECT GETDATE())
    );

    UPDATE dbo.Users
    SET
        AccountNumber = (YEAR(GETDATE()) * 100000000000) + SCOPE_IDENTITY()
    WHERE
        UserId = SCOPE_IDENTITY();

    SELECT
        UserId,
        LoginName,
        [Password],
        Balance,
        RegisterDate,
        AccountNumber
    FROM
        [dbo].[Users]
    WHERE
        UserId = SCOPE_IDENTITY();
GO
CREATE PROCEDURE [dbo].[AccountNumberExist]
    @AccountNumber BIGINT
AS

    SELECT
        COUNT(AccountNumber)
    FROM
        [dbo].[Users]
    WHERE
        AccountNumber = @AccountNumber
    GROUP BY
        AccountNumber;
GO
CREATE PROCEDURE [dbo].[GetUserByAccountNumber]
    @AccountNumber BIGINT
AS

    SELECT
        UserId,
        LoginName,
        Password,
        Balance,
        RegisterDate,
        AccountNumber
    FROM
        [dbo].[Users]
    WHERE
        AccountNumber = @AccountNumber;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ProcessWalletTransaction]
    @Amount DECIMAL,
    @FromAccountNumber BIGINT,
    @ToAccountNumber BIGINT,
    @TransactionType INT
AS
    DECLARE @FromEndBalance DECIMAL;

    SELECT TOP 1 @FromEndBalance = dbo.GetEndBalance(Balance, @Amount, @TransactionType) FROM dbo.Users WHERE AccountNumber = @FromAccountNumber;

    IF (@FromEndBalance < 0)
    BEGIN
        PRINT N'You dont have balance to proceed in this request.';
        THROW 51000, 'You dont have balance to proceed in this request.', 1;
    END

    INSERT INTO [dbo].[TransactionHistory] (
        Amount,
        FromAccountNumber,
        ToAccountNumber,
        TransactionDate,
        EndBalance,
        TransactionType
    ) VALUES (
        @Amount,
        @FromAccountNumber,
        @ToAccountNumber,
        (SELECT GETDATE()),
        @FromEndBalance,
        @TransactionType
    );

    IF (@TransactionType = 0)
    BEGIN
        UPDATE dbo.Users
        SET
            Balance += @Amount
        WHERE
            AccountNumber = @FromAccountNumber;
    END

    IF (@TransactionType = 1)
    BEGIN
        UPDATE dbo.Users
        SET
            Balance -= @Amount
        WHERE
            AccountNumber = @FromAccountNumber;
    END

    IF (@TransactionType = 2)
    BEGIN
        UPDATE dbo.Users
        SET
            Balance -= @Amount
        WHERE
            AccountNumber = @FromAccountNumber;
        
        UPDATE dbo.Users
        SET
            Balance += @Amount
        WHERE
            AccountNumber = @ToAccountNumber;
    END

    SELECT
        TransactionId,
        Amount,
        FromAccountNumber,
        ToAccountNumber,
        TransactionDate,
        EndBalance,
        TransactionType
    FROM
        [dbo].[TransactionHistory]
    WHERE
        TransactionId = SCOPE_IDENTITY();
GO

