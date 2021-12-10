
using System;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM{
    public class User : TableEntity {
        public enum UserTypes {
            Patient,
            Doctor
        }
        public User(string name, string password, UserTypes type, string email)
        {
            this.PartitionKey = "p1";
            this.RowKey = Guid.NewGuid().ToString();
            this.Name = name;
            this.Password = password;
            this.Type = type.ToString();
            this.Email = email;
        }
        public User() { }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserTypes TypeId { get { return Type != null ? (UserTypes)Enum.Parse(typeof(UserTypes), Type) : UserTypes.Patient; } }
        public Guid UserID { get { return Guid.Parse(RowKey); } }

        public static explicit operator User(TableResult v)
        {
            DynamicTableEntity entity = (DynamicTableEntity)v.Result;
            User userProfile = new User();
            userProfile.PartitionKey = entity.PartitionKey;
            userProfile.RowKey = entity.RowKey;
            userProfile.Timestamp = entity.Timestamp;
            userProfile.ETag = entity.ETag;
            userProfile.Name = entity.Properties.ContainsKey("Name") ? entity.Properties["Name"].StringValue : null;
            userProfile.Type = entity.Properties.ContainsKey("Type") ? entity.Properties["Type"].StringValue : null;
            userProfile.Password = entity.Properties.ContainsKey("Password") ? entity.Properties["Password"].StringValue : null;
            userProfile.Email = entity.Properties.ContainsKey("Email") ? entity.Properties["Email"].StringValue : null;

            return userProfile;
        }

    }
}