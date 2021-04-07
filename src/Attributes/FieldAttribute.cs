namespace ConstantDB.Attributes
{
  using System;

  [AttributeUsage(AttributeTargets.Property)]
  public class FieldAttribute : Attribute
  {
    public string FieldName { get; set; }
    public int FieldLength { get; set; }

    public FieldAttribute(string fieldName, int fieldLen)
    {
      FieldName = fieldName;
      FieldLength = fieldLen;
    }
  }
}
