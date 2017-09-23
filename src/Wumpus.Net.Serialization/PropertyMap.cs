﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Utf8;

namespace Wumpus.Serialization
{
    public abstract class PropertyMap
    {
        public string Key { get; }
        public ReadOnlyBuffer<byte> Utf8Key { get; }
        public string Name { get; }
        public string Path { get; }
        public bool CanWrite { get; }
        public bool CanRead { get; }
        public bool ExcludeNull { get; }

        internal PropertyMap(Serializer serializer, PropertyInfo propInfo, ModelMap modelMap)
        {
            Name = propInfo.Name;
            Path = $"{modelMap.Path}.{propInfo.Name}";

            CanWrite = propInfo.GetMethod != null;
            CanRead = propInfo.SetMethod != null;

            var attr = propInfo.GetCustomAttribute<ModelPropertyAttribute>();
            Key = attr.Key ?? propInfo.Name;
            Utf8Key = new ReadOnlyBuffer<byte>(new Utf8String(Key).Bytes.ToArray());
            ExcludeNull = attr.ExcludeNull;
        }
        
        public abstract object GetDynamicConverter(object model, bool throwOnMissing);
    }

    public abstract class PropertyMap<TModel, TValue> : PropertyMap
    {
        private class Selector
        {
            private static readonly MethodInfo _getSelectorKeyFunc
                = typeof(Selector).GetTypeInfo().GetDeclaredMethod(nameof(GetSelectorKey));

            private readonly ISelectorGroup _group;
            private readonly Delegate _getSelectorFunc;
            private readonly Delegate _getWrappedSelectorFunc;

            public Selector(PropertyInfo propInfo, ISelectorGroup group, Delegate getSelectorFunc)
            {
                _group = group;
                _getSelectorFunc = getSelectorFunc;

                var funcType = typeof(Func<,>).MakeGenericType(typeof(object), propInfo.PropertyType);
                _getWrappedSelectorFunc = _getSelectorKeyFunc.MakeGenericMethod(propInfo.PropertyType).CreateDelegate(funcType, this);
            }

            private TKey GetSelectorKey<TKey>(object model)
                => (_getSelectorFunc as Func<TModel, TKey>)((TModel)model);
            public object GetDynamicConverter(object model)
                => _group?.GetDynamicConverter(_getWrappedSelectorFunc, model);
        }
        
        private readonly IReadOnlyList<Selector> _selectors;

        protected PropertyMap(Serializer serializer, ModelMap modelMap, PropertyInfo propInfo)
            : base(serializer, propInfo, modelMap)
        {
            _selectors = propInfo.GetCustomAttributes<ModelSelectorAttribute>()
                .Select(x =>
                {
                    var prop = typeof(TModel).GetTypeInfo().DeclaredProperties.FirstOrDefault(y => y.Name == x.SelectorProperty);
                    if (prop == null)
                        throw new SerializationException($"Selector key \"{x.SelectorProperty}\" not found");
                    var selectorGroup = serializer.GetSelectorGroup(prop.PropertyType, x.SelectorGroup);

                    var funcType = typeof(Func<,>).MakeGenericType(typeof(TModel), prop.PropertyType);
                    var selectorFunc = prop.GetMethod.CreateDelegate(funcType);

                    return new Selector(prop, selectorGroup, selectorFunc);
                })
                .ToList();
        }

        public override object GetDynamicConverter(object model, bool throwOnMissing)
        {
            for (int i = 0; i < _selectors.Count; i++)
            {
                object converter = _selectors[i].GetDynamicConverter(model);
                if (converter != null)
                    return converter;
            }
            if (throwOnMissing)
                throw new SerializationException($"Unable to find a converter for {Name}.");
            return null;
        }
    }
}
