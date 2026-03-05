SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    -- Generated from myapp.db (SQLite)
    -- User-related seed/migration data for SQL Server

    DELETE FROM [AspNetUserTokens];
    DELETE FROM [AspNetUserLogins];
    DELETE FROM [AspNetUserClaims];
    DELETE FROM [AspNetRoleClaims];
    DELETE FROM [AspNetUserRoles];
    DELETE FROM [AspNetUsers];
    DELETE FROM [AspNetRoles];

    PRINT N'[INFO] Inserting 2 row(s) into AspNetRoles';
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'd5799200-145d-4666-8bf2-5e2d50f372d1', N'Admin', N'ADMIN', NULL);
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'a1f0d945-711f-417f-8c90-d9dc30d7214d', N'User', N'USER', NULL);

    PRINT N'[INFO] Inserting 7 row(s) into AspNetUsers';
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'ea690051-85f4-4bad-a74a-8b900e073efe', N'Admin', N'User', N'DX Technology', N'Enterprise System', N'Headquarters', 1, 1, N'ituser@example.com', N'ITUSER@EXAMPLE.COM', N'ituser@example.com', N'ITUSER@EXAMPLE.COM', 1, N'AQAAAAIAAYagAAAAELfNd+WX4GRlO262jYJW8evr/mPpBuwguVlqfCpNS+HBrs+cw0eiTi9b3isexTrEXA==', N'Y2S3SOTNEVPYFBHUUZNGSQYREV6QEHEM', N'7c6cc991-35a4-4bf1-b208-2183db658ba4', NULL, 0, 0, NULL, 1, 0, N'0001-01-01 00:00:00', NULL, N'0001-01-01 00:00:00', NULL);
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f805c787-1c8e-417a-8be1-8b03e39f2171', N'มัลลิกา', N'โพธิ์นอก', N'DX Technology', N'Enterprise System', N'6090', 1, 1, N'ASI006038', N'ASI006038', N'ASI006038@stanley-electic.com', N'ASI006038@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEIq0WbAfdYTOm3C6SnGS9/m9w7ZZ83lHhu6nUcYSKuXkxyGpeSC1Ph/p1ArPAkcDpw==', N'SMNL7CXPOOWUAS3IFIFDSILOG7GGDKRU', N'ba7b3a05-629b-4653-bab2-97cb0428ed76', NULL, 0, 0, NULL, 1, 0, N'2026-02-24 09:10:51.5308496', NULL, N'2026-02-24 09:17:58.9478647', N'ASI006038');
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6419de13-ecc1-46e5-a7c2-0dac58329843', N'Chadagon', N'Mangkalasatian', N'DX Technology', N'Enterprise System', N'6090', 1, 1, N'ASI002525', N'ASI002525', N'ASI002525@stanley-electic.com', N'ASI002525@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEBr3MFhrT//251Ap35gyeHdofq5oT1BWA6gZ04U0FuwcYFJojvx5MJWUlaNeYKcJdQ==', N'I6CJCQTJZPK7TAXCDEUWH6ITQYJDTYK3', N'78fb2c7f-38b0-4199-9ae1-71075031bae3', NULL, 0, 0, NULL, 1, 0, N'2026-02-24 09:17:47.6505605', N'ASI006038', N'2026-02-24 09:17:47.6507154', N'ASI006038');
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'b3bd13fe-5931-4976-bedc-2389123d7918', N'Purchase1', N'Normal', N'Purchase', N'Purchase LCD', N'6021', 1, 0, N'ASI000001', N'ASI000001', N'ASI000001@stanley-electic.com', N'ASI000001@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEBNywxewyAlKY0S0xC95U5rtD3pSPHomDOVB04HNW46jpfxU7sg4R6v6jLfGedPXsw==', N'T3RCUQHA5M2OGPSZ27YVC3HXMK4RMUJD', N'd64a983f-474f-4cb9-8a96-34502bad28a0', NULL, 0, 0, NULL, 1, 0, N'2026-02-25 01:30:53.1312071', N'ASI006038', N'2026-02-25 01:37:15.5259726', N'ASI006038');
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'4fb8864d-a44a-446d-9278-2340f0d07bf9', N'Planner1', N'Normal', N'Planner', N'Planner LCD', N'6021', 1, 0, N'ASI000002', N'ASI000002', N'ASI000002@stanley-electic.com', N'ASI000002@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEJNjMhpS0QDUCPpkNM//wkmeyV6AHUCVnu+gXgAdgp6fsnPfSvYVsE63pCVJT4d4dw==', N'YLR5ANKEO3IMNBR7U3WGZYV6OGM2BZIS', N'4136f5df-3f8b-4da5-b526-bf48102648b6', NULL, 0, 0, NULL, 1, 0, N'2026-02-25 01:39:43.2386324', N'ASI006038', N'2026-02-25 01:39:43.238712', N'ASI006038');
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'b9a26cf8-7968-4fd0-a7bb-4fff4b5c4807', N'Planner2', N'Normal', N'Planner', N'Planner LCD', N'6021', 1, 0, N'ASI000003', N'ASI000003', N'ASI000003@stanley-electic.com', N'ASI000003@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEBx8MhqxTDenkbWp2bqbgV7cmZoIH984+n4MiFnRqOhvhXOr0jySF+pWf2QhXS5SNA==', N'3G6324PZQHVWMYROMFJLBMTFWIZKJZD6', N'b8fee489-a138-4ae8-8324-a318a2e0de16', NULL, 0, 0, NULL, 1, 0, N'2026-02-25 01:50:26.1450548', N'ASI000001', N'2026-02-25 01:50:26.1451094', N'ASI000001');
    INSERT INTO [AspNetUsers] ([Id], [FirstName], [LastName], [Department], [Section], [Plant], [IsActive], [IsIT], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'a20870d4-6cc8-43f4-a4e3-e17b0510ca11', N'Planner3', N'Normal', N'Planner ', N'Planner LED', N'6021', 1, 0, N'ASI000004@stanley-electic.com', N'ASI000004@STANLEY-ELECTIC.COM', N'ASI000004@stanley-electic.com', N'ASI000004@STANLEY-ELECTIC.COM', 0, N'AQAAAAIAAYagAAAAEOrYxIT1pdSGvcxOMLBi4csOFPrrcRSSMmwkvsdUw0jvo62PLzkscrslYyY9i9zflQ==', N'YXAODVLZIMENAOAWLKW5Q6T2RDFCGBFS', N'13b0885c-fedd-4000-93df-fd4bfe90acc3', NULL, 0, 0, NULL, 1, 0, N'2026-02-25 02:01:18.9496133', N'ASI000001', N'2026-02-25 02:01:18.9496993', N'ASI000001');

    PRINT N'[INFO] Inserting 1 row(s) into AspNetUserRoles';
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ea690051-85f4-4bad-a74a-8b900e073efe', N'd5799200-145d-4666-8bf2-5e2d50f372d1');

    PRINT N'[INFO] No rows in AspNetUserClaims';

    PRINT N'[INFO] No rows in AspNetRoleClaims';

    PRINT N'[INFO] No rows in AspNetUserLogins';

    PRINT N'[INFO] No rows in AspNetUserTokens';

    COMMIT TRANSACTION;
    PRINT N'[SUCCESS] User data import completed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
