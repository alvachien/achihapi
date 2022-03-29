
-- TBD.

-- Update T_FIN_ACCOUNT status
UPDATE t_fin_account SET [STATUS] = 0 WHERE STATUS IS NULL;

-- Change table
ALTER TABLE t_fin_account ALTER COLUMN [STATUS] TINYTINE NOT NULL;

-- Library: Person


-- Library: Book


-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (18,'2022.05.31');

-- The end.
