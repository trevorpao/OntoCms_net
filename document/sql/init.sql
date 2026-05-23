-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- 主機： mariadb:3306
-- 產生時間： 2025 年 09 月 21 日 12:38
-- 伺服器版本： 10.4.6-MariaDB-1:10.4.6+maria~bionic
-- PHP 版本： 8.3.25

--
-- 資料庫： [target_db]
--

-- --------------------------------------------------------

--
-- 資料表結構 [sessions]
--

DROP TABLE IF EXISTS [dbo].[sessions];
IF OBJECT_ID(N'[dbo].[sessions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[sessions] (
    [session_id] NVARCHAR(255) NOT NULL,
    [data] NVARCHAR(MAX) DEFAULT NULL,
    [ip] NVARCHAR(45) DEFAULT NULL,
    [agent] NVARCHAR(300) DEFAULT NULL,
    [stamp] INT DEFAULT NULL,
    PRIMARY KEY ([session_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_adv]
--

DROP TABLE IF EXISTS [dbo].[tbl_adv];
IF OBJECT_ID(N'[dbo].[tbl_adv]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_adv] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [position_id] INT NOT NULL,
    [counter] INT NOT NULL,
    [exposure] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Disabled',
    [weight] INT NOT NULL DEFAULT 0,
    [theme] NVARCHAR(10) DEFAULT NULL,
    [start_date] DATETIME2 NULL DEFAULT NULL,
    [end_date] DATETIME2 NULL DEFAULT NULL,
    [uri] NVARCHAR(255) NOT NULL,
    [cover] NVARCHAR(255) NOT NULL,
    [background] NVARCHAR(255) NOT NULL,
    [last_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT NOT NULL,
    [insert_user] INT NOT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_adv_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'category_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_adv]'))
    CREATE INDEX [category_id] ON [dbo].[tbl_adv] ([position_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'uri' AND object_id = OBJECT_ID(N'[dbo].[tbl_adv]'))
    CREATE INDEX [uri] ON [dbo].[tbl_adv] ([uri]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_adv_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_adv_lang];
IF OBJECT_ID(N'[dbo].[tbl_adv_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_adv_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [subtitle] NVARCHAR(255) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_adv_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_adv_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_adv_meta];
IF OBJECT_ID(N'[dbo].[tbl_adv_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_adv_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [k] NVARCHAR(50) DEFAULT NULL,
    [v] NVARCHAR(MAX) DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'fk_meta_press_idx' AND object_id = OBJECT_ID(N'[dbo].[tbl_adv_meta]'))
    CREATE INDEX [fk_meta_press_idx] ON [dbo].[tbl_adv_meta] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_author]
--

DROP TABLE IF EXISTS [dbo].[tbl_author];
IF OBJECT_ID(N'[dbo].[tbl_author]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_author] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [slug] NVARCHAR(255) NOT NULL,
    [online_date] date DEFAULT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    [cover] NVARCHAR(255) NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_author_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

--
-- 傾印資料表的資料 [tbl_author]
--

SET IDENTITY_INSERT [dbo].[tbl_author] ON;
INSERT INTO [dbo].[tbl_author] ([id], [status], [slug], [online_date], [sorter], [cover], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(2, N'Enabled', N'editor', N'2019-04-05', 0, N'', N'2025-05-26 06:40:47', 1, N'2019-04-04 21:02:04', 1);
SET IDENTITY_INSERT [dbo].[tbl_author] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_author_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_author_lang];
IF OBJECT_ID(N'[dbo].[tbl_author_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_author_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [jobtitle] NVARCHAR(100) DEFAULT NULL,
    [slogan] NVARCHAR(255) DEFAULT NULL,
    [summary] NVARCHAR(255) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_author_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

--
-- 傾印資料表的資料 [tbl_author_lang]
--

SET IDENTITY_INSERT [dbo].[tbl_author_lang] ON;
INSERT INTO [dbo].[tbl_author_lang] ([id], [lang], [parent_id], [title], [jobtitle], [slogan], [summary], [content], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'tw', 2, N'farm tyc', N'編輯', N'', NULL, N'', N'2025-05-26 06:40:47', 1, N'2019-04-04 21:02:04', 1);
SET IDENTITY_INSERT [dbo].[tbl_author_lang] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_author_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_author_tag];
IF OBJECT_ID(N'[dbo].[tbl_author_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_author_tag] (
    [author_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    PRIMARY KEY ([author_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_book]
--

DROP TABLE IF EXISTS [dbo].[tbl_book];
IF OBJECT_ID(N'[dbo].[tbl_book]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_book] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [cate_id] INT DEFAULT 0,
    [counter] INT DEFAULT 0,
    [exposure] INT DEFAULT 0,
    [uri] NVARCHAR(255) NOT NULL,
    [cover] NVARCHAR(100) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_book_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_book_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_book_lang];
IF OBJECT_ID(N'[dbo].[tbl_book_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_book_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [subtitle] NVARCHAR(255) DEFAULT NULL,
    [alias] NVARCHAR(255) DEFAULT NULL,
    [summary] NVARCHAR(MAX) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_book_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_category]
--

DROP TABLE IF EXISTS [dbo].[tbl_category];
IF OBJECT_ID(N'[dbo].[tbl_category]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_category] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [sorter] TINYINT NOT NULL DEFAULT 0,
    [group] NVARCHAR(50) NOT NULL,
    [slug] NVARCHAR(50) NOT NULL,
    [cover] NVARCHAR(100) NOT NULL DEFAULT N'',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_category_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'group' AND object_id = OBJECT_ID(N'[dbo].[tbl_category]'))
    CREATE INDEX [group] ON [dbo].[tbl_category] ([group]);

--
-- 傾印資料表的資料 [tbl_category]
--

SET IDENTITY_INSERT [dbo].[tbl_category] ON;
INSERT INTO [dbo].[tbl_category] ([id], [status], [sorter], [group], [slug], [cover], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'Enabled', 0, N'press', N'undefined', N'', N'2025-02-28 10:36:19', 1, N'2025-02-19 07:49:26', 1);
SET IDENTITY_INSERT [dbo].[tbl_category] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_category_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_category_lang];
IF OBJECT_ID(N'[dbo].[tbl_category_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_category_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [info] NVARCHAR(700) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_category_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

--
-- 傾印資料表的資料 [tbl_category_lang]
--

SET IDENTITY_INSERT [dbo].[tbl_category_lang] ON;
INSERT INTO [dbo].[tbl_category_lang] ([id], [lang], [parent_id], [title], [info], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'tw', 1, N'雜談', N'', N'2025-02-28 10:36:19', 1, N'2025-02-19 07:49:26', 1),
(2, N'en', 1, N'Undefined', N'', N'2025-02-28 10:36:19', 1, N'2025-02-19 07:49:26', 1),
(9, N'jp', 1, N'雑談', N'', N'2025-02-28 10:36:19', 1, N'2025-02-28 10:36:19', 1),
(10, N'ko', 1, N'잡담', N'', N'2025-02-28 10:36:19', 1, N'2025-02-28 10:36:19', 1);
SET IDENTITY_INSERT [dbo].[tbl_category_lang] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_category_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_category_tag];
IF OBJECT_ID(N'[dbo].[tbl_category_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_category_tag] (
    [tag_id] INT NOT NULL,
    [category_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 99,
    PRIMARY KEY ([category_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_collection]
--

DROP TABLE IF EXISTS [dbo].[tbl_collection];
IF OBJECT_ID(N'[dbo].[tbl_collection]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_collection] (
    [id] INT NOT NULL,
    [parent_id] INT DEFAULT NULL,
    [cover] NVARCHAR(255) DEFAULT NULL,
    [txt_color] NVARCHAR(10) NOT NULL DEFAULT N'dark',
    [txt_algin] NVARCHAR(10) NOT NULL DEFAULT N'left',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_collection_parent_id] UNIQUE ([parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_collection_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_collection_lang];
IF OBJECT_ID(N'[dbo].[tbl_collection_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_collection_lang] (
    [id] INT NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'lang_pid' AND object_id = OBJECT_ID(N'[dbo].[tbl_collection_lang]'))
    CREATE INDEX [lang_pid] ON [dbo].[tbl_collection_lang] ([lang],[parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_contact]
--

DROP TABLE IF EXISTS [dbo].[tbl_contact];
IF OBJECT_ID(N'[dbo].[tbl_contact]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_contact] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(7) NOT NULL DEFAULT N'New',
    [type] NVARCHAR(50) DEFAULT NULL,
    [name] NVARCHAR(255) NOT NULL,
    [phone] NVARCHAR(50) NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [message] NVARCHAR(MAX) NOT NULL,
    [other] NVARCHAR(MAX) DEFAULT NULL,
    [response] NVARCHAR(MAX) NOT NULL,
    [last_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT NOT NULL,
    [insert_user] INT NOT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_contact_status] CHECK ([status] IN (N'New', N'Process', N'Done'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_dictionary]
--

DROP TABLE IF EXISTS [dbo].[tbl_dictionary];
IF OBJECT_ID(N'[dbo].[tbl_dictionary]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_dictionary] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [slug] NVARCHAR(255) NOT NULL,
    [cover] NVARCHAR(100) NOT NULL DEFAULT N'',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_dictionary_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_dictionary_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_dictionary_lang];
IF OBJECT_ID(N'[dbo].[tbl_dictionary_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_dictionary_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [alias] NVARCHAR(255) DEFAULT NULL,
    [summary] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_dictionary_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_doorman]
--

DROP TABLE IF EXISTS [dbo].[tbl_doorman];
IF OBJECT_ID(N'[dbo].[tbl_doorman]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_doorman] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [user_id] INT DEFAULT NULL,
    [type] NVARCHAR(6) DEFAULT N'Member',
    [status] NVARCHAR(7) NOT NULL DEFAULT N'New',
    [pwd] NVARCHAR(100) DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_doorman_type] CHECK ([type] IN (N'Member', N'Staff', N'Admin')),
    CONSTRAINT [CK_tbl_doorman_status] CHECK ([status] IN (N'New', N'Invalid'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'user_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_doorman]'))
    CREATE INDEX [user_id] ON [dbo].[tbl_doorman] ([user_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_draft]
--

DROP TABLE IF EXISTS [dbo].[tbl_draft];
IF OBJECT_ID(N'[dbo].[tbl_draft]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_draft] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [press_id] INT NOT NULL DEFAULT 0,
    [owner_id] INT NOT NULL DEFAULT 0,
    [request_id] NVARCHAR(36) NOT NULL DEFAULT N'',
    [status] NVARCHAR(7) DEFAULT N'New',
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [method] NVARCHAR(50) NOT NULL DEFAULT N'',
    [intent] NVARCHAR(MAX) DEFAULT N'',
    [guideline] NVARCHAR(MAX) DEFAULT N'',
    [content] NVARCHAR(MAX) DEFAULT N'',
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    [last_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_draft_status] CHECK ([status] IN (N'New', N'Waiting', N'Done', N'Invalid', N'Used'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_flash]
--

DROP TABLE IF EXISTS [dbo].[tbl_flash];
IF OBJECT_ID(N'[dbo].[tbl_flash]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_flash] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [slug] NVARCHAR(32) DEFAULT NULL,
    [press_id] INT NOT NULL DEFAULT 0,
    [hit] INT NOT NULL DEFAULT 0,
    [exposure] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'New',
    [auto] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [genus] INT NOT NULL,
    [weight] INT NOT NULL DEFAULT 0,
    [reliable] INT NOT NULL DEFAULT 0,
    [international] INT NOT NULL DEFAULT 0,
    [source] NVARCHAR(25) NOT NULL,
    [uri] NVARCHAR(255) NOT NULL,
    [cover] NVARCHAR(255) NOT NULL,
    [online_date] DATETIME2 NULL DEFAULT NULL,
    [filename] NVARCHAR(20) NOT NULL DEFAULT N'',
    [last_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT NOT NULL,
    [insert_user] INT NOT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_flash_uri] UNIQUE ([uri]),
    CONSTRAINT [CK_tbl_flash_status] CHECK ([status] IN (N'New', N'Done', N'Enabled', N'Disabled')),
    CONSTRAINT [CK_tbl_flash_auto] CHECK ([auto] IN (N'Yes', N'No'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'genus' AND object_id = OBJECT_ID(N'[dbo].[tbl_flash]'))
    CREATE INDEX [genus] ON [dbo].[tbl_flash] ([genus]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'status' AND object_id = OBJECT_ID(N'[dbo].[tbl_flash]'))
    CREATE INDEX [status] ON [dbo].[tbl_flash] ([status]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_flash_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_flash_lang];
IF OBJECT_ID(N'[dbo].[tbl_flash_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_flash_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [summary] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_flash_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_flash_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_flash_meta];
IF OBJECT_ID(N'[dbo].[tbl_flash_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_flash_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [k] NVARCHAR(50) DEFAULT NULL,
    [v] NVARCHAR(MAX) DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'fk_meta_flash_idx' AND object_id = OBJECT_ID(N'[dbo].[tbl_flash_meta]'))
    CREATE INDEX [fk_meta_flash_idx] ON [dbo].[tbl_flash_meta] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_flash_raw]
--

DROP TABLE IF EXISTS [dbo].[tbl_flash_raw];
IF OBJECT_ID(N'[dbo].[tbl_flash_raw]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_flash_raw] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [cover] NVARCHAR(255) DEFAULT NULL,
    [summary] NVARCHAR(MAX) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_flash_raw_pid] UNIQUE ([parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_genus]
--

DROP TABLE IF EXISTS [dbo].[tbl_genus];
IF OBJECT_ID(N'[dbo].[tbl_genus]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_genus] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [sorter] TINYINT NOT NULL DEFAULT 0,
    [group] NVARCHAR(50) NOT NULL,
    [name] NVARCHAR(255) DEFAULT NULL,
    [color] NVARCHAR(10) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_genus_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'group' AND object_id = OBJECT_ID(N'[dbo].[tbl_genus]'))
    CREATE INDEX [group] ON [dbo].[tbl_genus] ([group]);

--
-- 傾印資料表的資料 [tbl_genus]
--

SET IDENTITY_INSERT [dbo].[tbl_genus] ON;
INSERT INTO [dbo].[tbl_genus] ([id], [status], [sorter], [group], [name], [color], [content], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'Enabled', 98, N'course', N'線上活動', NULL, N'', N'2021-11-11 17:25:01', 1, N'2020-10-30 02:36:34', 1),
(2, N'Enabled', 99, N'course', N'實體活動', NULL, N'', N'2021-11-11 17:25:11', 1, N'2020-10-30 02:36:50', 1),
(3, N'Enabled', 2, N'press', N'影音文章', N'', N'不放首圖、文中有影音', N'2024-06-06 03:07:56', 1, N'2020-10-30 03:00:26', 1),
(4, N'Enabled', 1, N'press', N'一般文章', N'', N'大版位圖片 + 簡介在前', N'2024-06-06 03:07:08', 1, N'2021-11-11 14:42:01', 1),
(5, N'Enabled', 4, N'adv', N'首頁友站連結', N'', N'小圖並列，最多六則', N'2025-06-13 01:13:44', 1, N'2023-09-22 05:31:41', 1),
(6, N'Enabled', 1, N'adv', N'首頁首屏', N'', N'大圖輪播，最多三則', N'2025-06-13 01:12:56', 1, N'2025-03-12 16:15:41', 1),
(7, N'Enabled', 2, N'adv', N'首頁特色活動', N'', N'', N'2025-06-13 01:10:52', 1, N'2025-03-12 16:16:30', 1),
(8, N'Enabled', 0, N'tag', N'一般標籤', N'', N'', N'2025-03-21 00:06:44', 1, N'2025-03-21 00:06:44', 1),
(9, N'Enabled', 1, N'tag', N'大標籤', N'', N'', N'2025-03-21 00:06:53', 1, N'2025-03-21 00:06:53', 1),
(10, N'Enabled', 3, N'adv', N'首頁Youtube', N'', N'每次單則', N'2025-06-13 01:12:36', 1, N'2025-06-13 01:12:36', 1);
SET IDENTITY_INSERT [dbo].[tbl_genus] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_media]
--

DROP TABLE IF EXISTS [dbo].[tbl_media];
IF OBJECT_ID(N'[dbo].[tbl_media]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_media] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [target] NVARCHAR(6) NOT NULL DEFAULT N'Normal',
    [parent_id] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [sorter] INT NOT NULL DEFAULT 0,
    [slug] NVARCHAR(255) NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [pic] NVARCHAR(255) NOT NULL,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_media_target] CHECK ([target] IN (N'Normal', N'Press')),
    CONSTRAINT [CK_tbl_media_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_media_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_media_lang];
IF OBJECT_ID(N'[dbo].[tbl_media_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_media_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [from_ai] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_media_lang_lang_pid] UNIQUE ([lang],[parent_id]),
    CONSTRAINT [CK_tbl_media_lang_from_ai] CHECK ([from_ai] IN (N'No', N'Yes'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_media_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_media_meta];
IF OBJECT_ID(N'[dbo].[tbl_media_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_media_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [k] NVARCHAR(50) DEFAULT NULL,
    [v] NVARCHAR(MAX) DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'fk_meta_media_idx' AND object_id = OBJECT_ID(N'[dbo].[tbl_media_meta]'))
    CREATE INDEX [fk_meta_media_idx] ON [dbo].[tbl_media_meta] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_media_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_media_tag];
IF OBJECT_ID(N'[dbo].[tbl_media_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_media_tag] (
    [media_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    PRIMARY KEY ([media_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_menu]
--

DROP TABLE IF EXISTS [dbo].[tbl_menu];
IF OBJECT_ID(N'[dbo].[tbl_menu]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_menu] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Disabled',
    [blank] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [parent_id] INT DEFAULT 0,
    [uri] NVARCHAR(255) NOT NULL,
    [theme] NVARCHAR(30) NOT NULL,
    [color] NVARCHAR(30) DEFAULT NULL,
    [icon] NVARCHAR(20) DEFAULT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    [cover] NVARCHAR(150) DEFAULT NULL,
    [last_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT NOT NULL,
    [insert_user] INT NOT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_menu_status] CHECK ([status] IN (N'Enabled', N'Disabled')),
    CONSTRAINT [CK_tbl_menu_blank] CHECK ([blank] IN (N'Yes', N'No'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'parent_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_menu]'))
    CREATE INDEX [parent_id] ON [dbo].[tbl_menu] ([parent_id]);

--
-- 傾印資料表的資料 [tbl_menu]
--

SET IDENTITY_INSERT [dbo].[tbl_menu] ON;
INSERT INTO [dbo].[tbl_menu] ([id], [status], [blank], [parent_id], [uri], [theme], [color], [icon], [sorter], [cover], [last_ts], [last_user], [insert_user], [insert_ts]) VALUES
(1, N'Enabled', N'No', 0, N'/nav', N'Basic', NULL, NULL, 0, N'', N'2017-01-17 13:09:45', 1, 1, N'2021-09-20 01:14:24'),
(2, N'Enabled', N'No', 0, N'/sidebar', N'Basic', NULL, NULL, 1, N'', N'2015-12-08 02:02:02', 1, 1, N'2021-09-20 01:14:24'),
(4, N'Enabled', N'No', 2, N'about', N'Basic', N'info', NULL, 0, NULL, N'2018-08-15 10:58:14', 1, 1, N'2018-08-15 10:58:14'),
(5, N'Enabled', N'No', 2, N'/s/privacy', N'Basic', N'info', N'', 1, NULL, N'2025-02-28 10:50:23', 1, 1, N'2018-08-15 10:58:14'),
(9, N'Enabled', N'No', 2, N'/contact', N'Basic', N'info', N'', 2, NULL, N'2025-02-28 10:50:46', 1, 1, N'2018-08-17 12:02:05'),
(16, N'Enabled', N'No', 0, N'Backend', N'Basic', N'info', NULL, 3, NULL, N'2021-05-15 10:45:43', 1, 1, N'2021-05-15 10:45:43'),
(17, N'Enabled', N'No', 16, N'cms', N'Basic', N'info', NULL, 1, NULL, N'2021-05-15 10:46:29', 1, 1, N'2021-05-15 10:46:29'),
(18, N'Enabled', N'No', 16, N'crm', N'Basic', N'info', NULL, 2, NULL, N'2021-05-15 10:47:10', 1, 1, N'2021-05-15 10:47:10'),
(19, N'Enabled', N'No', 16, N'site', N'Basic', N'info', NULL, 3, NULL, N'2021-05-15 10:47:47', 1, 1, N'2021-05-15 10:47:47'),
(21, N'Enabled', N'No', 19, N'menu/simple', N'Basic', N'info', N'sitemap', 1, NULL, N'2023-06-12 21:01:05', 1, 1, N'2021-05-16 11:18:31'),
(22, N'Enabled', N'No', 19, N'post/list', N'Basic', N'info', N'file-text-o', 0, NULL, N'2021-05-16 11:19:11', 1, 1, N'2021-05-16 11:19:11'),
(23, N'Enabled', N'No', 19, N'staff/simple', N'Basic', N'info', N'users', 2, NULL, N'2023-06-12 21:01:05', 1, 1, N'2021-05-16 11:20:14'),
(25, N'Enabled', N'No', 18, N'contact/simple', N'Basic', N'info', N'phone', 1, NULL, N'2023-06-13 14:13:48', 1, 1, N'2021-05-16 11:24:01'),
(27, N'Enabled', N'No', 17, N'press/list', N'Basic', N'info', N'rss', 0, NULL, N'2021-09-22 08:37:49', 1, 1, N'2021-05-16 11:26:33'),
(29, N'Disabled', N'No', 17, N'dashboard/collections', N'Basic', N'info', N'cogs', 5, NULL, N'2025-07-02 08:13:27', 1, 1, N'2021-05-16 11:27:52'),
(31, N'Enabled', N'No', 18, N'adv/list', N'Basic', N'info', N'newspaper-o', 0, NULL, N'2023-06-12 21:01:05', 1, 1, N'2021-05-16 11:28:46'),
(35, N'Enabled', N'No', 18, N'stream/simple', N'Basic', N'info', N'stack-overflow', 2, NULL, N'2023-06-13 14:13:42', 1, 1, N'2021-05-22 00:18:52'),
(36, N'Enabled', N'No', 19, N'dashboard/advanced', N'Basic', N'info', N'cogs', 5, NULL, N'2023-06-12 21:08:11', 1, 1, N'2021-05-22 00:22:40'),
(43, N'Disabled', N'No', 17, N'dictionary/list', N'', NULL, N'wikipedia-w', 2, NULL, N'2025-07-02 13:00:22', 1, 1, N'2025-02-19 07:46:12'),
(48, N'Enabled', N'No', 17, N'draft/list', N'', NULL, N'pencil', 1, NULL, N'2025-07-02 13:00:53', 1, 1, N'2025-02-27 19:04:04'),
(77, N'Enabled', N'No', 16, N'board', N'', NULL, N'', 0, NULL, N'2025-07-21 04:01:11', 1, 1, N'2025-05-30 02:20:02'),
(78, N'Enabled', N'No', 77, N'stats/simple', N'', NULL, N'dashboard', 0, NULL, N'2025-05-30 02:21:36', 1, 1, N'2025-05-30 02:21:36'),
(79, N'Enabled', N'No', 17, N'tag/simple', N'', NULL, N'tags', 3, NULL, N'2025-07-02 08:14:08', 1, 1, N'2025-07-02 08:14:08'),
(80, N'Enabled', N'No', 17, N'author/list', N'', NULL, N'users', 4, NULL, N'2025-07-02 08:15:14', 1, 1, N'2025-07-02 08:15:14'),
(81, N'Enabled', N'No', 17, N'category/simple', N'', NULL, N'folder', 7, NULL, N'2025-07-02 08:16:32', 1, 1, N'2025-07-02 08:16:32');
SET IDENTITY_INSERT [dbo].[tbl_menu] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_menu_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_menu_lang];
IF OBJECT_ID(N'[dbo].[tbl_menu_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_menu_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [badge] NVARCHAR(50) DEFAULT NULL,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'lang_pid' AND object_id = OBJECT_ID(N'[dbo].[tbl_menu_lang]'))
    CREATE INDEX [lang_pid] ON [dbo].[tbl_menu_lang] ([lang],[parent_id]);

--
-- 傾印資料表的資料 [tbl_menu_lang]
--

SET IDENTITY_INSERT [dbo].[tbl_menu_lang] ON;
INSERT INTO [dbo].[tbl_menu_lang] ([id], [lang], [parent_id], [title], [badge], [info], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'tw', 1, N'上方導覽', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(2, N'en', 1, N'Nav', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(3, N'en', 2, N'Sidebar', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(4, N'tw', 2, N'側邊欄', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(5, N'tw', 4, N'關於我們', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(6, N'en', 4, N'About', NULL, NULL, N'2018-08-15 09:36:49', 1, N'2018-08-15 09:36:49', 1),
(7, N'tw', 5, N'隱私權政策', N'', N'', N'2025-02-28 10:50:23', 1, N'2018-08-15 09:36:49', 1),
(8, N'en', 5, N'Privacy', N'', N'', N'2025-02-28 10:50:23', 1, N'2018-08-15 09:36:49', 1),
(9, N'tw', 9, N'聯絡我們', N'', N'', N'2025-02-28 10:50:46', 1, N'2018-08-17 12:02:05', 1),
(10, N'en', 9, N'Contact us', N'', N'', N'2025-02-28 10:50:46', 1, N'2018-08-17 12:02:05', 1),
(11, N'tw', 10, N'關於我們', N'', N'', N'2025-02-28 10:51:00', 1, N'2018-09-27 03:52:10', 1),
(12, N'en', 10, N'About us', N'', N'', N'2025-02-28 10:51:00', 1, N'2018-09-27 03:52:10', 1),
(21, N'tw', 15, N'聯絡我們', N'', N'', N'2025-02-28 10:46:42', 1, N'2018-09-27 04:54:09', 1),
(22, N'en', 15, N'Contact us', N'', N'', N'2025-02-28 10:46:42', 1, N'2018-09-27 04:54:09', 1),
(23, N'tw', 16, N'後台選單', N'', N'', N'2021-05-15 10:45:43', 1, N'2021-05-15 10:45:43', 1),
(25, N'tw', 17, N'內容管理', N'', N'', N'2021-05-15 10:46:29', 1, N'2021-05-15 10:46:29', 1),
(27, N'tw', 18, N'客戶管理', N'', N'', N'2021-05-15 10:47:10', 1, N'2021-05-15 10:47:10', 1),
(29, N'tw', 19, N'網站管理', N'', N'', N'2021-05-15 10:47:47', 1, N'2021-05-15 10:47:47', 1),
(33, N'tw', 21, N'選單', N'', N'', N'2021-05-16 11:18:31', 1, N'2021-05-16 11:18:31', 1),
(35, N'tw', 22, N'固定單頁', N'', N'', N'2021-05-16 11:19:11', 1, N'2021-05-16 11:19:11', 1),
(37, N'tw', 23, N'管理員', N'', N'', N'2021-05-22 00:20:27', 1, N'2021-05-16 11:20:14', 1),
(41, N'tw', 25, N'聯絡我們', N'', N'', N'2021-05-16 11:24:01', 1, N'2021-05-16 11:24:01', 1),
(45, N'tw', 27, N'文章', N'', N'', N'2021-05-16 11:26:33', 1, N'2021-05-16 11:26:33', 1),
(47, N'tw', 28, N'標籤', N'', N'', N'2021-05-16 11:27:10', 1, N'2021-05-16 11:27:10', 1),
(49, N'tw', 29, N'集合管理', N'', N'', N'2025-07-02 08:13:27', 1, N'2021-05-16 11:27:52', 1),
(53, N'tw', 31, N'廣告', N'', N'', N'2021-05-16 11:28:46', 1, N'2021-05-16 11:28:46', 1),
(61, N'tw', 35, N'服務歷程', N'', N'', N'2023-06-12 21:00:12', 1, N'2021-05-22 00:18:52', 1),
(63, N'tw', 36, N'進階', N'', N'', N'2023-06-12 20:52:08', 1, N'2021-05-22 00:22:40', 1),
(69, N'tw', 39, N'使用者', N'', N'', N'2024-02-13 17:34:48', 1, N'2021-05-22 00:34:27', 1),
(71, N'tw', 40, N'客戶', N'', N'', N'2024-02-13 17:35:41', 1, N'2024-02-13 17:34:36', 1),
(73, N'tw', 41, N'專案', N'', N'', N'2025-02-19 07:48:19', 1, N'2024-02-13 17:37:03', 1),
(75, N'tw', 42, N'發票', N'', N'', N'2024-02-13 17:40:12', 1, N'2024-02-13 17:40:12', 1),
(77, N'tw', 43, N'詞彙表', N'', N'', N'2025-07-02 13:00:22', 1, N'2025-02-19 07:46:12', 1),
(79, N'tw', 44, N'推薦書目', N'', N'', N'2025-02-19 07:47:08', 1, N'2025-02-19 07:47:08', 1),
(81, N'tw', 45, N'開拓行者', N'', N'', N'2025-02-28 10:45:15', 1, N'2025-02-19 08:39:04', 1),
(82, N'en', 45, N'Junior Wayfarer', N'', N'', N'2025-02-28 10:45:15', 1, N'2025-02-19 08:39:04', 1),
(83, N'tw', 46, N'歷練行者', N'', N'', N'2025-02-28 10:46:20', 1, N'2025-02-21 08:16:57', 1),
(84, N'en', 46, N'Senior Wayfarer', N'', N'', N'2025-02-28 10:46:20', 1, N'2025-02-21 08:16:57', 1),
(85, N'tw', 47, N'遊戲假說', N'', N'', N'2025-02-28 10:44:22', 1, N'2025-02-21 19:05:27', 1),
(86, N'en', 47, N'Game Hypothesis', N'', N'', N'2025-02-28 10:44:22', 1, N'2025-02-21 19:05:27', 1),
(87, N'tw', 48, N'AI 小編', N'', N'', N'2025-07-02 13:00:53', 1, N'2025-02-27 19:04:04', 1),
(89, N'tw', 49, N'外站文章', N'', N'', N'2025-02-27 19:04:45', 1, N'2025-02-27 19:04:45', 1),
(134, N'tw', 77, N'總覽', N'', N'', N'2025-07-21 04:01:11', 1, N'2025-05-30 02:20:02', 1),
(138, N'tw', 78, N'常用數據', N'', N'', N'2025-05-30 02:21:36', 1, N'2025-05-30 02:21:36', 1),
(147, N'tw', 79, N'標籤', N'', N'', N'2025-07-02 08:14:08', 1, N'2025-07-02 08:14:08', 1),
(151, N'tw', 80, N'作者', N'', N'', N'2025-07-02 08:15:14', 1, N'2025-07-02 08:15:14', 1),
(155, N'tw', 81, N'分類', N'', N'', N'2025-07-02 08:16:32', 1, N'2025-07-02 08:16:32', 1);
SET IDENTITY_INSERT [dbo].[tbl_menu_lang] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_menu_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_menu_tag];
IF OBJECT_ID(N'[dbo].[tbl_menu_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_menu_tag] (
    [menu_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([menu_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_meta];
IF OBJECT_ID(N'[dbo].[tbl_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [fence] NVARCHAR(50) DEFAULT NULL,
    [label] NVARCHAR(150) DEFAULT NULL,
    [preset] NVARCHAR(100) DEFAULT NULL,
    [type] NVARCHAR(20) DEFAULT NULL,
    [input] NVARCHAR(20) DEFAULT NULL,
    [option] NVARCHAR(20) DEFAULT NULL,
    [sorter] TINYINT NOT NULL DEFAULT 10,
    [ps] NVARCHAR(250) DEFAULT NULL,
    [last_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_meta_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

--
-- 傾印資料表的資料 [tbl_meta]
--

SET IDENTITY_INSERT [dbo].[tbl_meta] ON;
INSERT INTO [dbo].[tbl_meta] ([id], [status], [fence], [label], [preset], [type], [input], [option], [sorter], [ps], [last_ts], [last_user], [insert_user], [insert_ts]) VALUES
(5, N'Enabled', N'seo_desc', N'SEO 描述', NULL, N'text', N'paragraph', NULL, 3, N'', N'2025-07-13 00:50:17', 1, 1, N'2021-07-04 06:20:11'),
(6, N'Enabled', N'seo_keyword', N'SEO 關鍵字', NULL, N'text', N'paragraph', NULL, 4, N'以英文逗號( , )間隔 ', N'2022-04-14 09:37:31', 1, 1, N'2021-07-04 06:26:12'),
(11, N'Enabled', N'btn_txt', N'CTA 文字', NULL, N'text', N'text', NULL, 10, N'簡潔有力', N'2025-07-13 00:47:24', 1, 1, N'2022-08-05 15:52:44');
SET IDENTITY_INSERT [dbo].[tbl_meta] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_meta_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_meta_tag];
IF OBJECT_ID(N'[dbo].[tbl_meta_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_meta_tag] (
    [meta_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([meta_id],[tag_id])
);
END;

--
-- 傾印資料表的資料 [tbl_meta_tag]
--

INSERT INTO [dbo].[tbl_meta_tag] ([meta_id], [tag_id], [sorter]) VALUES
(5, 4, 0);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_option]
--

DROP TABLE IF EXISTS [dbo].[tbl_option];
IF OBJECT_ID(N'[dbo].[tbl_option]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_option] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [loader] NVARCHAR(7) NOT NULL DEFAULT N'Demand',
    [group] NVARCHAR(50) NOT NULL,
    [name] NVARCHAR(255) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_option_status] CHECK ([status] IN (N'Enabled', N'Disabled')),
    CONSTRAINT [CK_tbl_option_loader] CHECK ([loader] IN (N'Preload', N'Demand'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'group' AND object_id = OBJECT_ID(N'[dbo].[tbl_option]'))
    CREATE INDEX [group] ON [dbo].[tbl_option] ([group]);

--
-- 傾印資料表的資料 [tbl_option]
--

SET IDENTITY_INSERT [dbo].[tbl_option] ON;
INSERT INTO [dbo].[tbl_option] ([id], [status], [loader], [group], [name], [content], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'Enabled', N'Demand', N'page', N'title', N'Demo', N'2025-07-16 22:19:38', 1, N'2015-12-29 06:43:32', 1),
(2, N'Enabled', N'Demand', N'page', N'keyword', N'Demo', N'2018-11-06 03:16:03', 1, N'2015-12-29 06:44:11', 1),
(4, N'Enabled', N'Demand', N'page', N'img', N'https://lifetrainee.org/media/social-img', N'2018-11-06 03:20:47', 1, N'2015-12-29 06:46:44', 1),
(5, N'Enabled', N'Preload', N'social', N'facebook_page', N'https://www.facebook.com/', N'2025-07-08 22:35:18', 1, N'2015-12-29 10:35:46', 1),
(8, N'Enabled', N'Preload', N'default', N'contact_mail', N'trevor@sense-info.co', N'2025-07-08 22:51:02', 1, N'2016-02-02 02:08:41', 1),
(12, N'Enabled', N'Demand', N'page', N'ga', N'G-', N'2025-03-03 03:11:39', 1, N'2016-05-03 23:51:12', 1),
(26, N'Enabled', N'Preload', N'default', N'contact_phone', N'02 3224 2399', N'2025-07-08 22:50:45', 1, N'2016-02-02 02:08:41', 1),
(27, N'Enabled', N'Preload', N'default', N'contact_address', N'林路3段9號', N'2025-07-08 22:50:26', 1, N'2016-02-02 02:08:41', 1),
(28, N'Enabled', N'Preload', N'social', N'gmap_page', N'https://www.google.com/maps/', N'2025-07-08 22:37:41', 1, N'2015-12-29 10:35:46', 1),
(29, N'Enabled', N'Preload', N'social', N'line_page', N'@', N'2025-07-08 22:47:08', 1, N'2015-12-29 10:35:46', 1),
(30, N'Enabled', N'Demand', N'page', N'desc', N'Demo', N'2018-11-06 03:15:26', 1, N'2018-11-06 03:15:26', 1),
(31, N'Enabled', N'Preload', N'default', N'color_name', N'color-theme-3', N'2025-03-03 03:55:48', 1, N'2016-02-02 02:08:41', 1),
(32, N'Enabled', N'Demand', N'page', N'subtitle', N'Demo', N'2025-07-16 22:20:18', 1, N'2025-07-16 22:20:18', 1);
SET IDENTITY_INSERT [dbo].[tbl_option] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_post]
--

DROP TABLE IF EXISTS [dbo].[tbl_post];
IF OBJECT_ID(N'[dbo].[tbl_post]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_post] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [slug] NVARCHAR(255) NOT NULL,
    [cover] NVARCHAR(255) NOT NULL,
    [layout] NVARCHAR(20) DEFAULT N'normal',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_post_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

--
-- 傾印資料表的資料 [tbl_post]
--

SET IDENTITY_INSERT [dbo].[tbl_post] ON;
INSERT INTO [dbo].[tbl_post] ([id], [status], [slug], [cover], [layout], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(376, N'Enabled', N'contact', N'', N'contact', N'2025-07-11 04:07:20', 1, N'2025-07-06 14:24:39', 1),
(378, N'Enabled', N'about', N'', N'normal', N'2025-08-26 01:11:07', 1, N'2019-06-21 02:03:07', 1);
SET IDENTITY_INSERT [dbo].[tbl_post] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_post_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_post_lang];
IF OBJECT_ID(N'[dbo].[tbl_post_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_post_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [from_ai] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_post_lang_from_ai] CHECK ([from_ai] IN (N'No', N'Yes'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'lang_pid' AND object_id = OBJECT_ID(N'[dbo].[tbl_post_lang]'))
    CREATE INDEX [lang_pid] ON [dbo].[tbl_post_lang] ([lang],[parent_id]);

--
-- 傾印資料表的資料 [tbl_post_lang]
--

SET IDENTITY_INSERT [dbo].[tbl_post_lang] ON;
INSERT INTO [dbo].[tbl_post_lang] ([id], [from_ai], [lang], [parent_id], [title], [content], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(12, N'No', N'tw', 376, N'聯絡我們', N'<p>麻煩您留下基本資訊與洽詢內容，待收到您的回饋後，我們將指派專人以最快的速度與您聯繫！</p><p>如果想發信給我們也歡迎寄信到 hello@sense-info.co。個資安全相關的問題也可以寄信到 pims@sense-info.co。</p>', N'2025-07-11 04:07:20', 1, N'2025-07-06 14:24:39', 1),
(13, N'No', N'en', 376, N'Contact Us', N'<p>Please leave your basic information and inquiry content. Once we receive your feedback, we will assign a dedicated person to contact you at the earliest possible time!</p><p>If you wish to send an email to us, feel free to write to hello@sense-info.co. For issues related to personal data security, you can also email pims@sense-info.co.</p>', N'2025-07-11 04:07:20', 1, N'2025-07-06 14:24:39', 1),
(14, N'No', N'jp', 376, N'お問い合わせ', N'<p>恐れ入りますが、基本情報とご相談内容をお知らせください。ご連絡をいただき次第、担当者が迅速に対応いたします！</p><p>また、メールでのご連絡をご希望の場合は、 hello@sense-info.co までお送りください。個人情報保護に関するご質問は、 pims@sense-info.co へもご連絡いただけます。</p>', N'2025-07-11 04:07:20', 1, N'2025-07-06 14:24:39', 1),
(15, N'No', N'ko', 376, N'연락처', N'<p>귀하의 기본 정보와 문의 내용을 남겨 주시면, 귀하의 피드백을 받은 후 최대한 신속하게 담당자가 연락드리겠습니다!</p><p>저희에게 이메일을 보내고 싶다면 hello@sense-info.co 로 보내 주세요. 개인정보 보안 관련 문의는 pims@sense-info.co 로도 보내실 수 있습니다.</p>', N'2025-07-11 04:07:20', 1, N'2025-07-06 14:24:39', 1),
(17, N'No', N'tw', 378, N'關於本區', N'', N'2025-08-26 01:11:07', 1, N'2025-08-26 01:11:07', 1),
(16, N'No', N'en', 378, N'關於本區', N'', N'2025-08-26 01:11:07', 1, N'2025-08-26 01:11:07', 1),
(18, N'No', N'ja', 378, N'關於本區', N'', N'2025-08-26 01:11:07', 1, N'2025-08-26 01:11:07', 1),
(19, N'No', N'ko', 378, N'關於本區', N'', N'2025-08-26 01:11:07', 1, N'2025-08-26 01:11:07', 1);
SET IDENTITY_INSERT [dbo].[tbl_post_lang] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_post_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_post_meta];
IF OBJECT_ID(N'[dbo].[tbl_post_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_post_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [k] NVARCHAR(50) DEFAULT NULL,
    [v] NVARCHAR(MAX) DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'fk_meta_press_idx' AND object_id = OBJECT_ID(N'[dbo].[tbl_post_meta]'))
    CREATE INDEX [fk_meta_press_idx] ON [dbo].[tbl_post_meta] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_post_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_post_tag];
IF OBJECT_ID(N'[dbo].[tbl_post_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_post_tag] (
    [post_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    PRIMARY KEY ([post_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press]
--

DROP TABLE IF EXISTS [dbo].[tbl_press];
IF OBJECT_ID(N'[dbo].[tbl_press]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [cate_id] INT NOT NULL DEFAULT 1,
    [layout] INT NOT NULL DEFAULT 1,
    [status] NVARCHAR(9) DEFAULT N'Draft',
    [mode] NVARCHAR(7) NOT NULL DEFAULT N'Article',
    [on_homepage] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [on_top] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [slug] NVARCHAR(255) NOT NULL,
    [online_date] DATETIME2 DEFAULT SYSUTCDATETIME(),
    [sorter] INT NOT NULL DEFAULT 99,
    [cover] NVARCHAR(255) NOT NULL,
    [banner] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_press_status] CHECK ([status] IN (N'Draft', N'Published', N'Scheduled', N'Changed', N'Offlined')),
    CONSTRAINT [CK_tbl_press_mode] CHECK ([mode] IN (N'Article', N'Slide')),
    CONSTRAINT [CK_tbl_press_on_homepage] CHECK ([on_homepage] IN (N'Yes', N'No')),
    CONSTRAINT [CK_tbl_press_on_top] CHECK ([on_top] IN (N'Yes', N'No'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_author]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_author];
IF OBJECT_ID(N'[dbo].[tbl_press_author]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_author] (
    [press_id] INT NOT NULL,
    [author_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([press_id],[author_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_book]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_book];
IF OBJECT_ID(N'[dbo].[tbl_press_book]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_book] (
    [press_id] INT NOT NULL,
    [book_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_lang];
IF OBJECT_ID(N'[dbo].[tbl_press_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [from_ai] NVARCHAR(3) NOT NULL DEFAULT N'No',
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT DEFAULT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [subtitle] NVARCHAR(255) DEFAULT NULL,
    [info] NVARCHAR(700) DEFAULT NULL,
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_press_lang_from_ai] CHECK ([from_ai] IN (N'No', N'Yes'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'lang_pid' AND object_id = OBJECT_ID(N'[dbo].[tbl_press_lang]'))
    CREATE INDEX [lang_pid] ON [dbo].[tbl_press_lang] ([lang],[parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_meta]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_meta];
IF OBJECT_ID(N'[dbo].[tbl_press_meta]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_meta] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [k] NVARCHAR(50) DEFAULT NULL,
    [v] NVARCHAR(MAX) DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'fk_meta_press_idx' AND object_id = OBJECT_ID(N'[dbo].[tbl_press_meta]'))
    CREATE INDEX [fk_meta_press_idx] ON [dbo].[tbl_press_meta] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_related]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_related];
IF OBJECT_ID(N'[dbo].[tbl_press_related]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_related] (
    [press_id] INT NOT NULL,
    [related_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([related_id],[press_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_tag];
IF OBJECT_ID(N'[dbo].[tbl_press_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_tag] (
    [press_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([press_id],[tag_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_term]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_term];
IF OBJECT_ID(N'[dbo].[tbl_press_term]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_term] (
    [press_id] INT NOT NULL,
    [term_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([term_id],[press_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_press_log]
--

DROP TABLE IF EXISTS [dbo].[tbl_press_log];
IF OBJECT_ID(N'[dbo].[tbl_press_log]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_log] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL,
    [action_code] NVARCHAR(64) NOT NULL DEFAULT N'',
    [old_state_code] NVARCHAR(64) DEFAULT NULL,
    [new_state_code] NVARCHAR(64) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'parent_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_press_log]'))
    CREATE INDEX [parent_id] ON [dbo].[tbl_press_log] ([parent_id]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_role]
--

DROP TABLE IF EXISTS [dbo].[tbl_role];
IF OBJECT_ID(N'[dbo].[tbl_role]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_role] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [menu_id] INT NOT NULL DEFAULT 0,
    [title] NVARCHAR(255) DEFAULT NULL,
    [priv] INT DEFAULT 0,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_role_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

--
-- 傾印資料表的資料 [tbl_role]
--

SET IDENTITY_INSERT [dbo].[tbl_role] ON;
INSERT INTO [dbo].[tbl_role] ([id], [status], [menu_id], [title], [priv], [info], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'Enabled', 16, N'Administrator', 31, N'基本管理&#13;&#10;進階管理', N'2025-07-15 09:51:18', 1, N'2018-01-13 17:58:43', 1),
(2, N'Enabled', 83, N'編輯', 5, N'基本管理', N'2025-07-15 09:51:03', 1, N'2018-01-17 03:37:15', 1);
SET IDENTITY_INSERT [dbo].[tbl_role] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_search]
--

DROP TABLE IF EXISTS [dbo].[tbl_search];
IF OBJECT_ID(N'[dbo].[tbl_search]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_search] (
    [id] INT NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Disabled',
    [site_id] INT DEFAULT NULL,
    [counter] INT NOT NULL DEFAULT 0,
    [title] NVARCHAR(255) DEFAULT NULL,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    CONSTRAINT [CK_tbl_search_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_search_press]
--

DROP TABLE IF EXISTS [dbo].[tbl_search_press];
IF OBJECT_ID(N'[dbo].[tbl_search_press]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_search_press] (
    [press_id] INT NOT NULL,
    [search_id] INT NOT NULL
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_shorten]
--

DROP TABLE IF EXISTS [dbo].[tbl_shorten];
IF OBJECT_ID(N'[dbo].[tbl_shorten]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_shorten] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [cap] INT NOT NULL DEFAULT 9999,
    [hits] INT NOT NULL DEFAULT 0,
    [finished] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Disabled',
    [origin] NVARCHAR(255) NOT NULL,
    [token] NVARCHAR(255) NOT NULL,
    [note] NVARCHAR(300) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_shorten_token_uni] UNIQUE ([token]),
    CONSTRAINT [CK_tbl_shorten_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'origin_uni' AND object_id = OBJECT_ID(N'[dbo].[tbl_shorten]'))
    CREATE INDEX [origin_uni] ON [dbo].[tbl_shorten] ([origin]);

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_staff]
--

DROP TABLE IF EXISTS [dbo].[tbl_staff];
IF OBJECT_ID(N'[dbo].[tbl_staff]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_staff] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) DEFAULT N'New',
    [needReset] TINYINT NOT NULL DEFAULT 0,
    [role_id] INT NOT NULL,
    [account] NVARCHAR(45) DEFAULT NULL,
    [pwd] NVARCHAR(72) DEFAULT NULL,
    [verify_code] NVARCHAR(64) NOT NULL DEFAULT N'',
    [email] NVARCHAR(250) DEFAULT NULL,
    [note] NVARCHAR(255) NOT NULL DEFAULT N'',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_staff_status] CHECK ([status] IN (N'New', N'Verified', N'Freeze'))
);
END;

--
-- 傾印資料表的資料 [tbl_staff]
--

SET IDENTITY_INSERT [dbo].[tbl_staff] ON;
INSERT INTO [dbo].[tbl_staff] ([id], [status], [needReset], [role_id], [account], [pwd], [verify_code], [email], [note], [last_ts], [last_user], [insert_ts], [insert_user]) VALUES
(1, N'Verified', 0, 1, N'trevor', N'$2y$10$6zGcEf6T8Mz9iNofPcCTuO8YVR.6UuAMNDcp7It.8seHxHJlp/jga', N'JAGRSTLTLLXN9HNTDN7QYLWFK8FXDLF5', N'trevor@sense-info.co', N'', N'2025-07-07 08:59:20', 1, N'2015-08-04 12:41:20', 1),
(2, N'Verified', 0, 2, N'editor', N'$2y$10$jfL3NNv9EIX7115ExL8osea8WT/yyOchUwNmhQxoEeVK/b9IYlADa', N'XYX95QGMM6F89NST6MQ8C8XG5FFDW8N9', N'shuaib25@gmail.com', N'', N'2025-07-15 09:45:30', 2, N'2025-07-15 09:06:30', 1);
SET IDENTITY_INSERT [dbo].[tbl_staff] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_staff_footmark]
--

DROP TABLE IF EXISTS [dbo].[tbl_staff_footmark];
IF OBJECT_ID(N'[dbo].[tbl_staff_footmark]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_staff_footmark] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT DEFAULT NULL,
    [pwd] NVARCHAR(100) DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'parent_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_staff_footmark]'))
    CREATE INDEX [parent_id] ON [dbo].[tbl_staff_footmark] ([parent_id]);

--
-- 傾印資料表的資料 [tbl_staff_footmark]
--

SET IDENTITY_INSERT [dbo].[tbl_staff_footmark] ON;
INSERT INTO [dbo].[tbl_staff_footmark] ([id], [parent_id], [pwd], [insert_ts], [insert_user]) VALUES
(1, 2, N'$2y$10$WkTwHPRrO6gpWo6lywoGW.68I1PT4GnnVokGyH1US2DLobWD2THqK', N'2025-07-15 09:08:32', 2);
SET IDENTITY_INSERT [dbo].[tbl_staff_footmark] OFF;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_staff_sudo]
--

DROP TABLE IF EXISTS [dbo].[tbl_staff_sudo];
IF OBJECT_ID(N'[dbo].[tbl_staff_sudo]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_staff_sudo] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_stream]
--

DROP TABLE IF EXISTS [dbo].[tbl_stream];
IF OBJECT_ID(N'[dbo].[tbl_stream]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_stream] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [target] NVARCHAR(10) NOT NULL DEFAULT N'Sudo',
    [parent_id] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [content] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_stream_target] CHECK ([target] IN (N'Task', N'Sudo', N'StaffLogin')),
    CONSTRAINT [CK_tbl_stream_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_subscription]
--

DROP TABLE IF EXISTS [dbo].[tbl_subscription];
IF OBJECT_ID(N'[dbo].[tbl_subscription]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_subscription] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [lancode] NVARCHAR(10) DEFAULT N'tw',
    [name] NVARCHAR(255) DEFAULT N'',
    [phone] NVARCHAR(50) DEFAULT N'',
    [email] NVARCHAR(255) NOT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_subscription_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_tag]
--

DROP TABLE IF EXISTS [dbo].[tbl_tag];
IF OBJECT_ID(N'[dbo].[tbl_tag]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_tag] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [cate_id] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) DEFAULT N'Disabled',
    [parent_id] INT NOT NULL DEFAULT 0,
    [counter] INT DEFAULT NULL,
    [slug] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_tag_status] CHECK ([status] IN (N'Disabled', N'Enabled'))
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_tag_lang]
--

DROP TABLE IF EXISTS [dbo].[tbl_tag_lang];
IF OBJECT_ID(N'[dbo].[tbl_tag_lang]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_tag_lang] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [lang] NVARCHAR(5) NOT NULL DEFAULT N'tw',
    [parent_id] INT NOT NULL,
    [title] NVARCHAR(255) DEFAULT NULL,
    [alias] NVARCHAR(255) DEFAULT NULL,
    [info] NVARCHAR(255) DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT NULL,
    [insert_user] INT DEFAULT NULL,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_tag_lang_lang_pid] UNIQUE ([lang],[parent_id])
);
END;

-- --------------------------------------------------------

--
-- 資料表結構 [tbl_tag_related]
--

DROP TABLE IF EXISTS [dbo].[tbl_tag_related];
IF OBJECT_ID(N'[dbo].[tbl_tag_related]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_tag_related] (
    [related_id] INT NOT NULL,
    [tag_id] INT NOT NULL,
    [sorter] INT NOT NULL DEFAULT 0
);
END;

