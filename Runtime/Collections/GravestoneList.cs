using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphFramework.Runtime
{
    [Serializable]
    public class GravestoneList<T>
    {
        [SerializeReference]
        private List<T> itemList = new List<T>();
        [SerializeField] 
        private List<int> hangingIndices = new List<int>();

        /// <summary>
        /// Adds item to our list.
        /// </summary>
        /// <returns>Index of the added item.</returns>
        public int Add(T item)
        {
            while (hangingIndices.Any())
            {
                int index = hangingIndices[hangingIndices.Count-1];
                hangingIndices.RemoveAt(hangingIndices.Count - 1);
                if (index >= itemList.Count) continue;
                itemList[index] = item;
                return index;
            }

            itemList.Add(item);
            return itemList.Count - 1;
        }

        /// <summary>
        /// Gravestones item without displacing other elements.
        /// </summary>
        public void Remove(T item)
        {
            int index = itemList.IndexOf(item);
            if (index == -1) return;
            hangingIndices.Add(index);
            itemList[index] = default;
        } 

        public void Remove(int index)
        {
            if (index >= itemList.Count) return;
            itemList[index] = default;
            hangingIndices.Add(index);
        }
    }
}