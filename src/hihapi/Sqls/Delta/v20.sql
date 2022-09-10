-- Tables
CREATE TABLE [t_lib_person_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NATIVE_NAME] [nvarchar](100) NOT NULL,
	[CHINESE_NAME] [nvarchar](100) NULL,
	[ISCHN] [bit] NULL,
	[DETAIL] [nvarchar](200) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_person_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_person_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_person_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_person_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_personrole_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](100) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_personrole_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_personrole_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_personrole_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_personrole_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_person_role](
	[PERSON_ID] [int] NOT NULL,
	[ROLE_ID] [int] NOT NULL,
 CONSTRAINT [PK_t_lib_person_role] PRIMARY KEY CLUSTERED ( [PERSON_ID] ASC,	[ROLE_ID] ASC ),
 CONSTRAINT [FK_lib_person_role_person] FOREIGN KEY([PERSON_ID]) REFERENCES [t_lib_person_def] ([ID]),
 CONSTRAINT [FK_t_lib_person_role_role] FOREIGN KEY([ROLE_ID]) REFERENCES [t_lib_personrole_def] ([ID])
);

CREATE TABLE [t_lib_org_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NATIVE_NAME] [nvarchar](100) NOT NULL,
	[CHINESE_NAME] [nvarchar](100) NULL,
	[ISCHN] [bit] NULL,
	[DETAIL] [nvarchar](200) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_org_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_org_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_org_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_org_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_orgtype_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](100) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_orgtype_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_orgtype_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_orgtype_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_orgtype_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_org_type](
	[ORG_ID] [int] NOT NULL,
	[TYPE_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_org_type] PRIMARY KEY CLUSTERED ( [ORG_ID] ASC, [TYPE_ID] ASC ),
	CONSTRAINT [FK_lib_org_type_org] FOREIGN KEY([ORG_ID]) REFERENCES [t_lib_org_def] ([ID]),
	CONSTRAINT [FK_t_lib_org_type_type] FOREIGN KEY([TYPE_ID]) REFERENCES [t_lib_orgtype_def] ([ID])
);


CREATE TABLE [t_lib_bookctgy_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](100) NULL,
	[PARID] [int] NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_bookctgy_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_bookctgy_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_bookctgy_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_bookctgy_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);


CREATE TABLE [t_lib_bookloc_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](100) NULL,
	[LOCTYPE] [int] CONSTRAINT [DEF_t_lib_bookloc_type] DEFAULT 1,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_bookloc_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_bookloc_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_bookloc_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_bookloc_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_book_def](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NATIVE_NAME] [nvarchar](200) NOT NULL,
	[CHINESE_NAME] [nvarchar](200) NULL,
	[ISCHN] [bit] NULL,
	[ISBN] [nvarchar](50) NULL,
	[PUB_YEAR] [INT] NULL,
	[DETAIL] [nvarchar](200) NULL,
	[ORIGIN_LANG] [INT] NULL,
	[BOOK_LANG] [INT] NULL,
	[PAGE_COUNT] [INT] NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_UPDATEDAT] DEFAULT (getdate()) NULL,
	CONSTRAINT [PK_t_lib_book_def] PRIMARY KEY CLUSTERED  ( [ID] ASC ),
	CONSTRAINT [FK_t_lib_book_HID] FOREIGN KEY ([HID]) REFERENCES [t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [t_lib_book_author](
	[BOOK_ID] [int] NOT NULL,
	[AUTHOR_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_book_author] PRIMARY KEY CLUSTERED ( [BOOK_ID] ASC, [AUTHOR_ID] ASC ),
	CONSTRAINT [FK_lib_book_author_book] FOREIGN KEY([BOOK_ID]) REFERENCES [t_lib_book_def] ([ID]),
	CONSTRAINT [FK_lib_book_author_author] FOREIGN KEY([AUTHOR_ID]) REFERENCES [t_lib_person_def] ([ID])
);

CREATE TABLE [t_lib_book_translator](
	[BOOK_ID] [int] NOT NULL,
	[TRANSLATOR_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_book_translator] PRIMARY KEY CLUSTERED ( [BOOK_ID] ASC, [TRANSLATOR_ID] ASC ),
	CONSTRAINT [FK_lib_book_translator_book] FOREIGN KEY([BOOK_ID]) REFERENCES [t_lib_book_def] ([ID]),
	CONSTRAINT [FK_lib_book_translator_translator] FOREIGN KEY([TRANSLATOR_ID]) REFERENCES [t_lib_person_def] ([ID])
);

CREATE TABLE [t_lib_book_press](
	[BOOK_ID] [int] NOT NULL,
	[PRESS_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_book_press] PRIMARY KEY CLUSTERED ( [BOOK_ID] ASC, [PRESS_ID] ASC ),
	CONSTRAINT [FK_lib_book_press_book] FOREIGN KEY([BOOK_ID]) REFERENCES [t_lib_book_def] ([ID]),
	CONSTRAINT [FK_lib_book_press_press] FOREIGN KEY([PRESS_ID]) REFERENCES [t_lib_org_def] ([ID])
);

CREATE TABLE [t_lib_book_ctgy](
	[BOOK_ID] [int] NOT NULL,
	[CTGY_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_book_ctgy] PRIMARY KEY CLUSTERED ( [BOOK_ID] ASC, [CTGY_ID] ASC ),
	CONSTRAINT [FK_lib_book_ctgy_book] FOREIGN KEY([BOOK_ID]) REFERENCES [t_lib_book_def] ([ID]),
	CONSTRAINT [FK_lib_book_ctgy_ctgy] FOREIGN KEY([CTGY_ID]) REFERENCES [t_lib_bookctgy_def] ([ID])
);

CREATE TABLE [t_lib_book_location](
	[BOOK_ID] [int] NOT NULL,
	[LOCATION_ID] [int] NOT NULL,
	CONSTRAINT [PK_t_lib_book_location] PRIMARY KEY CLUSTERED ( [BOOK_ID] ASC, [LOCATION_ID] ASC ),
	CONSTRAINT [FK_lib_book_location_book] FOREIGN KEY([BOOK_ID]) REFERENCES [t_lib_book_def] ([ID]),
	CONSTRAINT [FK_lib_book_location_location] FOREIGN KEY([LOCATION_ID]) REFERENCES [t_lib_bookloc_def] ([ID])
);

-- Content
-- Person role
-- Organzation type
-- Book category

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (20, '2022.12.31');

-- The end.
