using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DefaultNamespace.Scenes
{
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text dialogueText;
        [SerializeField] 
        private Choicer dialogueChoicer;
        public bool awaitingInput = false;
        private static Dialogue instance = null;
        private int clickedIndex = -1;

        private void Awake()
        {
            instance = this;
        }

        public static bool AwaitingInput => instance.awaitingInput;

        public static void SendText(string text)
        {
            instance.dialogueText.text = text;
        }

        public static void SendChoice(string text, int index, Action onChosen)
        {
            instance.dialogueChoicer.CreateChoice(text, index, onChosen);
        }

        public static void ClearChoices()
        {
            instance.dialogueChoicer.Hide();
            instance.dialogueChoicer.ClearChoices();
        }

        public static void StartChoices()
        {
            instance.dialogueChoicer.Show();
        }

        public static void OnChoiceClicked()
        {
            ClearChoices();
            instance.clickedIndex = Convert.ToInt32(EventSystem.current.currentSelectedGameObject.name);
        }

        public static int ConsumeClickedIndex()
        {
            int val = instance.clickedIndex;
            instance.clickedIndex = -1;
            return val;
        }
    }
}