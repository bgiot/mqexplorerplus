using Dotc.MQExplorerPlus.Application.Models.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dotc.MQExplorerPlus.Application.Models.Parser.Configuration;

namespace ParserTests
{
    [TestClass]
    public class ParserTests
    {

        [TestMethod]
        public void TestParseWithFields()
        {
            var conf = new ParserConfiguration();
            conf.Message
                .Field("Field01", 5)
                .Constant("space", 1, " ")
                .Field("Field02", 6);

            var g1 = conf.Message.Group("group01")
                .Field("subfield01",1)
                .Field("subfield02",1);


            var parser = new ParserEngine();

            parser.Configuration = conf;

            var ok = parser.ParseMessage("Hello World!$*");

            Assert.IsTrue(ok);
            Assert.IsTrue(parser.Result.Nodes[0].Value == "Hello");
            Assert.IsTrue(parser.Result.Nodes[2].Value == "World!");
            Assert.IsTrue(parser.Result.Nodes[3].Children[0].Value == "$");
            Assert.IsTrue(parser.Result.Nodes[3].Children[1].Value == "*");

        }

        [TestMethod]
        public void TestParseWithLoop()
        {
            var conf = new ParserConfiguration();

            conf.Message
                .Field("From", 1, "F")
                .Field("To", 1, "T");
            var loop = conf.Message.Loop("test", "{F}", "{T}");
            loop.Field("char", 1);

            var parser = new ParserEngine();

            parser.Configuration = conf;

            var ok = parser.ParseMessage("15ABCDE");

            Assert.IsTrue(ok);


        }

        [TestMethod]
        public void TestParseWithSwitch()
        {
            var conf = new ParserConfiguration();

            conf.Message
                .Field("X", 1, "X")
                .Field("Y", 1, "Y")
                .Field("SW", 1, "SW");

            var sw = conf.Message.Switch("test", "{SW}");

            sw.Case("case1", "{X}")
                .Field("case1_f1", 1);
            sw.Case("case2", "{Y}")
                .Field("case2_f1", 1);
            sw.Else("else")
                .Field("else_f1", 1);

            var parser = new ParserEngine();

            parser.Configuration = conf;

            var ok = parser.ParseMessage("abad");
            Assert.IsTrue(ok);
            Assert.IsTrue(parser.Result.Nodes[3].Children[0].Label=="case1");

            ok = parser.ParseMessage("abbd");
            Assert.IsTrue(ok);
            Assert.IsTrue(parser.Result.Nodes[3].Children[0].Label == "case2");

            ok = parser.ParseMessage("abzd");
            Assert.IsTrue(ok);
            Assert.IsTrue(parser.Result.Nodes[3].Children[0].Label == "else");

        }

        [TestMethod]
        public void TestParseWithPart()
        {
            var conf = new ParserConfiguration();

            conf.Parts.Part("part1")
                .Field("f1", 1)
                .Field("f2", 1);

            conf.Message.PartRef("p1", "part1");
            conf.Message.PartRef("p2", "part1");


            var parser = new ParserEngine();

            parser.Configuration = conf;

            var ok = parser.ParseMessage("abcd");

            Assert.IsTrue(ok);


        }

        [TestMethod]
        public void TestParseWithLoopAndIdRef()
        {
            var conf = new ParserConfiguration();

            var loop = conf.Message.Loop("test", "1", "2");
            loop.Field("val", 1, "V");
            var loop2 = conf.Message.Loop("test2", "{V}", "{V}");
            loop2.Field("x", 1);


            var parser = new ParserEngine();

            parser.Configuration = conf;

            var ok = parser.ParseMessage("123");

            Assert.IsTrue(ok);
        }

    }
}
