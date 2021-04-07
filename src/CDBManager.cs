namespace ConstantDB
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  using ConstantDB.Attributes;
  using ConstantDB.External;

  using Newtonsoft.Json;

  public class CDBManager
  {
    private MemoryStream _cdbStream;
    private long _offset;

    private CDBHeader _header;
    private List<CDBField> _fields;

    public int? FieldCount
    { get => _header?.FieldCount; }

    public int? DataCount
    { get => _header?.RowCount; }

    public int? DataSize
    { get => _header?.RowSize; }

    private CDBManager()
    { _fields = new List<CDBField>(); }

    internal CDBManager(MemoryStream stream, long offset, CDBHeader header, List<CDBField> fields)
    {
      _cdbStream  = stream;
      _offset     = offset;
      _header     = header;
      _fields     = fields;
    }

    public static CDBManager LoadFromCDB(string path)
    {
      var stream = new MemoryStream(File.ReadAllBytes(path));
      return LoadFromCDB(stream);
    }

    public static CDBManager LoadFromCDB(MemoryStream stream)
    {
      var fieldNames = new List<string>();
      var fieldTypes = new List<int>();

      var memoryStream = new MemoryStream();
      stream.CopyTo(memoryStream);

      var reader = new BinaryReader(stream);
      reader.BaseStream.Seek(0, SeekOrigin.Begin);

      var fieldCount = reader.ReadInt32();
      for (var index = 0; index < fieldCount; index++)
        fieldNames.Add(reader.ReadString(Constants.MAX_NAME_LEN));
      for (var index = 0; index < fieldCount; index++)
        fieldTypes.Add(reader.ReadInt32());

      var fields = new List<CDBField>();
      for (var index = 0; index < fieldCount; index++)
      {
        fields.Add(new CDBField
        {
          Name      = fieldNames[index],
          FieldType = fieldTypes[index] > 4 ? FieldType.Field_String
                                            : (FieldType)fieldTypes[index],

          Offset    = index == 0            ? 0
                                            : fieldTypes.Take(index).Sum(x => x),

          Size      = fieldTypes[index] > 4 ? fieldTypes[index]
                                            : Constants.GetSizeFromFieldType((FieldType)fieldTypes[index])
        });
      }

      var offset = reader.BaseStream.Position;
      var header = new CDBHeader
      {
        FieldCount  = fields.Count,
        RowSize     = fields.Sum(x => x.Size),
        RowCount    = (int)((stream.Length - offset) / fields.Sum(x => x.Size))
      };

      return new CDBManager(memoryStream, offset, header, fields);
    }

    public static CDBManager LoadFromJson(string structurePath, string dataPath)
    {
      if (!CheckIntegrity(structurePath, dataPath, out var error))
        return null;

      var memoryStream = new MemoryStream();

      var structure = JsonConvert.DeserializeObject<List<DataStructure>>(File.ReadAllText(structurePath));
      if (structure == null)
        return null;

      var writer = new BinaryWriter(memoryStream);
      writer.Write(structure.Count);

      for (var index = 0; index < structure.Count; index++)
        writer.WriteStringAndFill(structure[index].Name, Constants.MAX_NAME_LEN);
      for (var index = 0; index < structure.Count; index++)
        writer.Write(structure[index].Type == FieldTypeExtended.Field_String ? structure[index].Size
                                                                             : Constants.GetSizeFromFieldType(Constants.ConvertToBase(structure[index].Type)));

      var fields = new List<CDBField>();
      for (var index = 0; index < structure.Count; index++)
      {
        fields.Add(new CDBField
        {
          Name      = structure[index].Name,
          FieldType = Constants.ConvertToBase(structure[index].Type),
          Offset    = index == 0 ? 0 : structure.Take(index).Sum(x => x.Size),
          Size      = structure[index].Size
        });
      }

      var offset = writer.BaseStream.Position;

      var elementIndex = 0;
      var totalElements = 0;

      var jsonText = File.ReadAllText(dataPath);
      var jsonReader = new JsonTextReader(new StringReader(jsonText));
      while (jsonReader.Read())
      {
        if (jsonReader.TokenType == JsonToken.StartObject)
          elementIndex = 0; //reset index
      
        if (jsonReader.TokenType == JsonToken.EndObject)
          totalElements += 1;
      
        if (jsonReader.TokenType != JsonToken.Boolean && jsonReader.TokenType != JsonToken.String &&
            jsonReader.TokenType != JsonToken.Float && jsonReader.TokenType != JsonToken.Integer)
          continue;
      
        writer.WriteByFieldType(structure[elementIndex].Type, jsonReader.Value, structure[elementIndex].Size, false);
        elementIndex += 1;
      }

      GC.Collect();

      var header = new CDBHeader
      {
        FieldCount  = fields.Count,
        RowSize     = fields.Sum(x => x.Size),
        RowCount    = totalElements
      };

      return new CDBManager(memoryStream, offset, header, fields);
    }

    public static bool CheckIntegrity(string structurePath, string dataPath, out string error)
    {
      var structure = JsonConvert.DeserializeObject<List<DataStructure>>(File.ReadAllText(structurePath));
      if (structure == null)
      {
        error = "Invalid structure, insert a valid structure and try again";
        return false;
      }

      var totalElementsMax = 0;
      var totalElements = 0;

      var jsonText = File.ReadAllText(dataPath);
      var jsonReader = new JsonTextReader(new StringReader(jsonText));
      while (jsonReader.Read())
      {
        if (jsonReader.TokenType == JsonToken.StartObject)
        {
          if (totalElements > totalElementsMax)
            totalElementsMax = totalElements;

          totalElements = 0; //reset index
        }

        if (jsonReader.TokenType != JsonToken.Boolean && jsonReader.TokenType != JsonToken.String &&
            jsonReader.TokenType != JsonToken.Float && jsonReader.TokenType != JsonToken.Integer)
          continue;

        totalElements += 1;
      }

      if (structure.Count != totalElementsMax)
      {
        error = "Invalid json data, something wrong inside data structure";
        return false;
      }

      error = "";
      return true;
    }

    public void ExportStructure(string structurePath)
    {
      if (_cdbStream == null)
        throw new NullReferenceException("Unable to export structure, stream is null");

      var stringBuilder = new StringBuilder();
      var stringWriter = new StringWriter(stringBuilder);

      using (var writer = new JsonTextWriter(stringWriter))
      {
        writer.Formatting = Formatting.Indented;

        //write comments
        var names = Enum.GetNames(typeof(FieldTypeExtended));
        var values = Enum.GetValues(typeof(FieldTypeExtended)).Cast<int>().ToArray();
        for (var index = 0; index < names.Length; index++)
          writer.WriteComment($"\n{names[index]} : {values[index]}");

        stringBuilder.Append("\n");

        writer.WriteStartArray();
        foreach (var field in _fields)
        {
          writer.WriteStartObject();

          writer.WritePropertyName("Name");
          writer.WriteValue(field.Name);
          writer.WritePropertyName("Type");
          writer.WriteValue(Constants.ConvertToExtended(field.FieldType));
          writer.WritePropertyName("Size");
          writer.WriteValue(field.Size);

          writer.WriteEndObject();
        }
        writer.WriteEndArray();
      }

      File.WriteAllText(structurePath, stringBuilder.ToString());
    }

    public bool ExportData(string structurePath, string dataPath)
    {
      if (_cdbStream == null)
        throw new NullReferenceException("Unable to export database, stream is null");

      var structure = JsonConvert.DeserializeObject<List<DataStructure>>(File.ReadAllText(structurePath));
      if (structure == null || structure.Count != FieldCount)
        return false;

      _cdbStream.Seek(_offset, SeekOrigin.Begin);
      var reader = new BinaryReader(_cdbStream);

      var stringBuilder = new StringBuilder();
      var stringWriter = new StringWriter(stringBuilder);

      using (var writer = new JsonTextWriter(stringWriter))
      {
        writer.Formatting = Formatting.Indented;

        writer.WriteStartArray();
        for (var index = 0; index < DataCount; index++)
        {
          writer.WriteStartObject();
          for (var index2 = 0; index2 < structure.Count; index2++)
          {
            writer.WritePropertyName($"{structure[index2].Name}");
            writer.WriteValue(reader.ReadByFieldType(structure[index2].Type, structure[index2].Size, false));
          }
          writer.WriteEndObject();
        }
        writer.WriteEndArray();
      }

      File.WriteAllText(dataPath, stringBuilder.ToString());
      return true;
    }

    public T GetData<T>(int index, bool secure = true) where T : new()
    {
      if (index < 0 || index >= DataCount)
        return default;

      var classType = typeof(T);
      var tableAttribute = classType.GetCustomAttribute<TableAttribute>();

      var properties = classType.GetProperties().Where(x => x.GetCustomAttribute<FieldAttribute>() != null).ToList();
      if (properties.Count() != FieldCount)
        return default;

      if (secure)
      {
        for (var i = 0; i < FieldCount; i++)
        {
          var attribute = properties[i].GetCustomAttribute<FieldAttribute>();
          if (attribute.FieldLength != _fields[i].Size || !attribute.FieldName.Equals(_fields[i].Name))
            return default;
        }
      }

      var attributeList = new List<FieldAttribute>();
      for (var i = 0; i < FieldCount; i++)
        attributeList.Add(properties[i].GetCustomAttribute<FieldAttribute>());

      var instance = Activator.CreateInstance(classType);
      var seekValue = index == 0 ? _offset : _offset + (index * DataSize);

      _cdbStream.Seek((long)seekValue, SeekOrigin.Begin);

      var binaryReader = new BinaryReader(_cdbStream);
      for (var i = 0; i < FieldCount; i++)
      {
        if (properties[i].PropertyType == typeof(char))
          properties[i].SetValue(instance, binaryReader.ReadChar());
        if (properties[i].PropertyType == typeof(bool))
          properties[i].SetValue(instance, binaryReader.ReadBoolean());
        if (properties[i].PropertyType == typeof(short))
          properties[i].SetValue(instance, binaryReader.ReadInt16());
        if (properties[i].PropertyType == typeof(int))
          properties[i].SetValue(instance, binaryReader.ReadInt32());
        if (properties[i].PropertyType == typeof(float))
          properties[i].SetValue(instance, binaryReader.ReadSingle());
        if (properties[i].PropertyType == typeof(string))
          properties[i].SetValue(instance, binaryReader.ReadString(attributeList[i].FieldLength, false));
      }

      return (T)instance;
    }

    public List<T> GetData<T>(bool secure = true) where T : new()
    {
      var classType = typeof(T);
      var tableAttribute = classType.GetCustomAttribute<TableAttribute>();

      var properties = classType.GetProperties().Where(x => x.GetCustomAttribute<FieldAttribute>() != null).ToList();
      if (properties.Count() != FieldCount)
        return default;

      if (secure)
      {
        for (var index = 0; index < FieldCount; index++)
        {
          var attribute = properties[index].GetCustomAttribute<FieldAttribute>();
          if (attribute.FieldLength != _fields[index].Size || !attribute.FieldName.Equals(_fields[index].Name))
            return default;
        }
      }

      var attributeList = new List<FieldAttribute>();
      for (var index = 0; index < FieldCount; index++)
        attributeList.Add(properties[index].GetCustomAttribute<FieldAttribute>());

      _cdbStream.Seek(_offset, SeekOrigin.Begin);
      var binaryReader = new BinaryReader(_cdbStream);

      var items = new List<T>();
      for (var index = 0; index < DataCount; index++)
      {
        var instance = Activator.CreateInstance(classType);
        for (var i = 0; i < FieldCount; i++)
        {
          if (properties[i].PropertyType == typeof(char))
            properties[i].SetValue(instance, binaryReader.ReadChar());
          else if (properties[i].PropertyType == typeof(bool))
            properties[i].SetValue(instance, binaryReader.ReadBoolean());
          else if(properties[i].PropertyType == typeof(short))
            properties[i].SetValue(instance, binaryReader.ReadInt16());
          else if(properties[i].PropertyType == typeof(int))
            properties[i].SetValue(instance, binaryReader.ReadInt32());
          else if(properties[i].PropertyType == typeof(float))
            properties[i].SetValue(instance, binaryReader.ReadSingle());
          else
            properties[i].SetValue(instance, binaryReader.ReadString(attributeList[i].FieldLength, false));
        }

        items.Add((T)instance);
      }

      return items;
    }

    public void Save(string path)
    {
      if (_cdbStream == null)
        throw new NullReferenceException("Unable to store database, stream is null");

      using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
      {
        _cdbStream.Seek(0, SeekOrigin.Begin);
        _cdbStream.CopyTo(fileStream);
      }
    }
  }
}
