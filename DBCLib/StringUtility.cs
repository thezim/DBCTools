using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DBCLib
{
  class StringUtility
  {
    public static string DecodeQuotedString(string quotedStringValue)
    {
      Debug.Assert(new Regex(@"^"".*""$").IsMatch(quotedStringValue));
      string text = quotedStringValue.Substring(1, quotedStringValue.Length - 2);
      return DecodeString(text);
    }

    public static string DecodeString(string stringValue)
    {
      string text = stringValue.Replace(@"\""", @"""");
      return text;
    }

    public static string EncodeString(string text)
    {
      string stringValue = text?.Replace(@"""", @"\""");
      return stringValue;
    }

    public static string EncodeAsQuotedString(string text)
    {
      string quotedStringValue = string.Format(@"""{0}""",
        EncodeString(text)
        );
      return quotedStringValue;
    }

    public static string SimplifyEmptyToNull(string input)
    {
      return string.IsNullOrEmpty(input) ? null : input; ;
    }
  }
}
