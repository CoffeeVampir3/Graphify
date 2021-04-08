using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("GraphFramework.GraphifyEditor")]
namespace GraphFramework
{
    [CreateAssetMenu]
    public class DataBlackboard : ScriptableObject, IDataBlackboard, ISerializationCallbackReceiver
    {
        [SerializeReference] 
        public object[] serializedObjects;
        [SerializeField] 
        public string[] serializedKeys;
        [SerializeField] 
        public UnityEngine.Object[] unityObjects;
        [SerializeField] 
        public string[] serializedUnityKeys;
        //Curves and gradients are not serialized correctly by SerializeReference[object]
        //So we handle these very special cases q-q
        [SerializeField] 
        public AnimationCurve[] serializedCurves;
        [SerializeField] 
        public string[] serializedCurveKeys;
        [SerializeField]
        public Gradient[] serializedGradients;
        [SerializeField] 
        public string[] serializedGradientKeys;

        private readonly Dictionary<string, object> data = new Dictionary<string, object>();
        public IReadOnlyDictionary<string, object> Members => data;

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

        public bool MoveRenamedItem(string originalKey, string newKey)
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
        
        public void OnBeforeSerialize()
        {
            var objects = new List<object>(128);
            var objectKeys = new List<string>(128);
            var unityObjs = new List<UnityEngine.Object>(64);
            var unityKeys = new List<string>(64);
            var curves = new List<AnimationCurve>(16);
            var curveKeys = new List<string>(16);
            var gradients = new List<Gradient>(16);
            var gradientKeys = new List<string>(16);
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
            
            serializedObjects = objects.ToArray();
            serializedKeys = objectKeys.ToArray();
            unityObjects = unityObjs.ToArray();
            serializedUnityKeys = unityKeys.ToArray();
            serializedGradients = gradients.ToArray();
            serializedGradientKeys = gradientKeys.ToArray();
            serializedCurves = curves.ToArray();
            serializedCurveKeys = curveKeys.ToArray();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void OnAfterDeserialize()
        {
            for (var i = 0; i < serializedObjects.Length; i++)
            {
                var item = serializedObjects[i];
                var key = serializedKeys[i];
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