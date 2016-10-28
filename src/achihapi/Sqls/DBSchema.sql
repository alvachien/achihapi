/****** Object:  Table [dbo].[t_fin_account]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CTGYID] [int] NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[OWNER] [nvarchar](40) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_account] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_fin_account_ctgy]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[ASSETFLAG] [bit] NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[SYSFLAG] [bit] NULL,
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
/****** Object:  Table [dbo].[t_fin_account_ext_dp]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_account_ext_dp](
	[ACCOUNTID] [int] NOT NULL,
	[DIRECT] [tinyint] NOT NULL,
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
/****** Object:  Table [dbo].[t_fin_controlcenter]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_controlcenter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[PARID] [int] NOT NULL,
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
/****** Object:  Table [dbo].[t_fin_currency]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_currency](
	[CURR] [nvarchar](5) NOT NULL,
	[NAME] [nvarchar](45) NOT NULL,
	[SYMBOL] [nvarchar](30) NULL,
	[SYSFLAG] [bit] NULL,
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
/****** Object:  Table [dbo].[t_fin_doc_type]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_doc_type](
	[ID] [smallint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[SYSFLAG] [bit] NULL,
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
/****** Object:  Table [dbo].[t_fin_document]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_document](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DOCTYPE] [smallint] NOT NULL,
	[TRANDATE] [date] NOT NULL,
	[TRANCURR] [nvarchar](5) NOT NULL,
	[DESP] [nvarchar](45) NOT NULL,
	[EXGRATE] [tinyint] NULL,
	[EXGRATE_PLAN] [tinyint] NULL,
	[TRANCURR2] [nvarchar](5) NULL,
	[EXGRATE2] [tinyint] NULL,
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
	[USECURR2] [tinyint] NULL,
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
/****** Object:  Table [dbo].[t_fin_exrate]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_exrate](
	[TRANDATE] [date] NOT NULL,
	[CURR] [nvarchar](5) NOT NULL,
	[RATE] [decimal](17, 4) NOT NULL,
	[REFDOCID] [int] NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_exrate] PRIMARY KEY CLUSTERED 
(
	[TRANDATE] ASC,
	[CURR] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_fin_order]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_order](
	[ID] [int] IDENTITY(1,1) NOT NULL,
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
/****** Object:  Table [dbo].[t_fin_setting]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_setting](
	[SETID] [nvarchar](20) NOT NULL,
	[SETVALUE] [nvarchar](80) NOT NULL,
	[COMMENT] [nvarchar](45) NULL,
	[CREATEDBY] [nvarchar](40) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](40) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_setting] PRIMARY KEY CLUSTERED 
(
	[SETID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_fin_tmpdoc_dp]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_tmpdoc_dp](
	[DOCID] [int] NOT NULL,
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
/****** Object:  Table [dbo].[t_fin_tran_type]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_tran_type](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](30) NOT NULL,
	[EXPENSE] [bit] NOT NULL,
	[PARID] [int] NULL,
	[COMMENT] [nvarchar](45) NULL,
	[SYSFLAG] [bit] NULL,
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
/****** Object:  Table [dbo].[t_learn_ctgy]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_ctgy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PARID] [int] NULL,
	[NAME] [nvarchar](45) NOT NULL,
	[COMMENT] [nvarchar](50) NULL,
	[SYSFLAG] [bit] NULL,
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
/****** Object:  Table [dbo].[t_learn_hist]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_hist](
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
	[USERID] ASC,
	[OBJECTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_learn_obj]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_learn_obj](
	[ID] [int] IDENTITY(1,1) NOT NULL,
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
/****** Object:  Table [dbo].[t_module]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_module](
	[MODULE] [nvarchar](3) NOT NULL,
	[NAME] [nvarchar](50) NOT NULL,
	[AUTHFLAG] [bit] NULL,
	[TAGFLAG] [bit] NULL,
 CONSTRAINT [PK_t_module] PRIMARY KEY CLUSTERED 
(
	[MODULE] ASC
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
/****** Object:  Table [dbo].[t_userdetail]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_userdetail](
	[USERID] [nvarchar](40) NOT NULL,
	[DISPLAYAS] [nvarchar](50) NOT NULL,
	[EMAIL] [nvarchar](100) NULL,
	[OTHERS] [nvarchar](100) NULL,
 CONSTRAINT [PK_t_userdetail] PRIMARY KEY CLUSTERED 
(
	[USERID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[t_userhist]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_userhist](
	[USERID] [nvarchar](40) NOT NULL,
	[SEQNO] [int] NOT NULL,
	[HISTTYP] [tinyint] NOT NULL,
	[TIMEPOINT] [datetime] NOT NULL,
	[OTHERS] [nvarchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IUX_t_tag_NAME]    Script Date: 2016-10-27 3:31:27 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IUX_t_tag_NAME] ON [dbo].[t_tag]
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[t_fin_account] ADD  CONSTRAINT [DF_t_fin_account_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_account] ADD  CONSTRAINT [DF_t_fin_account_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_ASSETFLAG]  DEFAULT ((1)) FOR [ASSETFLAG]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_SYSFLAG]  DEFAULT ((0)) FOR [SYSFLAG]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_account_ctgy] ADD  CONSTRAINT [DF_t_fin_account_ctgy_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_controlcenter] ADD  CONSTRAINT [DF_t_fin_controlcenter_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_controlcenter] ADD  CONSTRAINT [DF_t_fin_controlcenter_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_currency] ADD  CONSTRAINT [DF_t_fin_currency_SYSFLAG]  DEFAULT ((0)) FOR [SYSFLAG]
GO
ALTER TABLE [dbo].[t_fin_currency] ADD  CONSTRAINT [DF_t_fin_currency_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_currency] ADD  CONSTRAINT [DF_t_fin_currency_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_doc_type] ADD  CONSTRAINT [DF_t_fin_doc_type_SYSFLAG]  DEFAULT ((0)) FOR [SYSFLAG]
GO
ALTER TABLE [dbo].[t_fin_doc_type] ADD  CONSTRAINT [DF_t_fin_doc_type_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_doc_type] ADD  CONSTRAINT [DF_t_fin_doc_type_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_document] ADD  CONSTRAINT [DF_t_fin_document_TRANDATE]  DEFAULT (getdate()) FOR [TRANDATE]
GO
ALTER TABLE [dbo].[t_fin_exrate] ADD  CONSTRAINT [DF_t_fin_exrate_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_exrate] ADD  CONSTRAINT [DF_t_fin_exrate_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_order] ADD  CONSTRAINT [DF_t_fin_order_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_order] ADD  CONSTRAINT [DF_t_fin_order_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_setting] ADD  CONSTRAINT [DF_t_fin_setting_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_setting] ADD  CONSTRAINT [DF_t_fin_setting_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tmpdoc_dp] ADD  CONSTRAINT [DF_t_fin_tmpdoc_dp_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tmpdoc_dp] ADD  CONSTRAINT [DF_t_fin_tmpdoc_dp_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tran_type] ADD  CONSTRAINT [DF_t_fin_tran_type_SYSFLAG]  DEFAULT ((0)) FOR [SYSFLAG]
GO
ALTER TABLE [dbo].[t_fin_tran_type] ADD  CONSTRAINT [DF_t_fin_tran_type_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_fin_tran_type] ADD  CONSTRAINT [DF_t_fin_tran_type_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_language] ADD  CONSTRAINT [DF_t_language_APPFLAG]  DEFAULT ((0)) FOR [APPFLAG]
GO
ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_award] ADD  CONSTRAINT [DF_t_learn_award_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_learn_ctgy] ADD  CONSTRAINT [DF_t_learn_ctgy_SYSFLAG]  DEFAULT ((0)) FOR [SYSFLAG]
GO
ALTER TABLE [dbo].[t_learn_ctgy] ADD  CONSTRAINT [DF_t_learn_ctgy_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_ctgy] ADD  CONSTRAINT [DF_t_learn_ctgy_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_learn_hist] ADD  CONSTRAINT [DF_t_learn_hist_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_hist] ADD  CONSTRAINT [DF_t_learn_hist_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
GO
ALTER TABLE [dbo].[t_learn_obj] ADD  CONSTRAINT [DF_t_learn_obj_CREATEDAT]  DEFAULT (getdate()) FOR [CREATEDAT]
GO
ALTER TABLE [dbo].[t_learn_obj] ADD  CONSTRAINT [DF_t_learn_obj_UPDATEDAT]  DEFAULT (getdate()) FOR [UPDATEDAT]
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
ALTER TABLE [dbo].[t_fin_account]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ctgy] FOREIGN KEY([CTGYID])
REFERENCES [dbo].[t_fin_account_ctgy] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_account] CHECK CONSTRAINT [FK_t_fin_account_ctgy]
GO
ALTER TABLE [dbo].[t_fin_account_ext_dp]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_dp_id] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_account_ext_dp] CHECK CONSTRAINT [FK_t_fin_account_ext_dp_id]
GO
ALTER TABLE [dbo].[t_fin_document_item]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_document_item_document] FOREIGN KEY([DOCID])
REFERENCES [dbo].[t_fin_document] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_document_item] CHECK CONSTRAINT [FK_t_fin_document_item_document]
GO
ALTER TABLE [dbo].[t_fin_order_srule]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_order_srule_order] FOREIGN KEY([ORDID])
REFERENCES [dbo].[t_fin_order] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_fin_order_srule] CHECK CONSTRAINT [FK_t_fin_order_srule_order]
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
