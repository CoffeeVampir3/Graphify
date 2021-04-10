
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class BlackboardField : VisualElement
    {
        private readonly Foldout fv;
        private readonly TextField textField;
        private readonly Button deleteBtn;
        private readonly VisualElement svParent;

        public BlackboardField(string fieldKey, Type t, 
            object someObject, DataBlackboard bb, Action updateViewAction, VisualElement svParent)
        {
            var field = FieldFactory.Create(t, someObject, bb, fieldKey);

            var labelName = ObjectNames.NicifyVariableName(t.Name);
            fv = new Foldout {text = labelName};
            textField = new TextField();
            deleteBtn = new Button {text = "-"};
            this.svParent = svParent;
            textField.SetValueWithoutNotify(fieldKey);

            textField.userData = field;
            deleteBtn.userData = field;
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

            deleteBtn.clicked += () =>
            {
                if (!(textField.userData is BindableElement relatedField))
                {
                    return;
                }

                bb.Remove(relatedField.userData as string);
                updateViewAction.Invoke();
            };

            var m = fv.Q<VisualElement>("unity-checkmark");
            deleteBtn.ClearClassList();
            deleteBtn.AddToClassList("--delBtn");
            
            m.parent.style.flexShrink = 1;
            m.parent.style.flexGrow = 1;
            m.parent.Add(deleteBtn);
            fv.Add(textField);
            fv.Add(field);
            fv.style.maxWidth = 265;
            
            RegisterCallback<GeometryChangedEvent>(e =>
            {
                fv.style.width = svParent.resolvedStyle.width;
                deleteBtn.style.left = textField.resolvedStyle.width-10;
            });

            svParent.RegisterCallback<GeometryChangedEvent>(e =>
            {
                fv.style.width = svParent.resolvedStyle.width;
                deleteBtn.style.left = textField.resolvedStyle.width-10;
            });

            field.AddToClassList("dataBlackboard-item");
            field.AddToClassList("dataBlackboard-field");
            field.AddToClassList("dataBlackboard-valuefield");
            fv.AddToClassList("dataBlackboard-item");
            fv.AddToClassList("dataBlackboard-foldout");
            textField.AddToClassList("dataBlackboard-item");
            textField.AddToClassList("dataBlackboard-field");
            textField.AddToClassList("dataBlackboard-textfield");

            Add(fv);
        }
    }
}