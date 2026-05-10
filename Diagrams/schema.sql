CREATE TABLE "__EFMigrationsLock" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK___EFMigrationsLock" PRIMARY KEY,
    "Timestamp" TEXT NOT NULL
);
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);
CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);
CREATE TABLE "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);
CREATE TABLE sqlite_sequence(name,seq);
CREATE TABLE "Images" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Images" PRIMARY KEY AUTOINCREMENT,
    "FileName" TEXT NOT NULL,
    "AltText" TEXT NULL
, "UploadDate" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00');
CREATE TABLE "Locations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Locations" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Address" TEXT NOT NULL
, "IsActive" INTEGER NOT NULL DEFAULT 0, "Type" INTEGER NOT NULL DEFAULT 0);
CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);
CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "ParentProductId" INTEGER NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Price" decimal(10,2) NOT NULL,
    "Size" INTEGER NULL,
    "Color" INTEGER NULL,
    "Material" TEXT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "DateAdded" TEXT NOT NULL,
    "ImageId" INTEGER NULL,
    "CategoryId" INTEGER NOT NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Products_Images_ImageId" FOREIGN KEY ("ImageId") REFERENCES "Images" ("Id"),
    CONSTRAINT "FK_Products_Products_ParentProductId" FOREIGN KEY ("ParentProductId") REFERENCES "Products" ("Id")
);
CREATE TABLE "ProductLocations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductLocations" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "LocationId" INTEGER NOT NULL,
    "LastUpdated" TEXT NOT NULL,
    CONSTRAINT "FK_ProductLocations_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductLocations_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX "IX_ProductLocations_LocationId" ON "ProductLocations" ("LocationId");
CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
CREATE INDEX "IX_Products_ParentProductId" ON "Products" ("ParentProductId");
CREATE INDEX "IX_Products_ImageId" ON "Products" ("ImageId");
CREATE TABLE "Inventories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Inventories" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "Location" TEXT NULL,
    "LastUpdated" TEXT NOT NULL,
    CONSTRAINT "FK_Inventories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Inventories_ProductId" ON "Inventories" ("ProductId");
CREATE TABLE "StockTransferBoxes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StockTransferBoxes" PRIMARY KEY AUTOINCREMENT,
    "FromLocationId" INTEGER NOT NULL,
    "ToLocationId" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "Notes" TEXT NULL, "IsDelivered" INTEGER NOT NULL DEFAULT 0, "IsPickedUp" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_StockTransferBoxes_Locations_FromLocationId" FOREIGN KEY ("FromLocationId") REFERENCES "Locations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_StockTransferBoxes_Locations_ToLocationId" FOREIGN KEY ("ToLocationId") REFERENCES "Locations" ("Id") ON DELETE CASCADE
);
CREATE TABLE "StockTransferBoxItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StockTransferBoxItems" PRIMARY KEY AUTOINCREMENT,
    "StockTransferBoxId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    CONSTRAINT "FK_StockTransferBoxItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_StockTransferBoxItems_StockTransferBoxes_StockTransferBoxId" FOREIGN KEY ("StockTransferBoxId") REFERENCES "StockTransferBoxes" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_ProductLocations_ProductId_LocationId" ON "ProductLocations" ("ProductId", "LocationId");
CREATE INDEX "IX_StockTransferBoxes_FromLocationId" ON "StockTransferBoxes" ("FromLocationId");
CREATE INDEX "IX_StockTransferBoxes_ToLocationId" ON "StockTransferBoxes" ("ToLocationId");
CREATE INDEX "IX_StockTransferBoxItems_ProductId" ON "StockTransferBoxItems" ("ProductId");
CREATE INDEX "IX_StockTransferBoxItems_StockTransferBoxId" ON "StockTransferBoxItems" ("StockTransferBoxId");
CREATE TABLE "Sales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Sales" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "LocationId" INTEGER NOT NULL,
    "QuantitySold" INTEGER NOT NULL,
    "UnitPriceAtSale" TEXT NOT NULL,
    "SaleDate" TEXT NOT NULL,
    "IsManualAdjustment" INTEGER NOT NULL,
    CONSTRAINT "FK_Sales_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sales_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Sales_LocationId" ON "Sales" ("LocationId");
CREATE INDEX "IX_Sales_ProductId" ON "Sales" ("ProductId");
