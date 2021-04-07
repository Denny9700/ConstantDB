namespace ConstantDB
{
  public class CDBHeader
  {
    private int _fieldCount;
    private int _rowCount;
    private int _rowSize;

    public int FieldCount
    {
      get => _fieldCount;
      set => _fieldCount = value;
    }

    public int RowCount
    {
      get => _rowCount;
      set => _rowCount = value;
    }

    public int RowSize
    {
      get => _rowSize;
      set => _rowSize = value;
    }

    public CDBHeader()
    {
      FieldCount  = 0;
      RowCount    = 0;
      RowSize     = 0;
    }

    ~CDBHeader()
    { }
  }
}
