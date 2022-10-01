
-- Table: T_LIB_BOOK_BORROW_RECORD
CREATE TABLE [t_lib_book_borrow_record](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[BOOK_ID] [int] NOT NULL,
	[USER] [nvarchar](40) NOT NULL,
	[FROMORG] [int] NULL,
	[FROMDATE] [date] NULL,
	[TODATE] [date] NULL,
	[ISRETURNED] [bit] NOT NULL CONSTRAINT [DF_t_lib_book_brwrd_isret] DEFAULT(0),
	[COMMENT] [nvarchar](50) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_brwrd_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_brwrd_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_book_brwrd_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_book_brwrd_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Content
SET IDENTITY_INSERT [t_lib_personrole_def] ON;
INSERT INTO [t_lib_personrole_def] ([ID],[NAME],[COMMENT]) VALUES (2, 'Library.Translator', N'译者');
SET IDENTITY_INSERT [t_lib_personrole_def] OFF;


SET IDENTITY_INSERT [t_lib_orgtype_def] ON;
INSERT INTO [t_lib_orgtype_def] ([ID],[NAME],[COMMENT]) VALUES (2, 'Library.Library', N'图书馆');
SET IDENTITY_INSERT [t_lib_orgtype_def] OFF;

SET IDENTITY_INSERT [t_lib_bookctgy_def] ON;
INSERT INTO [t_lib_bookctgy_def] ([ID],[NAME],[COMMENT],[PARID]) VALUES (5, 'Sys.BkCtgy.DetectiveStory', N'侦探、推理类小说', 1);
INSERT INTO [t_lib_bookctgy_def] ([ID],[NAME],[COMMENT],[PARID]) VALUES (6, 'Sys.BkCtgy.KungfuNovels', N'武侠小说类', 1);
INSERT INTO [t_lib_bookctgy_def] ([ID],[NAME],[COMMENT],[PARID]) VALUES (7, 'Sys.BkCtgy.FantasyNovel', N'玄幻小说类', 1);
INSERT INTO [t_lib_bookctgy_def] ([ID],[NAME],[COMMENT],[PARID]) VALUES (8, 'Sys.BkCtgy.ChineseClassical', N'中国古典小说', 1);
INSERT INTO [t_lib_bookctgy_def] ([ID],[NAME],[COMMENT],[PARID]) VALUES (9, 'Sys.BkCtgy.WorldFamousBook', N'世界名著', 1);
SET IDENTITY_INSERT [t_lib_bookctgy_def] OFF;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (21, '2023.1.1');

-- The end.
