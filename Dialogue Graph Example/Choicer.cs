using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DefaultNamespace.Scenes
{
    public class Choicer : MonoBehaviour
    {
        public Button choiceButton;
        public Transform layoutParent;
        public CanvasGroup CanvasGroup;
        private readonly List<Button> choiceButtons = new List<Button>();

        public void ClearChoices()
        {
            for (int i = choiceButtons.Count-1; i >= 0; i--)
            {
                Destroy(choiceButtons[i].gameObject);
            }
            choiceButtons.Clear();
        }

        public void Show()
        {
            CanvasGroup.alpha = 1;
        }
        
        public void Hide()
        {
            CanvasGroup.alpha = 0;
        }
        
        public void CreateChoice(string choiceText, int choiceIndex, Action onChooose)
        {
            var item = Instantiate(choiceButton, layoutParent);
            var text = item.GetComponentInChildren<TMP_Text>();
            text.text = choiceText;
            choiceButtons.Add(item);
            item.name = choiceIndex.ToString();
            item.onClick.AddListener(new UnityAction(onChooose));
            item.gameObject.SetActive(true);
        }
    }
}