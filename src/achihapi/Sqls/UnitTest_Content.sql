-- Table: Home Define
SET IDENTITY_INSERT dbo.[t_homedef] ON;

INSERT INTO [dbo].[t_homedef]
           ([ID]
		   ,[NAME]
           ,[DETAILS]
           ,[HOST]
           ,[BASECURR]
           ,[CREATEDBY])
     VALUES
           (1
		   ,N'UnitTest'
           ,N'Unit Test'
           ,N'Tester'
           ,N'CNY'
           ,N'Tester');
SET IDENTITY_INSERT dbo.[t_homedef] OFF;

-- Table: Home Member
INSERT INTO [dbo].[t_homemem]
           ([HID]
           ,[USER]
           ,[DISPLAYAS]
           ,[RELT]
           ,[CREATEDBY])
     VALUES
           (1
           ,N'Tester'
           ,N'Tester'
           ,1
           ,N'Tester');

