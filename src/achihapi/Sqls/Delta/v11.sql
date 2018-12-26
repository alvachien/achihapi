
-- Finance Plan
/****** Object:  Table [dbo].[t_fin_plan]    Script Date: 2018-12-18 12:04:46 PM ******/

CREATE TABLE [dbo].[t_fin_plan](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[HID] [int] NOT NULL,
	[PTYPE] [tinyint] NOT NULL,
	[ACCOUNTID] [int] NULL,
	[ACNTCTGYID] [int] NULL,
	[CCID] [int] NULL,
	[TTID] [int] NULL,
	[STARTDATE] [date] NOT NULL,
	[TGTDATE] [date] NOT NULL,
	[TGTBAL] [decimal](17, 2) NOT NULL,
	[TRANCURR] [nvarchar](5) NOT NULL,
	[DESP] [nvarchar](50) NOT NULL,
	[CREATEDBY] [nvarchar](50) NULL,
	[CREATEDAT] [date] NULL,
	[UPDATEDBY] [nvarchar](50) NULL,
	[UPDATEDAT] [date] NULL,
 CONSTRAINT [PK_t_fin_plan] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[t_fin_plan] ADD  CONSTRAINT [DF_t_fin_plan_PTYPE]  DEFAULT ((0)) FOR [PTYPE];

ALTER TABLE [dbo].[t_fin_plan]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_plan_HID] FOREIGN KEY([HID])
REFERENCES [dbo].[t_homedef] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
;

ALTER TABLE [dbo].[t_fin_plan] CHECK CONSTRAINT [FK_t_fin_plan_HID];

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (11,'2018.12.20');

-- The end.
