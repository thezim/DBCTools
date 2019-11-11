using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DBCLib
{
  [DataContract]
  public class Message : Entry
  {
    public static string Symbol = "BO_";

    [DataMember(EmitDefaultValue = true, Name = "Symbol")]
    private string SymbolDataMember
    {
      get { return Message.Symbol; }
      set { }
    }

    static Regex regexFirstLine = new Regex(
      string.Format(@"^\s*{0}\s+{1}\s+{2}\s*:\s*{1}\s+{3}$",
        Symbol,
        R.C.uintValue,
        R.C.messageName,
        R.C.nodeName
        ),
      RegexOptions.Compiled
      );

    [DataMember(EmitDefaultValue = false)]
    public uint Id
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
    public uint Size
    {
      get;
      private set;
    }

    [DataMember(EmitDefaultValue = false)]
    public string Transmitter
    {
      get;
      private set;
    }

    public override string ToString()
    {
      return string.Format("[{0}] {1}|{2}|{3}|{4}",
        GetType().Name,
        Id,
        Name,
        Size,
        Transmitter
        );
    }

    public override bool TryParse(ref ParseContext parseContext)
    {
      Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
      if (match != null)
      {
        if (match.Groups.Count != 5)
        {
          throw new DataMisalignedException();
        }

        if (
          (parseContext.stage == ParseContext.Stage.BU) ||
          (parseContext.stage == ParseContext.Stage.BO)
          )
        {
          parseContext.stage = ParseContext.Stage.BO;
        }
        else
        {
          parseContext.warnings.Add(new KeyValuePair<uint, string>(parseContext.numLines,
            "BO_ expected immediately after BU_."
            ));
        }

        Id = uint.Parse(match.Groups[1].Value);
        Name = match.Groups[2].Value;
        Size = uint.Parse(match.Groups[3].Value);
        Transmitter = match.Groups[4].Value;

        parseContext.line = null;
        while (!parseContext.streamReader.EndOfStream)
        {
          parseContext.line = parseContext.streamReader.ReadLine();
          parseContext.numLines++;
          if (parseContext.line.Trim().Length > 0)
          {
            break;
          }
        }

        bool additionalSignalFound;
        do
        {
          additionalSignalFound = false;

          if (parseContext.line != null)
          {
            Signal signal = new Signal();
            if (signal.TryParse(ref parseContext))
            {
              signals.Add(signal);
              additionalSignalFound = true;
            }
          }
        } while (additionalSignalFound);

        return true;
      }

      return false;
    }

    [DataContract]
    public class Signal : Entry
    {
      public static string Symbol = "SG_";

      [DataMember(EmitDefaultValue = true, Name = "Symbol")]
      private string SymbolDataMember
      {
        get { return Signal.Symbol; }
        set { }
      }

      static Regex regexFirstLine = new Regex(
        string.Format(@"^\s+{0}\s+{1}(?:\s+(M|m{2}))?\s*:\s*{3}\|{3}@([01])([-+])\s+\({4},{4}\)\s+\[{4}\|{4}\]\s+{5}\s+{6}\s*$",
          Symbol,
          R.C.signalName,
          R.U.uintValue,
          R.C.uintValue,
          R.C.doubleValue,
          R.C.quotedStringValue,
          R.C.nodeNameListComma
          ),
        RegexOptions.Compiled
        );

      [DataMember(EmitDefaultValue = false)]
      public uint BitSize
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public IEnumerable<string> Receivers
      {
        get { return receivers; }
      }
      List<string> receivers = new List<string>();

      public enum ByteOrderEnum
      {
        BigEndian = 0,
        LittleEndian = 1
      }
      [DataMember(EmitDefaultValue = false)]
      public ByteOrderEnum ByteOrder
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public double Maximum
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public double Minimum
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public bool Multiplexer
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public uint? MultiplexerIdentifier
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
      public double Offset
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public double ScaleFactor
      {
        get;
        private set;
      }

      public enum ValueTypeEnum
      {
        Signed = '-',
        Unsigned = '+'
      }
      [DataMember(EmitDefaultValue = false)]
      public ValueTypeEnum ValueType
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public uint StartBit
      {
        get;
        private set;
      }

      [DataMember(EmitDefaultValue = false)]
      public string Unit
      {
        get;
        private set;
      }

      public override string ToString()
      {
        string multiplexerString = "";
        if (Multiplexer)
        {
          multiplexerString = "M|";
        }
        else if (MultiplexerIdentifier.HasValue)
        {
          multiplexerString = "m" + MultiplexerIdentifier.Value + "|";
        }

        return string.Format("[{0}] {1}|{2}{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}",
          GetType().Name,
          Name,
          multiplexerString,
          StartBit,
          BitSize,
          ByteOrder,
          ValueType,
          ScaleFactor,
          Offset,
          Minimum,
          Maximum,
          Unit,
          string.Join(",", Receivers)
          );
      }

      public override bool TryParse(ref ParseContext parseContext)
      {
        Match match = Entry.MatchFirstLine(parseContext.line, Symbol, regexFirstLine);
        if (match != null)
        {
          if (match.Groups.Count != 13)
          {
            throw new DataMisalignedException();
          }

          Name = match.Groups[1].Value;

          Multiplexer = false;
          if (match.Groups[2].Value.Length > 0)
          {
            if (match.Groups[2].Value == "M")
            {
              Multiplexer = true;
            }
            else
            {
              MultiplexerIdentifier = uint.Parse(match.Groups[2].Value.Substring(1));
            }
          }

          StartBit = uint.Parse(match.Groups[3].Value);
          BitSize = uint.Parse(match.Groups[4].Value);
          ByteOrder = (match.Groups[5].Value == "0") ? ByteOrderEnum.BigEndian : ByteOrderEnum.LittleEndian;
          ValueType = (match.Groups[6].Value == "-") ? ValueTypeEnum.Signed : ValueTypeEnum.Unsigned;
          ScaleFactor = double.Parse(match.Groups[7].Value);
          Offset = double.Parse(match.Groups[8].Value);
          Minimum = double.Parse(match.Groups[9].Value);
          Maximum = double.Parse(match.Groups[10].Value);
          Unit = StringUtility.SimplifyEmptyToNull(StringUtility.DecodeQuotedString(match.Groups[11].Value));

          receivers.AddRange(match.Groups[12].Value.Split(','));

          parseContext.line = null;
          while (!parseContext.streamReader.EndOfStream)
          {
            parseContext.line = parseContext.streamReader.ReadLine();
            parseContext.numLines++;
            if (parseContext.line.Trim().Length > 0)
            {
              break;
            }
          }

          return true;
        }

        return false;
      }

      public override void WriteDBC(StreamWriter streamWriter)
      {
        string multiplexerString = "";
        if (Multiplexer)
        {
          multiplexerString = " M";
        }
        else if (MultiplexerIdentifier.HasValue)
        {
          multiplexerString = " m" + MultiplexerIdentifier.Value + "";
        }

        streamWriter.WriteLine(" {0} {1}{2}: {3}|{4}@{5}{6} ({7},{8}) [{9}|{10}] {11} {12}",
          Symbol,
          Name,
          multiplexerString,
          StartBit,
          BitSize,
          (uint)ByteOrder,
          (ValueType == ValueTypeEnum.Signed) ? '-' : '+',
          ScaleFactor,
          Offset,
          Minimum,
          Maximum,
          StringUtility.EncodeAsQuotedString(Unit),
          string.Join(",", Receivers)
          );
      }
    }


    public override void WriteDBC(StreamWriter streamWriter)
    {
      streamWriter.WriteLine("{0} {1} {2}: {3} {4}",
        Symbol,
        Id,
        Name,
        Size,
        Transmitter
        );

      foreach (Signal signal in Signals)
      {
        signal.WriteDBC(streamWriter);
      }
    }

    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<Signal> Signals
    {
      get { return signals; }
    }
    private List<Signal> signals = new List<Signal>();
  }
}
