using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DAL.Tests
{
    [TestClass()]
    public class DbMessageTests
    {
        [TestMethod]
        public void SerializeToXmlTest()
        {
            var message = new DbMessage()
            {
                From = "From_value<",
                Text = "Text_value> <hsh",
                To = new String[] { "To_value_0", "To_value_1" },
                Time = DateTime.UtcNow
            };

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            DbMessage.SerializeToXml(message, doc.Element(rootTag));

            var text = doc.ToString();

            Assert.IsTrue( text.Contains("From_value"), "From" );
            Assert.IsTrue( text.Contains("Text_value") && text.Contains("hsh"), "Text");
            Assert.IsTrue(text.Contains("To_value_0"),"To[0]");
            Assert.IsTrue(text.Contains("To_value_1"), "To[1]");
            Assert.IsTrue(text.Contains(message.Time.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")), "Time");
        }

        [TestMethod]
        public void SerializeToXmlTestEmptyValues()
        {
            var message = new DbMessage();

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            //DbMessage.SerializeToXml(message, doc.Element(rootTag));
            string exceptionMsg = null;
            try
            {
                DbMessage.SerializeToXml(message, doc.Element(rootTag));
            }
            catch (Exception e)
            {
                exceptionMsg = e.Message;
            }

            Assert.IsNotNull(exceptionMsg, "Unexpected serialization");

            message = new DbMessage()
            {
                From = "From_value<",
                //Text = "Text_value> <hsh",
                //To = new String[] { "To_value_0", "To_value_1" },
                Time = DateTime.Now
            };

            exceptionMsg = null;
            try
            {
                DbMessage.SerializeToXml(message, doc.Element(rootTag));
            }
            catch (Exception e)
            {
                exceptionMsg = e.Message;
            }

            Assert.IsNull(exceptionMsg, "Expects serialization");
        }

        [TestMethod]
        public void SerializeAndDesrializeUTC()
        {
            var src = new DbMessage()
            {
                From = "From_value<",
                Text = "Text_value> <hsh",
                To = new String[] { "To_value_0", "To_value_1" },
                Time = DateTime.UtcNow
            };

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            DbMessage.SerializeToXml(src, doc.Element(rootTag));

            var dst = DbMessage.DeserializeFromXml(doc.Element(rootTag).Elements().First());

            Assert.AreEqual(src.From, dst.From, "From");
            Assert.AreEqual(src.Text, dst.Text, "Text");
            Assert.AreEqual(src.Time, dst.Time, "Time");
            Assert.AreEqual(src.To.Aggregate(string.Empty,(all, t) => all + "|" + t ), dst.To.Aggregate(string.Empty, (all, t) => all + "|" + t), "To");

        }

        [TestMethod]
        public void SerializeAndDesrializeLocalTime()
        {
            var src = new DbMessage()
            {
                From = "From_value<",
                Text = "Text_value> <hsh",
                //To = new String[] { "To_value_0", "To_value_1" },
                Time = DateTime.Now
            };

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            DbMessage.SerializeToXml(src, doc.Element(rootTag));

            var dst = DbMessage.DeserializeFromXml(doc.Element(rootTag).Elements().First());

            Assert.AreEqual(src.From, dst.From, "From");
            Assert.AreEqual(src.Text, dst.Text, "Text");
            Assert.AreEqual(src.Time, dst.Time, "Time");
            Assert.IsTrue(src.To.Count() == dst.To.Count());

            if( src.To.Any())
                Assert.AreEqual(src.To.Aggregate(string.Empty, (all, t) => all + "|" + t), dst.To.Aggregate(string.Empty, (all, t) => all + "|" + t), "To");

        }
    }
}