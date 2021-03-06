using Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(UpdatableData), true)]
    public class UpdatableDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var data = (UpdatableData) target;

            if (!GUILayout.Button("Update")) return;
            data.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
