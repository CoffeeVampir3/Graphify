using Sirenix.OdinInspector;
using UnityEngine;
using VisualNovelFramework.GraphFramework.Attributes;

namespace GraphFramework
{
    public class ModelTester : RuntimeNode
    {
        [In, SerializeField]
        public ValuePort<string> stringValue = new ValuePort<string>();
        [In, SerializeField] 
        private ValuePort<Flow> flowPortIn = new ValuePort<Flow>();
        [Out, SerializeField]
        public ValuePort<string> stringValue2 = new ValuePort<string>();
        [Out, SerializeField] 
        private ValuePort<Flow> flowPortOut = new ValuePort<Flow>();

        [Button]
        public void DoThing()
        {
            stringValue.FirstValue();
        }
    }
}