using Dotc.MQExplorerPlus.Application.Models.Parser.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParserTests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void TestLoad()
        {
            string test = "<parser>" +
                   "<parts>" +
                   "<part id=\"part01\"></part>" +
                   "</parts>" +
                   "<message />" +
              "</parser>";

            var conf = ParserConfiguration.LoadFromString(test);

            PartElement pe;
            Assert.IsTrue(conf.Parts.TryFindById("part01", out pe) == true && pe.Id == "part01");
        }

    }
}
