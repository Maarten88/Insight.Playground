CREATE TABLE [dbo].[Contact](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MailAddress] [nvarchar](50) NOT NULL,
	[IsEmailConfirmed] [bit] NOT NULL Default(0),
	[IsDisabled] [bit] NOT NULL Default(0),
	[PasswordHash] [nvarchar](max) NULL,
	[LastUpdatedBy] [int] NOT NULL Default(IDENT_CURRENT('dbo.Contact')),
	[LastUpdatedOn] [datetime2] NOT NULL Default(SYSUTCDATETIME())
)
GO

ALTER TABLE [dbo].[Contact]
ADD CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
GO

CREATE INDEX [IX_Contact_Email] ON [dbo].[Contact] ([MailAddress])                                                                                                                                                           
GO

-- SCRIPT InsertFirstUser
SET IDENTITY_INSERT [dbo].[Contact] ON
IF NOT EXISTS (SELECT [ID] FROM [dbo].[Contact] WHERE [ID] = 1) INSERT [dbo].[Contact] ([ID], [MailAddress], [PasswordHash]) VALUES (1, N'test@test.com', 'AHDPvOSdJSm/Hqd78v+W6mdIXnt6SnF4uzQdld5COEtGqot3nknQXazdMv0LLEwWlw==') -- 123456
SET IDENTITY_INSERT [dbo].[Contact] OFF
GO



CREATE PROCEDURE InsertContact
(
	@MailAddress nvarchar(255), 
	@IsEmailConfirmed bit, 
	@IsDisabled bit, 
	@PasswordHash nvarchar(max),
	@LastUpdatedBy int = 0
)
AS
BEGIN
	INSERT INTO [dbo].[Contact]
	(
		MailAddress,
		IsEmailConfirmed,
		IsDisabled,
		PasswordHash, 
		LastUpdatedBy, 
		LastUpdatedOn
	)
	VALUES
	(
		@MailAddress,
		@IsEmailConfirmed,
		@IsDisabled,
		@PasswordHash, 
		IIF(@LastUpdatedBy = 0, IDENT_CURRENT('dbo.Contact'), @LastUpdatedBy),
		GETUTCDATE()
	)
	RETURN @@IDENTITY
END
GO

CREATE PROCEDURE UpdateContact
(
	@Id int,
	@MailAddress nvarchar(255), 
	@IsEmailConfirmed bit, 
	@IsDisabled bit, 
	@PasswordHash nvarchar(max),
	@LastUpdatedBy int = 0
)
AS
BEGIN
	UPDATE 
		[dbo].[Contact] 
	SET
		MailAddress		  = @MailAddress,
		IsEmailConfirmed  = @IsEmailConfirmed,
		IsDisabled		  = @IsDisabled,
		PasswordHash	  = @PasswordHash, 
		LastUpdatedBy	  = IIF(@LastUpdatedBy = 0, IDENT_CURRENT('dbo.Contact'), @LastUpdatedBy),
		LastUpdatedOn	  = GETUTCDATE()
	WHERE
		Id = @Id
END
GO

CREATE PROCEDURE DeleteContact
(
	@Id int
)
AS
BEGIN
	DELETE FROM
		[dbo].[Contact] 
	WHERE
		Id = @Id
END
GO


CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL
)
GO

ALTER TABLE [dbo].[Role]
ADD CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
GO

CREATE TABLE [dbo].[ContactRole](
	[ContactId] int NOT NULL,
	[RoleId] int NOT NULL
)
GO

ALTER TABLE [dbo].[ContactRole]
ADD CONSTRAINT [PK_ContactRole] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC,
	[RoleId] ASC
)
GO

CREATE TABLE [dbo].[ContactLogin](
	[ContactId] int NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL
)
GO

ALTER TABLE [dbo].[ContactLogin]
ADD CONSTRAINT [PK_ContactLogin] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC,
	[LoginProvider] ASC,
	[ProviderKey] ASC
)
GO

CREATE PROCEDURE [dbo].[SelectContactLogin]
(
	@ContactId int
)
AS
SELECT * FROM [ContactLogin] WHERE 
	[ContactId]=@ContactId
GO

CREATE PROCEDURE InsertContactLogin
(
	@ContactId int, 
	@LoginProvider nvarchar(255), 
	@ProviderKey nvarchar(255)
)
AS
BEGIN
	INSERT INTO [dbo].[ContactLogin]
	(
	[ContactId],
	[LoginProvider],
	[ProviderKey]
	)
	VALUES
	(
	@ContactId,
	@LoginProvider,
	@ProviderKey
	)
END
GO

CREATE PROCEDURE DeleteContactLogin
(
	@ContactId int, 
	@LoginProvider nvarchar(255), 
	@ProviderKey nvarchar(255)
)
AS
BEGIN
	DELETE FROM [dbo].[ContactLogin] 
	WHERE 
		[ContactId] = @ContactId AND
		[LoginProvider] = @LoginProvider AND
		[ProviderKey] = @ProviderKey
END
GO

CREATE PROCEDURE GetContactLogins
(
	@ContactId int
)
AS
BEGIN
	SELECT * FROM [dbo].[ContactLogin] WHERE [ContactId] = @ContactId
END
GO


CREATE PROCEDURE FindContactFromExternalLogin
(
	@LoginProvider nvarchar(255), 
	@ProviderKey nvarchar(255)
)
AS
BEGIN
	SELECT * FROM Contact INNER JOIN ContactLogin ON Contact.ID = ContactLogin.ContactId WHERE ContactLogin.LoginProvider = @LoginProvider AND ContactLogin.ProviderKey = @ProviderKey AND Contact.IsDisabled = 0
END
GO


CREATE PROCEDURE [dbo].[GetContactRolesByContactId]
	@Id int
AS
BEGIN
	SELECT [Name] FROM [dbo].[Role]
		INNER JOIN [dbo].[ContactRole] ON [dbo].[Role].[Id] = [dbo].[ContactRole].[RoleId]
		WHERE [ContactRole].[ContactId] = @Id
END
GO

CREATE PROCEDURE [dbo].[IsContactInRole]
	@ContactId int,
	@RoleName nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT CAST(CASE WHEN EXISTS(
		SELECT * FROM [dbo].[ContactRole]
			INNER JOIN [dbo].[Role] ON [dbo].[ContactRole].[RoleId] = [dbo].[Role].[Id]
			WHERE [ContactId] = @ContactId
			AND [Name] = @RoleName
	) THEN 1 ELSE 0 END AS BIT)
END
GO

CREATE PROCEDURE [dbo].[InsertContactRole]
(
	@ContactId int,
	@RoleName nvarchar(max)
)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RoleId int;

	SELECT @RoleId = [Id] FROM [dbo].[Role]
	WHERE [Name] = @RoleName

	IF (@RoleId IS NOT NULL)
	BEGIN
		INSERT INTO [dbo].[ContactRole] ([ContactId],	[RoleId]) VALUES(@ContactId, @RoleId)
	END
	ELSE
		THROW 2630001, 'Role does not exists in Role table!', 1
END
GO

CREATE PROCEDURE [dbo].[DeleteContactRoleByContactIdAndRole]
(
	@ContactId int,
	@RoleName nvarchar(max)
)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @RoleId int;

	SELECT @RoleId = [Id] FROM [dbo].[Role]
	WHERE [Name] = @RoleName

	DELETE FROM [ContactRole] WHERE
	[ContactId] = @ContactId AND
	[RoleId] = @RoleId
END
GO

CREATE PROCEDURE [dbo].GetContactById
(
	@Id int
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		*
	FROM 
		[dbo].[Contact]
	WHERE 
		Id = @Id 
END
GO

CREATE PROCEDURE [dbo].GetContactByEmail
(
	@Email nvarchar(250)
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		*
	FROM 
		[dbo].[Contact]
	WHERE 
		[MailAddress] = @Email
END
GO

CREATE PROCEDURE [dbo].[WriteLoginHistory]
	@MailAddress nvarchar(255),
	@LoginSuccess bit,
	@IPAddress nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN
		INSERT [dbo].[LoginHistory]
			(MailAddress, LoginSuccess, IPAddress)
		VALUES
			(@MailAddress, @LoginSuccess, @IPAddress)
	END
END
GO

CREATE NONCLUSTERED INDEX [IX_ContactId] ON [dbo].[ContactLogin]
(
	[ContactId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[ContactRole]
(
	[RoleId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_ContactId] ON [dbo].[ContactRole]
(
	[ContactId] ASC
)
GO

CREATE PROCEDURE [dbo].[GetContactPasswordHashById]
	@Id int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		[PasswordHash]
	FROM 
		[dbo].[Contact]
	WHERE 
		[ID] = @Id
END
GO

CREATE PROCEDURE [dbo].[SetPasswordHash]
	@Id int,
	@PasswordHash nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE [dbo].[Contact]
	SET [PasswordHash] = @PasswordHash
	WHERE [ID] = @Id
END
GO


CREATE PROCEDURE [dbo].[HasPassword]
	@Id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CAST(CASE WHEN [PasswordHash] IS NULL THEN 0 ELSE 1 END AS BIT)
	FROM [dbo].[Contact] 
	WHERE [ID] = @Id
END
GO

ALTER TABLE [dbo].[ContactLogin]  WITH CHECK ADD CONSTRAINT [FK_ContactLogins_Contact_Id] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contact] ([ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ContactRole]  WITH CHECK ADD CONSTRAINT [FK_ContactRole_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ContactRole]  WITH CHECK ADD CONSTRAINT [FK_ContactRole_Contact_Id] FOREIGN KEY([ContactId])
REFERENCES [dbo].[Contact] ([ID])
ON DELETE CASCADE
GO

CREATE PROCEDURE [dbo].GetClaims
(
	@Id int
)
AS
BEGIN
	--SELECT 'http://shakethecounter.com/identity/claims/saleschannel' ClaimType, [Name] ClaimValue FROM [dbo].[Contact] INNER JOIN [dbo].[SalesChannel] ON [SalesChannel].[ID] = [Contact].[SalesChannelID] WHERE [Contact].[ID] = @Id
	--UNION
	--SELECT 'http://shakethecounter.com/identity/claims/partner' ClaimType, [Name] ClaimValue FROM [dbo].[Partner] INNER JOIN [dbo].[PartnerContact] ON [Partner].[ID] = [PartnerContact].[PartnerID] WHERE [PartnerContact].[ContactID] = @Id
	
	-- dummy
	SELECT 'http://shakethecounter.com/identity/claims/saleschannel' ClaimType, '1' ClaimValue
END
GO

CREATE TABLE [dbo].[LoginHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MailAddress] [nvarchar](50) NOT NULL,
	[LoginSuccess] [bit] NOT NULL Default(0),
	[IPAddress] [nvarchar](50),
	[LogTime] [datetime2] NOT NULL Default(SYSUTCDATETIME())
)
GO
