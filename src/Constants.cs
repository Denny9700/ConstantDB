namespace ConstantDB
{
  using System;

  public enum FieldType
  {
    Field_Char    = 0,
    Field_Bool    = 1,
    Field_Short   = 2,
    Field_Enum    = 3,
    Field_Float   = 4,

    Field_String
  }

  public enum FieldTypeExtended
  {
    Field_Char    = 0,
    Field_Bool    = 1,
    Field_Short   = 2,
    Field_Enum    = 3,
    Field_Int     = 4,
    Field_Float   = 5,
    Field_String  = 6
  }

  public class Constants
  {
    public static readonly int MAX_NAME_LEN = 0x1E;

    public static FieldTypeExtended ConvertToExtended(FieldType fieldType)
    {
      switch (fieldType)
      {
        case FieldType.Field_Char:
          return FieldTypeExtended.Field_Char;
        case FieldType.Field_Bool:
          return FieldTypeExtended.Field_Bool;
        case FieldType.Field_Short:
          return FieldTypeExtended.Field_Short;
        case FieldType.Field_Float:
          return FieldTypeExtended.Field_Float;
        case FieldType.Field_String:
          return FieldTypeExtended.Field_String;
        default:
          return FieldTypeExtended.Field_String;
      }
    }

    public static FieldType ConvertToBase(FieldTypeExtended fieldType)
    {
      switch (fieldType)
      {
        case FieldTypeExtended.Field_Char:
          return FieldType.Field_Char;
        case FieldTypeExtended.Field_Bool:
          return FieldType.Field_Bool;
        case FieldTypeExtended.Field_Short:
          return FieldType.Field_Short;
        case FieldTypeExtended.Field_Int:
        case FieldTypeExtended.Field_Float:
          return FieldType.Field_Float;
        case FieldTypeExtended.Field_String:
          return FieldType.Field_String;
        default:
          return FieldType.Field_String;
      }
    }

    public static int GetSizeFromFieldType(FieldType fieldType)
    {
      switch (fieldType)
      {
        case FieldType.Field_Char:
          return sizeof(char);
        case FieldType.Field_Bool:
          return sizeof(bool);
        case FieldType.Field_Short:
          return sizeof(short);
        case FieldType.Field_Float:
          return sizeof(float);
        default:
          return 0;
      }
    }

    public static Type GetTypeFromFieldType(FieldType fieldType)
    {
      switch (fieldType)
      {
        case FieldType.Field_Char:
          return typeof(char);
        case FieldType.Field_Bool:
          return typeof(bool);
        case FieldType.Field_Short:
          return typeof(short);
        case FieldType.Field_Float:
          return typeof(float);
        case FieldType.Field_String:
          return typeof(string);
        default:
          return typeof(object);
      }
    }
  }
}
