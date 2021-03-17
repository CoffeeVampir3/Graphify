using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GraphFramework
{
    /// <summary>
    /// Reflection that allows us to save the relationship between actual value field and value port.
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
        internal ValuePort Bind()
        {
            var remotePortInfo = portField.FieldFromInfo;

            if (remotePortInfo == null)
            {
                Debug.LogError(
                    "Attempted to regenerate a port but the field has been renamed or removed, previously known as: " + portField.FieldName);
                return null;
            }
            
            ValuePort port = remotePortInfo.GetValue(node) as ValuePort;
            
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
        public RuntimeNode linkedTo;
        [SerializeReference] 
        private LinkBinder remoteLinkBinder;
        //This is only used by the editor, but is quite difficult to factor out and only saves
        //a few bytes per link.
        [SerializeReference]
        private LinkBinder localLinkBinder;
        [SerializeReference] 
        public string GUID;
        //This is the field we bind at runtime, which acts as a pointer to our data values.
        [NonSerialized]
        protected internal ValuePort distantEndValueKey;
        [NonSerialized] 
        private bool valueBound = false;

        public Link(RuntimeNode localSide, SerializedFieldInfo localPortField,
            RuntimeNode remoteSide, SerializedFieldInfo remotePortField)
        {
            linkedTo = remoteSide;
            localLinkBinder = new LinkBinder(localSide, localPortField);
            remoteLinkBinder = new LinkBinder(remoteSide, remotePortField);
            GUID = Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// Creates the value key binding from serialization.
        /// </summary>
        private void BindRemote()
        {
            if (valueBound) return;
            distantEndValueKey = remoteLinkBinder.Bind();
            valueBound = true;
        }

        public void CreateVirtualizedLinks(int graphId)
        {
            BindRemote();
            distantEndValueKey.CreateVirtualPorts(graphId);
        }

        public void Reset(int graphId)
        {
            #if UNITY_EDITOR
            //Safeguard for editor mutations
            BindRemote();
            #endif
            distantEndValueKey.Reset(graphId);
        }
        
        //Not cached because caching is not safe here. This ideally is not useful at runtime.
        /// <summary>
        /// Gets the local side of this connection, ideally don't call this ever you shouldn't have to.
        /// </summary>
        public ValuePort GetLocalPort()
        {
            return localLinkBinder.Bind();
        }

        /// <summary>
        /// Attempts to resolve the value of the connection if the distant end type might not be
        /// what you expect. If there's only one possible type the connection could be, use GetValue.
        /// </summary>
        public bool TryGetValue<T>(out T value)
        {
            //Lazy binding for the editor, this is an edgecase safeguard where a new element was added
            //to the graph but was not bound by the VG initialization.
            #if UNITY_EDITOR
            BindRemote();
            #endif
            
            if (distantEndValueKey is ValuePort<T> valuePort)
            {
                //If we're in the unity editor, we have to account for the fact that the graph can be altered,
                //this weird looking thing checks to see if we failed to find a value, if we failed to find
                //the assumption is that this is a new element added to the graph, so we reset it and try again.
                #if UNITY_EDITOR
                if (!valuePort.virtualizedMutablePortValues.TryGetValue(ValuePort.CurrentGraphIndex, out value))
                {
                    valuePort.Reset(ValuePort.CurrentGraphIndex);
                    return valuePort.virtualizedMutablePortValues.TryGetValue(ValuePort.CurrentGraphIndex, out value);
                }
                #else
                return valuePort.virtualizedMutablePortValues.TryGetValue(ValuePort.CurrentGraphIndex, out value);
                #endif
            }
            
            //No error because this was a try get.
            value = default;
            return false;
        }
        
        /// <summary>
        /// Attempts to resolve the connection value. If the type *might* not be what you expect,
        /// use TryGetValue.
        /// </summary>
        public T GetValueAs<T>()
        {
            //Lazy binding for the editor, this is an edgecase safeguard where a new element was added
            //to the graph but was not bound by the VG initialization.
            #if UNITY_EDITOR
            BindRemote();
            #endif

            if (distantEndValueKey is ValuePort<T> valuePort)
            {
                //If we're in the unity editor, we have to account for the fact that the graph can be altered,
                //this weird looking thing checks to see if we failed to find a value, if we failed to find
                //the assumption is that this is a new element added to the graph, so we reset it and try again.
                #if UNITY_EDITOR
                if (!valuePort.virtualizedMutablePortValues.TryGetValue(ValuePort.CurrentGraphIndex, out var value))
                {
                    valuePort.Reset(ValuePort.CurrentGraphIndex);
                    valuePort.virtualizedMutablePortValues.TryGetValue(ValuePort.CurrentGraphIndex, out var value2);
                    return value2;
                }
                return value;
                #else
                return valuePort.virtualizedMutablePortValues[ValuePort.CurrentGraphIndex];
                #endif
            }

            //This should be an error, as GetValueAs should not be trying to get an illegal type.
            Debug.LogError("Attempted to resolve value port on " + 
                           remoteLinkBinder.node + " with field name: " + remoteLinkBinder.portField + 
                           " but it was not able to be resolved. Likely a mismatched type.");
            return default;
        }

        /// <summary>
        /// Returns the node this link is connected to.
        /// </summary>
        public RuntimeNode GetNode()
        {
            return linkedTo;
        }
    }
}