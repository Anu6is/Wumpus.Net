using System;
using System.Text.Json;

namespace Wumpus.Serialization.Json.Converters
{
    public class ReadOnlyObjectPropertyConverter<T> : JsonPropertyConverter<T>
        where T : class
    {
        protected readonly ModelMap _map;

        public ReadOnlyObjectPropertyConverter(Serializer serializer)
        {
            _map = serializer.MapModel<T>();
        }

        public override T Read(Serializer serializer, ModelMap modelMap, PropertyMap propMap, object model, ref JsonReader reader, bool isTopLevel)
            => throw new NotSupportedException($"{typeof(T).Name} has no valid constructor defined");
        public override void Write(Serializer serializer, ModelMap modelMap, PropertyMap propMap, object model, ref JsonWriter writer, T value, string key)
        {
            if (value == null)
            {
                if (key != null)
                    writer.WriteAttributeNull(key);
                else
                    writer.WriteNull();
            }
            else
            {
                if (key != null)
                    writer.WriteObjectStart(key);
                else
                    writer.WriteObjectStart();
                for (int i = 0; i < _map.Properties.Count; i++)
                {
                    if (_map.Properties[i].CanWrite)
                        (_map.Properties[i] as IJsonPropertyMap<T>).Write(serializer, value, ref writer);
                }
                writer.WriteObjectEnd();
            }
        }
    }

    public class ObjectPropertyConverter<T> : ReadOnlyObjectPropertyConverter<T>
        where T : class, new()
    {
        public ObjectPropertyConverter(Serializer serializer)
            : base(serializer) { }

        public override T Read(Serializer serializer, ModelMap modelMap, PropertyMap propMap, object model, ref JsonReader reader, bool isTopLevel)
        {
            var subModel = new T();

            if ((isTopLevel && !reader.Read()) || (reader.TokenType != JsonTokenType.StartObject && reader.ValueType != JsonValueType.Null))
                throw new SerializationException("Bad input, expected StartObject or Null");

            if (reader.ValueType == JsonValueType.Null)
                return null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return subModel;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new SerializationException("Bad input, expected PropertyName");

                if (_map.TryGetProperty(reader.Value, out var property) && property.CanRead)
                {
                    try { (property as IJsonPropertyMap<T>).Read(serializer, subModel, ref reader); }
                    catch (Exception ex) { RaiseModelError(serializer, property, ex); }
                }
                else
                {
                    RaiseUnmappedProperty(serializer, _map, reader.Value);
                    JsonReaderUtils.Skip(ref reader); //Unknown property, skip
                }
            }
            throw new SerializationException("Bad input, expected EndObject");
        }
    }
}