using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DAL
{
    public static class XMLExtention
    {
        public static String AsNonEmptyString(this XAttribute attr)
        {
            if (attr == null)
                throw new ArgumentNullException("attr");
            if (String.IsNullOrEmpty(attr.Value))
                throw new ArgumentException("attribute has null or empty value", attr.Name.ToString());

            return attr.Value;
        }

        public static DateTime AsDateTime(this XAttribute attr)
        {
            var strVal = attr.AsNonEmptyString();

            DateTime time;
            if (!DateTime.TryParse(strVal, out time))
                throw new ArgumentException("Can't parse attribute value as DateTime", attr.Name.ToString());

            return strVal.EndsWith("Z") ? /*DateTime.SpecifyKind(time, DateTimeKind.Utc)*/ time.ToUniversalTime() : time;
        }

        public static String AsNonEmptyString(this XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (String.IsNullOrWhiteSpace(node.Value))
                throw new ArgumentException("node has null or empty value", node.Name.ToString());

            return node.Value;
        }

        public static String AsString(this XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return node.Value;
        }

        public static Guid AsGuid(this XAttribute attr)
        {
            var strVal = attr.AsNonEmptyString();

            Guid result;
            if( !Guid.TryParse(strVal, out result))
                throw new ArgumentException("Can't parse attribute value as Guid", attr.Name.ToString());

            return result;
        }

        public static UInt64 AsUInt64(this XAttribute attr)
        {
            var strVal = attr.AsNonEmptyString();

            UInt64 result;
            if (!UInt64.TryParse(strVal, out result))
                throw new ArgumentException("Can't parse attribute value as UInt64", attr.Name.ToString());

            return result;
        }
    }
}
