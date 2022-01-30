using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;
using hihapi.Models;

namespace hihapi.test.UnitTests.Finance
{
    public class ControlCenterTestData : IXunitSerializable
    {
        public Int32 ID { get; set; }
        public Int32 HomeID { get; set; }
        public String Name { get; set; }
        public Int32? ParentID { get; set; }
        public String Comment { get; set; }
        public String Owner { get; set; }
        public Boolean ExpectedValidResult { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            ControlCenterTestData other = JsonSerializer.Deserialize<ControlCenterTestData>(val);
            ID = other.ID;
            HomeID = other.HomeID;
            ParentID = other.ParentID;
            Name = other.Name;
            Comment = other.Comment;
            Owner = other.Owner;

            ExpectedValidResult = other.ExpectedValidResult;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }
}
