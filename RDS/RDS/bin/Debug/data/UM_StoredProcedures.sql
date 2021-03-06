if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_UM_PrivilegeSetEntry_UM_Privileges]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[UM_PrivilegeSetEntries] DROP CONSTRAINT FK_UM_PrivilegeSetEntry_UM_Privileges
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_UM_PrivilegeSetEntry_UM_PrivilegeSets]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[UM_PrivilegeSetEntries] DROP CONSTRAINT FK_UM_PrivilegeSetEntry_UM_PrivilegeSets
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[FK_UM_Users_UM_PrivilegeSets]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)
ALTER TABLE [dbo].[UM_Users] DROP CONSTRAINT FK_UM_Users_UM_PrivilegeSets
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeletePrivilegeByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeletePrivilegeByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeletePrivilegesForPrivilegeSet]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeletePrivilegesForPrivilegeSet]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeleteUserByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeleteUserByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeleteUserByName]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeleteUserByName]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_InsertPrivilegeSetEntry]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_InsertPrivilegeSetEntry]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_InsertUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_InsertUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegesForPrivilegeSetByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegesForPrivilegeSetByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectUserByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectUserByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectUserByUsername]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectUserByUsername]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectUserByUsernameAndPassword]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectUserByUsernameAndPassword]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_UpdateUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_UpdateUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeletePrivilegeByName]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeletePrivilegeByName]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeletePrivilegeSetByName]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeletePrivilegeSetByName]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_InsertPrivilege]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_InsertPrivilege]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_InsertPrivilegeSet]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_InsertPrivilegeSet]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegeByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegeByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegeSetByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegeSetByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_UpdatePrivilege]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_UpdatePrivilege]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_UpdatePrivilegeSet]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_UpdatePrivilegeSet]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_DeletePrivilegeSetByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_DeletePrivilegeSetByID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegeByName]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegeByName]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegeSetByName]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegeSetByName]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivilegeSets]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivilegeSets]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectPrivileges]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectPrivileges]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[um_SelectUsers]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[um_SelectUsers]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UM_PrivilegeSetEntries]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UM_PrivilegeSetEntries]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UM_Users]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UM_Users]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UM_Privileges]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UM_Privileges]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UM_PrivilegeSets]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UM_PrivilegeSets]
GO

CREATE TABLE [dbo].[UM_Privileges] (
	[PrivilegeID] [bigint] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Description] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UM_PrivilegeSets] (
	[PrivilegeSetID] [bigint] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UM_PrivilegeSetEntries] (
	[PrivilegeSet] [bigint] NOT NULL ,
	[Privilege] [bigint] NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UM_Users] (
	[UserID] [bigint] IDENTITY (1, 1) NOT NULL ,
	[Username] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Password] [varchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[FirstName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[LastName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Active] [bit] NOT NULL ,
	[PrivilegeSet] [bigint] NOT NULL ,
	[CreationDate] [datetime] NOT NULL ,
	[ModificationDate] [datetime] NULL ,
	[DeletionDate] [datetime] NULL 
) ON [PRIMARY]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeletePrivilegeSetByID
	@privilegeSetID bigint

AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_PrivilegeSetEntries table */
DELETE FROM [UM_PrivilegeSetEntries] WHERE PrivilegeSet=@privilegeSetID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*delete UM_PrivilegeSets table*/
DELETE FROM [UM_PrivilegesSet] WHERE PrivilegeSetID=@privilegeSetID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegeByName
	@name NVARCHAR(50)

AS

SET NOCOUNT ON

SELECT PrivilegeID, Name, Description
FROM UM_Privileges
WHERE Name=@name
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegeSetByName
	@name NVARCHAR(50)

AS

SET NOCOUNT ON

SELECT PrivilegeSetID, Name
FROM UM_PrivilegeSets
WHERE Name=@name
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegeSets
AS

SET NOCOUNT ON

SELECT PrivilegeSetID, Name
FROM UM_PrivilegeSets
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivileges

AS

SET NOCOUNT ON

SELECT PrivilegeID, Name, Description
FROM UM_Privileges
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectUsers

AS

SET NOCOUNT ON

SELECT UserID, Username, Password, FirstName, LastName, PrivilegeSet
FROM UM_Users
WHERE Active = 1
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeletePrivilegeByName
	@name NVarChar 

AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_PrivilegeSetEntries table */
DECLARE @privilegeID bigint;
SELECT @privilegeID=PrivilegeID  FROM [UM_Privileges] WHERE Name = @name
DELETE FROM [UM_PrivilegeSetEntries] WHERE Privilege = @privilegeID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*delete UM_Privileges table*/
DELETE FROM [UM_Privileges] WHERE Name=@name
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeletePrivilegeSetByName
	@name NVarChar

AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_PrivilegeSetEntries table */
DECLARE @privilegeSetID bigint;
SELECT @privilegeSetID=PrivilegeSetID FROM [UM_PrivilegeSets] WHERE Name = @name
DELETE FROM [UM_PrivilegeSetEntries] WHERE PrivilegeSet=@privilegeSetID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*delete UM_PrivilegeSets table*/
DELETE FROM [UM_PrivilegeSets] WHERE Name = @name
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_InsertPrivilege
	@name NVARCHAR(50),
	@description NVARCHAR(500),
	@privilegeID BIGINT OUT

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_Privileges table*/
INSERT [UM_Privileges]
(
	Name,
	Description
)
VALUES
(
	@name,
	@description
)
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

SET @privilegeID=@@identity;

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE um_InsertPrivilegeSet
	@name NVARCHAR(50),
	@privilegeSetID BIGINT OUT

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_PrivilegeSets table*/
INSERT [UM_PrivilegeSets]
(
	Name
)
VALUES
(
	@name
)
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

SET @privilegeSetID=@@identity;

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegeByID
	@privilegeID bigint

AS

SET NOCOUNT ON

SELECT PrivilegeID, Name, Description
FROM UM_Privileges
WHERE PrivilegeID=@privilegeID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegeSetByID
	@privilegeSetID bigint

AS

SET NOCOUNT ON

SELECT PrivilegeSetID, Name
FROM UM_PrivilegeSets
WHERE PrivilegeSetID=@privilegeSetID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_UpdatePrivilege
	@name NVARCHAR(50),
	@description NVARCHAR(500),
	@privilegeID bigint

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_Privileges table*/
UPDATE [UM_Privileges]
SET
	Name = @name,
	Description = @description
WHERE PrivilegeID = @privilegeID

SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_UpdatePrivilegeSet
	@name NVARCHAR(50),
	@privilegeSetID bigint

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_PrivilegeSets table*/
UPDATE [UM_PrivilegeSets]
SET
	Name = @name
WHERE PrivilegeSetID = @privilegeSetID

SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END


/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeletePrivilegeByID
	@privilegeID bigint

AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_PrivilegeSetEntries table */
DELETE FROM [UM_PrivilegeSetEntries] WHERE Privilege=@privilegeID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*delete UM_Privileges table*/
DELETE FROM [UM_Privileges] WHERE PrivilegeID=@privilegeID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeletePrivilegesForPrivilegeSet
	@privilegeSetID bigint

AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_PrivilegeSetEntries table */
DELETE FROM [UM_PrivilegeSetEntries] WHERE PrivilegeSet=@privilegeSetID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeleteUserByID
	@userID bigint,
	@deletionDate datetime
AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_Users table*/
UPDATE [UM_Users] 
SET Active = 0, DeletionDate = @deletionDate
WHERE UserID = @userID
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_DeleteUserByName
	@username NVARCHAR (50),
	@deletionDate datetime
AS

SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*delete UM_Users table*/
UPDATE [UM_Users] 
SET Active = 0, DeletionDate = @deletionDate
WHERE Username = @username
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_InsertPrivilegeSetEntry
	@privilegeSetID BIGINT,
	@privilegeID BIGINT

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_Privileges table*/
INSERT [UM_PrivilegeSetEntries]
(
	PrivilegeSet,
	Privilege
)
VALUES
(
	@privilegeSetID,
	@privilegeID
)
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_InsertUser
	@username NVARCHAR(50),
	@password NVARCHAR(512),
	@firstName NVARCHAR(50),
	@lastName NVARCHAR(50),
	@privilegeSet BIGINT,
	@creationDate datetime,
	@userID BIGINT OUT

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_Users table*/
INSERT [UM_Users]
(
	Username,
	Password,
	FirstName,
	LastName,
	Active,
	PrivilegeSet,
	CreationDate
)
VALUES
(
	@username,
	@password,
	@firstName,
	@lastName,
	1,
	@privilegeSet,
	@creationDate
)
SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

SET @userID=@@identity;

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectPrivilegesForPrivilegeSetByID
	@privilegeSetID bigint
AS

SET NOCOUNT ON

SELECT UM_Privileges.PrivilegeID, UM_Privileges.Name, UM_Privileges.Description
FROM UM_Privileges, UM_PrivilegeSetEntries
WHERE UM_Privileges.PrivilegeID = UM_PrivilegeSetEntries.Privilege
AND UM_PrivilegeSetEntries.PrivilegeSet = @privilegeSetID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectUserByID
	@userID bigint

AS

SET NOCOUNT ON

SELECT UserID, Username, Password, FirstName, LastName, PrivilegeSet
FROM UM_Users
WHERE UserID=@userID
AND Active = 1
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectUserByUsername
	@username NVARCHAR (50)

AS

SET NOCOUNT ON

SELECT UserID, Username, Password, FirstName, LastName, PrivilegeSet
FROM UM_Users
WHERE Username=@username
AND Active = 1
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_SelectUserByUsernameAndPassword
	@username NVARCHAR (50),
	@password NVARCHAR (512)

AS

SET NOCOUNT ON

SELECT UserID, Username, Password, FirstName, LastName, PrivilegeSet
FROM UM_Users
WHERE	Username = @username 
AND		Password = @password
AND		Active = 1
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE um_UpdateUser
	@username NVARCHAR(50),
	@password NVARCHAR(512),
	@firstName NVARCHAR(50),
	@lastName NVARCHAR(50),
	@privilegeSet bigint,
	@modificationDate datetime,
	@userID bigint

AS
SET NOCOUNT ON

DECLARE @err INT;

BEGIN TRANSACTION

/*fill UM_Users table*/
UPDATE [UM_Users]
SET
	Username = @username,
	Password = @password,
	FirstName = @firstName,
	LastName = @lastName,
	PrivilegeSet = @privilegeSet,
	ModificationDate = @modificationDate
WHERE UserID = @userID

SET @err = @@error IF @err <> 0 BEGIN ROLLBACK TRANSACTION RETURN @err END

/*success*/
COMMIT TRANSACTION
SET @err = @@error IF @err <> 0 RETURN @err
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

