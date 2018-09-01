
-- History table
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

-----------------------------------------------------------------------------------------------------------------------------------------------
-- Table of CreditCard
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_fin_account_ext_cc](
	[ACCOUNTID] [int] NOT NULL,
	[BILLDATE] [smallint] NOT NULL,
	[LASTPAYDATE] [smallint] NOT NULL,
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

-- New version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (7,'2018.08.10');
