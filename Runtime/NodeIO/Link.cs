using System;
using System.Collections.Generic;
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
        
        #if !ENABLE_IL2CPP
        //A faster alternative to reflection, and a better all around solution but does not 
        //work for IL2CPP users due to using Il.Emit.
        private static Dictionary<(Type, string), Func<RuntimeNode, ValuePort>> fastGetters =
            new Dictionary<(Type, string), Func<RuntimeNode, ValuePort>>();
        #endif

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

            #if ENABLE_IL2CPP
            ValuePort port = remotePortInfo.GetValue(node) as ValuePort;
            #else
            //See comment for fastGetters
            if (!fastGetters.TryGetValue((node.GetType(), portField.FieldName), out var fastGetter))
            {
                fastGetter = CreateFastGetter.Create<RuntimeNode, ValuePort>(remotePortInfo);
                fastGetters.Add((node.GetType(), portField.FieldName), fastGetter);
            }
            ValuePort port = fastGetter(node);
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
            //Lazy binding, this is the most optimal strategy as we will not attempt to create
            //tons of binding data all at once, and the binding we do create gets cached incrementally
            //as a result.
            if(!valueBound)
                BindRemote();
            if (!(distantEndValueKey is ValuePort<T> valuePort))
            {
                //No error because this was a try get.
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
            //Lazy binding, this is the most optimal strategy as we will not attempt to create
            //tons of binding data all at once, and the binding we do create gets cached incrementally
            //as a result.
            if(!valueBound)
                BindRemote();
            if (distantEndValueKey is ValuePort<T> valuePort)
            {
                return valuePort.portValue;
            }

            //This should be an error, as GetValueAs should not be trying to get an illegal type.
            Debug.LogError("Attempted to resolve value port on " + remoteLinkBinder.node + " with field name: " + remoteLinkBinder.portField + " but it was not able to be resolved. Likely a mismatched type.");
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