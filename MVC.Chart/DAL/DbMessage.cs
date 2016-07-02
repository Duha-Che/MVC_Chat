using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DAL
{
    [Serializable]
    public class DbMessage
    {
        private IEnumerable<string> _to;
        public string Text { get; set; }
        public string From { get; set; }
        public IEnumerable<string> To
        {
            get { return _to ?? new String[0]; }
            set { _to = value;  }
        }
        public DateTime Time { get; set; }

        public static void SerializeToXml(DbMessage item, XElement parentNode)
        {
            if (parentNode == null)
                throw new ArgumentNullException("node");

            if (item == null)
                throw new ArgumentNullException("item");

            if (String.IsNullOrEmpty(item.From))
                throw new ArgumentException("item.From must be non-empty string", "item");

            XElement node = new XElement(tagRoot);
            XElement dstList = new XElement(tagDstList);

            node.Add(
                new XAttribute(tagFrom, item.From), 
                new XAttribute(tagTime, item.Time),
                dstList,
                new XElement(tagText, item.Text ?? String.Empty )
            );

            if (item.To != null)
                foreach (var each in item.To)
                {
                    dstList.Add(new XElement(tagTo, each));
                }

            parentNode.Add(node);
        }

        public static DbMessage DeserializeFromXml(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.Name != tagRoot)
                throw new ArgumentException(String.Format("Node has tag {0} but expected '{1}'.", node.Name, tagRoot), "node");

            if (!(node.HasAttributes && node.HasElements))
                throw new ArgumentException(String.Format("Bad format of Node : {0}", node.ToString()), "node");

            var attributes = node.Attributes();
            var elements = node.Elements();

            if (!attributes.Any(a => a.Name == tagFrom) ||
                !attributes.Any(a => a.Name == tagTime) ||
                !elements.Any( e => e.Name == tagText) ||
                !elements.Any(e => e.Name == tagDstList))
                throw new ArgumentException(String.Format("Bad format of Node : {0}", node.ToString()), "node");

            var dstNode = node.Element(tagDstList);

            var result = new DbMessage()
            {
                From = node.Attribute(tagFrom).AsNonEmptyString(),
                Time = node.Attribute(tagTime).AsDateTime(),
                Text = node.Element(tagText).AsString(),
                To = node.Element(tagDstList)
                .Elements()
                .Where(e => e.Name == tagTo)
                .Select(e => e.AsNonEmptyString())
                .ToList()
            };

            return result;     
        }

        private const string tagRoot = "dbMessage";
        private const string tagFrom = "from";
        private const string tagDstList = "tos";
        private const string tagTime = "time";
        private const string tagTo = "to";
        private const string tagText = "text";
    }
}
