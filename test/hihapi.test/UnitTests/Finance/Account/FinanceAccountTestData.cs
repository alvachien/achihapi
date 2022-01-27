using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;
using hihapi.Models;

namespace hihapi.test.UnitTests.Finance
{
    public class FinanceAccountTestData : IXunitSerializable
    {
        public Int32 HomeID { get; set; }
        public Int32 CategoryID { get; set; }
        public String Name { get; set; }
        public String Comment { get; set; }
        public String Owner { get; set; }
        public Byte? Status { get; set; }
        public FinanceAccountExtraDP ExtraDP { get; set; }
        // public FinanceAccountExtraLoan ExtraLoan { get; set; }
        public FinanceAccountExtraAS ExtraAsset { get; set; }
        public FinanceAccountExtraLoan ExtraLoan { get; set; }
        public Boolean ExpectedValidResult { get; set; }
        public Boolean ExpectedIsDeleteAllowedResult { get; set; }
        public Boolean ExpectedIsCloseAllowedResult { get; set; }

        public FinanceAccountTestData()
        {
            ExpectedValidResult = false;
            ExpectedIsCloseAllowedResult = false;
            ExpectedIsDeleteAllowedResult = false;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceAccountTestData other = JsonSerializer.Deserialize<FinanceAccountTestData>(val);
            HomeID = other.HomeID;
            CategoryID = other.CategoryID;
            Name = other.Name;
            Comment = other.Comment;
            Owner = other.Owner;
            Status = other.Status;

            ExtraDP = other.ExtraDP;
            ExtraLoan = other.ExtraLoan;
            ExtraAsset = other.ExtraAsset;

            ExpectedValidResult = other.ExpectedValidResult;
            ExpectedIsCloseAllowedResult = other.ExpectedIsCloseAllowedResult;
            ExpectedIsDeleteAllowedResult = other.ExpectedIsDeleteAllowedResult;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }
}
