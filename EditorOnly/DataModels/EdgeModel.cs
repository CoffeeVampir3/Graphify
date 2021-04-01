using System;
using UnityEngine;

namespace GraphFramework.Editor
{
    [Serializable]
    public class EdgeModel
    {
        [SerializeReference] 
        protected internal NodeModel inputModel;
        [SerializeReference] 
        protected internal NodeModel outputModel;
        [SerializeReference] 
        protected internal PortModel inputPortModel;
        [SerializeReference] 
        protected internal PortModel outputPortModel;
        [SerializeReference]
        protected internal string inputConnectionGuid;
        [SerializeReference]
        protected internal string outputConnectionGuid;

        public EdgeModel(NodeModel inputNode, PortModel inputPort, 
            NodeModel outputNode, PortModel outputPort,
            string inputConnectionGuid, string outputConnectionGuid)
        {
            this.inputModel = inputNode;
            this.outputModel = outputNode;
            this.inputPortModel = inputPort;
            this.outputPortModel = outputPort;
            this.inputConnectionGuid = inputConnectionGuid;
            this.outputConnectionGuid = outputConnectionGuid;
        }
    }
}