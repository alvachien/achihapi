USE [alvachiendb]
GO
/****** Object:  Table [dbo].[Knowledge]    Script Date: 7/19/2016 11:50:32 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Knowledge](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ContentType] [smallint] NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Tags] [nchar](100) NULL,
	[CreatedAt] [datetime] NULL,
	[ModifiedAt] [datetime] NULL,
 CONSTRAINT [PK_Knowledge] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[KnowledgeType]    Script Date: 7/19/2016 11:50:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KnowledgeType](
	[ID] [smallint] NOT NULL,
	[ParentID] [smallint] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Comment] [nvarchar](100) NULL,
 CONSTRAINT [PK_KnowledgeType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [IX_KnowledgeTypeName] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Index [IX_KnowledgeTitle]    Script Date: 7/19/2016 11:50:42 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_KnowledgeTitle] ON [dbo].[Knowledge]
(
	[Title] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

ALTER TABLE [dbo].[Knowledge] ADD  CONSTRAINT [DF_Knowledge_CreatedAt]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Knowledge] ADD  CONSTRAINT [DF_Knowledge_ModifiedAt]  DEFAULT (getdate()) FOR [ModifiedAt]
GO
