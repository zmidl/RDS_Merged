IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_Delete];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_DeleteAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_DeleteAll];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_Insert];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_Read]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_Read];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_ReadAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_ReadAll];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[{0}_Update];
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}_Data]') AND type in (N'U'))
DROP TABLE [dbo].[{0}_Data];
DELETE FROM Components
WHERE Prefix = '{0}';