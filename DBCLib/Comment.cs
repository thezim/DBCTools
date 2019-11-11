using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Comment : Entry
  {
    public static string Symbol = "CM_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Comment.Symbol; }
      set { }
    }

    static string commentTerminatorRegexSubstring = @"""\s*;\s*";

    static Regex regexFirstLine = new Regex(
      string.Format(@"^{0}\s+{1}""{2}({3})?$",
        Symbol,
        R.C.context,
        R.C.stringValue,
        commentTerminatorRegexSubstring
        ),
      RegexOptions.Compiled
      );

    static Regex regexLastLine = new Regex(
      string.Format(@"^(.*){0}$",
        commentTerminatorRegexSubstring
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
    public string SubTypeSymbol
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string Text
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

      return string.Format("[{0}] {1}{2}",
        GetType().Name,
        contextString,
        Text?.Replace("\n", "|")
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 10)
        {
          throw new DataMisalignedException();
        }

        if (
          (parseContext.stage == ParseContext.Stage.BO) ||
          (parseContext.stage == ParseContext.Stage.CM)
          )
        {
          parseContext.stage = ParseContext.Stage.CM;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "CM_ (optional) expected immediately after BO_ (and associated SG_)."
            ));
        }

        ContextMessageId = null;
        ContextNode = null;
        ContextSignalName = null;
        SubTypeSymbol = null;

        if (match.Groups[1].Value == "BO_")
        {
          SubTypeSymbol = "BO_";
          ContextMessageId = uint.Parse(match.Groups[2].Value);
        }
        else if (match.Groups[3].Value == "BU_")
        {
          SubTypeSymbol = "BU_";
          ContextNode = match.Groups[4].Value;
        }
        else if (match.Groups[5].Value == "SG_")
        {
          SubTypeSymbol = "SG_";
          ContextMessageId = uint.Parse(match.Groups[6].Value);
          ContextSignalName = match.Groups[7].Value;
        }

        Text = StringUtility.DecodeString(match.Groups[8].Value);

        bool incompleteText = (match.Groups[9].Value.Length == 0);

        parseContext.line = null;
        while (!parseContext.streamReader.EndOfStream)
        {
          parseContext.line = parseContext.streamReader.ReadLine();
          parseContext.numLines++;

          if (!incompleteText)
          {
            break;
          }

          match = regexLastLine.Match(parseContext.line);
          if (match.Success)
          {
            parseContext.line = match.Groups[1].Value;
            incompleteText = false;
          }
          Text += "\n" + parseContext.line;
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

      streamWriter.WriteLine(string.Format(@"{0} {1}{2};",
        Symbol,
        contextString,
        StringUtility.EncodeAsQuotedString(Text)
        ));
    }
  }
}
