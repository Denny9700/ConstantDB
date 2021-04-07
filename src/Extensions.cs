namespace ConstantDB
{
  using System;
  using System.IO;
  using System.Text;

  public static class BinaryWriterExtensions
  {
    public static void WriteStringAndFill(this BinaryWriter writer, string str, int strLen, bool useUnicode = false)
    {
      var bytes = useUnicode ? Encoding.Unicode.GetBytes(str)
                             : Encoding.ASCII.GetBytes(str);

      Array.Resize(ref bytes, strLen);
      writer.Write(bytes);
    }

    public static void WriteByFieldType(this BinaryWriter writer, FieldTypeExtended fieldType, object value, int len = 0, bool useUnicode = false)
    {
      switch (fieldType)
      {
        case FieldTypeExtended.Field_Char:
          writer.Write(Convert.ToChar(value));
          break;
        case FieldTypeExtended.Field_Bool:
          writer.Write(Convert.ToBoolean(value));
          break;
        case FieldTypeExtended.Field_Short:
          writer.Write(Convert.ToUInt16(value));
          break;
        case FieldTypeExtended.Field_Int:
          writer.Write(Convert.ToUInt32(value));
          break;
        case FieldTypeExtended.Field_Float:
          writer.Write(Convert.ToSingle(value));
          break;
        case FieldTypeExtended.Field_String:
        default:
          writer.WriteStringAndFill((string)value, len, useUnicode);
          break;
      }
    }
  }

  public static class BinaryReaderExtensions
  {
    public static string ReadString(this BinaryReader reader, int size, bool useUnicode = false)
    {
      var bytes = reader.ReadBytes(size);
      return useUnicode ? Encoding.Unicode.GetString(bytes).Replace("\0", "")
                        : Encoding.ASCII.GetString(bytes).Replace("\0", "");
    }

    public static object ReadByFieldType(this BinaryReader reader, FieldTypeExtended fieldType, int len = 0, bool useUnicode = false)
    {
      switch (fieldType)
      {
        case FieldTypeExtended.Field_Char:
          return reader.ReadChar();
        case FieldTypeExtended.Field_Bool:
          return reader.ReadBoolean();
        case FieldTypeExtended.Field_Short:
          return reader.ReadInt16();
        case FieldTypeExtended.Field_Int:
          return reader.ReadUInt32();
        case FieldTypeExtended.Field_Float:
          return reader.ReadSingle();
        case FieldTypeExtended.Field_String:
        default:
          return reader.ReadString(len, useUnicode);
      }
    }
  }
}
