/*
 * Database Tables
 *
 */

/****** Object:  Table [dbo].[t_homedef]    Script Date: 2017-05-05 11:36:16 PM ******/
-- New table
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_homedef](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](50) NOT NULL,
	[DETAILS] [nvarchar](50) NULL,
	[HOST] [nvarchar](50) NOT NULL,
	[BASECURR] [nvarchar](5) NOT NULL,
	[CREATEDBY] [nvarchar](50) NOT NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_homedef] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_t_homedef_NAME] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[t_homemem]    Script Date: 2017-05-05 11:36:16 PM ******/
-- New table
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_homemem](
	[HID] [int] NOT NULL,
	[USER] [nvarchar](50) NOT NULL,
	[DISPLAYAS] [nvarchar](50) NULL,
	[RELT] [smallint] NOT NULL,
	[CREATEDBY] [nvarchar](50) NOT NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_homemem] PRIMARY KEY CLUSTERED 
(
	[HID] ASC,
	[USER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_t_homemem_USER] UNIQUE NONCLUSTERED 
(
	[HID] ASC,
	[USER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_homemem]  WITH CHECK ADD  CONSTRAINT [FK_t_homemem_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_homemem] CHECK CONSTRAINT [FK_t_homemem_HID]
GO

/****** Object:  Table [dbo].[t_homemsg]    Script Date: 2017-09-07 12:33:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_homemsg](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[USERTO] [nvarchar](50) NOT NULL,
	[SENDDATE] [date] NOT NULL,
	[USERFROM] [nvarchar](50) NOT NULL,
	[TITLE] [nvarchar](20) NOT NULL,
	[CONTENT] [nvarchar](50) NULL,
	[READFLAG] [bit] NOT NULL,
 CONSTRAINT [PK_t_homemsg] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_homemsg] ADD  CONSTRAINT [DF_t_homemsg_SENDDATE]  DEFAULT (getdate()) FOR [SENDDATE]
GO

ALTER TABLE [dbo].[t_homemsg] ADD  CONSTRAINT [DF_t_homemsg_READFLAG]  DEFAULT ((0)) FOR [READFLAG]
GO

ALTER TABLE [dbo].[t_homemsg]  WITH CHECK ADD  CONSTRAINT [FK_t_homemsg_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_homemsg] CHECK CONSTRAINT [FK_t_homemsg_HID]
GO

/****** Object:  Table [dbo].[t_learn_ctgy]    Script Date: 2017-05-05 11:36:16 PM ******/
-- Change: [HID] Added
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[PARID] [int] NULL,
	[NAME] [nvarchar](45) NOT NULL,
	[COMMENT] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_ctgy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[t_learn_ctgy]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_ctgy_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_learn_ctgy] CHECK CONSTRAINT [FK_t_learn_ctgy_HID]
GO

ALTER TABLE [dbo].[t_learn_ctgy] ADD  CONSTRAINT [DF_t_learn_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_ctgy] ADD  CONSTRAINT [DF_t_learn_ctgy_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO


/****** Object:  Table [dbo].[t_fin_account_ctgy]    Script Date: 2016-10-27 3:31:27 PM ******/
-- Changes: [HID] added
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[ASSETFLAG] [bit] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_account_ctgy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[t_fin_account_ctgy]  WITH CHECK ADD  CONSTRAINT [FK_t_account_ctgy_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] CHECK CONSTRAINT [FK_t_account_ctgy_HID]
GO

ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_ASSETFLAG]  DEFAULT ((1)) FOR [ASSETFLAG]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_fin_account]    Script Date: 2016-10-27 3:31:27 PM ******/
-- Change: [HID] changed
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[CTGYID] [int] NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[OWNER] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](50) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_account] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_account]  WITH CHECK ADD  CONSTRAINT [FK_t_account_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_account] CHECK CONSTRAINT [FK_t_account_HID]
GO

--ALTER TABLE [dbo].[t_fin_account]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ctgy] FOREIGN KEY([CTGYID])
--REFERENCES [dbo].[t_fin_account_ctgy] ([ID])
--ON UPDATE CASCADE
--ON DELETE CASCADE
--GO
--ALTER TABLE [dbo].[t_fin_account] CHECK CONSTRAINT [FK_t_fin_account_ctgy]
--GO

ALTER TABLE [dbo].[t_fin_account] ADD  CONSTRAINT [DF_t_fin_account_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_account] ADD  CONSTRAINT [DF_t_fin_account_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_fin_account_ext_dp]    Script Date: 2017-03-15 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account_ext_dp](
	[ACCOUNTID] [int] NOT NULL,
	[DIRECT] [bit] NOT NULL,
	[STARTDATE] [date] NOT NULL,
	[ENDDATE] [date] NOT NULL,
	[RPTTYPE] [tinyint] NOT NULL,
	[REFDOCID] [int] NOT NULL,
	[DEFRRDAYS] [nvarchar](100) NULL,
	[COMMENT] [nvarchar](45) NULL,
 CONSTRAINT [PK_t_fin_account_ext_dp] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_account_ext_dp]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_dp_id] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_account_ext_dp] CHECK CONSTRAINT [FK_t_fin_account_ext_dp_id]
GO


/****** Object:  Table [dbo].[t_fin_controlcenter]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_controlcenter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[PARID] [int] NULL,
	[COMMENT] [nvarchar](45) NULL,
	[OWNER] [nvarchar](40) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_controlcenter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_controlcenter]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_cc_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_controlcenter] CHECK CONSTRAINT [FK_t_fin_cc_HID]
GO

ALTER TABLE [dbo].[t_fin_controlcenter] ADD  CONSTRAINT [DF_t_fin_controlcenter_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_controlcenter] ADD  CONSTRAINT [DF_t_fin_controlcenter_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO


/****** Object:  Table [dbo].[t_fin_currency]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_currency](
	[CURR] [nvarchar](5) NOT NULL,
	[NAME] [nvarchar](45) NOT NULL,
	[SYMBOL] [nvarchar](30) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_currency] PRIMARY KEY CLUSTERED 
(
	[CURR] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_currency] ADD  CONSTRAINT [DF_t_fin_currency_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_currency] ADD  CONSTRAINT [DF_t_fin_currency_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO


/****** Object:  Table [dbo].[t_fin_doc_type]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_doc_type](
	[ID] [smallint] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_doc_type] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_doc_type]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_doctype_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_doc_type] CHECK CONSTRAINT [FK_t_fin_doctype_HID]
GO

ALTER TABLE [dbo].[t_fin_doc_type] ADD  CONSTRAINT [DF_t_fin_doc_type_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_doc_type] ADD  CONSTRAINT [DF_t_fin_doc_type_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_fin_document]    Script Date: 2017-02-20 12:11:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_document](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[DOCTYPE] [smallint] NOT NULL,
	[TRANDATE] [date] NOT NULL,
	[TRANCURR] [nvarchar](5) NOT NULL,
	[DESP] [nvarchar](45) NOT NULL,
	[EXGRATE] [decimal](17, 4) NULL, --[tinyint] NULL, Changed since 2017.09.27
	[EXGRATE_PLAN] [bit] NULL, --[tinyint] NULL, Changed since 2017.09.27
	[EXGRATE_PLAN2] [bit] NULL, --[tinyint] NULL, Changed since 2017.09.27
	[TRANCURR2] [nvarchar](5) NULL,
	[EXGRATE2] [decimal](17, 4) NULL, --[tinyint] NULL, Changed since 2017.09.27
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_document] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_document]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_document_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_document] CHECK CONSTRAINT [FK_t_fin_document_HID]
GO

ALTER TABLE [dbo].[t_fin_document] ADD  CONSTRAINT [DF_t_fin_document_TRANDATE]  DEFAULT (getdate()) FOR [TRANDATE]
GO


/****** Object:  Table [dbo].[t_fin_document_item]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_document_item](
	[DOCID] [int] NOT NULL,
	[ITEMID] [int] NOT NULL,
	[ACCOUNTID] [int] NOT NULL,
	[TRANTYPE] [int] NOT NULL,
	[TRANAMOUNT] [decimal](17, 2) NOT NULL,
	[USECURR2] [bit] NULL, -- [tinyint] NULL, Changed since 2017.09.27
	[CONTROLCENTERID] [int] NULL,
	[ORDERID] [int] NULL,
	[DESP] [nvarchar](45) NULL,
 CONSTRAINT [PK_t_fin_document_item] PRIMARY KEY CLUSTERED 
(
	[DOCID] ASC,
	[ITEMID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_document_item]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_document_header] FOREIGN KEY([DOCID])
REFERENCES [dbo].[t_fin_document] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_document_item] CHECK CONSTRAINT [FK_t_fin_document_header]
GO

/****** Object:  Table [dbo].[t_fin_exrate]    Script Date: 2016-10-27 3:31:27 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_fin_exrate](
--	[HID] [int] NOT NULL,
--	[TRANDATE] [date] NOT NULL,
--	[CURR] [nvarchar](5) NOT NULL,
--	[RATE] [decimal](17, 4) NOT NULL,
--	[REFDOCID] [int] NULL,
--	[CREATEDBY] [nvarchar](40) NULL,
--	[CREATEDAT] [date] NULL,
--	[UPDATEDBY] [nvarchar](40) NULL,
--	[UPDATEDAT] [date] NULL,
-- CONSTRAINT [PK_t_fin_exrate] PRIMARY KEY CLUSTERED 
--(
--	[HID] ASC,
--	[TRANDATE] ASC,
--	[CURR] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO

--ALTER TABLE [dbo].[t_fin_exrate]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_exrate_HID] FOREIGN KEY([HID])
--REFERENCES [dbo].[t_homedef] ([ID])
--ON UPDATE CASCADE
--ON DELETE CASCADE
--GO
--ALTER TABLE [dbo].[t_fin_exrate] CHECK CONSTRAINT [FK_t_fin_exrate_HID]
--GO

--ALTER TABLE [dbo].[t_fin_exrate] ADD  CONSTRAINT [DF_t_fin_exrate_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
--GO
--ALTER TABLE [dbo].[t_fin_exrate] ADD  CONSTRAINT [DF_t_fin_exrate_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
--GO

/****** Object:  Table [dbo].[t_fin_order]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_order](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[VALID_FROM] [date] NOT NULL,
	[VALID_TO] [date] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_order] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_order]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_order_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_order] CHECK CONSTRAINT [FK_t_fin_order_HID]
GO

ALTER TABLE [dbo].[t_fin_order] ADD  CONSTRAINT [DF_t_fin_order_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_order] ADD  CONSTRAINT [DF_t_fin_order_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_fin_order_srule]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_order_srule](
	[ORDID] [int] NOT NULL,
	[RULEID] [int] NOT NULL,
	[CONTROLCENTERID] [int] NOT NULL,
	[PRECENT] [int] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
 CONSTRAINT [PK_t_fin_order_srule] PRIMARY KEY CLUSTERED 
(
	[ORDID] ASC,
	[RULEID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_order_srule]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_order_srule_order] FOREIGN KEY([ORDID])
REFERENCES [dbo].[t_fin_order] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_order_srule] CHECK CONSTRAINT [FK_t_fin_order_srule_order]
GO

/****** Object:  Table [dbo].[t_fin_setting]    Script Date: 2016-10-27 3:31:27 PM ******/
-- Not need anymore, merge into HOMEDEF, 
-- updated at 2017.9.10

--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--CREATE TABLE [dbo].[t_fin_setting](
--	[HID] [int] NOT NULL,
--	[SETID] [nvarchar](20) NOT NULL,
--	[SETVALUE] [nvarchar](80) NOT NULL,
--	[COMMENT] [nvarchar](45) NULL,
--	[CREATEDBY] [nvarchar](40) NULL,
--	[CREATEDAT] [date] NULL,
--	[UPDATEDBY] [nvarchar](40) NULL,
--	[UPDATEDAT] [date] NULL,
-- CONSTRAINT [PK_t_fin_setting] PRIMARY KEY CLUSTERED 
--(
--	[HID] ASC,
--	[SETID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
--) ON [PRIMARY]

--GO

--ALTER TABLE [dbo].[t_fin_setting]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_setting_HID] FOREIGN KEY([HID])
--REFERENCES [dbo].[t_homedef] ([ID])
--ON UPDATE CASCADE
--ON DELETE CASCADE
--GO
--ALTER TABLE [dbo].[t_fin_setting] CHECK CONSTRAINT [FK_t_fin_setting_HID]
--GO

--ALTER TABLE [dbo].[t_fin_setting] ADD  CONSTRAINT [DF_t_fin_setting_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
--GO
--ALTER TABLE [dbo].[t_fin_setting] ADD  CONSTRAINT [DF_t_fin_setting_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
--GO


/****** Object:  Table [dbo].[t_fin_tmpdoc_dp]    Script Date: 2017-03-10 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_tmpdoc_dp](
	[DOCID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[REFDOCID] [int] NULL,
	[ACCOUNTID] [int] NOT NULL,
	[TRANDATE] [date] NOT NULL,
	[TRANTYPE] [int] NOT NULL,
	[TRANAMOUNT] [decimal](17, 2) NOT NULL,
	[CONTROLCENTERID] [int] NULL,
	[ORDERID] [int] NULL,
	[DESP] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_tmpdoc_dp] PRIMARY KEY CLUSTERED 
(
	[DOCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_tmpdoc_dp]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_tmpdocdp_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_tmpdoc_dp] CHECK CONSTRAINT [FK_t_fin_tmpdocdp_HID]
GO

ALTER TABLE [dbo].[t_fin_tmpdoc_dp] ADD  CONSTRAINT [DF_t_fin_tmpdoc_dp_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tmpdoc_dp] ADD  CONSTRAINT [DF_t_fin_tmpdoc_dp_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_fin_tran_type]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_tran_type](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[EXPENSE] [bit] NOT NULL,
	[PARID] [int] NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_tran_type] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_fin_tran_type]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_trantype_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_tran_type] CHECK CONSTRAINT [FK_t_fin_trantype_HID]
GO

ALTER TABLE [dbo].[t_fin_tran_type] ADD  CONSTRAINT [DF_t_fin_tran_type_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tran_type] ADD  CONSTRAINT [DF_t_fin_tran_type_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_language]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_language](
	[LCID] [int] NOT NULL,
	[ISONAME] [nvarchar](20) NOT NULL,
	[ENNAME] [nvarchar](100) NOT NULL,
	[NAVNAME] [nvarchar](100) NOT NULL,
	[APPFLAG] [bit] NULL,
 CONSTRAINT [PK_t_language] PRIMARY KEY CLUSTERED 
(
	[LCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_language] ADD  CONSTRAINT [DF_t_language_APPFLAG]  DEFAULT ((0)) FOR [APPFLAG]
GO

/****** Object:  Table [dbo].[t_learn_obj]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_obj](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[CATEGORY] [int] NULL,
	[NAME] [nvarchar](45) NULL,
	[CONTENT] [nvarchar](max) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_obj] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[t_learn_obj]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_obj_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_learn_obj] CHECK CONSTRAINT [FK_t_learn_obj_HID]
GO

ALTER TABLE [dbo].[t_learn_obj] ADD  CONSTRAINT [DF_t_learn_obj_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_obj] ADD  CONSTRAINT [DF_t_learn_obj_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

/****** Object:  Table [dbo].[t_learn_hist]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_hist](
	[HID] [int] NOT NULL,
	[USERID] [nvarchar](40) NOT NULL,
	[OBJECTID] [int] NOT NULL,
	[LEARNDATE] [date] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_hist] PRIMARY KEY CLUSTERED 
(
	[HID] ASC,
	[USERID] ASC,
	[OBJECTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t_learn_hist]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_hist_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_learn_hist] CHECK CONSTRAINT [FK_t_learn_hist_HID]
GO

ALTER TABLE [dbo].[t_learn_hist] ADD  CONSTRAINT [DF_t_learn_hist_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_hist] ADD  CONSTRAINT [DF_t_learn_hist_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

-- Updated at 2017.9.27

-- Updatae tcolumn in t_fin_document
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_document' AND COLUMN_NAME = 'EXGRATE' AND DATA_TYPE = 'tinyint')
BEGIN

	ALTER TABLE [dbo].[t_fin_document]
	ALTER COLUMN [EXGRATE] [decimal](17, 4) NULL;

	ALTER TABLE [dbo].[t_fin_document]
	ALTER COLUMN [EXGRATE2] [decimal](17, 4) NULL;

	ALTER TABLE [dbo].[t_fin_document]
	ALTER COLUMN [EXGRATE_PLAN] [bit] NULL;

	ALTER TABLE [dbo].[t_fin_document]
	ALTER COLUMN [EXGRATE_PLAN2] [bit]  NULL;

	-- Example to execute the SQL!
    --EXEC sp_executesql
    --    N'UPDATE [dbo].[PurchaseOrder] SET [IsDownloadable] = 1 WHERE [Ref] IS NOT NULL'

END

-- Drop table: t_fin_setting
IF (EXISTS (SELECT * 
  FROM INFORMATION_SCHEMA.TABLES 
  WHERE TABLE_SCHEMA = 'dbo' 
  AND  TABLE_NAME = 't_fin_setting'))
BEGIN
	DROP TABLE [dbo].[t_fin_setting];
END
GO

-- Drop table: t_fin_exrate
IF (EXISTS (SELECT * 
  FROM INFORMATION_SCHEMA.TABLES 
  WHERE TABLE_SCHEMA = 'dbo' 
  AND  TABLE_NAME = 't_fin_exrate'))
BEGIN
	DROP TABLE [dbo].[t_fin_exrate];
END
GO

-- Update column in t_fin_document_item
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_document_item' AND COLUMN_NAME = 'USECURR2' AND DATA_TYPE = 'tinyint')
BEGIN

	ALTER TABLE [dbo].[t_fin_document_item]
	ALTER COLUMN [USECURR2] [bit] NULL;

END


---------------------------------
-- TODO...








/****** Object:  Table [dbo].[t_learn_award]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_award](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[USERID] [nvarchar](40) NOT NULL,
	[ADATE] [date] NOT NULL,
	[SCORE] [int] NOT NULL,
	[REASON] [nvarchar](40) NOT NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_award] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[t_learn_plan]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_plan](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](45) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_plan] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_learn_plandtl]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_plandtl](
	[ID] [int] NOT NULL,
	[OBJECTID] [int] NOT NULL,
	[DEFERREDDAY] [int] NOT NULL,
	[RECURTYPE] [tinyint] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
 CONSTRAINT [PK_t_learn_plandtl] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[OBJECTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_learn_planpat]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_planpat](
	[ID] [int] NOT NULL,
	[USERID] [nvarchar](40) NOT NULL,
	[STARTDATE] [date] NOT NULL,
	[STATUS] [tinyint] NULL,
	[COMMENT] [nvarchar](45) NULL,
 CONSTRAINT [PK_t_learn_planpat] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[USERID] ASC,
	[STARTDATE] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_learn_recurtypedates]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_recurtypedates](
	[ID] [smallint] NOT NULL,
	[DEFDAYS] [int] NOT NULL,
 CONSTRAINT [PK_t_learn_recurtypedates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[DEFDAYS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_tag]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_tag](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_t_tag] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_tag_link]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_tag_link](
	[TAGID] [int] NOT NULL,
	[MODULE] [nvarchar](3) NOT NULL,
	[OBJID] [int] NOT NULL,
 CONSTRAINT [PK_t_tag_link] PRIMARY KEY CLUSTERED 
(
	[TAGID] ASC,
	[MODULE] ASC,
	[OBJID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[t_event]    Script Date: 2017-05-04 5:54:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_event](
	[HID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Content] [nvarchar](max) NULL,
	[IsPublic] [bit] NOT NULL,
	[Owner] [nvarchar](40) NULL,
	[RefID] [int] NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_event] PRIMARY KEY CLUSTERED 
(
	[HID] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UX_t_event_name] UNIQUE NONCLUSTERED 
(
	[HID] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO
ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_EndTime]  DEFAULT (getdate()) FOR [EndTime]
GO
ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO
ALTER TABLE [dbo].[t_event]  WITH CHECK ADD  CONSTRAINT [FK_t_event_t_hid] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_event] CHECK CONSTRAINT [FK_t_event_t_hid]
GO

/****** Object:  Index [IUX_t_tag_NAME]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IUX_t_tag_NAME] ON [dbo].[t_tag]
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_learn_plan] ADD  CONSTRAINT [DF_t_learn_plan_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_plan] ADD  CONSTRAINT [DF_t_learn_plan_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_learn_plandtl] ADD  CONSTRAINT [DF_t_learn_plandtl_DEFERREDDAY]  DEFAULT ((0)) FOR [DEFERREDDAY]
GO
ALTER TABLE [dbo].[t_learn_plandtl] ADD  CONSTRAINT [DF_t_learn_plandtl_RECURTYPE]  DEFAULT ((0)) FOR [RECURTYPE]
GO
ALTER TABLE [dbo].[t_learn_planpat] ADD  CONSTRAINT [DF_t_learn_planpat_STARTDATE]  DEFAULT (getdate()) FOR [STARTDATE]
GO
ALTER TABLE [dbo].[t_learn_plandtl]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_plandtl_plan] FOREIGN KEY([ID])
REFERENCES [dbo].[t_learn_plan] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_learn_plandtl] CHECK CONSTRAINT [FK_t_learn_plandtl_plan]
GO
ALTER TABLE [dbo].[t_learn_plandtl]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_plandtl_t_learn_plandtl] FOREIGN KEY([ID], [OBJECTID])
REFERENCES [dbo].[t_learn_plandtl] ([ID], [OBJECTID])
GO
ALTER TABLE [dbo].[t_learn_plandtl] CHECK CONSTRAINT [FK_t_learn_plandtl_t_learn_plandtl]
GO
ALTER TABLE [dbo].[t_learn_planpat]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_planpat_plan] FOREIGN KEY([ID])
REFERENCES [dbo].[t_learn_plan] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_learn_planpat] CHECK CONSTRAINT [FK_t_learn_planpat_plan]
GO
ALTER TABLE [dbo].[t_tag_link]  WITH CHECK ADD  CONSTRAINT [FK_t_tag_link_tag] FOREIGN KEY([TAGID])
REFERENCES [dbo].[t_tag] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_tag_link] CHECK CONSTRAINT [FK_t_tag_link_tag]
GO

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_EndTime]  DEFAULT (getdate()) FOR [EndTime]
GO

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO





/****** Object:  View [dbo].[v_fin_grp_acnt]    Script Date: 2017-04-22 11:27:20 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create view [dbo].[v_fin_grp_acnt]
as
select 
        [accountid] AS [accountid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
    from
        [v_fin_document_item]
		group by [accountid];
GO

/****** Object:  View [dbo].[v_fin_grp_acnt2]    Script Date: 2017-04-21 6:58:01 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt2]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create view [dbo].[v_fin_grp_acnt2]
as
select 
        [v_fin_grp_acnt].[accountid] AS [accountid],
		[t_fin_account_ctgy].ID AS [categoryid],
		[t_fin_account_ctgy].ASSETFLAG AS [categoryassetflag],
        "debitbalance" = 
		case [t_fin_account_ctgy].ASSETFLAG
            when 1 then [v_fin_grp_acnt].[balance_lc]
            else 0
        end,
        "creditbalance" = 
		case [t_fin_account_ctgy].ASSETFLAG
            when  0 then -(1) * [v_fin_grp_acnt].[balance_lc]
            else 0
        end,
		[v_fin_grp_acnt].[balance_lc] AS [balance_lc]
    from
        [v_fin_grp_acnt]
		join [t_fin_account] ON [v_fin_grp_acnt].[accountid] = [t_fin_account].ID
        join [t_fin_account_ctgy] ON [t_fin_account].CTGYID = t_fin_account_ctgy.ID

GO

/****** Object:  View [dbo].[v_fin_grp_acnt_tranexp]    Script Date: 2017-04-22 11:28:01 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt_tranexp]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create view [dbo].[v_fin_grp_acnt_tranexp]
as
select 
        [accountid] AS [accountid],
		[TRANTYPE_EXP],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
    from
        [v_fin_document_item]
		group by [accountid], [TRANTYPE_EXP];
GO

/****** Object:  View [dbo].[v_fin_report_bs]    Script Date: 2017-04-21 9:04:14 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_report_bs]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_report_bs]
AS 
SELECT tab_a.[accountid],
	   tab_a.[ACCOUNTNAME],
	   tab_a.[ACCOUNTCTGYID],
	   tab_a.[ACCOUNTCTGYNAME],
        tab_a.[balance_lc] AS [debit_balance],
        tab_b.[balance_lc] AS [credit_balance],
        (tab_a.[balance_lc] - tab_b.[balance_lc]) AS [balance]
 FROM 
	(SELECT [t_fin_account].[ID] AS [ACCOUNTID],
		[t_fin_account].[NAME] AS [ACCOUNTNAME],
		[t_fin_account_ctgy].[ID] AS [ACCOUNTCTGYID],
		[t_fin_account_ctgy].[NAME] AS [ACCOUNTCTGYNAME],
		(case
            when ([v_fin_grp_acnt_tranexp].[balance_lc] is not null) then [v_fin_grp_acnt_tranexp].[balance_lc]
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_account]
	JOIN [dbo].[t_fin_account_ctgy] ON [t_fin_account].CTGYID = [t_fin_account_ctgy].[ID]
	LEFT OUTER JOIN [dbo].[v_fin_grp_acnt_tranexp] ON [t_fin_account].[ID] = [v_fin_grp_acnt_tranexp].[accountid]
		AND [v_fin_grp_acnt_tranexp].[trantype_exp] = 0 ) tab_a

	JOIN 

	( SELECT [t_fin_account].[ID] AS [ACCOUNTID],
		[t_fin_account].[NAME] AS [ACCOUNTNAME],
		[t_fin_account_ctgy].[ID] AS [ACCOUNTCTGYID],
		[t_fin_account_ctgy].[NAME] AS [ACCOUNTCTGYNAME],
		(case
            when ([v_fin_grp_acnt_tranexp].[balance_lc] is not null) then [v_fin_grp_acnt_tranexp].[balance_lc] * -1
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_account]
	JOIN [dbo].[t_fin_account_ctgy] ON [t_fin_account].CTGYID = [t_fin_account_ctgy].[ID]
	LEFT OUTER JOIN [dbo].[v_fin_grp_acnt_tranexp] ON [t_fin_account].[ID] = [v_fin_grp_acnt_tranexp].[accountid]
		AND [v_fin_grp_acnt_tranexp].[trantype_exp] = 1 ) tab_b

	ON tab_a.[ACCOUNTID] = tab_b.[ACCOUNTID]

GO

/****** Object:  View [dbo].[v_fin_grp_cc]    Script Date: 2017-04-22 8:05:20 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_cc]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_grp_cc]
AS
SELECT
        [controlcenterid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [controlcenterid] IS NOT NULL
		GROUP BY [controlcenterid];

GO

/****** Object:  View [dbo].[v_fin_grp_cc_tranexp]    Script Date: 2017-04-22 8:06:29 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_cc_tranexp]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_grp_cc_tranexp]
AS
SELECT
        [controlcenterid],
		[TRANTYPE_EXP],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [controlcenterid] IS NOT NULL
		GROUP BY [controlcenterid], [TRANTYPE_EXP];

GO

/****** Object:  View [dbo].[v_fin_report_cc]    Script Date: 2017-04-22 8:11:26 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_report_cc]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create view [dbo].[v_fin_report_cc]
AS 
SELECT tab_a.[CONTROLCENTERID],
	   tab_a.[CONTROLCENTERNAME],
       tab_a.[balance_lc] AS [debit_balance],
       tab_b.[balance_lc] AS [credit_balance],
        (tab_a.[balance_lc] - tab_b.[balance_lc]) AS [balance]
 FROM 
	(SELECT [t_fin_controlcenter].[ID] AS [CONTROLCENTERID],
		[t_fin_controlcenter].[NAME] AS [CONTROLCENTERNAME],
		(case
            when ([v_fin_grp_cc_tranexp].[balance_lc] is not null) then [v_fin_grp_cc_tranexp].[balance_lc]
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_controlcenter]
	LEFT OUTER JOIN [dbo].[v_fin_grp_cc_tranexp] ON [t_fin_controlcenter].[ID] = [v_fin_grp_cc_tranexp].controlcenterid
		AND [v_fin_grp_cc_tranexp].[trantype_exp] = 0 ) tab_a

	JOIN 

	( SELECT [t_fin_controlcenter].[ID] AS [CONTROLCENTERID],
		[t_fin_controlcenter].[NAME] AS [CONTROLCENTERNAME],
		(case
            when ([v_fin_grp_cc_tranexp].[balance_lc] is not null) then [v_fin_grp_cc_tranexp].[balance_lc] * -1
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_controlcenter]
	LEFT OUTER JOIN [dbo].[v_fin_grp_cc_tranexp] ON [t_fin_controlcenter].[ID] = [v_fin_grp_cc_tranexp].controlcenterid
		AND [v_fin_grp_cc_tranexp].[trantype_exp] = 1 ) tab_b

	ON tab_a.[CONTROLCENTERID] = tab_b.[CONTROLCENTERID]

GO

/****** Object:  View [dbo].[v_fin_grp_ord]    Script Date: 2017-04-22 8:46:17 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_ord]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_grp_ord]
AS
SELECT
        [orderid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [orderid] IS NOT NULL
		GROUP BY [orderid];

GO

/****** Object:  View [dbo].[v_fin_grp_order_tranexp]    Script Date: 2017-04-22 8:47:02 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_grp_order_tranexp]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[v_fin_grp_order_tranexp]
AS
SELECT
        [ORDERID],
		[TRANTYPE_EXP],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [ORDERID] IS NOT NULL
		GROUP BY [ORDERID], [TRANTYPE_EXP];

GO

/****** Object:  View [dbo].[v_fin_report_order]    Script Date: 2017-04-22 8:47:43 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_report_order]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create view [dbo].[v_fin_report_order]
AS 
SELECT tab_a.[ORDERID],
	   tab_a.[ORDERNAME],
       tab_a.[balance_lc] AS [debit_balance],
       tab_b.[balance_lc] AS [credit_balance],
        (tab_a.[balance_lc] - tab_b.[balance_lc]) AS [balance]
 FROM 
	(SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc]
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 0 ) tab_a

	JOIN 

	( SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc] * -1
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 1 ) tab_b

	ON tab_a.[ORDERID] = tab_b.[ORDERID]

GO

