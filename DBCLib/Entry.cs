using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public abstract class Entry
  {
    public abstract override string ToString();

    static Regex regexInitialSymbol = new Regex(
      @"^\s*" + R.C.symbolName + @"[^A-Z_0-9].*$",
      RegexOptions.Compiled
      );
    protected static Match MatchFirstLine(string line, string symbol, Regex regexFirstLine)
    {
      Match match = regexFirstLine.Match(line);
      if (match.Success)
      {
        return match;
      }

      match = regexInitialSymbol.Match(line);
      if (match.Success && (match.Groups[1].Value == symbol))
      {
        throw new DataMisalignedException();
      }

      return null;
    }

    public abstract bool TryParse(ref ParseContext parseContext);

    public abstract void WriteDBC(StreamWriter streamWriter);
  }
}
