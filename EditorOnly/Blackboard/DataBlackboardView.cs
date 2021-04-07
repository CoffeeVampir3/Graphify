using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class DataBlackboardView : UnityEditor.Experimental.GraphView.Blackboard
    {
        private readonly DataBlackboard bbData;
        private readonly ScrollView scrollView;
        
        public DataBlackboardView(DataBlackboard blackboard)
        {
            bbData = blackboard;
            
            AddToClassList("customBB");
            var blackboardSubNameLabel = this.Q<Label>("subTitleLabel");
            blackboardSubNameLabel.text = "";
            blackboardSubNameLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            var blackboardNameLabel = this.Q<Label>("titleLabel");
            blackboardNameLabel.text = "Data Blackboard";

            var btn = this.Q<Button>();
            btn.RegisterCallback<ClickEvent>(e =>
            {
                var drawTypes = FieldFactory.GetDrawableTypes();

                int index = UnityEngine.Random.Range(0, drawTypes.Count);
                var randomType = drawTypes.ElementAt(index);

                var thing = Activator.CreateInstance(randomType);
                var randomGuid = Guid.NewGuid().ToString();
                
                Debug.Log("Created: " + thing.GetType() + " with guid: " + randomGuid);

                bbData[randomGuid] = thing;
            });
            
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
            scrollView.Clear();
            foreach (var item in bbData.blackboardDictionary)
            {
                scrollView.Add(BlackboardFieldFactory.Create(
                    item.Key, item.Value.GetType(), item.Value, bbData));
            }
            UnregisterCallback<GeometryChangedEvent>(OnGeoInit);
        }
    }
}