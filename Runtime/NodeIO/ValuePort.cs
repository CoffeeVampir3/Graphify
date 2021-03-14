using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//This is so we can make sure nobody does dumb modifications to links by accident.
//We share the internals instead of facades because it makes more sense to give the graph
//full control over this class.
[assembly: InternalsVisibleTo("GraphEditor")]
namespace GraphFramework
{
    /// <summary>
    /// An I/O port for the coffee graph. Internally this works as a simple dictionary.
    /// Connections with other nodes use a simple "pointer" key exchange,
    /// so retrieving a value is always one single dictionary lookup with no other overhead.
    /// </summary>
    [Serializable]
    public abstract class ValuePort : RuntimePort
    {
        //Internals shared with graph editor.
        [SerializeReference, HideInInspector]
        protected internal List<Link> links = new List<Link>();

        /// <summary>
        /// The list of links this port has.
        /// </summary>
        public IEnumerable<Link> Links => links;

        /// <summary>
        /// Returns true if this port has any links.
        /// </summary>
        public bool IsLinked()
        {
            return links.Count > 0;
        }

        /// <summary>
        /// Returns true if this port has more than one link.
        /// </summary>
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
        //The is the backing value of the port, connections reference this value.
        [SerializeField]
        protected internal T portValue = default;

        /// <summary>
        /// Used to set the value of this port.
        /// </summary>
        public T PortValue
        {
            set => portValue = value;
        }

        /// <summary>
        /// Returns the value of the first link or default.
        /// </summary>
        public T FirstValue()
        {
            return links.Count > 0 ? links[0].GetValueAs<T>() : default;
        }

        /// <summary>
        /// Returns the node for the first link or null.
        /// </summary>
        public RuntimeNode FirstNode()
        {
            return links.Count > 0 ? links[0].GetNode() : null;
        }
        
        /// <summary>
        /// Returns the first link or null.
        /// </summary>
        public Link FirstLink()
        {
            return links.Count > 0 ? links[0] : null;
        }

        /// <summary>
        /// Returns the value of the link at the given index or default.
        /// </summary>
        public T ValueOf(int index)
        {
            return links.Count > index ? links[index].GetValueAs<T>() : default;
        }
        
        /// <summary>
        /// Returns the linked node at the given index or null.
        /// </summary>
        public RuntimeNode NodeOf(int index)
        {
            return links.Count > index ? links[index].GetNode() : null;
        }
    }
}