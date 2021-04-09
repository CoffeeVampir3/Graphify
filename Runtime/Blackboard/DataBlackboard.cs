using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("GraphFramework.GraphifyEditor")]
namespace GraphFramework
{
    [CreateAssetMenu(menuName = "Graphify/Blackboard")]
    public class DataBlackboard : ScriptableObject, IDataBlackboard, ISerializationCallbackReceiver
    {
        [SerializeReference, HideInInspector] 
        private object[] serializedObjects;
        [SerializeField, HideInInspector] 
        private string[] serializedKeys;
        //then we handle all the edge cases object mysteriously doesn't work on ~,~
        [SerializeField, HideInInspector] 
        private UnityEngine.Object[] unityObjects;
        [SerializeField, HideInInspector] 
        private string[] serializedUnityKeys;
        [SerializeField, HideInInspector] 
        private AnimationCurve[] serializedCurves;
        [SerializeField, HideInInspector] 
        private string[] serializedCurveKeys;
        [SerializeField, HideInInspector]
        private Gradient[] serializedGradients;
        [SerializeField, HideInInspector] 
        private string[] serializedGradientKeys;
        [SerializeField, HideInInspector]
        private string[] serializedStrings;
        [SerializeField, HideInInspector] 
        private string[] serializedStringKeys;
        internal readonly Dictionary<string, object> data = new Dictionary<string, object>();
        public IReadOnlyDictionary<string, object> Members => data;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string lookupKey, out object val)
        {
            return data.TryGetValue(lookupKey, out val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<T>(string lookupKey, out T val)
        {
            if (!data.TryGetValue(lookupKey, out var objValue) || 
                !(objValue is T tVal))
            {
                val = default;
                return false;
            }

            val = tVal;
            return true;
        }

        public void Remove(string key)
        {
            if (data.ContainsKey(key))
                data.Remove(key);
        }
        
        public object this[string key]
        {
            get => data[key];
            set
            {
                if (data.ContainsKey(key))
                    data[key] = value;
                else
                    data.Add(key, value);
                #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                #endif
            }
        }

        internal Dictionary<string, object> Copy()
        {
            return new Dictionary<string, object>(data);
        }

        internal bool MoveRenamedItem(string originalKey, string newKey)
        {
            if (!data.TryGetValue(originalKey, out var val))
            {
                return false;
            }
            if(data.ContainsKey(originalKey))
                data.Remove(originalKey);
            
            #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
            #endif
            if (data.ContainsKey(newKey))
            {
                return false;
            }

            data.Add(newKey, val);
            return true;
        }

        public void OnBeforeSerialize()
        {
            var objects = new List<object>(16);
            var objectKeys = new List<string>(16);
            var strings = new List<string>(16);
            var stringKeys = new List<string>(16);
            var unityObjs = new List<UnityEngine.Object>(16);
            var unityKeys = new List<string>(16);
            var curves = new List<AnimationCurve>(8);
            var curveKeys = new List<string>(8);
            var gradients = new List<Gradient>(8);
            var gradientKeys = new List<string>(8);
            var hashedKeys = new HashSet<string>();
            
            foreach (var item in data)
            {
                if (hashedKeys.Contains(item.Key))
                {
                    Debug.LogWarning("Attempted to serialize multiple of the same key named: " + item.Key);
                    continue;
                }
                hashedKeys.Add(item.Key);
                switch (item.Value)
                {
                    case UnityEngine.Object uObj:
                        unityObjs.Add(uObj);
                        unityKeys.Add(item.Key);
                        continue;
                    case Gradient grad:
                        gradients.Add(grad);
                        gradientKeys.Add(item.Key);
                        continue;
                    case AnimationCurve curve:
                        curves.Add(curve);
                        curveKeys.Add(item.Key);
                        continue;
                    case string str:
                        strings.Add(str);
                        stringKeys.Add(item.Key);
                        continue;
                    default:
                        objects.Add(item.Value);
                        objectKeys.Add(item.Key);
                        continue;
                }
            }
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif

            //Seems to be needed to prod the serialized object to actually do something.
            serializedObjects = null;
            serializedKeys = null;
            serializedUnityKeys = null;
            unityObjects = null;
            serializedCurves = null;
            serializedCurveKeys = null;
            serializedGradients = null;
            serializedGradientKeys = null;
            serializedStrings = null;
            serializedStringKeys = null;
            
            serializedObjects = objects.ToArray();
            serializedKeys = objectKeys.ToArray();
            unityObjects = unityObjs.ToArray();
            serializedUnityKeys = unityKeys.ToArray();
            serializedGradients = gradients.ToArray();
            serializedGradientKeys = gradientKeys.ToArray();
            serializedCurves = curves.ToArray();
            serializedCurveKeys = curveKeys.ToArray();
            serializedStrings = strings.ToArray();
            serializedStringKeys = stringKeys.ToArray();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void OnAfterDeserialize()
        {
            data.Clear();
            for (var i = 0; i < serializedObjects.Length; i++)
            {
                var item = serializedObjects[i];
                var key = serializedKeys[i];
                data.Add(key, item);
            }

            for (var i = 0; i < serializedStrings.Length; i++)
            {
                var item = serializedStrings[i];
                var key = serializedStringKeys[i];
                data.Add(key, item);
            }

            for (var i = 0; i < unityObjects.Length; i++)
            {
                var item = unityObjects[i];
                var key = serializedUnityKeys[i];
                data.Add(key, item);
            }
            
            for (var i = 0; i < serializedCurves.Length; i++)
            {
                var item = serializedCurves[i];
                var key = serializedCurveKeys[i];
                data.Add(key, item);
            }
            
            for (var i = 0; i < serializedGradients.Length; i++)
            {
                var item = serializedGradients[i];
                var key = serializedGradientKeys[i];
                data.Add(key, item);
            }
        }
    }
}