
/****** Object:  View [dbo].[v_homemember]    Script Date: 2017-09-06 11:25:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[v_homemember]
AS
	SELECT t_homedef.[ID], t_homedef.[NAME], t_homedef.[HOST], t_homedef.[DETAILS], t_homemem.[USER],
		t_homedef.[CREATEDBY], t_homedef.[CREATEDAT], t_homedef.[UPDATEDBY], t_homedef.[UPDATEDAT]
	 FROM dbo.t_homedef
		LEFT OUTER JOIN dbo.t_homemem ON t_homedef.[ID] = t_homemem.[HID]

GO

