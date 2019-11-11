using System.Collections.Generic;
using System.IO;

namespace DBCLib
{
  public class ParseContext
  {
    public enum Stage
    {
      Init,
      Version,
      NS,
      BS,
      BU,
      BO,
      CM,
      VAL
    }

    public Stage stage;
    public StreamReader streamReader;
    public string line;
    public uint numLines;
    public List<KeyValuePair<uint, string>> errors;
    public List<KeyValuePair<uint, string>> warnings;

    public ParseContext(
      StreamReader streamReader,
      List<KeyValuePair<uint, string>> errors = null,
      List<KeyValuePair<uint, string>> warnings = null
      )
    {
      this.stage = Stage.Init;
      this.streamReader = streamReader;
      this.line = null;
      this.numLines = 0;
      this.errors = errors ?? new List<KeyValuePair<uint, string>>();
      this.warnings = warnings ?? new List<KeyValuePair<uint, string>>();
    }
  }
}
