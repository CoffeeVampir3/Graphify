using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphFramework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class FieldFactory
{
    private delegate BindableElement CreationDelegate(Type t, object o);
    private static readonly Dictionary<Type, CreationDelegate> creationDictionary =
        new Dictionary<Type, CreationDelegate>();
    private static readonly List<Type> creationTypes = new List<Type>();
    private static bool initialized = false;
    private static DataBlackboard dataBb;
    private static string bindKey;
    
    public static IReadOnlyCollection<Type> GetDrawableTypes()
    {
        if (!initialized)
        {
            RegisterAll();
            initialized = true;
        }

        return creationTypes;
    }

    public static BindableElement Create(Type t, object someObject, DataBlackboard bb, string bindTo)
    {
        dataBb = bb;
        bindKey = bindTo;
        CreationDelegate func = CheckIfRegistered(t);
        return func?.Invoke(t, someObject);
    }

    private static void Register<T>(CreationDelegate creationAction)
    {
        creationTypes.Add(typeof(T));
        creationDictionary.Add(typeof(T), creationAction);
    }

    private static void FastRegister<ArgType, FieldType>()
        where FieldType : BaseField<ArgType>, new()
    {
        Register<ArgType>(ConstructField<ArgType, FieldType>);
    }

    private static FieldType ConstructField<ArgType, FieldType>(Type t, object o)
        where FieldType : BaseField<ArgType>, new()
    {
        var m = new FieldType();
        if (o != null)
            m.SetValueWithoutNotify((ArgType) o);

        m.userData = bindKey.Clone();
        m.RegisterValueChangedCallback(e =>
        {
            string lookupKey = m.userData as string;
            if(!dataBb.TryGetValue<ArgType>(lookupKey, out _))
            {
                Debug.Log("Failed to find lookup for: " + lookupKey);
                return;
            }
            
            dataBb[lookupKey] = e.newValue;
        });

        return m;
    }

    private static void RegisterAll()
    {
        FastRegister<float, FloatField>();
        FastRegister<int, IntegerField>();
        FastRegister<string, TextField>();
        FastRegister<Vector2, Vector2Field>();
        FastRegister<Vector3, Vector3Field>();
        FastRegister<Vector4, Vector4Field>();
        FastRegister<Vector2Int, Vector2IntField>();
        FastRegister<Vector3Int, Vector3IntField>();
        FastRegister<Bounds, BoundsField>();
        FastRegister<BoundsInt, BoundsIntField>();
        FastRegister<Color, ColorField>();
        FastRegister<AnimationCurve, CurveField>();
        FastRegister<Gradient, GradientField>();
        FastRegister<Rect, RectField>();
        FastRegister<RectInt, RectIntField>();
        FastRegister<bool, Toggle>();
        Register<UnityEngine.Object>((t, o) =>
        {
            var m = new ObjectField {objectType = t, allowSceneObjects = true};
            if (o != null)
            {
                m.SetValueWithoutNotify((UnityEngine.Object) o);
            }
            
            m.userData = bindKey.Clone();
            m.RegisterValueChangedCallback(e =>
            {
                string lookupKey = m.userData as string;
                if(!dataBb.TryGetValue<UnityEngine.Object>(lookupKey, out _))
                {
                    Debug.Log("Failed to find lookup for: " + lookupKey);
                    return;
                }
            
                dataBb[lookupKey] = e.newValue;
            });

            return m;
        });
        Register<Enum>(CreateEnum);
    }

    private static BindableElement CreateEnum(Type t, object o)
    {
        if (o == null)
            return null;

        var en = (Enum) o;
        bool isFlags = t.GetCustomAttributes<FlagsAttribute>().Any();

        static EnumFlagsField CreateFlagsEnums(Enum someEnum)
        {
            EnumFlagsField ff = new EnumFlagsField(someEnum) {userData = bindKey.Clone()};

            ff.RegisterValueChangedCallback(e =>
            {
                string lookupKey = ff.userData as string;
                if(!dataBb.TryGetValue<UnityEngine.Object>(lookupKey, out _))
                {
                    Debug.Log("Failed to find lookup for: " + lookupKey);
                    return;
                }
            
                dataBb[lookupKey] = e.newValue;
            });
            return ff;
        }

        static PopupField<string> CreateNormalEnumField(Enum someEnum, Type t)
        {
            Array vals = Enum.GetValues(t);
            List<string> strings = new List<string>();
            foreach (var item in vals)
            {
                strings.Add(item.ToString());
            }

            if (strings.Count == 0)
                return null;

            PopupField<string> pf = new PopupField<string>(strings, someEnum.ToString())
                {userData = bindKey.Clone()};
            
            pf.SetValueWithoutNotify(someEnum.ToString());
            pf.RegisterValueChangedCallback(e =>
            {
                string lookupKey = pf.userData as string;
                if(!dataBb.TryGetValue<UnityEngine.Object>(lookupKey, out _))
                {
                    Debug.Log("Failed to find lookup for: " + lookupKey);
                    return;
                }
            
                dataBb[lookupKey] = e.newValue;
            });
            return pf;
        }

        return isFlags ? CreateFlagsEnums(en) as BindableElement : CreateNormalEnumField(en, t);
    }

    private static CreationDelegate CheckIfRegistered(Type t)
    {
        if (!initialized)
        {
            RegisterAll();
            initialized = true;
        }

        //Check for exact matches
        if (creationDictionary.TryGetValue(t, out var creationFunc))
            return creationFunc;

        //We found no exact match, our fall back is checking the entire inheritance tree.
        return (from item
                in creationTypes
            where item.IsAssignableFrom(t)
            select creationDictionary[item]).FirstOrDefault();
    }
}
