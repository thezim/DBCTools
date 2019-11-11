using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DBCLib
{
  [DataContract]
  public class ValueTable : Entry
  {
    public static string Symbol = "VAL_TABLE_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return ValueTable.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}(?:\s+{2}\s+{3})+\s*;$",
        Symbol,
        R.C.valueTableName,
        R.C.intValue,
        R.C.quotedStringValue
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public string Name
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<KeyValuePair<int, string>> Mapping
    {
      get { return mapping; }
    }
    List<KeyValuePair<int, string>> mapping = new List<KeyValuePair<int, string>>();

    public override string ToString()
    {
      string mappingString = "";
      foreach (KeyValuePair<int, string> pair in Mapping)
      {
        mappingString += string.Format("|{0}|{1}", pair.Key, pair.Value);
      }
      return string.Format("[{0}] {1}{2}",
        GetType().Name,
        Name,
        mappingString
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 4)
        {
          throw new DataMisalignedException();
        }

        Name = match.Groups[1].Value;

        for (int i = 0; i < match.Groups[2].Captures.Count; i++)
        {
          KeyValuePair<int, string> pair = new KeyValuePair<int, string>(
            int.Parse(match.Groups[2].Captures[i].Value),
            StringUtility.DecodeQuotedString(match.Groups[3].Captures[i].Value)
            );
          mapping.Add(pair);
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
      string mappingString = "";
      foreach (KeyValuePair<int, string> pair in Mapping)
      {
        mappingString += string.Format(@" {0} {1}",
          pair.Key,
          StringUtility.EncodeAsQuotedString(pair.Value)
          );
      }

      streamWriter.WriteLine(string.Format("{0} {1}{2};",
        Symbol,
        Name,
        mappingString
        ));
    }
  }
}
