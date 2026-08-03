CREATE TABLE Roles (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);
GO

CREATE TABLE Currencies (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Code      NVARCHAR(10)  NOT NULL UNIQUE,
    Name      NVARCHAR(50)  NOT NULL,
    Symbol    NVARCHAR(5)   NOT NULL
);
GO

CREATE TABLE Users (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    RoleId      INT            NOT NULL,
    Name        NVARCHAR(100)  NOT NULL,
    Email       NVARCHAR(150)  NOT NULL UNIQUE,
    Password    NVARCHAR(255)  NOT NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL,
    UpdatedAt   DATETIME       NULL,
    UpdatedBy   INT            NULL,
    DeletedAt   DATETIME       NULL,
    DeletedBy   INT            NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

CREATE TABLE Accounts (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL,
    CurrencyId  INT            NOT NULL,
    Name        NVARCHAR(100)  NOT NULL,
    Balance     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL,
    UpdatedAt   DATETIME       NULL,
    UpdatedBy   INT            NULL,
    DeletedAt   DATETIME       NULL,
    DeletedBy   INT            NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Accounts_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_Accounts_Currencies
        FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id)
);
GO

---
CREATE TABLE Categories (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL,
    Name        NVARCHAR(100)  NOT NULL,
    Type        NVARCHAR(10)   NOT NULL,
    Icon        NVARCHAR(50)   NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL,
    UpdatedAt   DATETIME       NULL,
    UpdatedBy   INT            NULL,
    DeletedAt   DATETIME       NULL,
    DeletedBy   INT            NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Categories_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT CHK_Categories_Type
        CHECK (Type IN ('INCOME', 'EXPENSE'))
);
GO

CREATE TABLE Transactions (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    AccountId    INT            NOT NULL,
    CategoryId   INT            NOT NULL,
    CurrencyId   INT            NOT NULL,
    Amount       DECIMAL(18,2)  NOT NULL,
    Type         NVARCHAR(10)   NOT NULL,
    Description  NVARCHAR(255)  NULL,
    Date         DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy    INT            NULL,
    UpdatedAt    DATETIME       NULL,
    UpdatedBy    INT            NULL,
    DeletedAt    DATETIME       NULL,
    DeletedBy    INT            NULL,
    IsActive     BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Transactions_Accounts
        FOREIGN KEY (AccountId) REFERENCES Accounts(Id),

    CONSTRAINT FK_Transactions_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id),

    CONSTRAINT FK_Transactions_Currencies
        FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),

    CONSTRAINT CHK_Transactions_Type
        CHECK (Type IN ('INCOME', 'EXPENSE'))
);
GO

CREATE TABLE [dbo].[ExchangeRates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[BaseCurrencyId] [int] NULL,
	[Rate] [decimal](18, 6) NOT NULL,
	[Date] [datetime] NOT NULL,
 CONSTRAINT [PK__Exchange__3214EC07067212F4] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ExchangeRates] ADD  CONSTRAINT [DF__ExchangeRa__Date__00200768]  DEFAULT (getdate()) FOR [Date]
GO

ALTER TABLE [dbo].[ExchangeRates]  WITH CHECK ADD  CONSTRAINT [FK_ExchangeRates_BaseCurrency] FOREIGN KEY([BaseCurrencyId])
REFERENCES [dbo].[Currencies] ([Id])
GO

ALTER TABLE [dbo].[ExchangeRates] CHECK CONSTRAINT [FK_ExchangeRates_BaseCurrency]
GO

ALTER TABLE [dbo].[ExchangeRates]  WITH CHECK ADD  CONSTRAINT [FK_ExchangeRates_Currencies] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[Currencies] ([Id])
GO

ALTER TABLE [dbo].[ExchangeRates] CHECK CONSTRAINT [FK_ExchangeRates_Currencies]
GO
----
CREATE TABLE SavingsGoals (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT            NOT NULL,
    CurrencyId      INT            NOT NULL,
    Name            NVARCHAR(100)  NOT NULL,
    TargetAmount    DECIMAL(18,2)  NOT NULL,
    CurrentAmount   DECIMAL(18,2)  NOT NULL DEFAULT 0,
    Deadline        DATETIME       NULL,
    Status          NVARCHAR(15)   NOT NULL DEFAULT 'IN_PROGRESS',
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy       INT            NULL,
    UpdatedAt       DATETIME       NULL,
    UpdatedBy       INT            NULL,
    DeletedAt       DATETIME       NULL,
    DeletedBy       INT            NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_SavingsGoals_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_SavingsGoals_Currencies
        FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),

    CONSTRAINT CHK_SavingsGoals_Status
        CHECK (Status IN ('IN_PROGRESS', 'COMPLETED', 'CANCELLED'))
);
GO

CREATE TABLE Budgets (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL,
    CategoryId  INT            NOT NULL,
    CurrencyId  INT            NOT NULL,
    Amount      DECIMAL(18,2)  NOT NULL,
    Month       INT            NOT NULL,
    Year        INT            NOT NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL,
    UpdatedAt   DATETIME       NULL,
    UpdatedBy   INT            NULL,
    DeletedAt   DATETIME       NULL,
    DeletedBy   INT            NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Budgets_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_Budgets_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id),

    CONSTRAINT FK_Budgets_Currencies
        FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id),

    CONSTRAINT UQ_Budgets_User_Category_Month_Year
        UNIQUE (UserId, CategoryId, Month, Year)
);
GO

CREATE TABLE Tags (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL,
    Name        NVARCHAR(50)   NOT NULL,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL,
    UpdatedAt   DATETIME       NULL,
    UpdatedBy   INT            NULL,
    DeletedAt   DATETIME       NULL,
    DeletedBy   INT            NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_Tags_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO
--
CREATE TABLE TransactionTags (
    TransactionId   INT  NOT NULL,
    TagId           INT  NOT NULL,

    CONSTRAINT PK_TransactionTags
        PRIMARY KEY (TransactionId, TagId),

    CONSTRAINT FK_TransactionTags_Transactions
        FOREIGN KEY (TransactionId) REFERENCES Transactions(Id),

    CONSTRAINT FK_TransactionTags_Tags
        FOREIGN KEY (TagId) REFERENCES Tags(Id)
);
GO

CREATE TABLE SharedExpenses (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT            NOT NULL,
    CurrencyId   INT            NOT NULL,
    Title        NVARCHAR(100)  NOT NULL,
    TotalAmount  DECIMAL(18,2)  NOT NULL,
    Date         DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy    INT            NULL,
    UpdatedAt    DATETIME       NULL,
    UpdatedBy    INT            NULL,
    DeletedAt    DATETIME       NULL,
    DeletedBy    INT            NULL,
    IsActive     BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_SharedExpenses_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_SharedExpenses_Currencies
        FOREIGN KEY (CurrencyId) REFERENCES Currencies(Id)
);
GO

CREATE TABLE SharedExpenseParticipants (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    SharedExpenseId   INT            NOT NULL,
    Name              NVARCHAR(100)  NOT NULL,
    AmountOwed        DECIMAL(18,2)  NOT NULL,
    IsPaid            BIT            NOT NULL DEFAULT 0,
    CreatedAt         DATETIME       NOT NULL DEFAULT GETDATE(),
    CreatedBy         INT            NULL,
    UpdatedAt         DATETIME       NULL,
    UpdatedBy         INT            NULL,
    DeletedAt         DATETIME       NULL,
    DeletedBy         INT            NULL,
    IsActive          BIT            NOT NULL DEFAULT 1,

    CONSTRAINT FK_SharedExpenseParticipants_SharedExpenses
        FOREIGN KEY (SharedExpenseId) REFERENCES SharedExpenses(Id)
);
GO