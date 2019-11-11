using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class BitTiming : Entry
  {
    public static string Symbol = "BS_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return BitTiming.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}:$", Symbol),
      RegexOptions.Compiled
      );

    public override string ToString()
    {
      return string.Format("[{0}]",
        GetType().Name
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 1)
        {
          throw new DataMisalignedException();
        }

        if (
          (parseContext.stage == ParseContext.Stage.Version) ||
          (parseContext.stage == ParseContext.Stage.NS)
          )
        {
          parseContext.stage = ParseContext.Stage.BS;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "BS_ expected immediately after VERSION or NS_."
            ));
        }

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
      streamWriter.WriteLine(string.Format("{0}:",
        Symbol
        ));
    }
  }
}
