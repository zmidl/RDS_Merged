CREATE PROCEDURE [dbo].[{0}_Delete] 
	@name nvarchar(255)
AS
BEGIN
	DELETE FROM {0}_Data
	WHERE Name = @name
END
GO

CREATE PROCEDURE [dbo].[{0}_DeleteAll] 
AS
BEGIN
	DELETE FROM {0}_Data
END
GO

CREATE PROCEDURE [dbo].[{0}_Insert] 
	@name nvarchar(255), 
	@data nvarchar(max),
	@modDate datetime
AS
BEGIN
	INSERT INTO {0}_Data (Name, Data, ModDate)
	VALUES( @name, @data, @modDate )
END
GO

CREATE PROCEDURE [dbo].[{0}_Read]
	@name nvarchar(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Data FROM {0}_Data
	WHERE Name = @name
END
GO

CREATE PROCEDURE [dbo].[{0}_ReadAll]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Name, Data FROM {0}_Data
END
GO

CREATE PROCEDURE [dbo].[{0}_Update]
	@name nvarchar(255), 
	@data nvarchar(max),
	@modDate datetime
AS
BEGIN
	UPDATE {0}_Data
	SET Data = @data, ModDate = @modDate
	WHERE Name = @name
END
GO

CREATE TABLE [dbo].[{0}_Data](
	[Name] [nvarchar](255) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
	[ModDate] [datetime] NOT NULL,
 CONSTRAINT [PK_{0}_Data] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]