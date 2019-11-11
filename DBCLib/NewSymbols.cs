using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class NewSymbols : Entry
  {
    public static string Symbol = "NS_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return NewSymbols.Symbol; }
      set { }
    }

    static string symbolsRegexSubstring = @"(" + R.U.symbolName + @"(?:\s+" + R.U.symbolName + @")*)";

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s*:\s*{1}?\s*$", Symbol, symbolsRegexSubstring),
      RegexOptions.Compiled
      );
    static Regex regexAdditionalLine = new Regex(
      string.Format(@"^\s+{0}\s*$", symbolsRegexSubstring),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<string> Symbols
    {
      get
      {
        return symbols;
      }
    }
    private List<string> symbols = new List<string>();

    public override string ToString()
    {
      return string.Format("[{0}] {1}",
        GetType().Name,
        string.Join("|", Symbols)
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

        if (parseContext.stage == ParseContext.Stage.Version)
        {
          parseContext.stage = ParseContext.Stage.NS;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "NS_ (optional) expected immediately after VERSION."
            ));
        }

        string symbolsString = match.Groups[1].Value;
        if (symbolsString.Length > 0)
        {
          symbols.AddRange(symbolsString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
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
              symbols.AddRange(symbolsString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
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
      streamWriter.WriteLine(string.Format("{0}:",
        Symbol
        ));

      foreach (string symbol in Symbols)
      {
        streamWriter.WriteLine(string.Format(" {0}",
          symbol
          ));
      }
    }
  }
}
