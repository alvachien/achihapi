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
    }
}
