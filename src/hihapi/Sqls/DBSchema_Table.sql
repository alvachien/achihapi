/*
 * Database Tables
 *
 */

/****** Object:  Table [dbo].[t_homedef]    Script Date: 2017-05-05 11:36:16 PM ******/
CREATE TABLE [dbo].[t_homedef] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [NAME]      NVARCHAR (50) NOT NULL,
    [DETAILS]   NVARCHAR (50) NULL,
    [HOST]      NVARCHAR (50) NOT NULL,
    [BASECURR]  NVARCHAR (5)  NOT NULL,
    [CREATEDBY] NVARCHAR (50) NOT NULL,
    [CREATEDAT] DATE          NULL,
    [UPDATEDBY] NVARCHAR (50) NULL,
    [UPDATEDAT] DATE          NULL,
    CONSTRAINT [PK_t_homedef] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UK_t_homedef_NAME] UNIQUE NONCLUSTERED ([NAME] ASC)
);
GO

/****** Object:  Table [dbo].[t_homemem]    Script Date: 2017-05-05 11:36:16 PM ******/
CREATE TABLE [dbo].[t_homemem] (
    [HID]       INT           NOT NULL,
    [USER]      NVARCHAR (50) NOT NULL,
    [DISPLAYAS] NVARCHAR (50) NULL,
    [RELT]      SMALLINT      NOT NULL,
    [CREATEDBY] NVARCHAR (50) NOT NULL,
    [CREATEDAT] DATE          NULL,
    [UPDATEDBY] NVARCHAR (50) NULL,
    [UPDATEDAT] DATE          NULL,
    CONSTRAINT [PK_t_homemem] PRIMARY KEY CLUSTERED ([HID] ASC, [USER] ASC),
--    CONSTRAINT [UK_t_homemem_USER] UNIQUE NONCLUSTERED ([HID] ASC, [USER] ASC),
    CONSTRAINT [FK_t_homemem_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_homemsg]    Script Date: 2017-09-07 12:33:22 PM ******/
CREATE TABLE [dbo].[t_homemsg] (
    [ID]       INT           IDENTITY (1, 1) NOT NULL,
    [HID]      INT           NOT NULL,
    [USERTO]   NVARCHAR (50) NOT NULL,
    [SENDDATE] DATE          CONSTRAINT [DF_t_homemsg_SENDDATE] DEFAULT (getdate()) NOT NULL,
    [USERFROM] NVARCHAR (50) NOT NULL,
    [TITLE]    NVARCHAR (20) NOT NULL,
    [CONTENT]  NVARCHAR (50) NULL,
    [READFLAG] BIT           CONSTRAINT [DF_t_homemsg_READFLAG] DEFAULT ((0)) NOT NULL,
    [SEND_DEL] BIT           CONSTRAINT [DF_t_homemsg_SEND_DEL] DEFAULT ((0)) NULL,
    [REV_DEL]  BIT           CONSTRAINT [DF_t_homemsg_REV_DEL] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_t_homemsg] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_t_homemsg_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_ctgy]    Script Date: 2017-05-05 11:36:16 PM ******/
CREATE TABLE [dbo].[t_learn_ctgy] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [PARID]     INT           NULL,
    [NAME]      NVARCHAR (45) NOT NULL,
    [COMMENT]   NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_learn_ctgy_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_learn_ctgy_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_learn_ctgy] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_learn_ctgy_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_fin_account_ctgy]    Script Date: 2016-10-27 3:31:27 PM ******/
-- Changes: [HID] added
CREATE TABLE [dbo].[t_fin_account_ctgy] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [NAME]      NVARCHAR (30) NOT NULL,
    [ASSETFLAG] BIT           CONSTRAINT [DF_t_fin_account_ctgy_ASSETFLAG] DEFAULT ((1)) NOT NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_account_ctgy_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_account_ctgy_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_account_ctgy] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_account_ctgy_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_account]    Script Date: 2016-10-27 3:31:27 PM ******/
-- Change: [HID] changed
CREATE TABLE [dbo].[t_fin_account] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NOT NULL,
    [CTGYID]    INT           NOT NULL,
    [NAME]      NVARCHAR (30) NOT NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [OWNER]     NVARCHAR (50) NULL,
    [STATUS]    TINYINT       NULL,
    [CREATEDBY] NVARCHAR (50) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_account_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (50) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_account_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_account] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_account_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_account_ext_dp]    Script Date: 2017-03-15 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_account_ext_dp] (
    [ACCOUNTID] INT            NOT NULL,
    [DIRECT]    BIT            NOT NULL,
    [STARTDATE] DATE           NOT NULL,
    [ENDDATE]   DATE           NOT NULL,
    [RPTTYPE]   TINYINT        NOT NULL,
    [REFDOCID]  INT            NOT NULL,
    [DEFRRDAYS] NVARCHAR (100) NULL,
    [COMMENT]   NVARCHAR (45)  NULL,
    CONSTRAINT [PK_t_fin_account_ext_dp] PRIMARY KEY CLUSTERED ([ACCOUNTID] ASC),
    CONSTRAINT [FK_t_fin_account_ext_dp_id] FOREIGN KEY ([ACCOUNTID]) REFERENCES [dbo].[t_fin_account] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_fin_controlcenter]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_controlcenter] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NOT NULL,
    [NAME]      NVARCHAR (30) NOT NULL,
    [PARID]     INT           NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [OWNER]     NVARCHAR (40) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_controlcenter_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_controlcenter_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_controlcenter] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_cc_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_fin_currency]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_currency] (
    [CURR]      NVARCHAR (5)  NOT NULL,
    [NAME]      NVARCHAR (45) NOT NULL,
    [SYMBOL]    NVARCHAR (30) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_currency_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_currency_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_currency] PRIMARY KEY CLUSTERED ([CURR] ASC)
);
GO


/****** Object:  Table [dbo].[t_fin_doc_type]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_doc_type] (
    [ID]        SMALLINT      IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [NAME]      NVARCHAR (30) NOT NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_doc_type_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_doc_type_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_doc_type] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_doctype_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_document]    Script Date: 2017-02-20 12:11:41 AM ******/
CREATE TABLE [dbo].[t_fin_document] (
    [ID]            INT             IDENTITY (1, 1) NOT NULL,
    [HID]           INT             NOT NULL,
    [DOCTYPE]       SMALLINT        NOT NULL,
    [TRANDATE]      DATE            CONSTRAINT [DF_t_fin_document_TRANDATE] DEFAULT (getdate()) NOT NULL,
    [TRANCURR]      NVARCHAR (5)    NOT NULL,
    [DESP]          NVARCHAR (45)   NOT NULL,
    [EXGRATE]       DECIMAL (17, 4) NULL,
    [EXGRATE_PLAN]  BIT             NULL,
    [EXGRATE_PLAN2] BIT             NULL,
    [TRANCURR2]     NVARCHAR (5)    NULL,
    [EXGRATE2]      DECIMAL (17, 4) NULL,
    [CREATEDBY]     NVARCHAR (40)   NULL,
    [CREATEDAT]     DATE            NULL,
    [UPDATEDBY]     NVARCHAR (40)   NULL,
    [UPDATEDAT]     DATE            NULL,
    CONSTRAINT [PK_t_fin_document] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_document_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_fin_document_item]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_document_item] (
    [DOCID]           INT             NOT NULL,
    [ITEMID]          INT             NOT NULL,
    [ACCOUNTID]       INT             NOT NULL,
    [TRANTYPE]        INT             NOT NULL,
    [TRANAMOUNT]      DECIMAL (17, 2) NOT NULL,
    [USECURR2]        BIT             NULL,
    [CONTROLCENTERID] INT             NULL,
    [ORDERID]         INT             NULL,
    [DESP]            NVARCHAR (45)   NULL,
    CONSTRAINT [PK_t_fin_document_item] PRIMARY KEY CLUSTERED ([DOCID] ASC, [ITEMID] ASC),
    CONSTRAINT [FK_t_fin_document_header] FOREIGN KEY ([DOCID]) REFERENCES [dbo].[t_fin_document] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_order]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_order] (
    [ID]         INT           IDENTITY (1, 1) NOT NULL,
    [HID]        INT           NOT NULL,
    [NAME]       NVARCHAR (30) NOT NULL,
    [VALID_FROM] DATE          NOT NULL,
    [VALID_TO]   DATE          NOT NULL,
    [COMMENT]    NVARCHAR (45) NULL,
    [CREATEDBY]  NVARCHAR (40) NULL,
    [CREATEDAT]  DATE          CONSTRAINT [DF_t_fin_order_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]  NVARCHAR (40) NULL,
    [UPDATEDAT]  DATE          CONSTRAINT [DF_t_fin_order_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_order] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_order_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_order_srule]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_order_srule] (
    [ORDID]           INT           NOT NULL,
    [RULEID]          INT           NOT NULL,
    [CONTROLCENTERID] INT           NOT NULL,
    [PRECENT]         INT           NOT NULL,
    [COMMENT]         NVARCHAR (45) NULL,
    CONSTRAINT [PK_t_fin_order_srule] PRIMARY KEY CLUSTERED ([ORDID] ASC, [RULEID] ASC),
    CONSTRAINT [FK_t_fin_order_srule_order] FOREIGN KEY ([ORDID]) REFERENCES [dbo].[t_fin_order] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_fin_tmpdoc_dp]    Script Date: 2017-03-10 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_tmpdoc_dp] (
    [DOCID]           INT             IDENTITY (1, 1) NOT NULL,
    [HID]             INT             NOT NULL,
    [REFDOCID]        INT             NULL,
    [ACCOUNTID]       INT             NOT NULL,
    [TRANDATE]        DATE            NOT NULL,
    [TRANTYPE]        INT             NOT NULL,
    [TRANAMOUNT]      DECIMAL (17, 2) NOT NULL,
    [CONTROLCENTERID] INT             NULL,
    [ORDERID]         INT             NULL,
    [DESP]            NVARCHAR (45)   NULL,
    [CREATEDBY]       NVARCHAR (40)   NULL,
    [CREATEDAT]       DATE            CONSTRAINT [DF_t_fin_tmpdoc_dp_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)   NULL,
    [UPDATEDAT]       DATE            CONSTRAINT [DF_t_fin_tmpdoc_dp_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_tmpdoc_dp] PRIMARY KEY CLUSTERED ([DOCID] ASC)
--    CONSTRAINT [FK_t_fin_tmpdocdp_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_tran_type]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_fin_tran_type] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [NAME]      NVARCHAR (30) NOT NULL,
    [EXPENSE]   BIT           NOT NULL,
    [PARID]     INT           NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_tran_type_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_fin_tran_type_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_tran_type] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_trantype_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_language]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_language] (
    [LCID]    INT            NOT NULL,
    [ISONAME] NVARCHAR (20)  NOT NULL,
    [ENNAME]  NVARCHAR (100) NOT NULL,
    [NAVNAME] NVARCHAR (100) NOT NULL,
    [APPFLAG] BIT            CONSTRAINT [DF_t_language_APPFLAG] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_t_language] PRIMARY KEY CLUSTERED ([LCID] ASC)
);
GO

/****** Object:  Table [dbo].[t_learn_obj]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_learn_obj] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [HID]       INT            NOT NULL,
    [CATEGORY]  INT            NULL,
    [NAME]      NVARCHAR (45)  NULL,
    [CONTENT]   NVARCHAR (MAX) NULL,
    [CREATEDBY] NVARCHAR (40)  NULL,
    [CREATEDAT] DATE           CONSTRAINT [DF_t_learn_obj_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40)  NULL,
    [UPDATEDAT] DATE           CONSTRAINT [DF_t_learn_obj_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_learn_obj] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_learn_obj_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_hist]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE TABLE [dbo].[t_learn_hist] (
    [HID]       INT           NOT NULL,
    [USERID]    NVARCHAR (40) NOT NULL,
    [OBJECTID]  INT           NOT NULL,
    [LEARNDATE] DATE          NOT NULL,
    [COMMENT]   NVARCHAR (45) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_learn_hist_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_learn_hist_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_learn_hist] PRIMARY KEY CLUSTERED ([HID] ASC, [USERID] ASC, [OBJECTID] ASC, [LEARNDATE] ASC)
--    CONSTRAINT [FK_t_learn_hist_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


-- Updated at 2017.10.06
/****** Object:  Table [dbo].[t_fin_asset_ctgy]    Script Date: 2017-10-06 12:18:56 PM ******/
CREATE TABLE [dbo].[t_fin_asset_ctgy] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [NAME]      NVARCHAR (50) NOT NULL,
    [DESP]      NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_fin_asset_ctgy_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          NULL,
    CONSTRAINT [PK_t_fin_asset_ctgy] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_asset_ctgy_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_fin_account_ext_as]    Script Date: 2017-10-06 10:58:24 PM ******/
CREATE TABLE [dbo].[t_fin_account_ext_as] (
    [ACCOUNTID]   INT            NOT NULL,
    [CTGYID]      INT            NOT NULL,
    [NAME]        NVARCHAR (50)  NOT NULL,
    [REFDOC_BUY]  INT            NOT NULL,
    [COMMENT]     NVARCHAR (100) NULL,
    [REFDOC_SOLD] INT            NULL,
    CONSTRAINT [PK_t_fin_account_ext_as] PRIMARY KEY CLUSTERED ([ACCOUNTID] ASC),
    CONSTRAINT [FK_t_fin_account_ext_as_ACNTID] FOREIGN KEY ([ACCOUNTID]) REFERENCES [dbo].[t_fin_account] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_t_fin_account_ext_as_CTGYID] FOREIGN KEY ([CTGYID]) REFERENCES [dbo].[t_fin_asset_ctgy] ([ID])
);
GO

-- Updated at 2017.10.15
/****** Object:  Table [dbo].[t_lib_person]    Script Date: 2017-10-15 7:00:47 PM ******/
CREATE TABLE [dbo].[t_lib_person] (
    [ID]              INT            IDENTITY (1, 1) NOT NULL,
    [HID]             INT            NOT NULL,
    [NativeName]      NVARCHAR (50)  NOT NULL,
    [EnglishName]     NVARCHAR (50)  NULL,
    [EnglishIsNative] BIT            NULL,
    [Gender]          TINYINT        NULL,
    [ShortIntro]      NVARCHAR (100) NULL,
    [ExtLink1]        NVARCHAR (100) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_person_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_person_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_lib_person] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_lib_person_t_lib_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_lib_book_ctgy]    Script Date: 2017-10-15 7:01:06 PM ******/
CREATE TABLE [dbo].[t_lib_book_ctgy] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [ParID]     INT           NULL,
    [Others]    NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_lib_book_ctgy_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_lib_book_ctgy_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_lib_book_ctgy] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_lib_book_ctgy_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_lib_book]    Script Date: 2017-10-15 7:00:13 PM ******/
CREATE TABLE [dbo].[t_lib_book] (
    [ID]              INT            IDENTITY (1, 1) NOT NULL,
    [HID]             INT            NOT NULL,
    [Ctgy]            INT            NULL,
    [NativeName]      NVARCHAR (50)  NOT NULL,
    [EnglishName]     NVARCHAR (50)  NULL,
    [EnglishIsNative] BIT            NULL,
    [ShortIntro]      NVARCHAR (100) NULL,
    [ExtLink1]        NVARCHAR (100) NULL,
    [CREATEDBY]       NVARCHAR (40)  NULL,
    [CREATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)  NULL,
    [UPDATEDAT]       DATE           CONSTRAINT [DF_t_lib_book_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_lib_book] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_t_lib_book_ctgy] FOREIGN KEY ([Ctgy]) REFERENCES [dbo].[t_lib_book_ctgy] ([ID])
--    CONSTRAINT [FK_t_lib_book_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_lib_location]    Script Date: 2017-10-15 7:01:29 PM ******/
CREATE TABLE [dbo].[t_lib_location] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NOT NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [IsDevice]  BIT           NULL,
    [Desp]      NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_lib_location_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_lib_location_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_lib_location] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_lib_location_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_lib_location_detail]    Script Date: 2017-10-15 6:20:34 PM ******/
CREATE TABLE [dbo].[t_lib_location_detail] (
    [LOCID]       INT           NOT NULL,
    [SEQNO]       INT           NOT NULL,
    [CONTENTTYPE] TINYINT       NOT NULL,
    [CONTENTID]   INT           NOT NULL,
    [Others]      NVARCHAR (50) NULL,
    CONSTRAINT [PK_t_lib_location_detail] PRIMARY KEY CLUSTERED ([LOCID] ASC, [SEQNO] ASC),
    CONSTRAINT [FK_t_lib_locationdet_LOCID] FOREIGN KEY ([LOCID]) REFERENCES [dbo].[t_lib_location] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


-- Updated at 2017.10.29
/****** Object:  Table [dbo].[t_learn_qtn_bank]    Script Date: 2017-10-29 12:17:24 AM ******/
CREATE TABLE [dbo].[t_learn_qtn_bank] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [HID]         INT            NOT NULL,
    [Type]        TINYINT        NOT NULL,
    [Question]    NVARCHAR (200) NOT NULL,
    [BriefAnswer] NVARCHAR (200) NULL,
    [CREATEDBY]   NVARCHAR (50)  NULL,
    [CREATEDAT]   DATE           CONSTRAINT [DF_t_learn_qtn_bank_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]   NVARCHAR (50)  NULL,
    [UPDATEDAT]   DATE           CONSTRAINT [DF_t_learn_qtn_bank_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_learn_qtn_bank] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_learn_qtn_bank_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_qtn_bank_sub]    Script Date: 2017-10-29 12:19:02 AM ******/
CREATE TABLE [dbo].[t_learn_qtn_bank_sub] (
    [QTNID]   INT            NOT NULL,
    [SUBITEM] NVARCHAR (20)  NOT NULL,
    [DETAIL]  NVARCHAR (200) NOT NULL,
    [OTHERS]  NVARCHAR (50)  NULL,
    CONSTRAINT [PK_t_learn_qtn_bank_sub] PRIMARY KEY CLUSTERED ([QTNID] ASC, [SUBITEM] ASC),
    CONSTRAINT [FK_t_learn_qtn_bank_sub_QTNID] FOREIGN KEY ([QTNID]) REFERENCES [dbo].[t_learn_qtn_bank] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_tag]    Script Date: 2017-10-29 12:19:22 AM ******/
CREATE TABLE [dbo].[t_tag] (
    [HID]      INT           NOT NULL,
    [TagType]  SMALLINT      NOT NULL,
    [TagID]    INT           NOT NULL,
    [Term]     NVARCHAR (50) NOT NULL,
    [TagSubID] INT           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_t_tag] PRIMARY KEY CLUSTERED ([HID] ASC, [TagType] ASC, [TagID] ASC, [Term] ASC, [TagSubID] ASC)
);
GO

-- Updated at 2017.11.6
/****** Object:  Table [dbo].[t_fin_account_ext_loan]    Script Date: 2017-11-06 3:57:42 PM ******/
CREATE TABLE [dbo].[t_fin_account_ext_loan] (
    [ACCOUNTID]     INT             NOT NULL,
    [STARTDATE]     DATETIME        NOT NULL,
    [ANNUALRATE]    DECIMAL (17, 2) NULL,
    [INTERESTFREE]  BIT             NULL,
    [REPAYMETHOD]   TINYINT         NULL,
    [TOTALMONTH]    SMALLINT        NULL,
    [REFDOCID]      INT             NOT NULL,
    [OTHERS]        NVARCHAR (100)  NULL,
    [ENDDATE]       DATE            DEFAULT (getdate()) NULL,
    [PAYINGACCOUNT] INT             NULL,
    [PARTNER]       NVARCHAR (50)   NULL,
    CONSTRAINT [PK_t_fin_account_ext_loan] PRIMARY KEY CLUSTERED ([ACCOUNTID] ASC),
    CONSTRAINT [FK_t_fin_account_ext_loan_ID] FOREIGN KEY ([ACCOUNTID]) REFERENCES [dbo].[t_fin_account] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
)
GO

/****** Object:  Table [dbo].[t_fin_tmpdoc_loan]    Script Date: 2017-11-06 5:47:05 PM ******/
CREATE TABLE [dbo].[t_fin_tmpdoc_loan] (
    [DOCID]           INT             IDENTITY (1, 1) NOT NULL,
    [HID]             INT             NOT NULL,
    [REFDOCID]        INT             NULL,
    [ACCOUNTID]       INT             NOT NULL,
    [TRANDATE]        DATE            NOT NULL,
    [TRANAMOUNT]      DECIMAL (17, 2) NOT NULL,
    [INTERESTAMOUNT]  DECIMAL (17, 2) NULL,
    [CONTROLCENTERID] INT             NULL,
    [ORDERID]         INT             NULL,
    [DESP]            NVARCHAR (45)   NULL,
    [CREATEDBY]       NVARCHAR (40)   NULL,
    [CREATEDAT]       DATE            CONSTRAINT [DF_t_fin_tmpdoc_loan_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]       NVARCHAR (40)   NULL,
    [UPDATEDAT]       DATE            CONSTRAINT [DF_t_fin_tmpdoc_loan_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_fin_tmpdoc_loan] PRIMARY KEY CLUSTERED ([DOCID] ASC)
--    CONSTRAINT [FK_t_fin_tmpdocloan_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Updated at 2017.11.13
/****** Object:  Table [dbo].[t_learn_enword]    Script Date: 2017-11-13 1:18:50 PM ******/
CREATE TABLE [dbo].[t_learn_enword] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [HID]        INT            NOT NULL,
    [Word]       NVARCHAR (100) NOT NULL,
    [UKPron]     NVARCHAR (50)  NULL,
    [USPron]     NVARCHAR (50)  NULL,
    [UKPronFile] NVARCHAR (100) NULL,
    [USPronFile] NVARCHAR (100) NULL,
    [CREATEDBY]  NVARCHAR (40)  NULL,
    [CREATEDAT]  DATE           CONSTRAINT [DF_t_learn_enword_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY]  NVARCHAR (50)  NULL,
    [UPDATEDAT]  DATE           CONSTRAINT [DF_t_learn_enword_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [UK_Table_learn_word] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_Table_learn_word_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_enwordexp]    Script Date: 2017-11-13 1:21:54 PM ******/
CREATE TABLE [dbo].[t_learn_enwordexp] (
    [WORDID]    INT            NOT NULL,
    [ExpID]     SMALLINT       NOT NULL,
    [POSAbb]    NVARCHAR (10)  NULL,
    [LangKey]   NVARCHAR (10)  NOT NULL,
    [ExpDetail] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_t_learn_enwordexp] PRIMARY KEY CLUSTERED ([WORDID] ASC, [ExpID] ASC),
    CONSTRAINT [FK_Table_learn_wordexp_word] FOREIGN KEY ([WORDID]) REFERENCES [dbo].[t_learn_enword] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_ensent]    Script Date: 2017-11-13 2:22:19 PM ******/
CREATE TABLE [dbo].[t_learn_ensent] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [HID]       INT            NOT NULL,
    [Sentence]  NVARCHAR (100) NOT NULL,
    [CREATEDBY] NVARCHAR (50)  NULL,
    [CREATEDAT] DATE           CONSTRAINT [DF_t_learn_ensent_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (50)  NULL,
    [UPDATEDAT] DATE           CONSTRAINT [DF_t_learn_ensent_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_learn_ensent] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_Table_learn_ensent_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_ensentexp]    Script Date: 2017-11-13 2:22:39 PM ******/
CREATE TABLE [dbo].[t_learn_ensentexp] (
    [SentID]    INT            NOT NULL,
    [ExpID]     SMALLINT       NOT NULL,
    [LangKey]   NVARCHAR (10)  NOT NULL,
    [ExpDetail] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_t_learn_ensentexp] PRIMARY KEY CLUSTERED ([SentID] ASC, [ExpID] ASC),
    CONSTRAINT [FK_Table_ensentexp_SentID] FOREIGN KEY ([SentID]) REFERENCES [dbo].[t_learn_ensent] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_learn_ensent_word]    Script Date: 2017-11-13 2:22:56 PM ******/
CREATE TABLE [dbo].[t_learn_ensent_word] (
    [SentID] INT NOT NULL,
    [WordID] INT NOT NULL,
    CONSTRAINT [PK_t_learn_ensent_word] PRIMARY KEY CLUSTERED ([SentID] ASC, [WordID] ASC),
    CONSTRAINT [FK_t_learn_ensent_word_sent] FOREIGN KEY ([SentID]) REFERENCES [dbo].[t_learn_ensent] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Updated at 2018.1.7
/****** Object:  Table [dbo].[t_event]    Script Date: 2017-05-04 5:54:17 PM ******/
CREATE TABLE [dbo].[t_event] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [HID]          INT            NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [StartTime]    DATETIME       CONSTRAINT [DF_t_event_StartTime] DEFAULT (getdate()) NOT NULL,
    [EndTime]      DATETIME       NULL,
    [CompleteTime] DATETIME       NULL,
    [Content]      NVARCHAR (MAX) NULL,
    [IsPublic]     BIT            CONSTRAINT [DF_t_event_IsPublic] DEFAULT ((1)) NOT NULL,
    [Assignee]     NVARCHAR (40)  NULL,
    [RefRecurID]   INT            NULL,
    [CREATEDBY]    NVARCHAR (40)  NULL,
    [CREATEDAT]    DATE           NULL,
    [UPDATEDBY]    NVARCHAR (40)  NULL,
    [UPDATEDAT]    DATE           NULL,
    CONSTRAINT [PK_t_event] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UX_t_event_name] UNIQUE NONCLUSTERED ([HID] ASC, [Name] ASC)
--    CONSTRAINT [FK_t_event_t_hid] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Updatged at 2018.1.18
/****** Object:  Table [dbo].[t_event_recur]    Script Date: 2018-01-18 2:52:44 PM ******/
CREATE TABLE [dbo].[t_event_recur] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [HID]       INT            NOT NULL,
    [STARTDATE] DATE           NOT NULL,
    [ENDDATE]   DATE           NOT NULL,
    [RPTTYPE]   TINYINT        NOT NULL,
    [NAME]      NVARCHAR (50)  NOT NULL,
    [CONTENT]   NVARCHAR (MAX) NULL,
    [ISPUBLIC]  BIT            CONSTRAINT [DF_t_event_recur_ISPUBLIC] DEFAULT ((0)) NULL,
    [ASSIGNEE]  NVARCHAR (40)  NULL,
    [CREATEDBY] NVARCHAR (40)  NULL,
    [CREATEDAT] DATE           NULL,
    [UPDATEDBY] NVARCHAR (40)  NULL,
    [UPDATEDAT] DATE           NULL,
    CONSTRAINT [PK_t_event_recur] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_event_recur_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Updated at 2018.3.13
/****** Object:  Table [dbo].[t_event_habit]    Script Date: 2018-03-13 2:00:44 PM ******/
CREATE TABLE [dbo].[t_event_habit] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [HID]       INT            NOT NULL,
    [Name]      NVARCHAR (50)  NOT NULL,
    [StartDate] DATE           NOT NULL,
    [EndDate]   DATE           NOT NULL,
    [RPTTYPE]   TINYINT        NOT NULL,
    [IsPublic]  BIT            NOT NULL,
    [CONTENT]   NVARCHAR (MAX) NULL,
    [Count]     INT            NOT NULL,
    [Assignee]  NVARCHAR (40)  NULL,
    [CREATEDBY] NVARCHAR (40)  NULL,
    [CREATEDAT] DATE           NULL,
    [UPDATEDBY] NVARCHAR (40)  NULL,
    [UPDATEDAT] DATE           NULL,
    CONSTRAINT [PK_t_event_habit] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_event_habit_t_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_event_habit_detail]    Script Date: 2018-03-13 2:01:27 PM ******/
CREATE TABLE [dbo].[t_event_habit_detail] (
    [ID]        INT  IDENTITY (1, 1) NOT NULL,
    [HabitID]   INT  NOT NULL,
    [StartDate] DATE NOT NULL,
    [EndDate]   DATE NOT NULL,
    CONSTRAINT [PK_t_event_habit_detail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_t_event_habit_HabitID] FOREIGN KEY ([HabitID]) REFERENCES [dbo].[t_event_habit] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/****** Object:  Table [dbo].[t_event_habit_checkin]    Script Date: 2018-03-13 2:01:43 PM ******/
CREATE TABLE [dbo].[t_event_habit_checkin] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [TranDate]  DATE          NOT NULL,
    [HabitID]   INT           NOT NULL,
    [Score]     INT           NULL,
    [Comment]   NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          NULL,
    CONSTRAINT [PK_t_event_habit_checkin] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_t_event_habit_checkin_HabitID] FOREIGN KEY ([HabitID]) REFERENCES [dbo].[t_event_habit] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Updated at 2018.6.6
/****** Object:  Table [dbo].[t_lib_movie_genre]    Script Date: 2018-06-06 7:01:06 PM ******/
CREATE TABLE [dbo].[t_lib_movie_genre] (
    [ID]        INT           IDENTITY (1, 1) NOT NULL,
    [HID]       INT           NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [ParID]     INT           NULL,
    [Others]    NVARCHAR (50) NULL,
    [CREATEDBY] NVARCHAR (40) NULL,
    [CREATEDAT] DATE          CONSTRAINT [DF_t_lib_movie_genre_CREATEDAT] DEFAULT (getdate()) NULL,
    [UPDATEDBY] NVARCHAR (40) NULL,
    [UPDATEDAT] DATE          CONSTRAINT [DF_t_lib_movie_genre_UPDATEDAT] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_t_lib_movie_genre] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_lib_movie_genre_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


/****** Object:  Table [dbo].[t_dbversion]    Script Date: 2018-07-03 8:09:20 PM ******/
CREATE TABLE [dbo].[t_dbversion] (
    [VersionID]    INT      NOT NULL,
    [ReleasedDate] DATETIME NOT NULL,
    [AppliedDate]  DATETIME CONSTRAINT [DF_t_dbversion_AppliedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_t_dbversion] PRIMARY KEY CLUSTERED ([VersionID] ASC)
);
GO

--------------------------------------------------------------------------------------------------------
-- Updated at 2018.8.10
/****** Object:  Table [dbo].[t_fin_account_ext_loan_h]    Script Date: 8/10/2018 9:51:53 PM ******/
CREATE TABLE [dbo].[t_fin_account_ext_loan_h] (
    [ACCOUNTID]     INT             NOT NULL,
    [STARTDATE]     DATETIME        NOT NULL,
    [ANNUALRATE]    DECIMAL (17, 2) NULL,
    [INTERESTFREE]  BIT             NULL,
    [REPAYMETHOD]   TINYINT         NULL,
    [TOTALMONTH]    SMALLINT        NULL,
    [REFDOCID]      INT             NOT NULL,
    [OTHERS]        NVARCHAR (100)  NULL,
    [EndDate]       DATE            DEFAULT (getdate()) NULL,
    [PAYINGACCOUNT] INT             NULL,
    [PARTNER]       NVARCHAR (50)   NULL,
    CONSTRAINT [PK_t_fin_account_ext_loan_h] PRIMARY KEY CLUSTERED ([ACCOUNTID] ASC),
    CONSTRAINT [FK_t_fin_account_ext_loan_h_ID] FOREIGN KEY ([ACCOUNTID]) REFERENCES [dbo].[t_fin_account] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Credit card
CREATE TABLE [dbo].[t_fin_account_ext_cc] (
    [ACCOUNTID]       INT            NOT NULL,
    [BILLDAYINMONTH]  SMALLINT       NOT NULL,
    [REPAYDAYINMONTH] SMALLINT       NOT NULL,
    [CARDNUM]         NVARCHAR (20)  NOT NULL,
    [OTHERS]          NVARCHAR (100) NULL,
    [BANK]            NVARCHAR (50)  NULL,
    [VALIDDATE]       DATETIME       NULL,
    CONSTRAINT [PK_t_fin_account_ext_cc] PRIMARY KEY CLUSTERED ([ACCOUNTID] ASC),
    CONSTRAINT [FK_t_fin_account_ext_cc_ID] FOREIGN KEY ([ACCOUNTID]) REFERENCES [dbo].[t_fin_account] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

---------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2018.12.18

-- Table: t_fin_plan
/****** Object:  Table [dbo].[t_fin_plan]    Script Date: 2018-12-18 12:04:46 PM ******/
CREATE TABLE [dbo].[t_fin_plan] (
    [ID]         INT             IDENTITY (1, 1) NOT NULL,
    [HID]        INT             NOT NULL,
    [PTYPE]      TINYINT         CONSTRAINT [DF_t_fin_plan_PTYPE] DEFAULT ((0)) NOT NULL,
    [ACCOUNTID]  INT             NULL,
    [ACNTCTGYID] INT             NULL,
    [CCID]       INT             NULL,
    [TTID]       INT             NULL,
    [STARTDATE]  DATE            NOT NULL,
    [TGTDATE]    DATE            NOT NULL,
    [TGTBAL]     DECIMAL (17, 2) NOT NULL,
    [TRANCURR]   NVARCHAR (5)    NOT NULL,
    [DESP]       NVARCHAR (50)   NOT NULL,
    [CREATEDBY]  NVARCHAR (50)   NULL,
    [CREATEDAT]  DATE            NULL,
    [UPDATEDBY]  NVARCHAR (50)   NULL,
    [UPDATEDAT]  DATE            NULL,
    CONSTRAINT [PK_t_fin_plan] PRIMARY KEY CLUSTERED ([ID] ASC)
--    CONSTRAINT [FK_t_fin_plan_HID] FOREIGN KEY ([HID]) REFERENCES [dbo].[t_homedef] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO


---------------------------------
-- TODO...













































--/****** Object:  Table [dbo].[t_learn_award]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_learn_award](
--	[ID] [int] IDENTITY(1,1) NOT NULL,
--	[USERID] [nvarchar](40) NOT NULL,
--	[ADATE] [date] NOT NULL,
--	[SCORE] [int] NOT NULL,
--	[REASON] [nvarchar](40) NOT NULL,
--	[CREATEDBY] [nvarchar](40) NULL,
--	[CREATEDAT] [date] NULL,
--	[UPDATEDBY] [nvarchar](40) NULL,
--	[UPDATEDAT] [date] NULL,
-- CONSTRAINT [PK_t_learn_award] PRIMARY KEY CLUSTERED 
--(
--	[ID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO

--/****** Object:  Table [dbo].[t_learn_plan]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_learn_plan](
--	[ID] [int] IDENTITY(1,1) NOT NULL,
--	[NAME] [nvarchar](45) NOT NULL,
--	[COMMENT] [nvarchar](45) NULL,
--	[CREATEDBY] [nvarchar](40) NULL,
--	[CREATEDAT] [date] NULL,
--	[UPDATEDBY] [nvarchar](40) NULL,
--	[UPDATEDAT] [date] NULL,
-- CONSTRAINT [PK_t_learn_plan] PRIMARY KEY CLUSTERED 
--(
--	[ID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO
--/****** Object:  Table [dbo].[t_learn_plandtl]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_learn_plandtl](
--	[ID] [int] NOT NULL,
--	[OBJECTID] [int] NOT NULL,
--	[DEFERREDDAY] [int] NOT NULL,
--	[RECURTYPE] [tinyint] NOT NULL,
--	[COMMENT] [nvarchar](45) NULL,
-- CONSTRAINT [PK_t_learn_plandtl] PRIMARY KEY CLUSTERED 
--(
--	[ID] ASC,
--	[OBJECTID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO
--/****** Object:  Table [dbo].[t_learn_planpat]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_learn_planpat](
--	[ID] [int] NOT NULL,
--	[USERID] [nvarchar](40) NOT NULL,
--	[STARTDATE] [date] NOT NULL,
--	[STATUS] [tinyint] NULL,
--	[COMMENT] [nvarchar](45) NULL,
-- CONSTRAINT [PK_t_learn_planpat] PRIMARY KEY CLUSTERED 
--(
--	[ID] ASC,
--	[USERID] ASC,
--	[STARTDATE] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO
--/****** Object:  Table [dbo].[t_learn_recurtypedates]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_learn_recurtypedates](
--	[ID] [smallint] NOT NULL,
--	[DEFDAYS] [int] NOT NULL,
-- CONSTRAINT [PK_t_learn_recurtypedates] PRIMARY KEY CLUSTERED 
--(
--	[ID] ASC,
--	[DEFDAYS] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO


--/****** Object:  Index [IUX_t_tag_NAME]    Script Date: 2016-10-27 3:31:27 PM ******/
--CREATE UNIQUE NONCLUSTERED INDEX [IUX_t_tag_NAME] ON [dbo].[t_tag]
--(
--	[NAME] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--GO
--ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
--GO
--ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
--GO
--ALTER TABLE [dbo].[t_learn_plan] ADD  CONSTRAINT [DF_t_learn_plan_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
--GO
--ALTER TABLE [dbo].[t_learn_plan] ADD  CONSTRAINT [DF_t_learn_plan_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
--GO
--ALTER TABLE [dbo].[t_learn_plandtl] ADD  CONSTRAINT [DF_t_learn_plandtl_DEFERREDDAY]  DEFAULT ((0)) FOR [DEFERREDDAY]
--GO
--ALTER TABLE [dbo].[t_learn_plandtl] ADD  CONSTRAINT [DF_t_learn_plandtl_RECURTYPE]  DEFAULT ((0)) FOR [RECURTYPE]
--GO
--ALTER TABLE [dbo].[t_learn_planpat] ADD  CONSTRAINT [DF_t_learn_planpat_STARTDATE]  DEFAULT (getdate()) FOR [STARTDATE]
--GO
--ALTER TABLE [dbo].[t_learn_plandtl]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_plandtl_plan] FOREIGN KEY([ID])
--REFERENCES [dbo].[t_learn_plan] ([ID])
--ON UPDATE CASCADE
--ON DELETE CASCADE
--GO
--ALTER TABLE [dbo].[t_learn_plandtl] CHECK CONSTRAINT [FK_t_learn_plandtl_plan]
--GO
--ALTER TABLE [dbo].[t_learn_plandtl]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_plandtl_t_learn_plandtl] FOREIGN KEY([ID], [OBJECTID])
--REFERENCES [dbo].[t_learn_plandtl] ([ID], [OBJECTID])
--GO
--ALTER TABLE [dbo].[t_learn_plandtl] CHECK CONSTRAINT [FK_t_learn_plandtl_t_learn_plandtl]
--GO
--ALTER TABLE [dbo].[t_learn_planpat]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_planpat_plan] FOREIGN KEY([ID])
--REFERENCES [dbo].[t_learn_plan] ([ID])
--ON UPDATE CASCADE
--ON DELETE CASCADE
--GO
--ALTER TABLE [dbo].[t_learn_planpat] CHECK CONSTRAINT [FK_t_learn_planpat_plan]
--GO

--/****** Object:  Table [dbo].[TodoItem]    Script Date: 7/19/2016 11:50:38 AM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[TodoItem](
--	[ToDoID] [int] NOT NULL,
--	[ItemName] [nvarchar](50) NOT NULL,
--	[Priority] [int] NOT NULL,
--	[Assignee] [nvarchar](50) NULL,
--	[Dependence] [nvarchar](50) NULL,
--	[StartDate] [datetime] NULL,
--	[EndDate] [datetime] NULL,
--	[ItemContent] [nvarchar](max) NULL,
--	[Tags] [nvarchar](50) NULL,
-- CONSTRAINT [PK_TodoItem] PRIMARY KEY CLUSTERED 
--(
--	[ToDoID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
--)

--GO



