using System.Collections.Generic;
using MoreExtras.Utils;

using Xunit;
using FluentAssertions;

namespace MoreExtras.Tests.Utils
{
    public class ListExtensionsTests
    {
        [Fact]
        public void ListTest1()
        {
            List<int> l = new List<int>();
            ListExtensions.Comparer<int> less = (a, b) => { return a < b; };
            l.InsertOrdered(3, less);
            l.Count.Should().Be(1);
            l.InsertOrdered(12, less);
            l.Count.Should().Be(2);
            l[0].Should().Be(3);
            l[1].Should().Be(12);
            l.InsertOrdered(8, less);
            l.Count.Should().Be(3);
            l[0].Should().Be(3);
            l[1].Should().Be(8);
            l[2].Should().Be(12);
            l.InsertOrdered(2, less);
            l.Count.Should().Be(4);
            l[0].Should().Be(2);
            l[1].Should().Be(3);
            l[2].Should().Be(8);
            l[3].Should().Be(12);
        }
    }
}