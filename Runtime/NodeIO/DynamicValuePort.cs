using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//This is so we can make sure nobody does dumb modifications to ports by accident.
//We share the internals instead of facades because it makes more sense to give the graph
//full control over this class.
[assembly: InternalsVisibleTo("GraphEditor")]
namespace GraphFramework
{
    //TODO:: This class should unpack to ValuePorts if possible?
    /// <summary>
    /// Dynamic(editor time) port, a collection of value ports.
    /// </summary>
    [Serializable]
    public abstract class DynamicValuePort : RuntimePort
    {
        [SerializeReference]
        protected List<ValuePort> ports = new List<ValuePort>();
        //True so we build the initial cache for the first call.
        [NonSerialized]
        protected bool portsChanged = true;

        protected internal void AddPort(ValuePort port)
        {
            ports.Add(port);
            portsChanged = true;
        }
        protected internal void RemovePort(ValuePort port)
        {
            if (!ports.Contains(port)) return;
            
            ports.Remove(port);
            portsChanged = true;
        }
        protected internal void ClearPorts()
        {
            ports.Clear();
        }
    }

    /// <summary>
    /// Dynamic(editor time) port, a collection of value ports.
    /// </summary>
    [Serializable]
    public class DynamicValuePort<T> : DynamicValuePort
    {
        [NonSerialized]
        private readonly List<ValuePort<T>> cachedPortCastedList = 
            new List<ValuePort<T>>();
        
        public List<ValuePort<T>> GetPorts()
        {
            if (!portsChanged) return cachedPortCastedList;
            
            cachedPortCastedList.Clear();
            foreach (var port in (ports))
            {
                cachedPortCastedList.Add(port as ValuePort<T>);
            }

            portsChanged = false;
            return cachedPortCastedList;
        }
    }
}