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
/****** Object:  Table [dbo].[t_fin_controlcenter]    Script Date: 2016-10-27 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_controlcenter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
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

/****** Object:  Table [dbo].[t_fin_document]    Script Date: 2017-02-20 12:11:41 AM ******/
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
	[EXGRATE_PLAN2] [tinyint] NULL,
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
/****** Object:  Table [dbo].[t_fin_tmpdoc_dp]    Script Date: 2017-03-10 3:31:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_fin_tmpdoc_dp](
	[DOCID] [int] IDENTITY(1,1) NOT NULL,
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
/****** Object:  Table [dbo].[t_event]    Script Date: 2016-11-20 6:17:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_event](
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
	[UPDATEDAT] [date] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

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

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_EndTime]  DEFAULT (getdate()) FOR [EndTime]
GO

ALTER TABLE [dbo].[t_event] ADD  CONSTRAINT [DF_t_event_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO

/*
 * Views
 *
 *
 */

/****** Object:  View [dbo].[v_fin_order_srule]    Script Date: 2017-02-02 4:53:55 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_order_srule]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_order_srule]
AS
SELECT        dbo.t_fin_order.ID, dbo.t_fin_order.NAME, dbo.t_fin_order.VALID_FROM, dbo.t_fin_order.VALID_TO, dbo.t_fin_order.COMMENT, dbo.t_fin_order.CREATEDBY, dbo.t_fin_order.CREATEDAT, 
                         dbo.t_fin_order.UPDATEDBY, dbo.t_fin_order.UPDATEDAT, dbo.t_fin_order_srule.RULEID, dbo.t_fin_order_srule.CONTROLCENTERID, dbo.t_fin_controlcenter.NAME AS CONTROLCENTERNAME, 
                         dbo.t_fin_order_srule.PRECENT
FROM            dbo.t_fin_order LEFT OUTER JOIN
                         dbo.t_fin_order_srule ON dbo.t_fin_order.ID = dbo.t_fin_order_srule.ORDID INNER JOIN
                         dbo.t_fin_controlcenter ON dbo.t_fin_order_srule.CONTROLCENTERID = dbo.t_fin_controlcenter.ID

GO

/****** Object:  View [dbo].[v_fin_document_item]    Script Date: 2017-04-22 10:49:07 AM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_document_item]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[v_fin_document_item]
AS 
select 
        [t_fin_document_item].[DOCID] AS [DOCID],
        [t_fin_document_item].[ITEMID] AS [ITEMID],
        [t_fin_document_item].[ACCOUNTID] AS [ACCOUNTID],
        [t_fin_document_item].[TRANTYPE] AS [TRANTYPE],
		[t_fin_tran_type].[NAME] AS [TRANTYPENAME],
		[t_fin_tran_type].[EXPENSE] AS [TRANTYPE_EXP],
		[t_fin_document_item].[USECURR2] AS [USECURR2],
        (case when ([t_fin_document_item].[USECURR2] is null or [t_fin_document_item].[USECURR2] = '') then ([t_fin_document].[TRANCURR])
        else ([t_fin_document].[TRANCURR2])
        end) AS [TRANCURR],
        [t_fin_document_item].[TRANAMOUNT] as [TRANAMOUNT_ORG],
        (case
            when ([t_fin_tran_type].[EXPENSE] = 1) then ([t_fin_document_item].[TRANAMOUNT] * -(1))
            when ([t_fin_tran_type].[EXPENSE] = 0) then [t_fin_document_item].[TRANAMOUNT]
        end) AS [TRANAMOUNT],
        (case when ([t_fin_document_item].[USECURR2] is null or [t_fin_document_item].[USECURR2] = '') 
			then (
                case when ([t_fin_document].[EXGRATE] IS NOT NULL) then (
					case
						when ([t_fin_tran_type].[EXPENSE] = 1) then ([t_fin_document_item].[TRANAMOUNT] / [t_fin_document].[EXGRATE]  * -(1))
						when ([t_fin_tran_type].[EXPENSE] = 0) then [t_fin_document_item].[TRANAMOUNT] / [t_fin_document].[EXGRATE]
					end)
                else (
                case
					when ([t_fin_tran_type].[EXPENSE] = 1) then ([t_fin_document_item].[TRANAMOUNT] * -(1))
					when ([t_fin_tran_type].[EXPENSE] = 0) then [t_fin_document_item].[TRANAMOUNT]
				end)
                end)
        else ( case when ([t_fin_document].[EXGRATE2] IS NOT NULL) then (
					case
						when ([t_fin_tran_type].[EXPENSE] = 1) then ([t_fin_document_item].[TRANAMOUNT] / [t_fin_document].[EXGRATE2]  * -(1))
						when ([t_fin_tran_type].[EXPENSE] = 0) then [t_fin_document_item].[TRANAMOUNT] / [t_fin_document].[EXGRATE2]
					end)
                else (
					case
						when ([t_fin_tran_type].[EXPENSE] = 1) then ([t_fin_document_item].[TRANAMOUNT] * -(1))
						when ([t_fin_tran_type].[EXPENSE] = 0) then [t_fin_document_item].[TRANAMOUNT]
					end)
                end)
        end) AS [TRANAMOUNT_LC],
        [t_fin_document_item].[CONTROLCENTERID] AS [CONTROLCENTERID],
        [t_fin_document_item].[ORDERID] AS [ORDERID],
        [t_fin_document_item].[DESP] AS [DESP]
    from
        [t_fin_document_item]
		join [t_fin_tran_type] on [t_fin_document_item].[TRANTYPE] = [t_fin_tran_type].[ID]
        left outer join [t_fin_document] on [t_fin_document_item].[DOCID] = [t_fin_document].[ID]

GO

/****** Object:  View [dbo].[v_fin_document_item1]    Script Date: 2017-04-21 10:40:00 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_document_item1]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_document_item1]
AS 
select 
        [v_fin_document_item].[DOCID],
        [v_fin_document_item].[ITEMID],
        [v_fin_document_item].[ACCOUNTID],
		[t_fin_account].[NAME] AS [ACCOUNTNAME],
        [v_fin_document_item].[TRANTYPE],
		[v_fin_document_item].[TRANTYPENAME],
		[v_fin_document_item].[TRANTYPE_EXP],
		[v_fin_document_item].[USECURR2],
        [v_fin_document_item].[TRANCURR],
        [v_fin_document_item].[TRANAMOUNT_ORG],
        [v_fin_document_item].[TRANAMOUNT],
        [v_fin_document_item].[TRANAMOUNT_LC],
        [v_fin_document_item].[CONTROLCENTERID],
		[t_fin_controlcenter].[NAME] AS [CONTROLCENTERNAME],
        [v_fin_document_item].[ORDERID],
		[t_fin_order].[NAME] AS [ORDERNAME],
        [v_fin_document_item].[DESP]
    from
        [v_fin_document_item]
		left outer join [t_fin_account] on [v_fin_document_item].[ACCOUNTID] = [t_fin_account].[ID]
		left outer join [t_fin_controlcenter] on [v_fin_document_item].[CONTROLCENTERID] = [t_fin_controlcenter].[ID]
		left outer join [t_fin_order] on [v_fin_document_item].[ORDERID] = [t_fin_order].[ID];
GO


/****** Object:  View [dbo].[v_fin_document]    Script Date: 2017-02-20 12:14:08 AM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_document]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create view [dbo].[v_fin_document]
as
 select 
        [t_fin_document].[ID] AS [ID],
        [t_fin_document].[DOCTYPE] AS [DOCTYPE],
		[t_fin_doc_type].[NAME] AS [DOCTYPENAME],
        [t_fin_document].[TRANDATE] AS [TRANDATE],
        [t_fin_document].[TRANCURR] AS [TRANCURR],
        [t_fin_document].[DESP] AS [DESP],
        [t_fin_document].[EXGRATE] AS [EXGRATE],
        [t_fin_document].[EXGRATE_PLAN] AS [EXGRATE_PLAN],
        [t_fin_document].[TRANCURR2] AS [TRANCURR2],
        [t_fin_document].[EXGRATE2] AS [EXGRATE2],
        [t_fin_document].[EXGRATE_PLAN2] AS [EXGRATE_PLAN2],
		[t_fin_document].[CREATEDBY],
		[t_fin_document].[CREATEDAT],
		[t_fin_document].[UPDATEDBY],
		[t_fin_document].[UPDATEDAT],
        [item_table].[TRANAMOUNT]
    from
        [t_fin_document]
		left outer join [t_fin_doc_type] on [t_fin_document].[DOCTYPE] = [t_fin_doc_type].[ID]
        left outer join (select [DOCID] AS [id], sum([tranamount_lc]) AS [tranamount] from [v_fin_document_item1] 
			group by [DOCID]
		) as item_table on ([t_fin_document].[ID] = [item_table].[id])  
    where [t_fin_document].[DOCTYPE] != 3 AND [t_fin_document].[DOCTYPE] != 2    
    
    union all
    
    select 
        [t_fin_document].[ID] AS [ID],
        [t_fin_document].[DOCTYPE] AS [DOCTYPE],
		[t_fin_doc_type].[NAME] AS [DOCTYPENAME],
        [t_fin_document].[TRANDATE] AS [TRANDATE],
        [t_fin_document].[TRANCURR] AS [TRANCURR],
        [t_fin_document].[DESP] AS [DESP],
        [t_fin_document].[EXGRATE] AS [EXGRATE],
        [t_fin_document].[EXGRATE_PLAN] AS [EXGRATE_PLAN],
        [t_fin_document].[TRANCURR2] AS [TRANCURR2],
        [t_fin_document].[EXGRATE2] AS [EXGRATE2],
        [t_fin_document].[EXGRATE_PLAN2] AS [EXGRATE_PLAN2],
		[t_fin_document].[CREATEDBY],
		[t_fin_document].[CREATEDAT],
		[t_fin_document].[UPDATEDBY],
		[t_fin_document].[UPDATEDAT],
        0 AS [TRANAMOUNT]
    from
        [t_fin_document]
		left outer  join [t_fin_doc_type] on [t_fin_document].[DOCTYPE] = [t_fin_doc_type].[ID]
    where [t_fin_document].[DOCTYPE] = 3 OR [t_fin_document].[DOCTYPE] = 2;

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
