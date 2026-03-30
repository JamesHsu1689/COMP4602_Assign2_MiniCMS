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
CREATE TABLE [Articles] (
    [Id] INTEGER NOT NULL,
    [Title] TEXT NOT NULL,
    [Content] TEXT NOT NULL,
    [CreatedAt] TEXT NOT NULL,
    [UpdatedAt] TEXT NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoles] (
    [Id] TEXT NOT NULL,
    [Name] TEXT NULL,
    [NormalizedName] TEXT NULL,
    [ConcurrencyStamp] TEXT NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] TEXT NOT NULL,
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

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]);

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260227031209_InitialCreate', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Articles] ADD [UserId] TEXT NULL;

CREATE INDEX [IX_Articles_UserId] ON [Articles] ([UserId]);

ALTER TABLE [Articles] ADD CONSTRAINT [FK_Articles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329230641_AddArticleUserFK', N'10.0.3');

COMMIT;
GO

