USE [alvachiendb]
GO

CREATE TABLE [TodoItem] (
    [ItemID] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
	[Content] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TodoItem] PRIMARY KEY ([ItemId])
);
GO

CREATE TABLE [EnPartOfSpeech] (
	[POSID] int NOT NULL,
	[POSName] nvarchar(50) NOT NULL,
	CONSTRAINT [PK_EnPartOfSpeech] PRIMARY KEY ([POSID]),
);
GO

CREATE TABLE [EnWord] (
    [WordID] int NOT NULL IDENTITY,
    [WordString] int NOT NULL,
    [Tags] nvarchar(max),
    CONSTRAINT [PK_EnWord] PRIMARY KEY ([WordID])
);
GO

CREATE TABLE [EnWordExplain] (
	[WordID] int NOT NULL,
	[SeqID] int NOT NULL,
	[POSID] int NOT NULL,
	[Explain] nvarchar(200) NOT NULL
    CONSTRAINT [PK_EnWordExplain] PRIMARY KEY ([WordID], [SeqID]),
    CONSTRAINT [FK_EnWordExplain_WordID] FOREIGN KEY ([WordID]) REFERENCES [EnWord] ([WordID]) ON DELETE CASCADE
);
GO
