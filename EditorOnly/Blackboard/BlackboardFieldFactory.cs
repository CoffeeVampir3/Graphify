using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public static class BlackboardFieldFactory
    {
        public static VisualElement Create(string fieldKey, Type t, object someObject, DataBlackboard bb)
        {
            var field = FieldFactory.Create(t, someObject, bb, fieldKey);

            var labelName = ObjectNames.NicifyVariableName(t.Name);
            Foldout fv = new Foldout {text = labelName};
            TextField textField = new TextField();
            textField.SetValueWithoutNotify(fieldKey);

            textField.userData = field;
            textField.RegisterValueChangedCallback(e =>
            {
                if (!(textField.userData is BindableElement relatedField))
                {
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(e.newValue))
                    return;

                if (bb.blackboardDictionary.ContainsKey(e.newValue))
                {
                    Debug.LogWarning("Attempted to move key but their is already an item with that name!");
                    var newKey = Guid.NewGuid().ToString();
                    var oldKey = relatedField.userData as string;
                    relatedField.userData = newKey;
                    textField.SetValueWithoutNotify(newKey);
                    bb.MoveRenamedItem(oldKey, newKey);
                    return;
                }
                
                relatedField.userData = e.newValue;
                bb.MoveRenamedItem(e.previousValue, e.newValue);
            });
            
            fv.Add(textField);
            fv.Add(field);

            field.AddToClassList("dataBlackboard-item");
            field.AddToClassList("dataBlackboard-field");
            field.AddToClassList("dataBlackboard-valuefield");
            fv.AddToClassList("dataBlackboard-item");
            fv.AddToClassList("dataBlackboard-foldout");
            textField.AddToClassList("dataBlackboard-item");
            textField.AddToClassList("dataBlackboard-field");
            textField.AddToClassList("dataBlackboard-textfield");
            
            return fv;
        }
    }
}