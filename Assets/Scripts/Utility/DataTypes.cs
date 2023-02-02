using UnityEngine;

namespace Utility
{
    [System.Serializable]
    public struct FloatRange
    {
        public FloatRange(float _min, float _max) { min = _min; max = _max; }

        public float min;
        public float max;

        public float GetRandom()
        {
            return Random.Range(min, max);
        }
    }

    [System.Serializable]
    public struct IntRange
    {
        public IntRange(int _min, int _max) { min = _min; max = _max; }

        public int min;
        public int max;

        public int GetRandom()
        {
            return Random.Range(min, max);
        }
    }
}