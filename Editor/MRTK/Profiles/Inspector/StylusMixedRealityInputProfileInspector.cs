using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoloLight.STK.MRTK
{
    [CustomEditor(typeof(StylusMixedRealityInputProfile))]
    public class StylusMixedRealityInputProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private const string ProfileTitle = "Stylus Input Settings";
        private const string ProfileDescription = "Settings used to configure the behavior of stylus controllers in Unity Editor.";

        private SerializedProperty _startingDistance;
        private SerializedProperty _stylusForwardKey;
        private SerializedProperty _stylusBackwardKey;
        private SerializedProperty _depthSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();

            _startingDistance = serializedObject.FindProperty("_startingDistance");
            _stylusForwardKey = serializedObject.FindProperty("_stylusForwardKey");
            _stylusBackwardKey = serializedObject.FindProperty("_stylusBackwardKey");
            _depthSpeed = serializedObject.FindProperty("_depthSpeed");
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;
            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile != null &&
                   profile == MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.ControllerMappingProfile;
        }


        public override void OnInspectorGUI()
        {
            if (!RenderProfileHeader(ProfileTitle, ProfileDescription, target, true, BackProfileType.Input))
            {
                return;
            }


            using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
            {

                serializedObject.Update();
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Label");
                EditorGUILayout.PropertyField(_startingDistance);
                EditorGUILayout.PropertyField(_stylusForwardKey);
                EditorGUILayout.PropertyField(_stylusBackwardKey);
                EditorGUILayout.PropertyField(_depthSpeed);
                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}