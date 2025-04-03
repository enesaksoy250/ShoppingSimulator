﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace CryingSnow.CheckoutFrenzy
{
    [CustomEditor(typeof(License))]
    public class LicenseEditor : Editor
    {
        private new SerializedProperty name;

        private SerializedProperty price;
        private SerializedProperty level;
        private SerializedProperty isOwnedByDefault;

        private ReorderableList productList;

        private void OnEnable()
        {
            name = serializedObject.FindProperty("name");
            price = serializedObject.FindProperty("price");
            level = serializedObject.FindProperty("level");
            isOwnedByDefault = serializedObject.FindProperty("isOwnedByDefault");

            SerializedProperty productsProperty = serializedObject.FindProperty("products");
            productList = new ReorderableList(serializedObject, productsProperty, true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Products");
                },

                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = productsProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        element,
                        GUIContent.none
                    );
                },

                elementHeightCallback = (int index) =>
                {
                    return EditorGUIUtility.singleLineHeight + 4;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(price);
            EditorGUILayout.PropertyField(level);
            EditorGUILayout.PropertyField(isOwnedByDefault);

            EditorGUILayout.Space(20);

            productList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
