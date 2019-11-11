using DBCLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace DBCToJSON
{
  class Program
  {
    static bool overwriteExisting = true;

    static void Main(string[] args)
    {
      DBCLib.Reader dbcReader = new Reader();

      IEnumerable<string> files = Directory.EnumerateFiles(".", "*.dbc");
      foreach (string fileSrc in files)
      {
        string fileDst = string.Format("{0}.json", Path.GetFileNameWithoutExtension(fileSrc));
        if (overwriteExisting || !File.Exists(fileDst) || (new System.IO.FileInfo(fileDst).Length == 0))
        {
          Console.Title = fileSrc;
          Console.WriteLine("R {0}", fileSrc);
          List<object> entries = dbcReader.Read(fileSrc);

          if (entries != null)
          {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
              DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                  entries[0].GetType(),
                  new DataContractJsonSerializerSettings()
                  {
                    UseSimpleDictionaryFormat = true,
                    KnownTypes = new Type[]
                    {
                      typeof(AttributeValue),
                      typeof(AttributeDefinition),
                      typeof(AttributeDefault),
                      typeof(BitTiming),
                      typeof(Comment),
                      typeof(Message),
                      typeof(MessageTransmitters),
                      typeof(NewSymbols),
                      typeof(Nodes),
                      typeof(Value),
                      typeof(ValueTable),
                      typeof(DBCLib.Version)
                    },
                    EmitTypeInformation = EmitTypeInformation.Never
                  }
                  );

              bool incompleteOutput = false;
              try
              {
                byte[] rootInit = Encoding.ASCII.GetBytes("[\n");
                byte[] rootDelimiter = Encoding.ASCII.GetBytes(",\n");
                byte[] rootTerm = Encoding.ASCII.GetBytes("\n]\n");

                Console.WriteLine("W {0}", fileDst);
                using (Stream streamDst = new FileStream(fileDst, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                  incompleteOutput = true;
                  streamDst.Write(rootInit, 0, rootInit.Length);

                  for (int i = 0; i < entries.Count; ++i)
                  {
                    object entry = entries[i];

                    if (i > 0)
                    {
                      streamDst.Write(rootDelimiter, 0, rootDelimiter.Length);
                    }

                    using (var writerDst = JsonReaderWriterFactory.CreateJsonWriter(streamDst, Encoding.UTF8, false, true, "  "))
                    {
                      serializer.WriteObject(writerDst, entry);
                      writerDst.Flush();
                    }
                  }

                  streamDst.Write(rootTerm, 0, rootTerm.Length);
                  streamDst.Flush();

                  incompleteOutput = false;
                }
              }
              finally
              {
                if (incompleteOutput)
                {
                  File.Delete(fileDst);
                }
              }
            }
            finally
            {
              Thread.CurrentThread.CurrentCulture = currentCulture;
            }
          }
        }
      }
    }
  }
}
