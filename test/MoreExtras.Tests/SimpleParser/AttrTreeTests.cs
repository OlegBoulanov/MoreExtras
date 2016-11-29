using System;
using System.IO;

using MoreExtras.SimpleParser;
using FluentAssertions;

using Xunit;

namespace MoreExtras.Tests.SimpleParser
{
    public class AttrTreeTests
    {
        [Fact]
        public void BasicTest()
        {
            var at = new AttrTree();
            at.Node.Should().BeNull("null by default");
            at.IsOptional.Should().BeFalse("not optional by default");
            at.Attributes.Should().BeNull("there should be no attributes");
        }
        [Fact]
        public void ReadFromStringTests()
        {
            var at = new AttrTree();
            at.Read(new StringReader(" Tree ( ONE : ( attr1(val1)), TWO)"));
            at.Node.Should().Be("Tree");
            at.Count.Should().Be(2);
            at[0].Node.Should().Be("ONE");
            at[1].Node.Should().Be("TWO");
            at[0].Attributes.Count.Should().Be(1);
            at[0].Attributes[0].Node.Should().Be("attr1");
            at[0].Attributes[0].Count.Should().Be(1);
            at[0].Attributes[0].Attributes.Should().BeNull();
            at[0].Attributes[0][0].Node.Should().Be("val1");
            at[0].Attributes[0][0].Count.Should().Be(0);
            at[0].Attributes[0][0].Attributes.Should().BeNull();

        }

        [Fact]
        public void TestFileRead()
        {
            var grammar = new SemTree();
            FileStream fs = null;
            // why does the next line fail in vs code, but passes in cmd line?
            try { fs = new FileStream("grammars/sample.sg", FileMode.Open); grammar.Read(new StreamReader(fs));}
            catch(FileNotFoundException){ true.Should().BeFalse();}return;
            
            System.Console.WriteLine($"grams: {grammar.Count}");
            grammar.Count.Should().Be(8);
return;
            Parser parser = null;
            try { parser = new Parser(); parser.Prepare(grammar); } catch(Exception) {}
            
            parser.Lexicon.Count.Should().Be(25);
            Assert.True(25 == parser.Lexicon.Count);
            parser = null;
            grammar = null;
        }

    }
}