using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GraphFramework
{
    /// <summary>
    /// A one-time binding per graph controller (not virtual graph) per port.
    /// </summary>
    [System.Serializable]
    internal sealed class LinkBinder
    {
        [SerializeReference]
        internal RuntimeNode node;
        [SerializeReference]
        internal SerializedFieldInfo portField;

        internal LinkBinder(RuntimeNode remote,
            SerializedFieldInfo field)
        {
            node = remote;
            portField = field;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BasePort Bind()
        {
            var remotePortInfo = portField.FieldFromInfo;

            if (remotePortInfo == null)
            {
                Debug.LogError(
                    "Attempted to regenerate a port but the field has been renamed or removed, previously known as: " + portField.FieldName);
                return null;
            }
            
            BasePort port = remotePortInfo.GetValue(node) as BasePort;
            
            return port;
        }
    }
    
    /// <summary>
    /// You can imagine this class as a wrapper around a "pointer" to the remote side's value port.
    /// </summary>
    [System.Serializable]
    public class Link
    {
        [SerializeReference]
        private RuntimeNode linkedTo;
        [SerializeReference] 
        private LinkBinder remoteLinkBinder;
        //This is only used by the editor, but is quite difficult to factor out and only saves
        //a few bytes per link.
        [SerializeReference]
        private LinkBinder localLinkBinder;
        [SerializeReference]
        public string GUID;
        [SerializeField]
        public int remoteDynamicIndex;
        [SerializeField]
        public int localDynamicIndex;
        //This is the field we bind at runtime, which acts as a pointer to our data values.
        [NonSerialized]
        protected internal BasePort distantEndValueKey;
        [NonSerialized] 
        private bool valueBound = false;

        public Link(RuntimeNode localSide, SerializedFieldInfo localPortField, int localDynamicIndex,
            RuntimeNode remoteSide, SerializedFieldInfo remotePortField, int remoteDynamicIndex)
        {
            linkedTo = remoteSide;
            localLinkBinder = new LinkBinder(localSide, localPortField);
            remoteLinkBinder = new LinkBinder(remoteSide, remotePortField);
            this.localDynamicIndex = localDynamicIndex;
            this.remoteDynamicIndex = remoteDynamicIndex;
            GUID = Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// Creates the value key binding from serialization.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void BindRemote()
        {
            //Binding multiple times would not create errors, it's just an avoidable cost.
            if (valueBound) return;
            distantEndValueKey = remoteLinkBinder.Bind();
            valueBound = true;
        }

        /// <summary>
        /// Initializes the link (if necessary) and sets the mutable port values to the initialization value
        /// of the node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CreateVirtualizedLinks(int graphId)
        {
            BindRemote();
            Reset(graphId);
        }

        /// <summary>
        /// Resets the value of this link for the given graph Id to their initial value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int graphId)
        {
            #if UNITY_EDITOR
            //Safeguard for editor mutations, protects against added element edge case.
            BindRemote();
            #endif
            distantEndValueKey.Reset(graphId);
        }

        //Not cached because caching is not safe here. This ideally is not useful at runtime.
        /// <summary>
        /// Gets the local side of this connection, ideally don't call this ever you shouldn't have to.
        /// </summary>
        public BasePort GetLocalPort()
        {
            return localLinkBinder.Bind();
        }

        /// <summary>
        /// The node this link is connected to.
        /// </summary>
        public RuntimeNode Node => linkedTo;
    }
}