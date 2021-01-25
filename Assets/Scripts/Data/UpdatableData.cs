using System;
using UnityEditor;
using UnityEngine;

namespace Data
{
    public class UpdatableData : ScriptableObject
    {
        public bool autoUpdate;

        private void OnValidate()
        {
            if (autoUpdate)
            {
                EditorApplication.update += NotifyOfUpdatedValues;
            }
        }

        public event Action OnValuesUpdated;

        public void NotifyOfUpdatedValues()
        {
            EditorApplication.update -= NotifyOfUpdatedValues;
            if (OnValuesUpdated != null) OnValuesUpdated();
        }
    }
}
