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
        public string Name { get; set; }
        public string Password { get; set; }

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

            if (!attributes.Any(a => a.Name == tagName) ||
                !attributes.Any(a => a.Name == tagPassword) )
                throw new ArgumentException(String.Format("Bad format of Node : {0}", node.ToString()), "node");

            var result = new DbUser()
            {
                Name = node.Attribute(tagName).AsNonEmptyString(),
                Password = node.Attribute(tagPassword).AsNonEmptyString()
            };

            return result;
        }

        private const string tagRoot = "dbUser";
        private const string tagName = "name";
        private const string tagPassword = "password";
    }
}
