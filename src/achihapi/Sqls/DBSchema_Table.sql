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
	[SEND_DEL] [bit] NULL,
	[REV_DEL] [bit] NULL,
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

ALTER TABLE [dbo].[t_homemsg] ADD  CONSTRAINT [DF_t_homemsg_SEND_DEL]  DEFAULT ((0)) FOR [SEND_DEL]
GO

ALTER TABLE [dbo].[t_homemsg] ADD  CONSTRAINT [DF_t_homemsg_REV_DEL]  DEFAULT ((0)) FOR [REV_DEL]
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
	[STATUS] [tinyint] NULL,
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
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_document_item' AND COLUMN_NAME = 'USECURR2' AND DATA_TYPE = 'tinyint')
BEGIN

	ALTER TABLE [dbo].[t_fin_document_item]
	ALTER COLUMN [USECURR2] [bit] NULL;

END


-- Updated at 2017.10.3
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_account' AND COLUMN_NAME = 'STATUS')
BEGIN

	ALTER TABLE [dbo].[t_fin_account]
	ADD [STATUS] [tinyint] NULL;

END

-- Updated at 2017.10.06
/****** Object:  Table [dbo].[t_fin_asset_ctgy]    Script Date: 2017-10-06 12:18:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_asset_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[NAME] [nvarchar](50) NOT NULL,
	[DESP] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_asset_ctgy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_asset_ctgy] ADD  CONSTRAINT [DF_t_fin_asset_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_fin_asset_ctgy]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_asset_ctgy_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_asset_ctgy] CHECK CONSTRAINT [FK_t_fin_asset_ctgy_HID]
GO

/****** Object:  Table [dbo].[t_fin_account_ext_as]    Script Date: 2017-10-06 10:58:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_account_ext_as](
	[ACCOUNTID] [int] NOT NULL,
	[CTGYID] [int] NOT NULL,
	[NAME] [nvarchar](50) NOT NULL,
	[REFDOC_BUY] [int] NOT NULL,
	[COMMENT] [nvarchar](100) NULL,
	[REFDOC_SOLD] [int] NULL,
 CONSTRAINT [PK_t_fin_account_ext_as] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_account_ext_as]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_as_ACNTID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_account_ext_as] CHECK CONSTRAINT [FK_t_fin_account_ext_as_ACNTID]
GO

ALTER TABLE [dbo].[t_fin_account_ext_as]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_as_CTGYID] FOREIGN KEY([CTGYID])
REFERENCES [dbo].[t_fin_asset_ctgy] ([ID])
GO

ALTER TABLE [dbo].[t_fin_account_ext_as] CHECK CONSTRAINT [FK_t_fin_account_ext_as_CTGYID]
GO

-- Updated at 2017.10.15
/****** Object:  Table [dbo].[t_lib_person]    Script Date: 2017-10-15 7:00:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_person](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[NativeName] [nvarchar](50) NOT NULL,
	[EnglishName] [nvarchar](50) NULL,
	[EnglishIsNative] [bit] NULL,
	[Gender] [tinyint] NULL,
	[ShortIntro] [nvarchar](100) NULL,
	[ExtLink1] [nvarchar](100) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_lib_person] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_person] ADD  CONSTRAINT [DF_t_lib_person_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_lib_person] ADD  CONSTRAINT [DF_t_lib_person_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_lib_person]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_person_t_lib_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_person] CHECK CONSTRAINT [FK_t_lib_person_t_lib_HID]
GO

/****** Object:  Table [dbo].[t_lib_book_ctgy]    Script Date: 2017-10-15 7:01:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_book_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ParID] [int] NULL,
	[Others] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_lib_book_ctgy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_book_ctgy] ADD  CONSTRAINT [DF_t_lib_book_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_lib_book_ctgy] ADD  CONSTRAINT [DF_t_lib_book_ctgy_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_lib_book_ctgy]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_book_ctgy_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_book_ctgy] CHECK CONSTRAINT [FK_t_lib_book_ctgy_HID]
GO

/****** Object:  Table [dbo].[t_lib_book]    Script Date: 2017-10-15 7:00:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_book](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Ctgy] [int] NULL,
	[NativeName] [nvarchar](50) NOT NULL,
	[EnglishName] [nvarchar](50) NULL,
	[EnglishIsNative] [bit] NULL,
	[ShortIntro] [nvarchar](100) NULL,
	[ExtLink1] [nvarchar](100) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_lib_book] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_book] ADD  CONSTRAINT [DF_t_lib_book_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_lib_book] ADD  CONSTRAINT [DF_t_lib_book_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_lib_book]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_book_ctgy] FOREIGN KEY([Ctgy])
REFERENCES [dbo].[t_lib_book_ctgy] ([ID])
GO

ALTER TABLE [dbo].[t_lib_book] CHECK CONSTRAINT [FK_t_lib_book_ctgy]
GO

ALTER TABLE [dbo].[t_lib_book]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_book_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_book] CHECK CONSTRAINT [FK_t_lib_book_HID]
GO

/****** Object:  Table [dbo].[t_lib_location]    Script Date: 2017-10-15 7:01:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_location](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IsDevice] [bit] NULL,
	[Desp] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_lib_location] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_location] ADD  CONSTRAINT [DF_t_lib_location_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_lib_location] ADD  CONSTRAINT [DF_t_lib_location_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_lib_location]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_location_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_location] CHECK CONSTRAINT [FK_t_lib_location_HID]
GO

/****** Object:  Table [dbo].[t_lib_location_detail]    Script Date: 2017-10-15 6:20:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_location_detail](
	[LOCID] [int] NOT NULL,
	[SEQNO] [int] NOT NULL,
	[CONTENTTYPE] [tinyint] NOT NULL,
	[CONTENTID] [int] NOT NULL,
	[Others] [nvarchar](50) NULL,
 CONSTRAINT [PK_t_lib_location_detail] PRIMARY KEY CLUSTERED 
(
	[LOCID] ASC,
	[SEQNO] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_location_detail]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_locationdet_LOCID] FOREIGN KEY([LOCID])
REFERENCES [dbo].[t_lib_location] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_location_detail] CHECK CONSTRAINT [FK_t_lib_locationdet_LOCID]
GO


-- Updated at 2017.10.29
/****** Object:  Table [dbo].[t_learn_qtn_bank]    Script Date: 2017-10-29 12:17:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_qtn_bank](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Type] [tinyint] NOT NULL,
	[Question] [nvarchar](200) NOT NULL,
	[BriefAnswer] [nvarchar](200) NULL,
	[CREATEDBY] [nvarchar](50) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_qtn_bank] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_qtn_bank] ADD  CONSTRAINT [DF_t_learn_qtn_bank_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_learn_qtn_bank] ADD  CONSTRAINT [DF_t_learn_qtn_bank_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_learn_qtn_bank]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_qtn_bank_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_qtn_bank] CHECK CONSTRAINT [FK_t_learn_qtn_bank_HID]
GO

/****** Object:  Table [dbo].[t_learn_qtn_bank_sub]    Script Date: 2017-10-29 12:19:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_qtn_bank_sub](
	[QTNID] [int] NOT NULL,
	[SUBITEM] [nvarchar](20) NOT NULL,
	[DETAIL] [nvarchar](200) NOT NULL,
	[OTHERS] [nvarchar](50) NULL,
 CONSTRAINT [PK_t_learn_qtn_bank_sub] PRIMARY KEY CLUSTERED 
(
	[QTNID] ASC,
	[SUBITEM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_qtn_bank_sub]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_qtn_bank_sub_QTNID] FOREIGN KEY([QTNID])
REFERENCES [dbo].[t_learn_qtn_bank] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_qtn_bank_sub] CHECK CONSTRAINT [FK_t_learn_qtn_bank_sub_QTNID]
GO

/****** Object:  Table [dbo].[t_tag]    Script Date: 2017-10-29 12:19:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_tag](
	[HID] [int] NOT NULL,
	[TagType] [smallint] NOT NULL,
	[TagID] [int] NOT NULL,
	[Term] [nvarchar](50) NOT NULL,
	[TagSubID] [int] NOT NULL,
 CONSTRAINT [PK_t_tag] PRIMARY KEY CLUSTERED 
(
	[HID] ASC,
	[TagType] ASC,
	[TagID] ASC,
	[Term] ASC,
	[TagSubID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


-- Updated at 2017.11.05
-- Update the BriefAnswer from 50 to 100
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_learn_qtn_bank' AND COLUMN_NAME = 'BriefAnswer' AND DATA_TYPE = 'nvarchar' AND CHARACTER_MAXIMUM_LENGTH = 50)
BEGIN

	ALTER TABLE [dbo].[t_learn_qtn_bank]
	ALTER COLUMN [Question] [nvarchar](200) NULL;

	ALTER TABLE [dbo].[t_learn_qtn_bank]
	ALTER COLUMN [BriefAnswer] [nvarchar](200) NULL;

	ALTER TABLE [dbo].[t_learn_qtn_bank_sub]
	ALTER COLUMN [DETAIL] [nvarchar](200) NULL;

END

-- Add subid
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_tag' AND COLUMN_NAME = 'TagSubID')
BEGIN

	ALTER TABLE [dbo].[t_tag] DROP CONSTRAINT [PK_t_tag];

	ALTER TABLE [dbo].[t_tag]
	ADD [TagSubID] [int] NOT NULL DEFAULT 0;

	ALTER TABLE [dbo].[t_tag] ADD CONSTRAINT [PK_t_tag] PRIMARY KEY CLUSTERED (
		[HID] ASC,
		[TagType] ASC,
		[TagID] ASC,
		[Term] ASC,
		[TagSubID] ASC
	);

END

-- Updated at 2017.11.6
/****** Object:  Table [dbo].[t_fin_account_ext_loan]    Script Date: 2017-11-06 3:57:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_account_ext_loan](
	[ACCOUNTID] [int] NOT NULL,
	[STARTDATE] [datetime] NOT NULL,
	[ANNUALRATE] [decimal](17, 2) NULL,
	[INTERESTFREE] [bit] NULL,
	[REPAYMETHOD] [tinyint] NULL,
	[TOTALMONTH] [smallint] NULL,
	[REFDOCID] [int] NOT NULL,
	[OTHERS] [nvarchar](100) NULL,
 CONSTRAINT [PK_t_fin_account_ext_loan] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_account_ext_loan]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_loan_ID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_account_ext_loan] CHECK CONSTRAINT [FK_t_fin_account_ext_loan_ID]
GO

/****** Object:  Table [dbo].[t_fin_tmpdoc_loan]    Script Date: 2017-11-06 5:47:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_tmpdoc_loan](
	[DOCID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[REFDOCID] [int] NULL,
	[ACCOUNTID] [int] NOT NULL,
	[TRANDATE] [date] NOT NULL,
	[TRANTYPE] [int] NOT NULL,
	[TRANAMOUNT] [decimal](17, 2) NOT NULL,
	[INTERESTAMOUNT] [decimal](17, 2) NULL,
	[CONTROLCENTERID] [int] NULL,
	[ORDERID] [int] NULL,
	[DESP] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_tmpdoc_loan] PRIMARY KEY CLUSTERED 
(
	[DOCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_tmpdoc_loan] ADD  CONSTRAINT [DF_t_fin_tmpdoc_loan_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_fin_tmpdoc_loan] ADD  CONSTRAINT [DF_t_fin_tmpdoc_loan_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_fin_tmpdoc_loan]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_tmpdocloan_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_tmpdoc_loan] CHECK CONSTRAINT [FK_t_fin_tmpdocloan_HID]
GO

-- Updated at 2017.11.13
/****** Object:  Table [dbo].[t_learn_enword]    Script Date: 2017-11-13 1:18:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_enword](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Word] [nvarchar](100) NOT NULL,
	[UKPron] [nvarchar] (50) NULL,
	[USPron] [nvarchar] (50) NULL,
	[UKPronFile] [nvarchar] (100) NULL,
	[USPronFile] [nvarchar] (100) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [UK_Table_learn_word] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_enword] ADD  CONSTRAINT [DF_t_learn_enword_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_learn_enword] ADD  CONSTRAINT [DF_t_learn_enword_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_learn_enword]  WITH CHECK ADD  CONSTRAINT [FK_Table_learn_word_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_enword] CHECK CONSTRAINT [FK_Table_learn_word_HID]
GO

/****** Object:  Table [dbo].[t_learn_enwordexp]    Script Date: 2017-11-13 1:21:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_enwordexp](
	[WORDID] [int] NOT NULL,
	[ExpID] [smallint] NOT NULL,
	[POSAbb] [nvarchar](10) NULL,
	[LangKey] [nvarchar](10) NOT NULL,
	[ExpDetail] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_t_learn_enwordexp] PRIMARY KEY CLUSTERED 
(
	[WORDID] ASC,
	[ExpID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_enwordexp]  WITH CHECK ADD  CONSTRAINT [FK_Table_learn_wordexp_word] FOREIGN KEY([WORDID])
REFERENCES [dbo].[t_learn_enword] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_enwordexp] CHECK CONSTRAINT [FK_Table_learn_wordexp_word]
GO

/****** Object:  Table [dbo].[t_learn_ensent]    Script Date: 2017-11-13 2:22:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_ensent](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Sentence] [nvarchar](100) NOT NULL,
	[CREATEDBY] [nvarchar](50) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_learn_ensent] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_ensent] ADD  CONSTRAINT [DF_t_learn_ensent_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_learn_ensent] ADD  CONSTRAINT [DF_t_learn_ensent_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_learn_ensent]  WITH CHECK ADD  CONSTRAINT [FK_Table_learn_ensent_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_ensent] CHECK CONSTRAINT [FK_Table_learn_ensent_HID]
GO

/****** Object:  Table [dbo].[t_learn_ensentexp]    Script Date: 2017-11-13 2:22:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_ensentexp](
	[SentID] [int] NOT NULL,
	[ExpID] [smallint] NOT NULL,
	[LangKey] [nvarchar](10) NOT NULL,
	[ExpDetail] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_t_learn_ensentexp] PRIMARY KEY CLUSTERED 
(
	[SentID] ASC,
	[ExpID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_ensentexp]  WITH CHECK ADD  CONSTRAINT [FK_Table_ensentexp_SentID] FOREIGN KEY([SentID])
REFERENCES [dbo].[t_learn_ensent] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_ensentexp] CHECK CONSTRAINT [FK_Table_ensentexp_SentID]
GO

/****** Object:  Table [dbo].[t_learn_ensent_word]    Script Date: 2017-11-13 2:22:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_learn_ensent_word](
	[SentID] [int] NOT NULL,
	[WordID] [int] NOT NULL,
 CONSTRAINT [PK_t_learn_ensent_word] PRIMARY KEY CLUSTERED 
(
	[SentID] ASC,
	[WordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_learn_ensent_word]  WITH CHECK ADD  CONSTRAINT [FK_t_learn_ensent_word_sent] FOREIGN KEY([SentID])
REFERENCES [dbo].[t_learn_ensent] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_learn_ensent_word] CHECK CONSTRAINT [FK_t_learn_ensent_word_sent]
GO

-- Updated at 2018.1.7
/****** Object:  Table [dbo].[t_event]    Script Date: 2017-05-04 5:54:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_event](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[CompleteTime] [datetime] NULL,
	[Content] [nvarchar](max) NULL,
	[IsPublic] [bit] NOT NULL,
	[Assignee] [nvarchar](40) NULL,
	[RefRecurID] [int] NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_event] PRIMARY KEY CLUSTERED 
(
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
ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO
ALTER TABLE [dbo].[t_event]  WITH CHECK ADD  CONSTRAINT [FK_t_event_t_hid] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_event] CHECK CONSTRAINT [FK_t_event_t_hid]
GO

-- Updatged at 2018.1.18
/****** Object:  Table [dbo].[t_event_recur]    Script Date: 2018-01-18 2:52:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_event_recur](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[STARTDATE] [date] NOT NULL,
	[ENDDATE] [date] NOT NULL,
	[RPTTYPE] [tinyint] NOT NULL,
	[NAME] [nvarchar](50) NOT NULL,
	[CONTENT] [nvarchar](max) NULL,
	[ISPUBLIC] [bit] NULL,
	[ASSIGNEE] [nvarchar](40) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_event_recur] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_event_recur] ADD  CONSTRAINT [DF_t_event_recur_ISPUBLIC]  DEFAULT ((0)) FOR [ISPUBLIC]
GO

ALTER TABLE [dbo].[t_event_recur]  WITH CHECK ADD  CONSTRAINT [FK_t_event_recur_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_event_recur] CHECK CONSTRAINT [FK_t_event_recur_HID]
GO

-- Updated at 2018.3.13
/****** Object:  Table [dbo].[t_event_habit]    Script Date: 2018-03-13 2:00:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_event_habit](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[RPTTYPE] [tinyint] NOT NULL,
	[IsPublic] [bit] NOT NULL,
	[CONTENT] [nvarchar](max) NULL,
	[Count] [int] NOT NULL,
	[Assignee] [nvarchar](40) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_event_habit] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_event_habit]  WITH CHECK ADD  CONSTRAINT [FK_t_event_habit_t_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_event_habit] CHECK CONSTRAINT [FK_t_event_habit_t_HID]
GO

/****** Object:  Table [dbo].[t_event_habit_detail]    Script Date: 2018-03-13 2:01:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_event_habit_detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HabitID] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
 CONSTRAINT [PK_t_event_habit_detail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_event_habit_detail]  WITH CHECK ADD  CONSTRAINT [FK_t_event_habit_HabitID] FOREIGN KEY([HabitID])
REFERENCES [dbo].[t_event_habit] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_event_habit_detail] CHECK CONSTRAINT [FK_t_event_habit_HabitID]
GO

/****** Object:  Table [dbo].[t_event_habit_checkin]    Script Date: 2018-03-13 2:01:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_event_habit_checkin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TranDate] [datetime] NOT NULL,
	[HabitID] [int] NOT NULL,
	[Score] [int] NULL,
	[Comment] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_event_habit_checkin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_event_habit_checkin]  WITH CHECK ADD  CONSTRAINT [FK_t_event_habit_checkin_HabitID] FOREIGN KEY([HabitID])
REFERENCES [dbo].[t_event_habit] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_event_habit_checkin] CHECK CONSTRAINT [FK_t_event_habit_checkin_HabitID]
GO

-- Updated at 2018.6.6
/****** Object:  Table [dbo].[t_lib_movie_genre]    Script Date: 2018-06-06 7:01:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_lib_movie_genre](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ParID] [int] NULL,
	[Others] [nvarchar](50) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_lib_movie_genre] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_lib_movie_genre] ADD  CONSTRAINT [DF_t_lib_movie_genre_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO

ALTER TABLE [dbo].[t_lib_movie_genre] ADD  CONSTRAINT [DF_t_lib_movie_genre_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO

ALTER TABLE [dbo].[t_lib_movie_genre]  WITH CHECK ADD  CONSTRAINT [FK_t_lib_movie_genre_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_lib_movie_genre] CHECK CONSTRAINT [FK_t_lib_movie_genre_HID]
GO

-- Updated at 2018.7.1
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_account_ext_loan' AND COLUMN_NAME = 'EndDate')
BEGIN

	ALTER TABLE [dbo].[t_fin_account_ext_loan] 
		ADD [EndDate] [date] NULL DEFAULT getdate();
	ALTER TABLE [dbo].[t_fin_account_ext_loan] 
		ADD [IsLendOut] [bit] NOT NULL DEFAULT 0;

END

/****** Object:  Table [dbo].[t_dbversion]    Script Date: 2018-07-03 8:09:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_dbversion](
	[VersionID] [int] NOT NULL,
	[ReleasedDate] [datetime] NOT NULL,
	[AppliedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_t_dbversion] PRIMARY KEY CLUSTERED 
(
	[VersionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_dbversion] ADD  CONSTRAINT [DF_t_dbversion_AppliedDate]  DEFAULT (getdate()) FOR [AppliedDate]
GO

-- Updated at 2018.8.2
-- Update table: t_fin_tmpdoc_loan 
IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_fin_tmpdoc_loan' AND COLUMN_NAME = 'TRANTYPE')
BEGIN

    ALTER TABLE [dbo].[t_fin_tmpdoc_loan] 
		DROP COLUMN TRANTYPE;

END
GO

-- Updated at 2018.8.5
-- Update table t_fin_account_ext_loan
IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_fin_account_ext_loan' AND COLUMN_NAME = 'IsLendOut')
BEGIN

    ALTER TABLE [dbo].[t_fin_account_ext_loan] 
		DROP COLUMN IsLendOut;

    ALTER TABLE [dbo].[t_fin_account_ext_loan]
		ADD [PAYINGACCOUNT] int NULL;

	ALTER TABLE [dbo].[t_fin_account_ext_loan]
		ADD [PARTNER] nvarchar(50) NULL;
END;

--------------------------------------------------------------------------------------------------------
-- Updated at 2018.8.10
/****** Object:  Table [dbo].[t_fin_account_ext_loan_h]    Script Date: 8/10/2018 9:51:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_account_ext_loan_h](
	[ACCOUNTID] [int] NOT NULL,
	[STARTDATE] [datetime] NOT NULL,
	[ANNUALRATE] [decimal](17, 2) NULL,
	[INTERESTFREE] [bit] NULL,
	[REPAYMETHOD] [tinyint] NULL,
	[TOTALMONTH] [smallint] NULL,
	[REFDOCID] [int] NOT NULL,
	[OTHERS] [nvarchar](100) NULL,
	[EndDate] [date] NULL,
	[PAYINGACCOUNT] [int] NULL,
	[PARTNER] [nvarchar](50) NULL,
 CONSTRAINT [PK_t_fin_account_ext_loan_h] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_account_ext_loan_h] ADD  DEFAULT (getdate()) FOR [EndDate]
GO

ALTER TABLE [dbo].[t_fin_account_ext_loan_h]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_loan_h_ID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_account_ext_loan_h] CHECK CONSTRAINT [FK_t_fin_account_ext_loan_h_ID]
GO

-- Credit card
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_account_ext_cc](
	[ACCOUNTID] [int] NOT NULL,
	[BILLDAYINMONTH] [smallint] NOT NULL,
	[REPAYDAYINMONTH] [smallint] NOT NULL,
	[CARDNUM] [nvarchar](20) NOT NULL,
	[OTHERS] [nvarchar](100) NULL,
	[BANK] [nvarchar](50) NULL,
	[VALIDDATE] [datetime] NULL,
 CONSTRAINT [PK_t_fin_account_ext_cc] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_fin_account_ext_cc]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_cc_ID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[t_fin_account_ext_cc] CHECK CONSTRAINT [FK_t_fin_account_ext_cc_ID]
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



