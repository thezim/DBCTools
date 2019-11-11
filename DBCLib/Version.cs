using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Version : Entry
  {
    public static string Symbol = "VERSION";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Version.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}$",
        Symbol,
        R.C.quotedStringValue
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public string Text
    {
      get;
      private set;
    }

    public override string ToString()
    {
      return string.Format("[{0}] {1}",
        GetType().Name,
        Text
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 2)
        {
          throw new DataMisalignedException();
        }

        if (parseContext.stage == ParseContext.Stage.Init)
        {
          parseContext.stage = ParseContext.Stage.Version;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "VERSION occurs multiple times."
            ));
        }

        Text = StringUtility.SimplifyEmptyToNull(StringUtility.DecodeQuotedString(match.Groups[1].Value));

        parseContext.line = null;
        if (!parseContext.streamReader.EndOfStream)
        {
          parseContext.line = parseContext.streamReader.ReadLine();
          parseContext.numLines++;
        }

        return true;
      }

      return false;
    }

    public override void WriteDBC(StreamWriter streamWriter)
    {
      streamWriter.WriteLine(string.Format(@"{0} ""{1}""",
        Symbol,
        Text
        ));
    }
  }
}
