using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GraphFramework
{
    [Serializable]
    public abstract class BasePort
    {
        //Internals shared with graph editor.
        [SerializeReference, HideInInspector]
        protected internal List<Link> links = new List<Link>();
        public static int CurrentGraphIndex { get; set; }

        public void Clear()
        {
            links.Clear();
        }

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