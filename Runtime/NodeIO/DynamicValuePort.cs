using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public class DynamicValuePort<T> : BasePort, PortWithValue<T>
    {
        //Backing value of the port, the is used as the initialization value for the port.
        [SerializeField] 
        private List<T> portValues = new List<T>();
        //Indexed by graph ID, this gives us a virtualized lookup of GraphId->CurrentPortValue
        [NonSerialized] 
        protected internal readonly Dictionary<int, List<T>> virtualizedMutablePortValues = 
            new Dictionary<int, List<T>>();

        public override void Reset(int graphId)
        {
            virtualizedMutablePortValues[graphId] = portValues;
        }
        
        public bool TryGetValue(Link link, out T value)
        {
            //DEBUG !!!!!!
            //TODO:: BAD
            link.BindRemote();
            if (link.distantEndValueKey is PortWithValue<T> valuePort)
            {
                return valuePort.TryGetValue(CurrentGraphIndex, link, out value);
            }

            value = default;
            return false;
        }

        bool PortWithValue<T>.TryGetValue(int graphId, Link link, out T value)
        {
            if (link.remoteDynamicIndex < 0 || link.remoteDynamicIndex >= portValues.Count)
            {
                value = default;
                return false;
            }
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