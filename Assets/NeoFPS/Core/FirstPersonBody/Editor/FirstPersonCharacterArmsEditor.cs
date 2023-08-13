#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoFPS;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace NeoFPSEditor
{
    [CustomEditor(typeof(FirstPersonCharacterArms))]
    public class FirstPersonCharacterArmsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var prop = serializedObject.FindProperty("m_ArmsRootTransform");
            EditorGUILayout.PropertyField(prop);

            if (prop.objectReferenceValue != null)
                EditorGUILayout.HelpBox("The transform above will be repositioned and rotated each frame to match the weapon view model. Use this when you have a dual-rig weapon setup where the arm and weapon animations are synced.\n\nIf you only want to align the hands and fingers then set the above property to \"None\".", MessageType.Info, true);


            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif