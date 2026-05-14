using UnityEngine;

namespace Core.Attributes.Drawer
{
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; private set; }
        public float Max { get; private set; }
    }
}