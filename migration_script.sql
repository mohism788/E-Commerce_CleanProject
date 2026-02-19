CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
CREATE TABLE `AspNetRoles` (
    `Id` uniqueidentifier NOT NULL,
    `Name` nvarchar(256) NULL,
    `NormalizedName` nvarchar(256) NULL,
    `ConcurrencyStamp` nvarchar(max) NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` uniqueidentifier NOT NULL,
    `CreatedAt` datetime2 NOT NULL,
    `UserName` nvarchar(256) NULL,
    `NormalizedUserName` nvarchar(256) NULL,
    `Email` nvarchar(256) NULL,
    `NormalizedEmail` nvarchar(256) NULL,
    `EmailConfirmed` bit NOT NULL,
    `PasswordHash` nvarchar(max) NULL,
    `SecurityStamp` nvarchar(max) NULL,
    `ConcurrencyStamp` nvarchar(max) NULL,
    `PhoneNumber` nvarchar(max) NULL,
    `PhoneNumberConfirmed` bit NOT NULL,
    `TwoFactorEnabled` bit NOT NULL,
    `LockoutEnd` datetimeoffset NULL,
    `LockoutEnabled` bit NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
);

CREATE TABLE `Categories` (
    `Id` int NOT NULL,
    `Name` nvarchar(max) NOT NULL,
    CONSTRAINT `PK_Categories` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL,
    `RoleId` uniqueidentifier NOT NULL,
    `ClaimType` nvarchar(max) NULL,
    `ClaimValue` nvarchar(max) NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL,
    `UserId` uniqueidentifier NOT NULL,
    `ClaimType` nvarchar(max) NULL,
    `ClaimValue` nvarchar(max) NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` nvarchar(450) NOT NULL,
    `ProviderKey` nvarchar(450) NOT NULL,
    `ProviderDisplayName` nvarchar(max) NULL,
    `UserId` uniqueidentifier NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` uniqueidentifier NOT NULL,
    `RoleId` uniqueidentifier NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` uniqueidentifier NOT NULL,
    `LoginProvider` nvarchar(450) NOT NULL,
    `Name` nvarchar(450) NOT NULL,
    `Value` nvarchar(max) NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Orders` (
    `Id` int NOT NULL,
    `UserId` uniqueidentifier NOT NULL,
    `OrderDate` datetime2 NOT NULL,
    `TotalAmount` decimal(18,2) NOT NULL,
    `Status` nvarchar(max) NOT NULL,
    CONSTRAINT `PK_Orders` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Orders_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Products` (
    `Id` int NOT NULL,
    `Name` nvarchar(max) NOT NULL,
    `Description` nvarchar(max) NOT NULL,
    `Price` decimal(18,2) NOT NULL,
    `Stock` int NOT NULL,
    `SellerId` uniqueidentifier NOT NULL,
    `CategoryId` int NOT NULL,
    `CreatedAt` datetime2 NOT NULL,
    `UserId` uniqueidentifier NULL,
    CONSTRAINT `PK_Products` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Products_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`),
    CONSTRAINT `FK_Products_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `CartItems` (
    `Id` int NOT NULL,
    `ProductId` int NOT NULL,
    `UserId` uniqueidentifier NOT NULL,
    `Quantity` int NOT NULL,
    CONSTRAINT `PK_CartItems` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_CartItems_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_CartItems_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `OrderItems` (
    `Id` int NOT NULL,
    `OrderId` int NOT NULL,
    `ProductId` int NOT NULL,
    `Quantity` int NOT NULL,
    `UnitPrice` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_OrderItems` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OrderItems_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OrderItems_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Reviews` (
    `Id` int NOT NULL,
    `ProductId` int NOT NULL,
    `UserId` uniqueidentifier NOT NULL,
    `Rating` int NOT NULL,
    `Comment` nvarchar(max) NOT NULL,
    `CreatedAt` datetime2 NOT NULL,
    CONSTRAINT `PK_Reviews` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reviews_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Reviews_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
);

INSERT INTO `AspNetRoles` (`Id`, `ConcurrencyStamp`, `Name`, `NormalizedName`)
VALUES ('4a098952-4720-4f23-ad15-56b4912204b6', NULL, 'Admin', 'ADMIN'),
('96264a7e-d129-459e-9a52-7d7e9d428801', NULL, 'Seller', 'SELLER'),
('a3d73c02-aeef-45a2-b2d9-4e6d298642e6', NULL, 'Customer', 'CUSTOMER');

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_CartItems_ProductId` ON `CartItems` (`ProductId`);

CREATE INDEX `IX_CartItems_UserId` ON `CartItems` (`UserId`);

CREATE INDEX `IX_OrderItems_OrderId` ON `OrderItems` (`OrderId`);

CREATE INDEX `IX_OrderItems_ProductId` ON `OrderItems` (`ProductId`);

CREATE INDEX `IX_Orders_UserId` ON `Orders` (`UserId`);

CREATE INDEX `IX_Products_CategoryId` ON `Products` (`CategoryId`);

CREATE INDEX `IX_Products_UserId` ON `Products` (`UserId`);

CREATE INDEX `IX_Reviews_ProductId` ON `Reviews` (`ProductId`);

CREATE INDEX `IX_Reviews_UserId` ON `Reviews` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260212054121_InitialMySql', '9.0.0');

ALTER TABLE `AspNetUsers` DROP INDEX `UserNameIndex`;

ALTER TABLE `AspNetRoles` DROP INDEX `RoleNameIndex`;

ALTER TABLE `Reviews` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `Reviews` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

ALTER TABLE `Reviews` MODIFY COLUMN `Comment` longtext CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Reviews` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `Products` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NULL;

ALTER TABLE `Products` MODIFY COLUMN `SellerId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `Products` MODIFY COLUMN `Name` longtext CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Products` MODIFY COLUMN `Description` longtext CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Products` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

ALTER TABLE `Products` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `Orders` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `Orders` MODIFY COLUMN `Status` longtext CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Orders` MODIFY COLUMN `OrderDate` datetime(6) NOT NULL;

ALTER TABLE `Orders` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `OrderItems` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `Categories` MODIFY COLUMN `Name` longtext CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Categories` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `CartItems` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `CartItems` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `Value` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserTokens` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `UserName` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `TwoFactorEnabled` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `PhoneNumberConfirmed` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `PasswordHash` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `LockoutEnd` datetime(6) NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `LockoutEnabled` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `EmailConfirmed` tinyint(1) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `Email` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUsers` MODIFY COLUMN `Id` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUserRoles` MODIFY COLUMN `RoleId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUserRoles` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserLogins` MODIFY COLUMN `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `AspNetUserClaims` MODIFY COLUMN `UserId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetUserClaims` MODIFY COLUMN `ClaimValue` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUserClaims` MODIFY COLUMN `ClaimType` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetUserClaims` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

ALTER TABLE `AspNetRoles` MODIFY COLUMN `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetRoles` MODIFY COLUMN `Name` varchar(256) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetRoles` MODIFY COLUMN `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetRoles` MODIFY COLUMN `Id` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetRoleClaims` MODIFY COLUMN `RoleId` char(36) COLLATE ascii_general_ci NOT NULL;

ALTER TABLE `AspNetRoleClaims` MODIFY COLUMN `ClaimValue` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetRoleClaims` MODIFY COLUMN `ClaimType` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `AspNetRoleClaims` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260212062200_UpdateMigration', '9.0.0');

COMMIT;

