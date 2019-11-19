
-- History table
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
) ON [PRIMARY];

ALTER TABLE [dbo].[t_fin_account_ext_loan_h] ADD  DEFAULT (getdate()) FOR [EndDate];

ALTER TABLE [dbo].[t_fin_account_ext_loan_h]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_loan_h_ID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE;

ALTER TABLE [dbo].[t_fin_account_ext_loan_h] CHECK CONSTRAINT [FK_t_fin_account_ext_loan_h_ID];

-----------------------------------------------------------------------------------------------------------------------------------------------
-- Table of CreditCard

CREATE TABLE [dbo].[t_fin_account_ext_cc](
	[ACCOUNTID] [int] NOT NULL,
	[BILLDAYINMONTH] [smallint] NOT NULL,
	[REPAYDAYINMONTH] [smallint] NOT NULL,
	[CARDNUM] [nvarchar](20) NOT NULL,
	[OTHERS] [nvarchar](100) NULL,
	[BANK] [nvarchar](50) NULL,
	[VALIDDATE] [datetime] NULL,
 CONSTRAINT [PK_t_fin_account_ext_cc] PRIMARY KEY CLUSTERED 
(
	[ACCOUNTID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[t_fin_account_ext_cc]  WITH CHECK ADD  CONSTRAINT [FK_t_fin_account_ext_cc_ID] FOREIGN KEY([ACCOUNTID])
REFERENCES [dbo].[t_fin_account] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE;

ALTER TABLE [dbo].[t_fin_account_ext_cc] CHECK CONSTRAINT [FK_t_fin_account_ext_cc_ID];


-----------------------------------------------------------------------------------------------------------------------------------------------
-- Content part
-- Account Category
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (11,N'Sys.AcntCty.AdvancedRecv',0,N'预收款');
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Doc. Type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (12,N'Sys.DocTy.AdvancedRecv', N'预收款');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;


-- New version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (7,'2018.10.10');

-- The end.