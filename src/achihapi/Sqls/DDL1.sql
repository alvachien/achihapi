USE [achihdb]
GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'VEnWord', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnWord'

GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'VEnWord', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnWord'

GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'VEnSentence', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnSentence'

GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'VEnSentence', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnSentence'

GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'VEnPOS', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnPOS'

GO
IF  EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'VEnPOS', NULL,NULL))
EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VEnPOS'

GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnWordExplainT]') AND type in (N'U'))
ALTER TABLE [dbo].[EnWordExplainT] DROP CONSTRAINT IF EXISTS [FK_EnWordExplainT_WordExplain]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnWordExplain]') AND type in (N'U'))
ALTER TABLE [dbo].[EnWordExplain] DROP CONSTRAINT IF EXISTS [FK_EnWordExplain_Word]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]') AND type in (N'U'))
ALTER TABLE [dbo].[EnSentenceWord] DROP CONSTRAINT IF EXISTS [FK_EnSentenceWord_Word]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]') AND type in (N'U'))
ALTER TABLE [dbo].[EnSentenceWord] DROP CONSTRAINT IF EXISTS [FK_EnSentenceWord_Sentence]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceExplainT]') AND type in (N'U'))
ALTER TABLE [dbo].[EnSentenceExplainT] DROP CONSTRAINT IF EXISTS [FK_EnSentenceExplainT_SentenceExplain]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceExplain]') AND type in (N'U'))
ALTER TABLE [dbo].[EnSentenceExplain] DROP CONSTRAINT IF EXISTS [FK_EnSentenceExplain_EnSentence]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnPOST]') AND type in (N'U'))
ALTER TABLE [dbo].[EnPOST] DROP CONSTRAINT IF EXISTS [FK_EnPOST_POS]
GO
/****** Object:  Table [dbo].[TodoItem]    Script Date: 6/6/2016 1:42:59 PM ******/
DROP TABLE IF EXISTS [dbo].[TodoItem]
GO
/****** Object:  Table [dbo].[EnSentenceWord]    Script Date: 6/6/2016 1:42:59 PM ******/
DROP TABLE IF EXISTS [dbo].[EnSentenceWord]
GO
/****** Object:  View [dbo].[VEnSentence]    Script Date: 6/6/2016 1:42:59 PM ******/
DROP VIEW IF EXISTS [dbo].[VEnSentence]
GO
/****** Object:  Table [dbo].[EnSentenceExplainT]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnSentenceExplainT]
GO
/****** Object:  Table [dbo].[EnSentenceExplain]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnSentenceExplain]
GO
/****** Object:  Table [dbo].[EnSentence]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnSentence]
GO
/****** Object:  View [dbo].[VEnWord]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP VIEW IF EXISTS [dbo].[VEnWord]
GO
/****** Object:  Table [dbo].[EnWordExplain]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnWordExplain]
GO
/****** Object:  Table [dbo].[EnWordExplainT]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnWordExplainT]
GO
/****** Object:  Table [dbo].[EnWord]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnWord]
GO
/****** Object:  View [dbo].[VEnPOS]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP VIEW IF EXISTS [dbo].[VEnPOS]
GO
/****** Object:  Table [dbo].[EnPOST]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[EnPOST]
GO
/****** Object:  Table [dbo].[ENPOS]    Script Date: 6/6/2016 1:43:00 PM ******/
DROP TABLE IF EXISTS [dbo].[ENPOS]
GO
/****** Object:  Table [dbo].[ENPOS]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ENPOS]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ENPOS](
	[POSAbb] [nvarchar](10) NOT NULL,
	[POSName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ENPOS] PRIMARY KEY CLUSTERED 
(
	[POSAbb] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnPOST]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnPOST]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnPOST](
	[POSAbb] [nvarchar](10) NOT NULL,
	[LangID] [nvarchar](5) NOT NULL,
	[POSName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_EnPOST] PRIMARY KEY CLUSTERED 
(
	[POSAbb] ASC,
	[LangID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[VEnPOS]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VEnPOS]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[VEnPOS]
AS
SELECT        dbo.ENPOS.*, dbo.EnPOST.LangID, dbo.EnPOST.POSName AS NativeName
FROM            dbo.ENPOS INNER JOIN
                         dbo.EnPOST ON dbo.ENPOS.POSAbb = dbo.EnPOST.POSAbb
' 
GO
/****** Object:  Table [dbo].[EnWord]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnWord]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnWord](
	[WordID] [int] IDENTITY(1,1) NOT NULL,
	[WordString] [nvarchar](100) NOT NULL,
	[Tags] [nvarchar](50) NULL,
 CONSTRAINT [PK_EnWords] PRIMARY KEY CLUSTERED 
(
	[WordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnWordExplainT]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnWordExplainT]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnWordExplainT](
	[WordID] [int] NOT NULL,
	[ExplainID] [int] NOT NULL,
	[LangID] [nvarchar](5) NOT NULL,
	[ExplainString] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_EnWordExplainT] PRIMARY KEY CLUSTERED 
(
	[WordID] ASC,
	[ExplainID] ASC,
	[LangID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnWordExplain]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnWordExplain]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnWordExplain](
	[WordID] [int] NOT NULL,
	[ExplainID] [int] NOT NULL,
	[POSAbb] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_EnWordExplain] PRIMARY KEY CLUSTERED 
(
	[WordID] ASC,
	[ExplainID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[VEnWord]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VEnWord]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[VEnWord]
AS
SELECT        dbo.EnWord.WordID, dbo.EnWord.WordString, dbo.EnWord.Tags, dbo.EnWordExplain.ExplainID, dbo.EnWordExplain.POSAbb, dbo.EnWordExplainT.LangID, dbo.EnWordExplainT.ExplainString
FROM            dbo.EnWord INNER JOIN
                         dbo.EnWordExplain ON dbo.EnWord.WordID = dbo.EnWordExplain.WordID INNER JOIN
                         dbo.EnWordExplainT ON dbo.EnWordExplain.WordID = dbo.EnWordExplainT.WordID AND dbo.EnWordExplain.ExplainID = dbo.EnWordExplainT.ExplainID
' 
GO
/****** Object:  Table [dbo].[EnSentence]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentence]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnSentence](
	[SentenceID] [int] NOT NULL,
	[SentenceString] [nvarchar](200) NOT NULL,
	[Tags] [nvarchar](100) NULL,
 CONSTRAINT [PK_EnSentence] PRIMARY KEY CLUSTERED 
(
	[SentenceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnSentenceExplain]    Script Date: 6/6/2016 1:43:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceExplain]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnSentenceExplain](
	[SentenceID] [int] NOT NULL,
	[ExplainID] [int] NOT NULL,
 CONSTRAINT [PK_EnSentenceExplain] PRIMARY KEY CLUSTERED 
(
	[SentenceID] ASC,
	[ExplainID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[EnSentenceExplainT]    Script Date: 6/6/2016 1:43:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceExplainT]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnSentenceExplainT](
	[SentenceID] [int] NOT NULL,
	[ExplainID] [int] NOT NULL,
	[LangID] [nvarchar](5) NOT NULL,
	[ExplainString] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_EnSentenceExplainT] PRIMARY KEY CLUSTERED 
(
	[SentenceID] ASC,
	[ExplainID] ASC,
	[LangID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[VEnSentence]    Script Date: 6/6/2016 1:43:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VEnSentence]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[VEnSentence]
AS
SELECT        dbo.EnSentence.*, dbo.EnSentenceExplain.ExplainID, dbo.EnSentenceExplainT.LangID, dbo.EnSentenceExplainT.ExplainString
FROM            dbo.EnSentence INNER JOIN
                         dbo.EnSentenceExplain ON dbo.EnSentence.SentenceID = dbo.EnSentenceExplain.SentenceID INNER JOIN
                         dbo.EnSentenceExplainT ON dbo.EnSentence.SentenceID = dbo.EnSentenceExplainT.SentenceID
' 
GO
/****** Object:  Table [dbo].[EnSentenceWord]    Script Date: 6/6/2016 1:43:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EnSentenceWord](
	[SentenceID] [int] NOT NULL,
	[WordID] [int] NOT NULL,
 CONSTRAINT [PK_EnSentenceWord] PRIMARY KEY CLUSTERED 
(
	[SentenceID] ASC,
	[WordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[TodoItem]    Script Date: 6/6/2016 1:43:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TodoItem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TodoItem](
	[ToDoID] [int] NOT NULL,
	[ItemName] [nvarchar](50) NOT NULL,
	[Priority] [int] NOT NULL,
	[Assignee] [nvarchar](50) NULL,
	[Dependence] [nvarchar](50) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[ItemContent] [nvarchar](max) NULL,
	[Tags] [nvarchar](50) NULL,
 CONSTRAINT [PK_TodoItem] PRIMARY KEY CLUSTERED 
(
	[ToDoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'adj.', N'adjective')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'adv.', N'adverb')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'art.', N'article')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'conj.', N'conjunction')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'interj.', N'interjection')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'n.', N'noun')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'num.', N'numeral')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'prep.', N'preposition')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'pron.', N'pronoun')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'v.', N'verb')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'vi.', N'verb intransitive')
INSERT [dbo].[ENPOS] ([POSAbb], [POSName]) VALUES (N'vt.', N'verb transitive')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'adj.', N'zh', N'形容词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'adv.', N'zh', N'副词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'art.', N'zh', N'冠词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'conj.', N'zh', N'连词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'interj.', N'zh', N'感叹词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'n.', N'zh', N'名词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'num.', N'zh', N'数词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'prep.', N'zh', N'介词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'v.', N'zh', N'动词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'vi.', N'zh', N'不及物动词')
INSERT [dbo].[EnPOST] ([POSAbb], [LangID], [POSName]) VALUES (N'vt.', N'zh', N'及物动词')
SET IDENTITY_INSERT [dbo].[EnWord] ON 

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnPOST_POS]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnPOST]'))
ALTER TABLE [dbo].[EnPOST]  WITH CHECK ADD  CONSTRAINT [FK_EnPOST_POS] FOREIGN KEY([POSAbb])
REFERENCES [dbo].[ENPOS] ([POSAbb])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnPOST_POS]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnPOST]'))
ALTER TABLE [dbo].[EnPOST] CHECK CONSTRAINT [FK_EnPOST_POS]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceExplain_EnSentence]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceExplain]'))
ALTER TABLE [dbo].[EnSentenceExplain]  WITH CHECK ADD  CONSTRAINT [FK_EnSentenceExplain_EnSentence] FOREIGN KEY([SentenceID])
REFERENCES [dbo].[EnSentence] ([SentenceID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceExplain_EnSentence]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceExplain]'))
ALTER TABLE [dbo].[EnSentenceExplain] CHECK CONSTRAINT [FK_EnSentenceExplain_EnSentence]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceExplainT_SentenceExplain]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceExplainT]'))
ALTER TABLE [dbo].[EnSentenceExplainT]  WITH CHECK ADD  CONSTRAINT [FK_EnSentenceExplainT_SentenceExplain] FOREIGN KEY([SentenceID], [ExplainID])
REFERENCES [dbo].[EnSentenceExplain] ([SentenceID], [ExplainID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceExplainT_SentenceExplain]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceExplainT]'))
ALTER TABLE [dbo].[EnSentenceExplainT] CHECK CONSTRAINT [FK_EnSentenceExplainT_SentenceExplain]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceWord_Sentence]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]'))
ALTER TABLE [dbo].[EnSentenceWord]  WITH CHECK ADD  CONSTRAINT [FK_EnSentenceWord_Sentence] FOREIGN KEY([SentenceID])
REFERENCES [dbo].[EnSentence] ([SentenceID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceWord_Sentence]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]'))
ALTER TABLE [dbo].[EnSentenceWord] CHECK CONSTRAINT [FK_EnSentenceWord_Sentence]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceWord_Word]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]'))
ALTER TABLE [dbo].[EnSentenceWord]  WITH CHECK ADD  CONSTRAINT [FK_EnSentenceWord_Word] FOREIGN KEY([WordID])
REFERENCES [dbo].[EnWord] ([WordID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnSentenceWord_Word]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnSentenceWord]'))
ALTER TABLE [dbo].[EnSentenceWord] CHECK CONSTRAINT [FK_EnSentenceWord_Word]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnWordExplain_Word]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnWordExplain]'))
ALTER TABLE [dbo].[EnWordExplain]  WITH CHECK ADD  CONSTRAINT [FK_EnWordExplain_Word] FOREIGN KEY([WordID])
REFERENCES [dbo].[EnWord] ([WordID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnWordExplain_Word]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnWordExplain]'))
ALTER TABLE [dbo].[EnWordExplain] CHECK CONSTRAINT [FK_EnWordExplain_Word]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnWordExplainT_WordExplain]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnWordExplainT]'))
ALTER TABLE [dbo].[EnWordExplainT]  WITH CHECK ADD  CONSTRAINT [FK_EnWordExplainT_WordExplain] FOREIGN KEY([WordID], [ExplainID])
REFERENCES [dbo].[EnWordExplain] ([WordID], [ExplainID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EnWordExplainT_WordExplain]') AND parent_object_id = OBJECT_ID(N'[dbo].[EnWordExplainT]'))
ALTER TABLE [dbo].[EnWordExplainT] CHECK CONSTRAINT [FK_EnWordExplainT_WordExplain]
GO
