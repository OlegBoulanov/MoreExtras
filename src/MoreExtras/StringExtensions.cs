using System.Text;

namespace MoreExtras.Util
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return false == string.IsNullOrEmpty(s);
        }
    }
}