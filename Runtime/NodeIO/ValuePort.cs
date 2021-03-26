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
    /// <summary>
    /// A port with a backing value.
    /// </summary>
    [Serializable]
    public class ValuePort<T> : BasePort, PortWithValue<T>
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
        public override void Reset(int graphId) => virtualizedMutablePortValues[graphId] = portValue;

        /// <summary>
        /// The local value of this port.
        /// </summary>
        public T LocalValue
        {
            set => virtualizedMutablePortValues[CurrentGraphIndex] = value;
            get => virtualizedMutablePortValues[CurrentGraphIndex];
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
            return virtualizedMutablePortValues.TryGetValue(graphId, out value);
        }
    }
}