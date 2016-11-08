using MoreExtras.Utils;
using FluentAssertions;

namespace MoreExtras.Tests
{
    public class StringExtensionTests
    {
        public void IsNullOrEmptyTest()
        {
            string s = "1234";
            s.IsNullOrEmpty().Should().BeFalse("I said so...");
            s = "";
            s.IsNullOrEmpty().Should().BeTrue("I set it to it");
            s = null;
            s.IsNullOrEmpty().Should().BeTrue("because I set it to null");
        }
    }
}