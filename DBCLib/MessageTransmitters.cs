using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class MessageTransmitters : Entry
  {
    public static string Symbol = "BO_TX_BU_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return MessageTransmitters.Symbol; }
      set { }
    }

    static string transmittersRegexSubstring = @"(" + R.U.nodeName + @"(?:," + R.U.nodeName + @")*)";

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}\s*:\s*{2};$",
        Symbol,
        R.C.uintValue,
        transmittersRegexSubstring
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public uint? ContextMessageId
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<string> Transmitters
    {
      get { return transmitters; }
    }
    List<string> transmitters = new List<string>();

    public override string ToString()
    {
      string transmittersString = "";
      foreach (string transmitter in Transmitters)
      {
        transmittersString += string.Format("|{0}", transmitter);
      }
      return string.Format("[{0}] {1}{2}",
        GetType().Name,
        ContextMessageId,
        transmittersString
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 3)
        {
          throw new DataMisalignedException();
        }

        ContextMessageId = uint.Parse(match.Groups[1].Value);

        transmitters.AddRange(match.Groups[2].Value.Split(','));

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
      string transmittersString = string.Join(",", Transmitters);
      if (transmittersString.Length > 0)
      {
        transmittersString = " " + transmittersString;
      }

      streamWriter.WriteLine(string.Format("{0} {1}:{2};",
        Symbol,
        ContextMessageId,
        transmittersString
        ));
    }
  }
}
