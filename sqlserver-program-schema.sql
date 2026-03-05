IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] TEXT NOT NULL,
    [Name] TEXT NULL,
    [NormalizedName] TEXT NULL,
    [ConcurrencyStamp] TEXT NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] TEXT NOT NULL,
    [FirstName] TEXT NULL,
    [LastName] TEXT NULL,
    [Department] TEXT NULL,
    [Section] TEXT NULL,
    [Plant] TEXT NULL,
    [IsActive] INTEGER NOT NULL,
    [IsIT] INTEGER NOT NULL,
    [UserName] TEXT NULL,
    [NormalizedUserName] TEXT NULL,
    [Email] TEXT NULL,
    [NormalizedEmail] TEXT NULL,
    [EmailConfirmed] INTEGER NOT NULL,
    [PasswordHash] TEXT NULL,
    [SecurityStamp] TEXT NULL,
    [ConcurrencyStamp] TEXT NULL,
    [PhoneNumber] TEXT NULL,
    [PhoneNumberConfirmed] INTEGER NOT NULL,
    [TwoFactorEnabled] INTEGER NOT NULL,
    [LockoutEnd] TEXT NULL,
    [LockoutEnabled] INTEGER NOT NULL,
    [AccessFailedCount] INTEGER NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Departments] (
    [Id] INTEGER NOT NULL,
    [Name] TEXT NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);

CREATE TABLE [MasterDataCombinations] (
    [Id] INTEGER NOT NULL,
    [DepartmentName] TEXT NOT NULL,
    [SectionName] TEXT NOT NULL,
    [PlantName] TEXT NOT NULL,
    [CreatedAt] TEXT NOT NULL,
    [UpdatedAt] TEXT NOT NULL,
    [CreatedBy] TEXT NOT NULL,
    [UpdatedBy] TEXT NOT NULL,
    CONSTRAINT [PK_MasterDataCombinations] PRIMARY KEY ([Id])
);

CREATE TABLE [RequestItems] (
    [Id] INTEGER NOT NULL,
    [Description] TEXT NOT NULL,
    [Requester] TEXT NOT NULL,
    [Status] TEXT NOT NULL,
    [RequestDate] TEXT NOT NULL,
    [PlantFG] TEXT NULL,
    [ItemCode] TEXT NULL,
    [EnglishMatDescription] TEXT NULL,
    [ModelName] TEXT NULL,
    [BaseUnit] TEXT NULL,
    [MaterialGroup] TEXT NULL,
    [DivisionCode] TEXT NULL,
    [ProfitCenter] TEXT NULL,
    [DistributionChannel] TEXT NULL,
    [StandardPack] TEXT NULL,
    [BoiCode] TEXT NULL,
    [MrpController] TEXT NULL,
    [StorageLocation] TEXT NULL,
    [ProductionSupervisor] TEXT NULL,
    [CostingLotSize] INTEGER NULL,
    [ValClass] TEXT NULL,
    [Price] decimal(18, 2) NULL,
    [CreatedBy] TEXT NULL,
    [CreatedAt] TEXT NULL,
    CONSTRAINT [PK_RequestItems] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] INTEGER NOT NULL,
    [RoleId] TEXT NOT NULL,
    [ClaimType] TEXT NULL,
    [ClaimValue] TEXT NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] INTEGER NOT NULL,
    [UserId] TEXT NOT NULL,
    [ClaimType] TEXT NULL,
    [ClaimValue] TEXT NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] TEXT NOT NULL,
    [ProviderKey] TEXT NOT NULL,
    [ProviderDisplayName] TEXT NULL,
    [UserId] TEXT NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] TEXT NOT NULL,
    [RoleId] TEXT NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] TEXT NOT NULL,
    [LoginProvider] TEXT NOT NULL,
    [Name] TEXT NOT NULL,
    [Value] TEXT NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Plants] (
    [Id] INTEGER NOT NULL,
    [Name] TEXT NOT NULL,
    [DepartmentId] INTEGER NOT NULL,
    CONSTRAINT [PK_Plants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Plants_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Sections] (
    [Id] INTEGER NOT NULL,
    [Name] TEXT NOT NULL,
    [DepartmentId] INTEGER NOT NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]);

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]);

CREATE INDEX [IX_Plants_DepartmentId] ON [Plants] ([DepartmentId]);

CREATE INDEX [IX_Sections_DepartmentId] ON [Sections] ([DepartmentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260221134529_InitialCreate', N'9.0.0');

EXEC sp_rename N'[RequestItems].[CreatedBy]', N'Transportation', 'COLUMN';

EXEC sp_rename N'[RequestItems].[CreatedAt]', N'ToolingBSection', 'COLUMN';

ALTER TABLE [RequestItems] ADD [AccountAssignment] TEXT NULL;

ALTER TABLE [RequestItems] ADD [AsiOfPlant] TEXT NULL;

ALTER TABLE [RequestItems] ADD [AssemblyPlant] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Availability] TEXT NULL;

ALTER TABLE [RequestItems] ADD [BoiDescription] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Check] INTEGER NOT NULL DEFAULT CAST(0 AS INTEGER);

ALTER TABLE [RequestItems] ADD [CodenMid] TEXT NULL;

ALTER TABLE [RequestItems] ADD [CommCodeTariffCode] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Currency] TEXT NULL;

ALTER TABLE [RequestItems] ADD [CurrentICS] TEXT NULL;

ALTER TABLE [RequestItems] ADD [DateIn] TEXT NULL;

ALTER TABLE [RequestItems] ADD [DevicePlant] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Effective] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ExternalMaterialGroup] TEXT NULL;

ALTER TABLE [RequestItems] ADD [FixedLot] TEXT NULL;

ALTER TABLE [RequestItems] ADD [GeneralItemCategory] TEXT NULL;

ALTER TABLE [RequestItems] ADD [IpoPlant] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Level] TEXT NULL;

ALTER TABLE [RequestItems] ADD [LoadingGroup] TEXT NULL;

ALTER TABLE [RequestItems] ADD [MakerMfrPartNumber] TEXT NULL;

ALTER TABLE [RequestItems] ADD [MatType] TEXT NULL;

ALTER TABLE [RequestItems] ADD [MaterialStatisticsGroup] TEXT NULL;

ALTER TABLE [RequestItems] ADD [MaxLot] TEXT NULL;

ALTER TABLE [RequestItems] ADD [MinLot] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Mtlsm] TEXT NULL;

ALTER TABLE [RequestItems] ADD [PlanDelTime] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Planner] TEXT NULL;

ALTER TABLE [RequestItems] ADD [PoNumber] TEXT NULL;

ALTER TABLE [RequestItems] ADD [PriceControl] TEXT NULL;

ALTER TABLE [RequestItems] ADD [PriceUnit] INTEGER NULL;

ALTER TABLE [RequestItems] ADD [PurchasingGroup] TEXT NULL;

ALTER TABLE [RequestItems] ADD [QuotationNumber] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ReceiveStorage] TEXT NULL;

ALTER TABLE [RequestItems] ADD [RequestType] TEXT NOT NULL DEFAULT '';

ALTER TABLE [RequestItems] ADD [Rohs] TEXT NULL;

ALTER TABLE [RequestItems] ADD [Rounding] TEXT NULL;

ALTER TABLE [RequestItems] ADD [SalesOrg] TEXT NULL;

ALTER TABLE [RequestItems] ADD [SchedMargin] TEXT NULL;

ALTER TABLE [RequestItems] ADD [StatusInA] TEXT NULL;

ALTER TABLE [RequestItems] ADD [StorageLoc] TEXT NULL;

ALTER TABLE [RequestItems] ADD [StorageLocationB1] TEXT NULL;

ALTER TABLE [RequestItems] ADD [StorageLocationEP] TEXT NULL;

ALTER TABLE [RequestItems] ADD [SupplierCode] TEXT NULL;

ALTER TABLE [RequestItems] ADD [TariffCode] TEXT NULL;

ALTER TABLE [RequestItems] ADD [TaxTh] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ToolingBModel] TEXT NULL;

ALTER TABLE [RequestItems] ADD [TraffCodePercentage] decimal(18, 2) NULL;

CREATE TABLE [BomComponents] (
    [Id] INTEGER NOT NULL,
    [RequestItemId] INTEGER NOT NULL,
    [Level] INTEGER NOT NULL,
    [Item] TEXT NULL,
    [ItemCat] TEXT NULL,
    [ComponentNumber] TEXT NULL,
    [Description] TEXT NULL,
    [ItemQuantity] decimal(18, 5) NULL,
    [Unit] TEXT NULL,
    [BomUsage] TEXT NULL,
    [Plant] TEXT NULL,
    [Sloc] TEXT NULL,
    CONSTRAINT [PK_BomComponents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BomComponents_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Routings] (
    [Id] INTEGER NOT NULL,
    [RequestItemId] INTEGER NOT NULL,
    [Material] TEXT NULL,
    [Description] TEXT NULL,
    [WorkCenter] TEXT NULL,
    [Operation] TEXT NULL,
    [BaseQty] decimal(18, 5) NULL,
    [Unit] TEXT NULL,
    [DirectLaborCosts] decimal(18, 5) NULL,
    [DirectExpenses] decimal(18, 5) NULL,
    [AllocationExpense] decimal(18, 5) NULL,
    [ProductionVersionCode] TEXT NULL,
    [Version] TEXT NULL,
    [ValidFrom] TEXT NULL,
    [ValidTo] TEXT NULL,
    [MaximumLotSize] decimal(18, 5) NULL,
    [Group] TEXT NULL,
    [GroupCounter] TEXT NULL,
    CONSTRAINT [PK_Routings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Routings_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_BomComponents_RequestItemId] ON [BomComponents] ([RequestItemId]);

CREATE INDEX [IX_Routings_RequestItemId] ON [Routings] ([RequestItemId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260223065828_AddBomAndRoutingTables', N'9.0.0');

ALTER TABLE [AspNetUsers] ADD [CreatedAt] TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE [AspNetUsers] ADD [CreatedBy] TEXT NULL;

ALTER TABLE [AspNetUsers] ADD [UpdatedAt] TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE [AspNetUsers] ADD [UpdatedBy] TEXT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260223092442_ConsolidateUser', N'9.0.0');

EXEC sp_rename N'[Sections].[Name]', N'SectionName', 'COLUMN';

EXEC sp_rename N'[Sections].[Id]', N'SectionId', 'COLUMN';

EXEC sp_rename N'[Plants].[Name]', N'PlantName', 'COLUMN';

EXEC sp_rename N'[Plants].[Id]', N'PlantId', 'COLUMN';

EXEC sp_rename N'[Departments].[Name]', N'DepartmentName', 'COLUMN';

EXEC sp_rename N'[Departments].[Id]', N'DepartmentId', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260223094110_UpdateSchema', N'9.0.0');

CREATE TABLE [DocumentTypes] (
    [Id] INTEGER NOT NULL,
    [Name] TEXT NOT NULL,
    [Description] TEXT NULL,
    CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [DocumentRoutings] (
    [Id] INTEGER NOT NULL,
    [DocumentTypeId] INTEGER NOT NULL,
    [DepartmentId] INTEGER NOT NULL,
    [SectionId] INTEGER NOT NULL,
    [PlantId] INTEGER NOT NULL,
    [Step] INTEGER NOT NULL,
    CONSTRAINT [PK_DocumentRoutings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DocumentRoutings_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentRoutings_DocumentTypes_DocumentTypeId] FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentRoutings_Plants_PlantId] FOREIGN KEY ([PlantId]) REFERENCES [Plants] ([PlantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_DocumentRoutings_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([SectionId]) ON DELETE CASCADE
);

CREATE INDEX [IX_DocumentRoutings_DepartmentId] ON [DocumentRoutings] ([DepartmentId]);

CREATE INDEX [IX_DocumentRoutings_DocumentTypeId] ON [DocumentRoutings] ([DocumentTypeId]);

CREATE INDEX [IX_DocumentRoutings_PlantId] ON [DocumentRoutings] ([PlantId]);

CREATE INDEX [IX_DocumentRoutings_SectionId] ON [DocumentRoutings] ([SectionId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260224093217_AddDocumentRouting', N'9.0.0');

ALTER TABLE [RequestItems] ADD [NextApproverId] TEXT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260225023651_AddNextApproverIdToRequestItem', N'9.0.0');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260225050147_AddDepartmentAndSectionTables', N'9.0.0');

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DocumentTypes]') AND [c].[name] = N'Description');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [DocumentTypes] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [DocumentTypes] DROP COLUMN [Description];

EXEC sp_rename N'[DocumentTypes].[Id]', N'DocumentTypeId', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260225065550_AddDocumentTypeIdToDocumentRoutings', N'9.0.0');

CREATE TABLE [NewsArticles] (
    [Id] INTEGER NOT NULL,
    [Title] TEXT NOT NULL,
    [Content] TEXT NOT NULL,
    [ImageUrl] TEXT NULL,
    [PublishedDate] TEXT NOT NULL,
    [Author] TEXT NOT NULL,
    [IsFeatured] INTEGER NOT NULL,
    CONSTRAINT [PK_NewsArticles] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260226015922_AddNewsArticle', N'9.0.0');

EXEC sp_rename N'[RequestItems].[PlantFG]', N'Plant', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260226040308_RenamePlantFGToPlant', N'9.0.0');

ALTER TABLE [RequestItems] ADD [ItemCodeForm] TEXT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260228021026_AddItemCodeForm', N'9.0.0');

EXEC sp_rename N'[RequestItems].[ItemCodeForm]', N'UnitTo', 'COLUMN';

ALTER TABLE [RequestItems] ADD [BomUsageFrom] TEXT NULL;

ALTER TABLE [RequestItems] ADD [BomUsageTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [DescriptionFrom] TEXT NULL;

ALTER TABLE [RequestItems] ADD [DescriptionTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ItemCodeTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ItemQuantityFrom] TEXT NULL;

ALTER TABLE [RequestItems] ADD [ItemQuantityTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [PlantTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [SlocFrom] TEXT NULL;

ALTER TABLE [RequestItems] ADD [SlocTo] TEXT NULL;

ALTER TABLE [RequestItems] ADD [UnitFrom] TEXT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260228034856_AddDescriptionFrom', N'9.0.0');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'BomUsageFrom');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [BomUsageFrom];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'BomUsageTo');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [BomUsageTo];

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'DescriptionFrom');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [DescriptionFrom];

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'DescriptionTo');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [DescriptionTo];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'ItemCodeTo');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [ItemCodeTo];

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'ItemQuantityFrom');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [ItemQuantityFrom];

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'ItemQuantityTo');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [ItemQuantityTo];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'PlantTo');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [PlantTo];

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'SlocFrom');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [SlocFrom];

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'SlocTo');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [SlocTo];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RequestItems]') AND [c].[name] = N'UnitFrom');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [RequestItems] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [RequestItems] DROP COLUMN [UnitFrom];

EXEC sp_rename N'[RequestItems].[UnitTo]', N'EditBomFg', 'COLUMN';

ALTER TABLE [RequestItems] ADD [EditBomAllFg] INTEGER NOT NULL DEFAULT CAST(0 AS INTEGER);

CREATE TABLE [BomEditComponent] (
    [Id] INTEGER NOT NULL,
    [ItemCodeFrom] TEXT NULL,
    [DescriptionFrom] TEXT NULL,
    [ItemQuantityFrom] TEXT NULL,
    [UnitFrom] TEXT NULL,
    [BomUsageFrom] TEXT NULL,
    [SlocFrom] TEXT NULL,
    [ItemCodeTo] TEXT NULL,
    [DescriptionTo] TEXT NULL,
    [ItemQuantityTo] TEXT NULL,
    [UnitTo] TEXT NULL,
    [BomUsageTo] TEXT NULL,
    [SlocTo] TEXT NULL,
    [PlantTo] TEXT NULL,
    [RequestItemId] INTEGER NULL,
    CONSTRAINT [PK_BomEditComponent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BomEditComponent_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id])
);

CREATE INDEX [IX_BomEditComponent_RequestItemId] ON [BomEditComponent] ([RequestItemId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260302040659_AddEditBomFgFields', N'9.0.0');

CREATE TABLE [LicensePermissionItems] (
    [Id] INTEGER NOT NULL,
    [RequestItemId] INTEGER NOT NULL,
    [SapUsername] TEXT NULL,
    [TCode] TEXT NULL,
    CONSTRAINT [PK_LicensePermissionItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LicensePermissionItems_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_LicensePermissionItems_RequestItemId] ON [LicensePermissionItems] ([RequestItemId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303011140_AddLicensePermissionItems', N'9.0.0');

ALTER TABLE [DocumentRoutings] DROP CONSTRAINT [FK_DocumentRoutings_Departments_DepartmentId];

ALTER TABLE [DocumentRoutings] DROP CONSTRAINT [FK_DocumentRoutings_DocumentTypes_DocumentTypeId];

ALTER TABLE [DocumentRoutings] DROP CONSTRAINT [FK_DocumentRoutings_Plants_PlantId];

ALTER TABLE [DocumentRoutings] DROP CONSTRAINT [FK_DocumentRoutings_Sections_SectionId];

CREATE TABLE [AuditLogs] (
    [Id] INTEGER NOT NULL,
    [EntityName] TEXT NOT NULL,
    [EntityId] TEXT NULL,
    [Action] TEXT NOT NULL,
    [PerformedBy] TEXT NULL,
    [PerformedAt] TEXT NOT NULL,
    [Details] TEXT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_AuditLogs_PerformedAt] ON [AuditLogs] ([PerformedAt]);

ALTER TABLE [DocumentRoutings] ADD CONSTRAINT [FK_DocumentRoutings_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]);

ALTER TABLE [DocumentRoutings] ADD CONSTRAINT [FK_DocumentRoutings_DocumentTypes_DocumentTypeId] FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([DocumentTypeId]);

ALTER TABLE [DocumentRoutings] ADD CONSTRAINT [FK_DocumentRoutings_Plants_PlantId] FOREIGN KEY ([PlantId]) REFERENCES [Plants] ([PlantId]);

ALTER TABLE [DocumentRoutings] ADD CONSTRAINT [FK_DocumentRoutings_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([SectionId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260305052442_AddAuditLogs', N'9.0.0');

COMMIT;
GO

