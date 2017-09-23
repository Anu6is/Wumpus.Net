using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Formatting;
using System.Text.Utf8;

namespace Wumpus.Serialization
{
    public abstract class Serializer
    {
        public event Action<string, Exception> ModelError;
        public event Action<string> UnmappedProperty;

        private static readonly MethodInfo _createPropertyMapMethod
            = typeof(Serializer).GetTypeInfo().GetDeclaredMethod(nameof(CreatePropertyMap));
        
        private readonly ConcurrentDictionary<Type, object> _maps;
        private readonly ConverterCollection _converters;
        private readonly ConcurrentHashSet<string> _unknownProps = new ConcurrentHashSet<string>();

        protected Serializer()
            : this(null) { }
        protected Serializer(Serializer parent)
        {
            _maps = new ConcurrentDictionary<Type, object>();
            _converters = new ConverterCollection(parent?._converters);
        }

        protected internal object GetConverter(Type valueType, PropertyInfo propInfo = null)
            => _converters.Get(this, valueType, propInfo);

        public void AddConverter(Type valueType, Type converterType)
            => _converters.Add(valueType, converterType);
        public void AddConverter(Type valueType, Type converterType, Func<TypeInfo, PropertyInfo, bool> condition)
            => _converters.Add(valueType, converterType, condition);

        public void AddGenericConverter(Type converterType, Func<Type, Type> typeSelector = null)
            => _converters.AddGeneric(converterType, typeSelector);
        public void AddGenericConverter(Type converterType, Func<TypeInfo, PropertyInfo, bool> condition, Func<Type, Type> typeSelector = null)
            => _converters.AddGeneric(converterType, condition, typeSelector);
        public void AddGenericConverter(Type valueType, Type converterType, Func<Type, Type> typeSelector = null)
            => _converters.AddGeneric(valueType, converterType, typeSelector);
        public void AddGenericConverter(Type valueType, Type converterType, Func<TypeInfo, PropertyInfo, bool> condition, Func<Type, Type> typeSelector = null)
            => _converters.AddGeneric(valueType, converterType, condition, typeSelector);

        public void AddSelectorConverter(string groupKey, Type keyType, object keyValue, Type converterType)
            => _converters.AddSelector(this, groupKey, keyType, keyValue, converterType);
        public void AddSelectorConverter<TKey, TConverter>(string groupKey, TKey keyValue)
            => _converters.AddSelector(this, groupKey, typeof(TKey), keyValue, typeof(TConverter));
        
        public ISelectorGroup GetSelectorGroup(Type keyType, string groupKey)
            => _converters.GetSelectorGroup(keyType, groupKey);

        public ModelMap MapModel<TModel>()
        {
            return _maps.GetOrAdd(typeof(TModel), _ =>
            {
                var type = typeof(TModel).GetTypeInfo();
                var searchType = type;
                var properties = new List<PropertyMap>();
                var map = new ModelMap(type.Name);

                while (searchType != null)
                {
                    foreach (var propInfo in searchType.DeclaredProperties)
                    {
                        if (propInfo.GetCustomAttribute<ModelPropertyAttribute>() != null)
                        {
                            var propMap = MapProperty<TModel>(map, propInfo);
                            map.AddProperty(propMap);
                        }
                    }

                    searchType = searchType.BaseType?.GetTypeInfo();
                }
                return map;
            }) as ModelMap;
        }

        private PropertyMap MapProperty<TModel>(ModelMap modelMap, PropertyInfo propInfo)
            => _createPropertyMapMethod.MakeGenericMethod(typeof(TModel), propInfo.PropertyType).Invoke(this, new object[] { modelMap, propInfo }) as PropertyMap;
        protected abstract PropertyMap CreatePropertyMap<TModel, TValue>(ModelMap modelMap, PropertyInfo propInfo);

        public TModel Read<TModel>(ReadOnlyBuffer<byte> data)
            => Read<TModel>(data.Span);
        public abstract TModel Read<TModel>(ReadOnlySpan<byte> data);

        public void Write<T>(ITextOutput stream, object model)
            => Write(stream, model, typeof(T));
        public abstract void Write(ITextOutput stream, object model, Type type);

        public ArraySegment<byte> WriteToArray<TModel>(TModel model, int initialCapacity = 1024, SymbolTable symbolTable = null)
            => WriteToArray(model, typeof(TModel), initialCapacity, symbolTable);
        public ArraySegment<byte> WriteToArray(object model, Type type, int initialCapacity = 1024, SymbolTable symbolTable = null)
        {
            var formatter = new ArrayFormatter(initialCapacity, symbolTable ?? SymbolTable.InvariantUtf8);
            Write(formatter, model, type);
            return formatter.Formatted;
        }
        public byte[] WriteToTrimmedArray<TModel>(TModel model, int initialCapacity = 1024, SymbolTable symbolTable = null)
            => WriteToTrimmedArray(model, typeof(TModel), initialCapacity, symbolTable);
        public byte[] WriteToTrimmedArray(object model, Type type, int initialCapacity = 1024, SymbolTable symbolTable = null)
        {
            var segment = WriteToArray(model, type, initialCapacity, symbolTable);
            var arr = segment.Array;
            if (arr.Length != segment.Count)
                Array.Resize(ref arr, segment.Count);
            return arr;
        }

        public void RaiseModelError(string path, Exception ex)
        {
            if (ModelError != null)
                ModelError?.Invoke(path, ex);
        }
        public void RaiseModelError(ModelMap modelMap, Exception ex)
        {
            if (ModelError != null)
                ModelError?.Invoke(modelMap.Path, ex);
        }
        public void RaiseModelError(PropertyMap propMap, Exception ex)
        {
            if (ModelError != null)
                ModelError?.Invoke(propMap.Path, ex);
        }
        public void RaiseUnmappedProperty(string model, string propertyMap)
        {
            if (UnmappedProperty != null)
            {
                string path = $"{model}.{propertyMap}";
                if (_unknownProps.TryAdd(path))
                    UnmappedProperty?.Invoke(path);
            }
        }
        public void RaiseUnknownProperty(string model, ReadOnlyBuffer<byte> propertyMap)
        {
            if (UnmappedProperty != null)
            {
                string path = $"{model}.{new Utf8String(propertyMap.Span).ToString()}";
                if (_unknownProps.TryAdd(path))
                    UnmappedProperty?.Invoke(path);
            }
        }
        public void RaiseUnknownProperty(string model, ReadOnlySpan<byte> propertyMap)
        {
            if (UnmappedProperty != null)
            {
                string path = $"{model}.{new Utf8String(propertyMap).ToString()}";
                if (_unknownProps.TryAdd(path))
                    UnmappedProperty?.Invoke(path);
            }
        }
    }
}
