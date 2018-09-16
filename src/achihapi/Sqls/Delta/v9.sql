
-- Learn category
SET IDENTITY_INSERT dbo.[t_learn_ctgy] ON;

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (16,NULL,N'摄影',N'摄影相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (17,16,N'摄影前期',N'前期相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (18,16,N'摄影后期',N'后期相关');

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (19,NULL,N'IT',N'计算机相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (20,19,N'基础理论',N'基础理论');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (21,19,N'硬件',N'计算机硬件');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (22,19,N'软件',N'计算机软件');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (23,22,N'编程语言',N'编程语言');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (24,22,N'数据库',N'数据库类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (25,22,N'操作系统',N'操作系统类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (26,19,N'网络',N'网络相关');

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (27,NULL,N'财务类',N'财务相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (28,27,N'财务会计',N'财务会计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (29,27,N'成本会计',N'成本会计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (30,27,N'审计',N'审计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (31,27,N'税法',N'税法');

SET IDENTITY_INSERT dbo.[t_learn_ctgy] OFF;

-- New version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (9,'2018.11.2');

-- The end.