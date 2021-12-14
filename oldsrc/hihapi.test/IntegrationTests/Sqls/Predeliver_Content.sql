
-- Content for learn category, updated at 2017.9.9
SET IDENTITY_INSERT dbo.[t_learn_ctgy] ON;

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (1,NULL,N'语文',N'语文');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (2,1,N'诗词',N'唐诗宋词等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (3,1,N'识字',N'拼音认读和笔画等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (4,1,N'文言文',N'文言文等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (5,1,N'古典名著',N'古典名著等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (6,NULL,N'数学',N'数学类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (7,6,N'算术',N'加减法');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (8,6,N'代数',N'代数');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (9,6,N'几何',N'几何类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (10,NULL,N'英语',N'英语类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (11,10,N'词汇',N'英语词汇');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (12,10,N'语法',N'英语语法');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (13,NULL,N'日语',N'日语类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (14,13,N'词汇',N'日语词汇');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (15,13,N'语法',N'日语语法');

SET IDENTITY_INSERT dbo.[t_learn_ctgy] OFF;

-- Update at 2017.10.15
SET IDENTITY_INSERT dbo.[t_lib_book_ctgy] ON;

DELETE FROM dbo.[t_lib_book_ctgy] WHERE [HID] IS NULL;
INSERT INTO [dbo].[t_lib_book_ctgy] ([ID], [NAME],[PARID], [OTHERS]) VALUES (1, N'Sys.BkCtgy.Novel', NULL, N'小说');

SET IDENTITY_INSERT dbo.[t_lib_book_ctgy] OFF;


-- Updated at 2018.6.6
SET IDENTITY_INSERT [dbo].[t_lib_movie_genre] ON;

INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (1,NULL,N'Sys.MovGenre.ActAdv',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (2,NULL,N'Sys.MovGenre.Animation',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (3,NULL,N'Sys.MovGenre.Anime',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (4,NULL,N'Sys.MovGenre.Classics',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (5,NULL,N'Sys.MovGenre.Comedy',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (6,NULL,N'Sys.MovGenre.Documentary',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (7,NULL,N'Sys.MovGenre.Drama',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (8,NULL,N'Sys.MovGenre.ExerFit',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (9,NULL,N'Sys.MovGenre.FaithSpirit',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (10,NULL,N'Sys.MovGenre.ForeignLang',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (11,NULL,N'Sys.MovGenre.GayLesbian',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (12,NULL,N'Sys.MovGenre.Horror',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (13,NULL,N'Sys.MovGenre.IndianMv',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (14,NULL,N'Sys.MovGenre.IndieArt',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (15,NULL,N'Sys.MovGenre.KidsFamily',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (16,NULL,N'Sys.MovGenre.MVConcert',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (17,NULL,N'Sys.MovGenre.Romance',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (18,NULL,N'Sys.MovGenre.SciFiction',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (19,NULL,N'Sys.MovGenre.Western',NULL,NULL);

SET IDENTITY_INSERT [dbo].[t_lib_movie_genre] OFF;

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.9.15

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

