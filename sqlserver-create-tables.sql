CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Department] nvarchar(max) NULL,
    [Section] nvarchar(max) NULL,
    [Plant] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsIT] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(100) NULL,
    [Action] nvarchar(50) NOT NULL,
    [PerformedBy] nvarchar(256) NULL,
    [PerformedAt] datetime2 NOT NULL,
    [Details] nvarchar(max) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Departments] (
    [DepartmentId] int NOT NULL IDENTITY,
    [DepartmentName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentId])
);
GO


CREATE TABLE [DocumentTypes] (
    [DocumentTypeId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([DocumentTypeId])
);
GO


CREATE TABLE [MasterDataCombinations] (
    [Id] int NOT NULL IDENTITY,
    [DepartmentName] nvarchar(max) NOT NULL,
    [SectionName] nvarchar(max) NOT NULL,
    [PlantName] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_MasterDataCombinations] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [NewsArticles] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(500) NULL,
    [PublishedDate] datetime2 NOT NULL,
    [Author] nvarchar(100) NOT NULL,
    [IsFeatured] bit NOT NULL,
    CONSTRAINT [PK_NewsArticles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [RequestItems] (
    [Id] int NOT NULL IDENTITY,
    [RequestType] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Requester] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    [NextApproverId] nvarchar(max) NULL,
    [Plant] nvarchar(max) NULL,
    [ItemCode] nvarchar(max) NULL,
    [EnglishMatDescription] nvarchar(max) NULL,
    [ModelName] nvarchar(max) NULL,
    [BaseUnit] nvarchar(max) NULL,
    [MaterialGroup] nvarchar(max) NULL,
    [ExternalMaterialGroup] nvarchar(max) NULL,
    [DivisionCode] nvarchar(max) NULL,
    [ProfitCenter] nvarchar(max) NULL,
    [DistributionChannel] nvarchar(max) NULL,
    [BoiCode] nvarchar(max) NULL,
    [MrpController] nvarchar(max) NULL,
    [StorageLocation] nvarchar(max) NULL,
    [ProductionSupervisor] nvarchar(max) NULL,
    [CostingLotSize] int NULL,
    [ValClass] nvarchar(max) NULL,
    [StandardPack] nvarchar(max) NULL,
    [BoiDescription] nvarchar(max) NULL,
    [MakerMfrPartNumber] nvarchar(max) NULL,
    [CommCodeTariffCode] nvarchar(max) NULL,
    [TraffCodePercentage] decimal(18,2) NULL,
    [StorageLocationB1] nvarchar(max) NULL,
    [PriceControl] nvarchar(max) NULL,
    [Currency] nvarchar(max) NULL,
    [SupplierCode] nvarchar(max) NULL,
    [MatType] nvarchar(max) NULL,
    [Check] bit NOT NULL,
    [DevicePlant] nvarchar(max) NULL,
    [AssemblyPlant] nvarchar(max) NULL,
    [IpoPlant] nvarchar(max) NULL,
    [AsiOfPlant] nvarchar(max) NULL,
    [PriceUnit] int NULL,
    [StorageLocationEP] nvarchar(max) NULL,
    [ToolingBSection] nvarchar(max) NULL,
    [PoNumber] nvarchar(max) NULL,
    [StatusInA] nvarchar(max) NULL,
    [DateIn] datetime2 NULL,
    [QuotationNumber] nvarchar(max) NULL,
    [ToolingBModel] nvarchar(max) NULL,
    [TariffCode] nvarchar(max) NULL,
    [Planner] nvarchar(max) NULL,
    [CurrentICS] nvarchar(max) NULL,
    [Level] nvarchar(max) NULL,
    [Rohs] nvarchar(max) NULL,
    [CodenMid] nvarchar(max) NULL,
    [SalesOrg] nvarchar(max) NULL,
    [TaxTh] nvarchar(max) NULL,
    [MaterialStatisticsGroup] nvarchar(max) NULL,
    [AccountAssignment] nvarchar(max) NULL,
    [GeneralItemCategory] nvarchar(max) NULL,
    [Availability] nvarchar(max) NULL,
    [Transportation] nvarchar(max) NULL,
    [LoadingGroup] nvarchar(max) NULL,
    [PlanDelTime] nvarchar(max) NULL,
    [SchedMargin] nvarchar(max) NULL,
    [MinLot] nvarchar(max) NULL,
    [MaxLot] nvarchar(max) NULL,
    [FixedLot] nvarchar(max) NULL,
    [Rounding] nvarchar(max) NULL,
    [Mtlsm] nvarchar(max) NULL,
    [Effective] datetime2 NULL,
    [StorageLoc] nvarchar(max) NULL,
    [ReceiveStorage] nvarchar(max) NULL,
    [PurchasingGroup] nvarchar(max) NULL,
    [EditBomFg] nvarchar(max) NULL,
    [EditBomAllFg] bit NOT NULL,
    [Price] decimal(18,2) NULL,
    CONSTRAINT [PK_RequestItems] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Plants] (
    [PlantId] int NOT NULL IDENTITY,
    [PlantName] nvarchar(max) NOT NULL,
    [DepartmentId] int NOT NULL,
    CONSTRAINT [PK_Plants] PRIMARY KEY ([PlantId]),
    CONSTRAINT [FK_Plants_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Sections] (
    [SectionId] int NOT NULL IDENTITY,
    [SectionName] nvarchar(max) NOT NULL,
    [DepartmentId] int NOT NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([SectionId]),
    CONSTRAINT [FK_Sections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE CASCADE
);
GO


CREATE TABLE [BomComponents] (
    [Id] int NOT NULL IDENTITY,
    [RequestItemId] int NOT NULL,
    [Level] int NOT NULL,
    [Item] nvarchar(max) NULL,
    [ItemCat] nvarchar(max) NULL,
    [ComponentNumber] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [ItemQuantity] decimal(18,5) NULL,
    [Unit] nvarchar(max) NULL,
    [BomUsage] nvarchar(max) NULL,
    [Plant] nvarchar(max) NULL,
    [Sloc] nvarchar(max) NULL,
    CONSTRAINT [PK_BomComponents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BomComponents_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [BomEditComponent] (
    [Id] int NOT NULL IDENTITY,
    [ItemCodeFrom] nvarchar(max) NULL,
    [DescriptionFrom] nvarchar(max) NULL,
    [ItemQuantityFrom] decimal(18,2) NULL,
    [UnitFrom] nvarchar(max) NULL,
    [BomUsageFrom] nvarchar(max) NULL,
    [SlocFrom] nvarchar(max) NULL,
    [ItemCodeTo] nvarchar(max) NULL,
    [DescriptionTo] nvarchar(max) NULL,
    [ItemQuantityTo] decimal(18,2) NULL,
    [UnitTo] nvarchar(max) NULL,
    [BomUsageTo] nvarchar(max) NULL,
    [SlocTo] nvarchar(max) NULL,
    [PlantTo] nvarchar(max) NULL,
    [RequestItemId] int NULL,
    CONSTRAINT [PK_BomEditComponent] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BomEditComponent_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id])
);
GO


CREATE TABLE [LicensePermissionItems] (
    [Id] int NOT NULL IDENTITY,
    [RequestItemId] int NOT NULL,
    [SapUsername] nvarchar(max) NULL,
    [TCode] nvarchar(max) NULL,
    CONSTRAINT [PK_LicensePermissionItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LicensePermissionItems_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Routings] (
    [Id] int NOT NULL IDENTITY,
    [RequestItemId] int NOT NULL,
    [Material] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [WorkCenter] nvarchar(max) NULL,
    [Operation] nvarchar(max) NULL,
    [BaseQty] decimal(18,5) NULL,
    [Unit] nvarchar(max) NULL,
    [DirectLaborCosts] decimal(18,5) NULL,
    [DirectExpenses] decimal(18,5) NULL,
    [AllocationExpense] decimal(18,5) NULL,
    [ProductionVersionCode] nvarchar(max) NULL,
    [Version] nvarchar(max) NULL,
    [ValidFrom] datetime2 NULL,
    [ValidTo] datetime2 NULL,
    [MaximumLotSize] decimal(18,5) NULL,
    [Group] nvarchar(max) NULL,
    [GroupCounter] nvarchar(max) NULL,
    CONSTRAINT [PK_Routings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Routings_RequestItems_RequestItemId] FOREIGN KEY ([RequestItemId]) REFERENCES [RequestItems] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DocumentRoutings] (
    [Id] int NOT NULL IDENTITY,
    [DocumentTypeId] int NOT NULL,
    [DepartmentId] int NOT NULL,
    [SectionId] int NOT NULL,
    [PlantId] int NOT NULL,
    [Step] int NOT NULL,
    CONSTRAINT [PK_DocumentRoutings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DocumentRoutings_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]),
    CONSTRAINT [FK_DocumentRoutings_DocumentTypes_DocumentTypeId] FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([DocumentTypeId]),
    CONSTRAINT [FK_DocumentRoutings_Plants_PlantId] FOREIGN KEY ([PlantId]) REFERENCES [Plants] ([PlantId]),
    CONSTRAINT [FK_DocumentRoutings_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([SectionId])
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_AuditLogs_PerformedAt] ON [AuditLogs] ([PerformedAt]);
GO


CREATE INDEX [IX_BomComponents_RequestItemId] ON [BomComponents] ([RequestItemId]);
GO


CREATE INDEX [IX_BomEditComponent_RequestItemId] ON [BomEditComponent] ([RequestItemId]);
GO


CREATE INDEX [IX_DocumentRoutings_DepartmentId] ON [DocumentRoutings] ([DepartmentId]);
GO


CREATE INDEX [IX_DocumentRoutings_DocumentTypeId] ON [DocumentRoutings] ([DocumentTypeId]);
GO


CREATE INDEX [IX_DocumentRoutings_PlantId] ON [DocumentRoutings] ([PlantId]);
GO


CREATE INDEX [IX_DocumentRoutings_SectionId] ON [DocumentRoutings] ([SectionId]);
GO


CREATE INDEX [IX_LicensePermissionItems_RequestItemId] ON [LicensePermissionItems] ([RequestItemId]);
GO


CREATE INDEX [IX_Plants_DepartmentId] ON [Plants] ([DepartmentId]);
GO


CREATE INDEX [IX_Routings_RequestItemId] ON [Routings] ([RequestItemId]);
GO


CREATE INDEX [IX_Sections_DepartmentId] ON [Sections] ([DepartmentId]);
GO


