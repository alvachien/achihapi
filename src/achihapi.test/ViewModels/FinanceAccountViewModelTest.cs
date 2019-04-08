using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;

namespace achihapi.test.ViewModels
{
    [TestClass]
    public class FinanceAccountViewModelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_NullInput()
        {
            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(null, null, "user1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_SameInstance()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc1, "user1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_DifferentAccountID()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 1;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_DifferentHID()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 3;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeltaUpdate_DifferentCategoryID()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 2;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
        }

        [TestMethod]
        public void DeltaUpdate_Name()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 3";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Name"));
        }
        [TestMethod]
        public void DeltaUpdate_Sql_Name()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 3";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaStringForUpdate(acc1, acc2, "user1");
            var tday = DateTime.Today.ToString("yyyy-MM-dd");
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account] SET [NAME] = N'Account 3',[UPDATEDAT] = '" + tday + "',[UPDATEDBY] = N'user1' WHERE [ID] = 2", rst);
        }

        [TestMethod]
        public void DeltaUpdate_Comment()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment 2";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Comment"));
        }
        [TestMethod]
        public void DeltaUpdate_Sql_Comment()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment 2";
            acc2.Status = FinanceAccountStatus.Normal;

            var rst = FinanceAccountViewModel.WorkoutDeltaStringForUpdate(acc1, acc2, "user1");
            var tday = DateTime.Today.ToString("yyyy-MM-dd");
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account] SET [COMMENT] = N'Comment 2',[UPDATEDAT] = '" + tday + "',[UPDATEDBY] = N'user1' WHERE [ID] = 2", rst);
        }

        [TestMethod]
        public void DeltaUpdate_Status()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Closed;

            var rst = FinanceAccountViewModel.WorkoutDeltaForUpdate(acc1, acc2, "user1");
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Status"));
        }

        [TestMethod]
        public void DeltaUpdate_Sql_Status()
        {
            var acc1 = new FinanceAccountViewModel();
            acc1.ID = 2;
            acc1.HID = 2;
            acc1.Name = "Account 2";
            acc1.CtgyID = 1;
            acc1.Comment = "Comment";
            acc1.Status = FinanceAccountStatus.Normal;
            var acc2 = new FinanceAccountViewModel();
            acc2.ID = 2;
            acc2.HID = 2;
            acc2.Name = "Account 2";
            acc2.CtgyID = 1;
            acc2.Comment = "Comment";
            acc2.Status = FinanceAccountStatus.Closed;

            var rst = FinanceAccountViewModel.WorkoutDeltaStringForUpdate(acc1, acc2, "user1");
            var tday = DateTime.Today.ToString("yyyy-MM-dd");
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account] SET [STATUS] = 1,[UPDATEDAT] = '" + tday + "',[UPDATEDBY] = N'user1' WHERE [ID] = 2", rst);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraDP_DeltaUpdate_NullInput()
        {
            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraDP_DeltaUpdate_SameInstance()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraDP_DeltaUpdate_DifferentAccountID()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;
            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 2;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(30);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac2);
        }

        [TestMethod]
        public void ExtraDP_DeltaUpdate_Comment()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test2";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(30);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Comment"));
        }
        [TestMethod]
        public void ExtraDP_DeltaUpdate_Sql_Comment()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test2";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(30);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_dp] SET [COMMENT] = N'Test2' WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        public void ExtraDP_DeltaUpdate_RptType()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Month;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(30);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("RptType"));
        }
        [TestMethod]
        public void ExtraDP_DeltaUpdate_Sql_RptType()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Month;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(30);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_dp] SET [RPTTYPE] = 0 WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        public void ExtraDP_DeltaUpdate_EndDate()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(40);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("EndDate"));
        }
        [TestMethod]
        public void ExtraDP_DeltaUpdate_Sql_EndDate()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today;
            ac2.EndDate = ac1.StartDate.AddDays(40);
            ac2.RefDocID = 111;

            var tday = ac2.EndDate.ToString("yyyy-MM-dd");
            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_dp] SET [ENDDATE] = '" + tday + "' WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        public void ExtraDP_DeltaUpdate_Dates()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today.AddDays(10);
            ac2.EndDate = ac1.StartDate.AddDays(40);
            ac2.RefDocID = 111;

            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(2, rst.Count);
            Assert.IsTrue(rst.ContainsKey("StartDate"));
            Assert.IsTrue(rst.ContainsKey("EndDate"));
        }
        [TestMethod]
        public void ExtraDP_DeltaUpdate_Sql_Dates()
        {
            var ac1 = new FinanceAccountExtDPViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.DefrrDays = "100";
            ac1.Direct = true;
            ac1.RptType = RepeatFrequency.Day;
            ac1.StartDate = DateTime.Today;
            ac1.EndDate = ac1.StartDate.AddDays(30);
            ac1.RefDocID = 111;

            var ac2 = new FinanceAccountExtDPViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.DefrrDays = "100";
            ac2.Direct = true;
            ac2.RptType = RepeatFrequency.Day;
            ac2.StartDate = DateTime.Today.AddDays(10);
            ac2.EndDate = ac1.StartDate.AddDays(40);
            ac2.RefDocID = 111;

            var sday = ac2.StartDate.ToString("yyyy-MM-dd");
            var tday = ac2.EndDate.ToString("yyyy-MM-dd");
            var rst = FinanceAccountExtDPViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_dp] SET [ENDDATE] = '" + tday + "',[STARTDATE] = '" + sday + "' WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraAS_DeltaUpdate_NullInput()
        {
            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraAS_DeltaUpdate_SameInstance()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraAS_DeltaUpdate_DifferentAccountID()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 2;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ExtraAS_DeltaUpdate_InvalidCategoryID()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 0;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac2);
        }

        [TestMethod]
        public void ExtraAS_DeltaUpdate_Name()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name2";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Name"));
        }
        [TestMethod]
        public void ExtraAS_DeltaUpdate_Sql_Name()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name2";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_as] SET [NAME] = N'Name2' WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        public void ExtraAS_DeltaUpdate_Comment()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test 2";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Comment"));
        }
        [TestMethod]
        public void ExtraAS_DeltaUpdate_Sql_Comment()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test 2";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_as] SET [COMMENT] = N'" + ac2.Comment + "' WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        public void ExtraAS_DeltaUpdate_RefDocForSold()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;
            ac2.RefDocForSold = 102;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaForUpdate(ac1, ac2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("RefDocForSold"));
        }
        [TestMethod]
        public void ExtraAS_DeltaUpdate_Sql_RefDocForSold()
        {
            var ac1 = new FinanceAccountExtASViewModel();
            ac1.AccountID = 1;
            ac1.Comment = "Test";
            ac1.CategoryID = 2;
            ac1.Name = "Name1";
            ac1.RefDocForBuy = 101;

            var ac2 = new FinanceAccountExtASViewModel();
            ac2.AccountID = 1;
            ac2.Comment = "Test";
            ac2.CategoryID = 2;
            ac2.Name = "Name1";
            ac2.RefDocForBuy = 101;
            ac2.RefDocForSold = 102;

            var rst = FinanceAccountExtASViewModel.WorkoutDeltaStringForUpdate(ac1, ac2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_account_ext_as] SET [REFDOC_SOLD] = " + ac2.RefDocForSold.ToString() + " WHERE [ACCOUNTID] = 1", rst);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraLoan_DeltaUpdate_NullInput()
        {
            var rst = FinanceAccountExtLoanViewModel.WorkoutDeltaForUpdate(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraLoan_DeltaUpdate_SameInstance()
        {
            var ac1 = new FinanceAccountExtLoanViewModel();
            ac1.AccountID = 1;
            ac1.InterestFree = true;
            ac1.Others = "Others";
            ac1.RefDocID = 101;

            var rst = FinanceAccountExtLoanViewModel.WorkoutDeltaForUpdate(ac1, ac1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtraLoan_DeltaUpdate_DifferentAccountID()
        {
            var ac1 = new FinanceAccountExtLoanViewModel();
            ac1.AccountID = 1;
            ac1.InterestFree = true;
            ac1.Others = "Others";
            ac1.RefDocID = 101;

            var ac2 = new FinanceAccountExtLoanViewModel();
            ac2.AccountID = 2;
            ac2.InterestFree = true;
            ac2.Others = "Others";
            ac2.RefDocID = 101;

            var rst = FinanceAccountExtLoanViewModel.WorkoutDeltaForUpdate(ac1, ac2);
        }
    }
}
