namespace ConstantDB
{
  public class CDBRecordField
  {
    private CDBField _field;
    private object _value;

    public CDBField Field
    {
      get => _field;
      private set => _field = value;
    }

    public object Value
    {
      get => _value;
      private set => _value = value;
    }

    public CDBRecordField()
    { }

    public CDBRecordField(CDBField field, object value)
    {
      _field = field;
      _value = value;
    }
  }
}
