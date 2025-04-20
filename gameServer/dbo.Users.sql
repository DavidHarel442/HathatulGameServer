CREATE TABLE [dbo].[Users] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [username]  NVARCHAR (50) NOT NULL,
    [password]  NVARCHAR (50) NOT NULL,
    [firstName] NVARCHAR (50) NOT NULL,
    [last name] NVARCHAR (50) NOT NULL,
    [email]     NVARCHAR (50) NOT NULL,
    [city]      NVARCHAR (50) NOT NULL,
    [gender]    NVARCHAR (50) NOT NULL,
    [Token] NVARCHAR(50) NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

