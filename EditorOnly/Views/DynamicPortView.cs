using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class DynamicPortView : VisualElement
    {
        private DynamicPortModel model;
        
        public DynamicPortView(string portName, DynamicPortModel model, Direction dir)
        {
            this.model = model;
            
            VisualElement buttonContainer = new VisualElement();
            Label title = new Label{text = portName};
            buttonContainer.style.alignContent = new StyleEnum<Align>(Align.Stretch);
            Button plusButton = new Button {text = "+"};
            Button minusButton = new Button {text = "-"};
            plusButton.style.fontSize = 12f;
            minusButton.style.fontSize = 15f;
            plusButton.style.width = 18f;
            plusButton.style.height = 18f;
            minusButton.style.width = 18f;
            minusButton.style.height = 18f;
            plusButton.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            minusButton.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            plusButton.clicked += model.OnAddClicked;
            minusButton.clicked += model.OnRemoveClicked;
            buttonContainer.Add(title);

            switch (dir)
            {
                case Direction.Input:
                    buttonContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.RowReverse);
                    buttonContainer.Add(minusButton);
                    buttonContainer.Add(plusButton);
                    style.justifyContent = new StyleEnum<Justify>(Justify.FlexStart);
                    style.alignItems = new StyleEnum<Align>(Align.FlexStart);
                    break;
                case Direction.Output:
                    buttonContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
                    buttonContainer.Add(plusButton);
                    buttonContainer.Add(minusButton);
                    style.justifyContent = new StyleEnum<Justify>(Justify.FlexEnd);
                    style.alignItems = new StyleEnum<Align>(Align.FlexEnd);
                    break;
            }
            
            style.flexGrow = 1;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            Add(buttonContainer);
            
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        public void OnDetachFromPanel(DetachFromPanelEvent panelEvent)
        {
            model.DeleteAllLinks();
        }
    }
}