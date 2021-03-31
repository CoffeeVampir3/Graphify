using System;
using UnityEngine;

namespace GraphFramework.EditorOnly.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DynamicRange : Attribute
    {
        public readonly int min;
        public readonly int max;

        public DynamicRange(int min = 0, int max = 0)
        {
            if (min < 0 || max < 0 || min > max)
            {
                Debug.LogError("Invalid Dynamic Range Attribute Settings, this attribute will be ignored!");
                this.min = 0;
                this.max = int.MaxValue;
                return;
            }

            this.min = min;
            this.max = max;
        }
    }
}