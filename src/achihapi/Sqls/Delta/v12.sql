-- Learn history
ALTER TABLE [dbo].[t_learn_hist]
DROP CONSTRAINT [PK_t_learn_hist];   

ALTER TABLE [dbo].[t_learn_hist]
ADD CONSTRAINT [PK_t_learn_hist] PRIMARY KEY CLUSTERED ([HID] ASC,
	[USERID] ASC,
	[OBJECTID] ASC,
	[LEARNDATE] ASC
	);  

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (12,'2019.04.20');

-- The end.