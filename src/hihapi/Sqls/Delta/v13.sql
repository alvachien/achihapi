-- Remove unnecessary foreign keys

-- dbo.t_dbversion
IF EXISTS( select * from INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'T_DBVERSION'
	AND ( COLUMN_NAME = 'ReleasedDate' OR COLUMN_NAME = 'AppliedDate' )
	AND DATA_TYPE = 'datetime' )
BEGIN
	ALTER TABLE dbo.t_dbversion
	 ALTER COLUMN  ReleasedDate date not null;
	ALTER TABLE dbo.t_dbversion DROP CONSTRAINT DF_t_dbversion_AppliedDate;
	ALTER TABLE dbo.t_dbversion
	 ALTER COLUMN AppliedDate date null;
	ALTER TABLE dbo.t_dbversion
	 ADD CONSTRAINT DF_t_dbversion_AppliedDate DEFAULT (getdate()) FOR AppliedDate;
END;

-- dbo.t_event
IF EXISTS( select * from INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_event'
	AND ( COLUMN_NAME = 'StartTime' OR COLUMN_NAME = 'EndTime' OR COLUMN_NAME = 'CompleteTime' )
	AND DATA_TYPE = 'datetime' )
BEGIN
	ALTER TABLE dbo.t_event DROP CONSTRAINT DF_t_event_StartTime;
	ALTER TABLE dbo.t_event
	 ALTER COLUMN  StartTime date not null;

	ALTER TABLE dbo.t_event
	 ALTER COLUMN  EndTime date null;

	ALTER TABLE dbo.t_event
	 ALTER COLUMN  CompleteTime date null;

	ALTER TABLE dbo.t_event ADD CONSTRAINT DF_t_event_StartTime DEFAULT (getdate()) FOR StartTime;
END;

-- dbo.t_event_habit_checkin
IF EXISTS( select * from INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_event_habit_checkin'
	AND ( COLUMN_NAME = 'TranDate' )
	AND DATA_TYPE = 'datetime' )
BEGIN
	ALTER TABLE dbo.t_event_habit_checkin
	 ALTER COLUMN  TranDate date not null;
END;

-- dbo.t_fin_account_ext_as
IF EXISTS( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_account_ext_as'
	AND CONSTRAINT_NAME = 'FK_t_fin_account_exp_as_ID'
	AND CONSTRAINT_TYPE = 'FOREIGN KEY' )
BEGIN 
	ALTER TABLE dbo.t_fin_account_ext_as
		DROP CONSTRAINT FK_t_fin_account_exp_as_ID;

	DELETE FROM dbo.t_fin_account_ext_as WHERE ACCOUNTID IN (
		SELECT ACCOUNTID from dbo.t_fin_account_ext_as
		WHERE ACCOUNTID NOT IN 	(SELECT ID from dbo.t_fin_account) 
	);

	ALTER TABLE dbo.t_fin_account_ext_as
		WITH CHECK
		ADD  CONSTRAINT FK_t_fin_account_ext_as_ACNT FOREIGN KEY(ACCOUNTID)
		REFERENCES [dbo].[t_fin_account] ([ID])
		ON UPDATE CASCADE
		ON DELETE CASCADE;

	ALTER TABLE dbo.t_fin_account_ext_as CHECK CONSTRAINT FK_t_fin_account_ext_as_ACNT;

	ALTER TABLE dbo.t_fin_account_ext_as 
		ADD  CONSTRAINT FK_t_fin_account_ext_as_CTGY FOREIGN KEY(CTGYID)
		REFERENCES dbo.t_fin_asset_ctgy (ID);
END;

-- dbo.t_fin_account_ext_cc
IF EXISTS ( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_account_ext_cc'
	AND CONSTRAINT_NAME = 'FK_t_fin_account_ext_cc_ID' )
BEGIN
	ALTER TABLE dbo.t_fin_account_ext_cc
		DROP CONSTRAINT FK_t_fin_account_ext_cc_ID;
	ALTER TABLE dbo.t_fin_account_ext_cc
		ADD CONSTRAINT FK_t_fin_account_ext_cc_ACNT FOREIGN KEY(ACCOUNTID)
		REFERENCES [dbo].[t_fin_account] ([ID])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
END;

-- dbo.t_fin_account_ext_dp
IF EXISTS ( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_account_ext_dp'
	AND CONSTRAINT_NAME = 'FK_t_fin_account_ext_dp_id' )
BEGIN
	ALTER TABLE dbo.t_fin_account_ext_dp
		DROP CONSTRAINT FK_t_fin_account_ext_dp_id;
	ALTER TABLE dbo.t_fin_account_ext_dp
		ADD CONSTRAINT FK_t_fin_account_ext_dp_ACNT FOREIGN KEY(ACCOUNTID)
		REFERENCES [dbo].[t_fin_account] ([ID])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
END;

-- dbo.t_fin_tmpdoc_dp
IF EXISTS( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_tmpdoc_dp'
	AND CONSTRAINT_NAME = 'FK_t_fin_tmpdocdp_HID' )
BEGIN 
	ALTER TABLE [dbo].[t_fin_tmpdoc_dp] DROP CONSTRAINT [FK_t_fin_tmpdocdp_HID];

	ALTER TABLE dbo.t_fin_tmpdoc_dp DROP CONSTRAINT DF_t_fin_tmpdoc_dp_CREATEDAT;

	ALTER TABLE dbo.t_fin_tmpdoc_dp DROP CONSTRAINT DF_t_fin_tmpdoc_dp_UPDATEDAT;

	CREATE TABLE dbo.Tmp_t_fin_tmpdoc_dp
	(
		DOCID int NOT NULL,
		ACCOUNTID int NOT NULL,
		HID int NOT NULL,
		REFDOCID int NULL,
		TRANDATE date NOT NULL,
		TRANTYPE int NOT NULL,
		TRANAMOUNT decimal(17, 2) NOT NULL,
		CONTROLCENTERID int NULL,
		ORDERID int NULL,
		DESP nvarchar(45) NULL,
		CREATEDBY nvarchar(40) NULL,
		CREATEDAT date NULL,
		UPDATEDBY nvarchar(40) NULL,
		UPDATEDAT date NULL
	);

	ALTER TABLE dbo.Tmp_t_fin_tmpdoc_dp ADD CONSTRAINT DF_t_fin_tmpdoc_dp_CREATEDAT DEFAULT (getdate()) FOR CREATEDAT;

	ALTER TABLE dbo.Tmp_t_fin_tmpdoc_dp ADD CONSTRAINT DF_t_fin_tmpdoc_dp_UPDATEDAT DEFAULT (getdate()) FOR UPDATEDAT;

	IF EXISTS(SELECT * FROM dbo.t_fin_tmpdoc_dp)
		EXEC('INSERT INTO dbo.Tmp_t_fin_tmpdoc_dp (DOCID, HID, REFDOCID, ACCOUNTID, TRANDATE, TRANTYPE, TRANAMOUNT, CONTROLCENTERID, ORDERID, DESP, CREATEDBY, CREATEDAT, UPDATEDBY, UPDATEDAT)
				SELECT DOCID, HID, REFDOCID, ACCOUNTID, TRANDATE, TRANTYPE, TRANAMOUNT, CONTROLCENTERID, ORDERID, DESP, CREATEDBY, CREATEDAT, UPDATEDBY, UPDATEDAT FROM dbo.t_fin_tmpdoc_dp');

	DROP TABLE dbo.t_fin_tmpdoc_dp;

	EXECUTE sp_rename N'dbo.Tmp_t_fin_tmpdoc_dp', N't_fin_tmpdoc_dp', 'OBJECT' ;

	ALTER TABLE dbo.t_fin_tmpdoc_dp ADD CONSTRAINT
		PK_t_fin_tmpdoc_dp PRIMARY KEY ( DOCID,  ACCOUNTID, HID );

	ALTER TABLE dbo.t_fin_tmpdoc_dp ADD CONSTRAINT 
		FK_t_fin_tmpdocdp_ACCOUNTEXT FOREIGN KEY ( ACCOUNTID ) 
			REFERENCES dbo.t_fin_account_ext_dp ( ACCOUNTID ) 
			 ON UPDATE  CASCADE 
			 ON DELETE  CASCADE;
END;

-- dbo.t_fin_account_ext_loan
IF EXISTS ( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_account_ext_loan'
	AND CONSTRAINT_NAME = 'FK_t_fin_account_ext_loan_ID' )
BEGIN
	ALTER TABLE dbo.t_fin_account_ext_loan
		DROP CONSTRAINT FK_t_fin_account_ext_loan_ID;
	ALTER TABLE dbo.t_fin_account_ext_loan
		ADD CONSTRAINT FK_t_fin_account_ext_loan_ACNT FOREIGN KEY(ACCOUNTID)
		REFERENCES [dbo].[t_fin_account] ([ID])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
END;

-- dbo.t_fin_account_ext_loan_h
IF EXISTS ( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_account_ext_loan_h'
	AND CONSTRAINT_NAME = 'FK_t_fin_account_ext_loan_h_ID' )
BEGIN
	ALTER TABLE dbo.t_fin_account_ext_loan_h
		DROP CONSTRAINT FK_t_fin_account_ext_loan_h_ID;
	ALTER TABLE dbo.t_fin_account_ext_loan_h
		ADD CONSTRAINT FK_t_fin_account_ext_loan_h_ACNT FOREIGN KEY(ACCOUNTID)
		REFERENCES [dbo].[t_fin_account] ([ID])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
END;

-- dbo.t_fin_tmpdoc_loan
IF EXISTS( select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE TABLE_NAME = 't_fin_tmpdoc_loan'
	AND CONSTRAINT_NAME = 'FK_t_fin_tmpdocloan_HID' )
BEGIN 
	ALTER TABLE [dbo].[t_fin_tmpdoc_loan] DROP CONSTRAINT [FK_t_fin_tmpdocloan_HID];

	ALTER TABLE [dbo].[t_fin_tmpdoc_loan] DROP CONSTRAINT [DF_t_fin_tmpdoc_loan_CREATEDAT];

	ALTER TABLE [dbo].[t_fin_tmpdoc_loan] DROP CONSTRAINT [DF_t_fin_tmpdoc_loan_UPDATEDAT];

	CREATE TABLE [dbo].[Tmp_t_fin_tmpdoc_loan](
		[DOCID] [int] NOT NULL,
		[ACCOUNTID] [int] NOT NULL,
		[HID] [int] NOT NULL,
		[REFDOCID] [int] NULL,
		[TRANDATE] [date] NOT NULL,
		[TRANAMOUNT] [decimal](17, 2) NOT NULL,
		[INTERESTAMOUNT] [decimal](17, 2) NULL,
		[CONTROLCENTERID] [int] NULL,
		[ORDERID] [int] NULL,
		[DESP] [nvarchar](45) NULL,
		[CREATEDBY] [nvarchar](40) NULL,
		[CREATEDAT] [date] NULL,
		[UPDATEDBY] [nvarchar](40) NULL,
		[UPDATEDAT] [date] NULL
	);

	ALTER TABLE [dbo].[t_fin_tmpdoc_loan] ADD CONSTRAINT [DF_t_fin_tmpdoc_loan_CREATEDAT] DEFAULT (getdate()) FOR CREATEDAT;

	ALTER TABLE [dbo].[t_fin_tmpdoc_loan] ADD CONSTRAINT [DF_t_fin_tmpdoc_loan_UPDATEDAT] DEFAULT (getdate()) FOR UPDATEDAT;


	IF EXISTS(SELECT * FROM dbo.t_fin_tmpdoc_loan)
		EXEC('INSERT INTO [dbo].[Tmp_t_fin_tmpdoc_loan] (
		[DOCID],[ACCOUNTID],[HID],[REFDOCID],[TRANDATE],[TRANAMOUNT],[INTERESTAMOUNT],[CONTROLCENTERID],[ORDERID],[DESP],[CREATEDBY],[CREATEDAT],[UPDATEDBY],[UPDATEDAT]
		) SELECT [DOCID],[ACCOUNTID],[HID],[REFDOCID],[TRANDATE],[TRANAMOUNT],[INTERESTAMOUNT],[CONTROLCENTERID],[ORDERID],[DESP],[CREATEDBY],[CREATEDAT],[UPDATEDBY],[UPDATEDAT]
		 FROM dbo.t_fin_tmpdoc_loan');

	DROP TABLE dbo.t_fin_tmpdoc_loan;

	EXECUTE sp_rename N'dbo.Tmp_t_fin_tmpdoc_loan', N't_fin_tmpdoc_loan', 'OBJECT' ;

	ALTER TABLE dbo.t_fin_tmpdoc_loan ADD CONSTRAINT
		PK_t_fin_tmpdoc_loan PRIMARY KEY ( DOCID,  ACCOUNTID, HID );

	ALTER TABLE dbo.t_fin_tmpdoc_loan ADD CONSTRAINT 
		FK_t_fin_tmpdoc_loan_ACCOUNTEXT FOREIGN KEY ( ACCOUNTID ) 
			REFERENCES dbo.t_fin_account_ext_loan ( ACCOUNTID ) 
			 ON UPDATE  CASCADE 
			 ON DELETE  CASCADE;
END;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (13,'2020.02.28');

-- The end.
