using hihapi.Models;
using System;
using System.Text.Json;
using Xunit.Abstractions;

namespace hihapi.unittest.Finance
{
    public class PlanTestData : IXunitSerializable
    {
        public Int32 ID { get; set; }
        public Int32 HomeID { get; set; }
        public FinancePlanTypeEnum PlanType { get; set; }
        public Int32? AccountID { get; set; }
        public Int32? AccountCategoryID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? TranTypeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public Decimal TargetBalance { get; set; }
        public String TranCurr { get; set; }
        public String Description { get; set; }
        public Boolean ExpectedValidResult { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            PlanTestData other = JsonSerializer.Deserialize<PlanTestData>(val);
            ID = other.ID;
            HomeID = other.HomeID;
            PlanType = other.PlanType;
            AccountID = other.AccountID;
            AccountCategoryID = other.AccountCategoryID;
            ControlCenterID = other.ControlCenterID;
            TranTypeID = other.TranTypeID;
            StartDate = other.StartDate;
            TargetDate = other.TargetDate;
            TargetBalance = other.TargetBalance;
            TranCurr = other.TranCurr;
            Description = other.Description;

            ExpectedValidResult = other.ExpectedValidResult;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }
}
