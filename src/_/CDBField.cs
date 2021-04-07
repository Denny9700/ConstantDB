namespace ConstantDB
{
  using System;

  public class CDBField
  {
    private string _name;

    private FieldType _fieldType;
    private int _offset;
    private int _size;

    public string Name
    {
      get => _name;
      set
      {
        if (value.Length > Constants.MAX_NAME_LEN)
          throw new Exception($"Invalid length. Max field name length = {Constants.MAX_NAME_LEN}");

        _name = value;
      }
    }

    public FieldType FieldType
    {
      get => _fieldType;
      set => _fieldType = value;
    }

    public int Offset
    {
      get => _offset;
      set => _offset = value;
    }

    public int Size
    {
      get => _size;
      set => _size = value;
    }

    public CDBField()
    { }

    public CDBField(string name, FieldType fieldType, int offset, int size)
    {
      Name      = name;
      FieldType = fieldType;
      Offset    = offset;
      Size      = size;
    }

    public CDBField(FieldType fieldType, int offset, int size)
      : this (string.Empty, fieldType, offset, size)
    { }

    ~CDBField()
    { }
  }
}
