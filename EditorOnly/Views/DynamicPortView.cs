using UnityEngine.UIElements;

namespace GraphFramework.Editor
{
    public class DynamicPortView : VisualElement
    {
        public DynamicPortView(string portName, DynamicPortModel model)
        {
            VisualElement buttonContainer = new VisualElement();
            Label title = new Label{text = portName};
            title.style.alignSelf = new StyleEnum<Align>(Align.Center);
            buttonContainer.style.flexGrow = 1;
            buttonContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            Button plusButton = new Button {text = "+"};
            Button minusButton = new Button {text = "-"};
            plusButton.style.flexGrow = .05f;
            minusButton.style.flexGrow = .05f;
            plusButton.style.alignSelf = new StyleEnum<Align>(Align.Center);
            minusButton.style.alignSelf = new StyleEnum<Align>(Align.FlexEnd);
            buttonContainer.style.alignSelf = new StyleEnum<Align>(Align.Center);

            plusButton.clicked += model.OnAddClicked;
            minusButton.clicked += model.OnRemoveClicked;

            Add(title);
            buttonContainer.Add(plusButton);
            buttonContainer.Add(minusButton);
            
            style.flexGrow = 1;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
            this.Add(buttonContainer);
        }
    }
}