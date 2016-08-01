USE [alvachiendb]
GO
/****** Object:  Table [dbo].[GalleryAlbum]    Script Date: 7/19/2016 11:50:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GalleryAlbum](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Comment] [nvarchar](50) NULL,
 CONSTRAINT [PK_GalleryAlbum] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_GalleryAlbum_Name]    Script Date: 7/19/2016 11:50:25 AM ******/
CREATE UNIQUE CLUSTERED INDEX [IX_GalleryAlbum_Name] ON [dbo].[GalleryAlbum]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Table [dbo].[GalleryAlbumItems]    Script Date: 7/19/2016 11:50:25 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GalleryAlbumItems](
	[AlbumID] [int] NOT NULL,
	[PhotoID] [int] NOT NULL,
 CONSTRAINT [PK_GalleryAlbumItems] PRIMARY KEY CLUSTERED 
(
	[AlbumID] ASC,
	[PhotoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GalleryItem]    Script Date: 7/19/2016 11:50:26 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GalleryItem](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[FullURL] [nvarchar](100) NULL,
	[IsPublic] [bit] NOT NULL,
	[FileFormat] [nchar](10) NULL,
	[UploadedBy] [nvarchar](50) NULL,
	[UploadedDate] [datetime] NULL,
	[Comment] [nvarchar](100) NULL,
	[CameraInfo] [nvarchar](50) NULL,
	[LensInfo] [nvarchar](50) NULL,
	[EXIFInfo] [nchar](100) NULL,
	[Tags] [nvarchar](50) NULL,
 CONSTRAINT [PK_GalleryItem] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_GalleryItem_Name]    Script Date: 7/19/2016 11:50:32 AM ******/
CREATE UNIQUE CLUSTERED INDEX [IX_GalleryItem_Name] ON [dbo].[GalleryItem]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

ALTER TABLE [dbo].[GalleryAlbumItems]  WITH CHECK ADD  CONSTRAINT [FK_GalleryAlbumItems_Album] FOREIGN KEY([AlbumID])
REFERENCES [dbo].[GalleryAlbum] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GalleryAlbumItems] CHECK CONSTRAINT [FK_GalleryAlbumItems_Album]
GO
ALTER TABLE [dbo].[GalleryAlbumItems]  WITH CHECK ADD  CONSTRAINT [FK_GalleryAlbumItems_Photo] FOREIGN KEY([PhotoID])
REFERENCES [dbo].[GalleryItem] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GalleryAlbumItems] CHECK CONSTRAINT [FK_GalleryAlbumItems_Photo]
GO
