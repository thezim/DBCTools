namespace DBCLib
{
  class R
  {
    public class C
    {
      public static string messageName = @"(" + U.messageName + @")";
      public static string nodeName = @"(" + U.nodeName + @")";
      public static string signalName = @"(" + U.signalName + @")";
      public static string symbolName = @"(" + U.symbolName + @")";
      public static string valueTableName = @"(" + U.valueTableName + @")";

      public static string doubleValue = @"(" + U.doubleValue + @")";
      public static string intValue = @"(" + U.intValue + @")";
      public static string stringValue = @"(" + U.stringValue + @")";
      public static string quotedStringValue = @"(" + U.quotedStringValue + @")";
      public static string uintValue = @"(" + U.uintValue + @")";

      static string messageContext = @"(BO_)\s+" + uintValue;
      static string nodeContext = @"(BU_)\s+" + R.C.nodeName;
      static string signalContext = @"(SG_)\s+" + uintValue + @"\s+" + R.C.signalName;
      public static string context = string.Format(@"(?:(?:{0}|{1}|{2})\s+)?",
        R.C.messageContext,
        R.C.nodeContext,
        R.C.signalContext
        );

      public static string nodeNameListComma = @"(" + U.nodeName + @"(?:," + U.nodeName + ")*)";
    }

    public class U
    {
      static string A0_ = @"[A-Z_0-9]+";
      static string Aa0_ = @"[A-Za-z_0-9]+";

      public static string messageName = Aa0_;
      public static string nodeName = Aa0_;
      public static string signalName = Aa0_;
      public static string symbolName = A0_;
      public static string valueTableName = Aa0_;

      public static string doubleValue = @"-?\d+(?:\.\d+)?(?:[Ee][+-]\d+)?";
      public static string intValue = @"-?\d+";
      public static string stringValue = @"(?:[^""]|\\"")*";
      public static string quotedStringValue = @"""" + stringValue + @"""";
      public static string uintValue = @"\d+";
    }
  }
}
