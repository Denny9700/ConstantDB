namespace ConstantDB
{
  using System;
  using System.Collections.Generic;

  public class CDBRecord : List<CDBRecordField>
  {
    public CDBRecordField this[string name]
    { get => Find(x => x.Field.Name == name); }

    public CDBRecord()
    { }

    public object GetValue(int index)
    {
      if (index > Count)
        throw new Exception($"Unable to get value, invalid index. Expected: index < {Count}, found: {index}");

      return this[index].Value;
    }
  }
}
