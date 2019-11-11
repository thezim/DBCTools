using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Nodes : Entry
  {
    public static string Symbol = "BU_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Nodes.Symbol; }
      set { }
    }

    static string nodesRegexSubstring = @"(" + R.U.nodeName + @"(?:\s+" + R.U.nodeName + @")*)";

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s*:\s*{1}?\s*$", Symbol, nodesRegexSubstring),
      RegexOptions.Compiled
      );
    static Regex regexAdditionalLine = new Regex(
      string.Format(@"^\s+{0}\s*$", nodesRegexSubstring),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<string> Names
    {
      get
      {
        return names;
      }
    }
    private List<string> names = new List<string>();

    public override string ToString()
    {
      return string.Format("[{0}] {1}",
        GetType().Name,
        string.Join("|", Names)
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

        if (parseContext.stage == ParseContext.Stage.BS)
        {
          parseContext.stage = ParseContext.Stage.BU;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "BU_ expected immediately after BS_."
            ));
        }

        string symbolsString = match.Groups[1].Value;
        if (symbolsString.Length > 0)
        {
          names.AddRange(symbolsString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        bool additionalSymbolFound;
        do
        {
          additionalSymbolFound = false;

          parseContext.line = null;
          if (!parseContext.streamReader.EndOfStream)
          {
            parseContext.line = parseContext.streamReader.ReadLine();
            parseContext.numLines++;
          }

          if (parseContext.line != null)
          {
            match = regexAdditionalLine.Match(parseContext.line);
            if (match.Success)
            {
              symbolsString = match.Groups[1].Value;
              names.AddRange(symbolsString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
              additionalSymbolFound = true;
            }
          }
        } while (additionalSymbolFound);

        return true;
      }

      return false;
    }

    public override void WriteDBC(StreamWriter streamWriter)
    {
      string namesString = string.Join(" ", Names);
      if (namesString.Length > 0)
      {
        namesString = " " + namesString;
      }

      streamWriter.WriteLine(string.Format("{0}:{1}",
        Symbol,
        namesString
        ));
    }
  }
}
