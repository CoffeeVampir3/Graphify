using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GraphFramework
{
    //Forwarding class for type compare.
    public abstract class DynamicValuePort : BasePort
    {
    }

    [Serializable]
    public class DynamicValuePort<T> : DynamicValuePort, PortWithValue<T>
    {
        //Backing value of the port, the is used as the initialization value for the port.
        [SerializeField] 
        private List<T> portValues = new List<T>();
        //Indexed by graph ID, this gives us a virtualized lookup of GraphId->CurrentPortValue
        [NonSerialized] 
        protected internal readonly Dictionary<int, List<T>> virtualizedMutablePortValues = 
            new Dictionary<int, List<T>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Reset(int graphId)
        {
            virtualizedMutablePortValues[graphId] = portValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLocalValue(List<T> val)
        {
            virtualizedMutablePortValues[CurrentGraphIndex] = val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> GetLocalValue(int index)
        {
            return virtualizedMutablePortValues[CurrentGraphIndex];
        }

        /// <summary>
        /// Tries to get the value of the given link or default.
        /// </summary>
        public bool TryGetValue(Link link, out T value)
        {
            #if UNITY_EDITOR
            if (link.BindRemote())
            {
                link.Reset(CurrentGraphIndex);
            }
            #endif
            if (link.distantEndValueKey is PortWithValue<T> valuePort)
            {
                return valuePort.TryGetValue(CurrentGraphIndex, link, out value);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get the value of the given link as SomeType or default.
        /// </summary>
        public bool TryGetValueAs<SomeType>(Link link, out SomeType value)
        {
            #if UNITY_EDITOR
            if (link.BindRemote())
            {
                link.Reset(CurrentGraphIndex);
            }
            #endif
            if (link.distantEndValueKey is PortWithValue<SomeType> valuePort)
            {
                return valuePort.TryGetValue(CurrentGraphIndex, link, out value);
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Tries to get the value of the given link as a covariant type type of <SomeType> and outs it as T
        /// </summary>
        public bool TryGetCovariant<SomeType>(Link link, out T value)
        {
            #if UNITY_EDITOR
            if (link.BindRemote())
            {
                link.Reset(CurrentGraphIndex);
            }
            #endif
            if (link.distantEndValueKey is PortWithValue<SomeType> valuePort)
            {
                valuePort.TryGetValue(CurrentGraphIndex, link, out var temp);
                if (temp is T val)
                {
                    value = val;
                    return true;
                }
            }
            value = default;
            return false;
        }
        
        bool PortWithValue<T>.TryGetValue(int graphId, Link link, out T value)
        {
            //Guard clause for editor adding new links in editor.
            #if UNITY_EDITOR
            if (link.BindRemote())
            {
                link.Reset(graphId);
            }
            #endif
            if (virtualizedMutablePortValues.TryGetValue(graphId, out var valueList))
            {
                value = valueList[link.remoteDynamicIndex];
                return true;
            }
            value = default;
            return false;
        }
    }
}
