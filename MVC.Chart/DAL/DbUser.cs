using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DAL
{
    [Serializable]
    public class DbUser
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public UInt64 Password { get; private set; }

        public static DbUser NewUser(string name, string password)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");

            return new DbUser()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Password = HashPassword( password )
            };

        }

        public void SetPassword(string newPassword)
        {
            if (String.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentNullException("newPassword");
            
            Password = HashPassword(newPassword);
        }

        public void SetPasswordHash(string newPasswordHash)
        {
            if (String.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentNullException("newPasswordHash");

            ulong hash;

            if(!ulong.TryParse(newPasswordHash, out hash))
                throw new FormatException( String.Format( "Can't parse {0} to UInt64", newPasswordHash ) );

            Password = hash;
        }

        public override bool Equals(object obj)
        {
            var user = (obj as DbUser);
            return user != null && user.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static UInt64 HashPassword(string text)
        {
            // TODO: Determine nullity policy.
            unchecked
            {
                UInt64 hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }

        public static void SerializeToXml(DbUser item, XElement parentNode)
        {
            if (parentNode == null)
                throw new ArgumentNullException("node");

            if (item == null)
                throw new ArgumentNullException("item");

            if (String.IsNullOrEmpty(item.Name))
                throw new ArgumentException("item.From must be non-empty string", "item");

            XElement node = new XElement(tagRoot);

            node.Add(
                new XAttribute(tagId, item.Id),
                new XAttribute(tagName, item.Name),
                new XAttribute(tagPassword, item.Password)
            );

            parentNode.Add(node);
        }

        public static DbUser DeserializeFromXml(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.Name != tagRoot)
                throw new ArgumentException(String.Format("Node has tag {0} but expected '{1}'.", node.Name, tagRoot), "node");

            if (!node.HasAttributes)
                throw new ArgumentException(String.Format("Bad format of Node : {0}", node.ToString()), "node");

            var attributes = node.Attributes();

            if (!attributes.Any(a => a.Name == tagId) ||
                !attributes.Any(a => a.Name == tagName) ||
                !attributes.Any(a => a.Name == tagPassword) )
                throw new ArgumentException(String.Format("Bad format of Node : {0}", node.ToString()), "node");

            var result = new DbUser()
            {
                Id = node.Attribute(tagId).AsGuid(),
                Name = node.Attribute(tagName).AsNonEmptyString(),
                Password = node.Attribute(tagPassword).AsUInt64()
            };

            return result;
        }

        public const string tagRoot = "dbUser";
        public const string tagId = "id";
        public const string tagName = "name";
        public const string tagPassword = "password";
    }
}
