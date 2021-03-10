using System;
using System.Runtime.CompilerServices;
using UnityEngine;

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
            ValuePort port = remotePortInfo.GetValue(node) as ValuePort;

#if UNITY_EDITOR
            if (remotePortInfo == null || port == null) {
                throw new ArgumentException("Unable to instantiate a port from field named: " + portField?.FieldName + "" +
                                            " . Field was likely renamed or removed.");
            }
#endif
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

        public Link(RuntimeNode localSide, SerializedFieldInfo localPortField,
            RuntimeNode remoteSide, SerializedFieldInfo remotePortField)
        {
            linkedTo = remoteSide;
            localLinkBinder = new LinkBinder(localSide, localPortField);
            remoteLinkBinder = new LinkBinder(remoteSide, remotePortField);
            GUID = Guid.NewGuid().ToString();
        }

        //Not cached because caching is not safe here, ensure this is called once at runtime only.
        /// <summary>
        /// Creates the value key binding from serialization.
        /// </summary>
        public void BindRemote()
        {
            distantEndValueKey = remoteLinkBinder.Bind();
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
            if (!(distantEndValueKey is ValuePort<T> valuePort))
            {
                value = default;
                return false;
            }
            value = valuePort.portValue;
            return true;
        }
        
        /// <summary>
        /// Attempts to resolve the connection value. If the type *might* not be what you expect,
        /// use TryGetValue.
        /// </summary>
        public T GetValueAs<T>()
        {
            if (distantEndValueKey is ValuePort<T> valuePort)
            {
                return valuePort.portValue;
            }
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