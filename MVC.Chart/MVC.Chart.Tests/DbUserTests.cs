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
    public class DbUserTests
    {
        [TestMethod]
        public void SerializeToXmlTest()
        {
            var user = new DbUser()
            {
                Name = "From_value<",
                Password = "Text_value> <hsh"
            };

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            DbUser.SerializeToXml(user, doc.Element(rootTag));

            var text = doc.ToString();

            Assert.IsTrue(text.Contains("From_value"), "Name");
            Assert.IsTrue(text.Contains("Text_value") && text.Contains("hsh"), "Password");
        }

        [TestMethod]
        public void SerializeToXmlTestEmptyValues()
        {
            var user = new DbUser();

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            string exceptionMsg = null;
            try
            {
                DbUser.SerializeToXml(user, doc.Element(rootTag));
            }
            catch (Exception e)
            {
                exceptionMsg = e.Message;
            }

            Assert.IsNotNull(exceptionMsg, "Unexpected serialization 1");

            user = new DbUser()
            {
                Name = "From_value<"
            };

            exceptionMsg = null;
            try
            {
                DbUser.SerializeToXml(user, doc.Element(rootTag));
            }
            catch (Exception e)
            {
                exceptionMsg = e.Message;
            }

            Assert.IsNotNull(exceptionMsg, "Unexpected serialization 2");
        }

        [TestMethod]
        public void SerializeAndDesrialize()
        {
            var src = new DbUser()
            {
                Name = "From_value<",
                Password = "Text_value> <hsh"
            };

            const string rootTag = "Root";

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(rootTag));

            DbUser.SerializeToXml(src, doc.Element(rootTag));

            var dst = DbUser.DeserializeFromXml(doc.Element(rootTag).Elements().First());

            Assert.AreEqual(src.Name, dst.Name, "Name");
            Assert.AreEqual(src.Password, dst.Password, "Password");
        }
    }
}