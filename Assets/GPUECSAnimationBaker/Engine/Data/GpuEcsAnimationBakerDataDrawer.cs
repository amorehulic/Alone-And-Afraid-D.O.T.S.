#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GpuEcsAnimationBaker.Engine.Data
{
    [CustomPropertyDrawer(typeof(GpuEcsAnimationBakerData))]
    public class GpuEcsAnimationBakerDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty animationsProperty = property.FindPropertyRelative("animations");
            SerializedProperty generateAnimationIdsEnumProperty = property.FindPropertyRelative("generateAnimationIdsEnum");
            SerializedProperty animationIdsEnumNameProperty = property.FindPropertyRelative("animationIdsEnumName");
            SerializedProperty boneUsageProperty = property.FindPropertyRelative("boneUsage");
            SerializedProperty transformUsageFlagsParentProperty = property.FindPropertyRelative("transformUsageFlagsParent");
            SerializedProperty transformUsageFlagsChildrenProperty = property.FindPropertyRelative("transformUsageFlagsChildren");

            EditorGUILayout.PropertyField(animationsProperty);
            EditorGUILayout.PropertyField(generateAnimationIdsEnumProperty);
            if(generateAnimationIdsEnumProperty.boolValue)
                EditorGUILayout.PropertyField(animationIdsEnumNameProperty);
            EditorGUILayout.PropertyField(boneUsageProperty);
            EditorGUILayout.PropertyField(transformUsageFlagsParentProperty);
            EditorGUILayout.PropertyField(transformUsageFlagsChildrenProperty);
        }
    }
}
#endif