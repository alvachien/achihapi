using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;
using hihapi.Models;

namespace hihapi.test.UnitTests.Finance
{
    public class OrderTestData : IXunitSerializable
    {
        public Int32 ID { get; set; }
        public Int32 HomeID { get; set; }
        public String Name { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public String Comment { get; set; }
        public List<FinanceOrderSRule> SRule { get; set; }
        public Boolean ExpectedValidResult { get; set; }

        public OrderTestData()
        {
            this.SRule = new List<FinanceOrderSRule>();
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            OrderTestData other = JsonSerializer.Deserialize<OrderTestData>(val);
            ID = other.ID;
            HomeID = other.HomeID;
            Name = other.Name;
            Comment = other.Comment;
            ValidFrom = other.ValidFrom;
            ValidTo = other.ValidTo;
            this.SRule.Clear();
            this.SRule.AddRange(other.SRule);

            ExpectedValidResult = other.ExpectedValidResult;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }
}
