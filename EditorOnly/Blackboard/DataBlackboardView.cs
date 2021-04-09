using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class DataBlackboardView : UnityEditor.Experimental.GraphView.Blackboard
    {
        private readonly DataBlackboard bbData;
        private readonly ScrollView scrollView;
        private bool isDrawn = false;

        public DataBlackboardView(DataBlackboard blackboard, GraphifyView view)
        {
            bbData = blackboard;
            
            AddToClassList("customBB");
            var blackboardSubNameLabel = this.Q<Label>("subTitleLabel");
            blackboardSubNameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            var blackboardNameLabel = this.Q<Label>("titleLabel");
            blackboardNameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            var btn = this.Q<Button>();
            btn.Clear();
            btn.text = "<";
            ToolbarMenu tm = new ToolbarMenu {text = "New Property: "};
            btn.parent.Add(tm);
            btn.clicked += view.SwitchBlackboard;
            
            var drawTypes = FieldFactory.GetDrawableTypes();
            foreach (var item in drawTypes)
            {
                if (item == typeof(Enum))
                    continue;
                tm.menu.AppendAction(item.Name, e =>
                {
                    object obj;
                    if (item == typeof(string))
                    {
                        obj = "";
                    }
                    else
                    {
                        obj = Activator.CreateInstance(item);
                    }
                    var randomGuid = Guid.NewGuid().ToString();
                    if (obj == null)
                    {
                        Debug.Log(item.Name);
                        Debug.Log("Null activator.");
                        return;
                    }
                    bbData[randomGuid] = obj;
                    RequestUpdateBlackboard();
                });
            }

            tm.parent.style.minHeight = 22;
            tm.style.minHeight = 17;
            tm.style.minWidth = 17;
            btn.style.minHeight = 17;
            btn.style.minWidth = 17;
            
            scrollView = new ScrollView();
            Add(scrollView);
            RegisterCallback<GeometryChangedEvent>(OnGeoChange);
            RegisterCallback<GeometryChangedEvent>(OnGeoInit);
        }

        private void OnGeoChange(GeometryChangedEvent geo)
        {
            scrollView.style.minHeight = resolvedStyle.height;
        }
        
        private void OnGeoInit(GeometryChangedEvent geo)
        {
            isDrawn = true;
            UpdateBlackboard();
            UnregisterCallback<GeometryChangedEvent>(OnGeoInit);
        }
        
        private void RequestUpdateBlackboard()
        {
            if (!isDrawn)
            {
                schedule.Execute(RequestUpdateBlackboard).StartingIn(100);
            }

            UpdateBlackboard();
        }

        private void UpdateBlackboard()
        {
            scrollView.Clear();
            foreach (var item in bbData.Members)
            {
                scrollView.Add(BlackboardFieldFactory.Create(
                    item.Key, item.Value.GetType(), item.Value, bbData));
            }
        }
    }
}