-- Insert tables for Blog
CREATE TABLE [dbo].[t_blog_format] (
    [ID]    INT NOT NULL,
    [Name]  NVARCHAR(10) NOT NULL,
    [Comment] NVARCHAR(50) NULL,
    CONSTRAINT [PK_t_blog_format] PRIMARY KEY ([ID] ASC)
);

CREATE TABLE [dbo].[t_blog_setting] (
    [Owner] NVARCHAR(40) NOT NULL,
    [Name]  NVARCHAR(50) NOT NULL,
    [Comment] NVARCHAR(50) NULL,
    [AllowComment] BIT NULL,
    CONSTRAINT [PK_t_blog_setting] PRIMARY KEY ([OWNER] ASC)
);


CREATE TABLE [dbo].[t_blog_coll] (
    [ID]    INT IDENTITY (1, 1) NOT NULL,
    [Owner] NVARCHAR(40) NOT NULL,
    [Name]  NVARCHAR(10) NOT NULL,
    [Comment] NVARCHAR(50) NULL,
    CONSTRAINT [PK_t_blog_coll] PRIMARY KEY ([ID] ASC)
);

CREATE TABLE [dbo].[t_blog_post] (
    [ID]  INT            IDENTITY (1, 1) NOT NULL,
    [Owner] NVARCHAR(40) NOT NULL,
    [FORMAT] INT           NOT NULL,
    [TITLE]  NVARCHAR (50) NOT NULL,
    [BRIEF] NVARCHAR(100) NOT NULL,
    [CONTENT] NVARCHAR (MAX) NOT NULL,
    [STATUS] INT CONSTRAINT [DF_t_blog_post_status] DEFAULT ((1)) NULL,
    [CreatedAt] DATE CONSTRAINT [DF_t_blog_post_CreatedAt] DEFAULT (getdate()) NOT NULL, 
    [UpdatedAt] DATE CONSTRAINT [DF_t_blog_post_UpdatedAt] DEFAULT (getdate()) NOT NULL, 
    CONSTRAINT [PK_t_blog_post] PRIMARY KEY CLUSTERED ([ID] ASC)
);

CREATE TABLE [dbo].[t_blog_post_coll] (
    [PostID]  INT NOT NULL,
    [CollID] INT NOT NULL,
    CONSTRAINT [PK_t_blog_post_coll] PRIMARY KEY CLUSTERED ([POSTID] ASC, [CollID] ASC),
    CONSTRAINT [FK_t_blog_post_coll_coll] FOREIGN KEY ([CollID]) REFERENCES [dbo].[t_blog_coll] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_t_blog_post_coll_post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[t_blog_post] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [dbo].[t_blog_post_tag] (
    [PostID]  INT NOT NULL,
    [Tag] NVARCHAR(20) NOT NULL,
    CONSTRAINT [PK_t_blog_post_tag] PRIMARY KEY CLUSTERED ([POSTID] ASC, [Tag] ASC),
    CONSTRAINT [FK_t_blog_post_tag_post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[t_blog_post] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE [dbo].[t_blog_post_reply] (
    [PostID]  INT NOT NULL,
    [ReplyID] INT NOT NULL,
    [RepliedBy] NVARCHAR(40) NOT NULL,
    [RepliedIP] nvarchar(20) NOT NULL,
    [TITLE]  NVARCHAR (100) NOT NULL,
    [CONTENT] NVARCHAR (200) NOT NULL,
    [RefReplyID] INT NULL,
    [CreatedAt] DATE CONSTRAINT [DF_t_blog_post_reply_CreatedAt] DEFAULT (getdate()) NOT NULL, 
    CONSTRAINT [PK_t_blog_post_reply] PRIMARY KEY CLUSTERED ([PostID] ASC, [ReplyID] ASC),
    CONSTRAINT [FK_t_blog_post_reply_post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[t_blog_post] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (16,'2020.04.15');

-- The end.
