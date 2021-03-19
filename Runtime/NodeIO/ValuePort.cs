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
    /// An I/O port for the graph. This holds the semantic links as well as the set of virtualized values.
    /// </summary>
    [Serializable]
    public abstract class ValuePort
    {
        //Internals shared with graph editor.
        [SerializeReference, HideInInspector]
        protected internal List<Link> links = new List<Link>();
        public static int CurrentGraphIndex { get; set; }

        /// <summary>
        /// The list of links this port has.
        /// </summary>
        public List<Link> Links => links;

        /// <summary>
        /// Resets the value of a virtual port back to it's original value.
        /// </summary>
        public abstract void Reset(int graphId);

        /// <summary>
        /// Returns true if this port has any links.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLinked()
        {
            return links.Count > 0;
        }

        /// <summary>
        /// Returns true if this port has more than one link.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasMultipleLinks()
        {
            return links.Count > 1;
        }
    }

    /// <summary>
    /// A port with a backing value.
    /// </summary>
    [Serializable]
    public class ValuePort<T> : ValuePort
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
        /// Sets the local value of this port.
        /// </summary>
        public T LocalValue
        {
            set => virtualizedMutablePortValues[CurrentGraphIndex] = value;
        }

        /// <summary>
        /// Returns the value of the first link or default.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T FirstValue()
        {
            return links.Count > 0 ? links[0].GetValueAs<T>() : default;
        }

        /// <summary>
        /// Returns the node for the first link or null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeNode FirstNode()
        {
            return links.Count > 0 ? links[0].Node : null;
        }
        
        /// <summary>
        /// Returns the first link or null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Link FirstLink()
        {
            return links.Count > 0 ? links[0] : null;
        }
    }
}