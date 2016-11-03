using System;
using Xunit;

//using MoreExtras.SomeService;
using FluentAssertions;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
        [Fact]
        public void Test2()
        {
            Assert.True(new MoreExtras.SomeService.Service().FailingMethod());
        }
        [Fact]
        public void TestFU()
        {
            "1234".Should().HaveLength(4);
        }
    }
}
