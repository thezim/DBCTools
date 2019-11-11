using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class AttributeValue : Entry
  {
    public static string Symbol = "BA_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return AttributeValue.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}\s+{2}(?:{3}|{4});$",
        Symbol,
        R.C.quotedStringValue,
        R.C.context,
        R.C.intValue,
        R.C.quotedStringValue
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
    public string ContextNode
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string ContextSignalName
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string Name
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string SubTypeSymbol
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public object Value
    {
      get;
      private set;
    }

    public override string ToString()
    {
      string contextString = "";
      switch (SubTypeSymbol)
      {
        case "BO_":
          contextString = string.Format("{0}|{1}|", SubTypeSymbol, ContextMessageId);
          break;
        case "BU_":
          contextString = string.Format("{0}|{1}|", SubTypeSymbol, ContextNode);
          break;
        case "SG_":
          contextString = string.Format("{0}|{1}|{2}|", SubTypeSymbol, ContextMessageId, ContextSignalName);
          break;
        default:
          break;
      }

      return string.Format("[{0}] {1}|{2}{3}",
        GetType().Name,
        Name,
        contextString,
        Value
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 11)
        {
          throw new DataMisalignedException();
        }

        Name = StringUtility.DecodeQuotedString(match.Groups[1].Value);

        if (match.Groups[2].Value == "BO_")
        {
          SubTypeSymbol = "BO_";
          ContextMessageId = uint.Parse(match.Groups[3].Value);
        }
        else if (match.Groups[4].Value == "BU_")
        {
          SubTypeSymbol = "BU_";
          ContextNode = match.Groups[5].Value;
        }
        else if (match.Groups[6].Value == "SG_")
        {
          SubTypeSymbol = "SG_";
          ContextMessageId = uint.Parse(match.Groups[7].Value);
          ContextSignalName = match.Groups[8].Value;
        }

        if (match.Groups[9].Value.Length > 0)
        {
          Value = int.Parse(match.Groups[9].Value);
        }
        if (match.Groups[10].Value.Length > 0)
        {
          Value = StringUtility.DecodeQuotedString(match.Groups[10].Value);
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
      string contextString = "";
      switch (SubTypeSymbol)
      {
        case "BO_":
          contextString = string.Format("{0} {1} ", SubTypeSymbol, ContextMessageId);
          break;
        case "BU_":
          contextString = string.Format("{0} {1} ", SubTypeSymbol, ContextNode);
          break;
        case "SG_":
          contextString = string.Format("{0} {1} {2} ", SubTypeSymbol, ContextMessageId, ContextSignalName);
          break;
        default:
          break;
      }

      streamWriter.WriteLine(string.Format("{0} {1} {2}{3};",
        Symbol,
        StringUtility.EncodeAsQuotedString(Name),
        contextString,
        (Value is string) ? StringUtility.EncodeAsQuotedString(Value as string) : Value
        ));
    }
  }
}
