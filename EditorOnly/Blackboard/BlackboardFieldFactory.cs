using System;
using UnityEditor;
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

                var newKey = e.newValue;
                //Ensures we don't add duplicate keys while also hopefully being useful
                tryagain:
                if (bb.Members.ContainsKey(newKey))
                {
                    char lastDig = newKey[newKey.Length - 1];
                    if (!char.IsDigit(lastDig))
                    {
                        newKey += "1";
                    }
                    else
                    {
                        if (lastDig == '9')
                        {
                            newKey += "1";
                            goto tryagain;
                        }

                        newKey = newKey.Substring(0, newKey.Length - 1);
                        newKey += (char)(lastDig + 1);
                    }
                    goto tryagain;
                }
                
                //related field uses data holds our lookup key value when it sets its value.
                relatedField.userData = newKey;
                textField.SetValueWithoutNotify(newKey);
                bb.MoveRenamedItem(e.previousValue, newKey);
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