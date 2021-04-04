using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//This is so we can make sure nobody does dumb modifications to links by accident.
//We share the internals instead of facades because it makes more sense to give the graph
//full control over this class.
[assembly: InternalsVisibleTo("GraphFramework.GraphifyEditor")]
namespace GraphFramework
{
    //Forwarding class for type compare.
    public abstract class ValuePort : BasePort
    {
    }

    /// <summary>
    /// A port with a backing value.
    /// </summary>
    [Serializable]
    public class ValuePort<T> : ValuePort, PortWithValue<T>
    {
        //Backing value of the port, the is used as the initialization value for the port.
        [SerializeField]
        private T portValue = default;
        //Indexed by graph ID, this gives us a virtualized lookup of GraphId->CurrentPortValue
        [NonSerialized] 
        internal readonly Dictionary<int, T> virtualizedMutablePortValues = new Dictionary<int, T>();

        /// <summary>
        /// Resets the value of the virtual port for this graph id to the original port value.
        /// </summary>
        public override void Reset(int graphId)
        {
            virtualizedMutablePortValues[graphId] = portValue;
        }

        /// <summary>
        /// The local value of this port.
        /// </summary>
        public T LocalValue
        {
            set => virtualizedMutablePortValues[CurrentGraphIndex] = value;
            get => virtualizedMutablePortValues[CurrentGraphIndex];
        }

        /// <summary>
        /// Returns the value of a given link as T or default.
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
        /// Tries to get the value of the given link as some type other than than this port type.
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
        /// Returns the value of the first link or default.
        /// </summary>
        public T FirstValue()
        {
            #if UNITY_EDITOR
            if (links[0] != null && links[0].BindRemote())
            {
                links[0].Reset(CurrentGraphIndex);
            }
            #endif
            if (!(links[0]?.distantEndValueKey is PortWithValue<T> valuePort)) return default;
            if(valuePort.TryGetValue(CurrentGraphIndex, links[0], out var value))
                return value;

            return default;
        }

        /// <summary>
        /// Trys to get the value of the first link.
        /// </summary>
        public bool TryGetFirstValue(out T value)
        {
            #if UNITY_EDITOR
            if (links[0] != null && links[0].BindRemote())
            {
                links[0].Reset(CurrentGraphIndex);
            }
            #endif
            if (links[0]?.distantEndValueKey is PortWithValue<T> valuePort)
                return valuePort.TryGetValue(CurrentGraphIndex, links[0], out value);
            
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
            return virtualizedMutablePortValues.TryGetValue(graphId, out value);
        }
    }
}