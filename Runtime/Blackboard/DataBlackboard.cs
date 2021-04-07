using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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
        protected internal readonly Dictionary<string, object> blackboardDictionary = new Dictionary<string, object>();

        public bool TryGetValue<T>(string lookupKey, out T val)
        {
            if (!blackboardDictionary.TryGetValue(lookupKey, out var objValue) || 
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
            if (!blackboardDictionary.TryGetValue(originalKey, out var val))
            {
                return false;
            }
            if(blackboardDictionary.ContainsKey(originalKey))
                blackboardDictionary.Remove(originalKey);
            if (blackboardDictionary.ContainsKey(newKey))
            {
                return false;
            }

            blackboardDictionary.Add(newKey, val);
            return true;
        }

        public object this[string key]
        {
            get => blackboardDictionary[key];
            set
            {
                if (blackboardDictionary.ContainsKey(key))
                    blackboardDictionary[key] = value;
                else
                    blackboardDictionary.Add(key, value);
            }
        }

        public void OnBeforeSerialize()
        {
            List<object> objects = new List<object>(100);
            List<string> objectKeys = new List<string>(100);
            List<UnityEngine.Object> unityObjs = new List<Object>(100);
            List<string> unityKeys = new List<string>(100);
            foreach (var item in blackboardDictionary)
            {
                if (item.Value is UnityEngine.Object uObj)
                {
                    unityObjs.Add(uObj);
                    unityKeys.Add(item.Key);
                    continue;
                }
                objects.Add(item.Value);
                objectKeys.Add(item.Key);
            }

            serializedObjects = objects.ToArray();
            serializedKeys = objectKeys.ToArray();
            unityObjects = unityObjs.ToArray();
            serializedUnityKeys = unityKeys.ToArray();
        }

        public void OnAfterDeserialize()
        {
            for (var i = 0; i < serializedObjects.Length; i++)
            {
                var item = serializedObjects[i];
                var key = serializedKeys[i];
                blackboardDictionary.Add(key, item);
            }
            for (var i = 0; i < unityObjects.Length; i++)
            {
                var item = unityObjects[i];
                var key = serializedUnityKeys[i];
                blackboardDictionary.Add(key, item);
            }
        }
    }
}