using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor
{
    public class BlueprintHelper
    {
        private readonly SerializedObject serializedBp;
        private readonly SerializedProperty bpNodes;
        private readonly SerializedProperty bpGraphs;

        public BlueprintHelper(GraphBlueprint blueprint)
        {
            serializedBp = new SerializedObject(blueprint);
            bpNodes = serializedBp.FindProperty(nameof(blueprint.nodes));
            bpGraphs = serializedBp.FindProperty(nameof(blueprint.childGraphs));
        }

        public void Apply()
        {
            serializedBp.ApplyModifiedPropertiesWithoutUndo();
        }

        public void AddNode(RuntimeNode node)
        {
            bpNodes.InsertArrayElementAtIndex(bpNodes.arraySize);
            SerializedProperty sp = bpNodes.GetArrayElementAtIndex(bpNodes.arraySize - 1);
            sp.objectReferenceValue = node;
        }

        public void RemoveNode(RuntimeNode node)
        {
            for (int i = 0; i < bpNodes.arraySize; i++)
            {
                var sp = bpNodes.GetArrayElementAtIndex(i);
                if (sp.objectReferenceValue != node) continue;
                bpNodes.DeleteArrayElementAtIndex(i);
                return;
            }
        }
        
        public void AddGraph(GraphBlueprint graph)
        {
            Debug.Log(bpGraphs.arraySize);
            bpGraphs.InsertArrayElementAtIndex(bpGraphs.arraySize);
            SerializedProperty sp = bpGraphs.GetArrayElementAtIndex(bpGraphs.arraySize - 1);
            sp.objectReferenceValue = graph;
        }

        public void RemoveGraph(GraphBlueprint graph)
        {
            for (int i = 0; i < bpGraphs.arraySize; i++)
            {
                var sp = bpGraphs.GetArrayElementAtIndex(i);
                if (sp.objectReferenceValue != graph) continue;
                bpGraphs.DeleteArrayElementAtIndex(i);
                return;
            }
        }
    }
}