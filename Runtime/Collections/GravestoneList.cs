using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphFramework.Runtime
{
    /// <summary>
    /// A list that is guaranteed to never reorder its elements even on remove.
    /// </summary>
    [Serializable]
    public class GravestoneList<T>
    {
        [SerializeReference]
        private List<T> itemList = new List<T>();
        [SerializeField] 
        private List<int> hangingIndices = new List<int>();
        private int actualCount = 0;
        //The count of actual items in our list.
        public int ActualCount => actualCount;
        //The length of the item list.
        public int ListLength => itemList.Count;

        /// <summary>
        /// Adds item to our list.
        /// </summary>
        /// <returns>Index of the added item.</returns>
        public int Add(T item)
        {
            if (hangingIndices.Any())
            {
                //If there's any hanging indices, they're guaranteed to be within range so just return the
                //most recently added index.
                int index = hangingIndices[hangingIndices.Count-1];
                hangingIndices.RemoveAt(hangingIndices.Count - 1);
                itemList[index] = item;
                actualCount++;
                return index;
            }

            //No indices are hanging add a new item.
            itemList.Add(item);
            actualCount++;
            return itemList.Count - 1;
        }

        /// <summary>
        /// Gravestones an index, frees the pointer (object) memory not the list memory.
        /// </summary>
        public void Remove(int index)
        {
            if (index >= itemList.Count || index == -1 || hangingIndices.Contains(index)) return;
            itemList[index] = default;
            hangingIndices.Add(index);
            actualCount--;
            
            if (actualCount != 0) return;
            //In the case we've removed all of our items we can completely free all the data we've been 
            //hoarding like some sort of data dragon.
            Clear();
        }

        public void Clear()
        {
            itemList.Clear();
            hangingIndices.Clear();
        }

        public T this[int key]
        {
            get => itemList[key];
            set => itemList[key] = value;
        }
    }
}