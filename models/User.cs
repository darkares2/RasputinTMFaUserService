
using System;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM{
    public class User : TableEntity {
        public enum UserTypes {
            Patient,
            Doctor
        }
        public User(string name, UserTypes type)
        {
            this.PartitionKey = "p1";
            this.RowKey = Guid.NewGuid().ToString();
            this.Name = name;
            this.Type = type.ToString();
        }
        User() { }
        public string Name { get; set; }
        public string Type { get; set; }
        public UserTypes TypeId { get { return (UserTypes)Enum.Parse(typeof(UserTypes), Type); } }
        public Guid UserID { get { return Guid.Parse(RowKey); } }

        public static explicit operator User(TableResult v)
        {
            DynamicTableEntity entity = (DynamicTableEntity)v.Result;
            User userProfile = new User();
            userProfile.PartitionKey = entity.PartitionKey;
            userProfile.RowKey = entity.RowKey;
            userProfile.Timestamp = entity.Timestamp;
            userProfile.ETag = entity.ETag;
            userProfile.Name = entity.Properties["Name"].StringValue;
            userProfile.Type = entity.Properties["Type"].StringValue;

            return userProfile;
        }

    }
}