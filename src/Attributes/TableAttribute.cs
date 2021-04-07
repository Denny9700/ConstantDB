namespace ConstantDB.Attributes
{
  using System;

  [AttributeUsage(AttributeTargets.Class)]
  public class TableAttribute : Attribute
  {
    public string TableName { get; set; }

    public TableAttribute(string tableName)
    {
      TableName = tableName;
    }
  }
}
