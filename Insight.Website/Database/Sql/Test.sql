CREATE TABLE [dbo].[Test] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullName] nvarchar(200) NOT NULL,
	[LastUpdatedBy] [int] NOT NULL,
	[LastUpdatedOn] [datetime2] NOT NULL Default(getutcdate())
)
GO

-- SCRIPT PopulateTest
SET IDENTITY_INSERT [dbo].[Test] ON
IF NOT EXISTS (SELECT [ID] FROM [dbo].[Test] WHERE [ID] = 1) INSERT [dbo].[Test] ([Id], [FullName], [LastUpdatedBy]) VALUES (1, N'This is a test', 1)
SET IDENTITY_INSERT [dbo].[Test] OFF
GO
