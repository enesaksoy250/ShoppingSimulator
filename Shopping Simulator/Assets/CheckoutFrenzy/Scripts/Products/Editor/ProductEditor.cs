﻿using UnityEditor;
using UnityEngine;
using System.IO;

namespace CryingSnow.CheckoutFrenzy
{
    [CustomEditor(typeof(Product))]
    [CanEditMultipleObjects]
    public class ProductEditor : Editor
    {
        SerializedProperty productId;
        SerializedProperty category;

        new SerializedProperty name;
        SerializedProperty icon;
        SerializedProperty priceInCents;
        SerializedProperty orderTime;
        SerializedProperty section;

        SerializedProperty model;
        SerializedProperty box;

        Product[] products;

        void OnEnable()
        {
            products = Resources.LoadAll<Product>("Products");

            productId = serializedObject.FindProperty("productId");
            category = serializedObject.FindProperty("category");

            name = serializedObject.FindProperty("name");
            icon = serializedObject.FindProperty("icon");
            priceInCents = serializedObject.FindProperty("priceInCents");
            orderTime = serializedObject.FindProperty("orderTime");
            section = serializedObject.FindProperty("section");

            model = serializedObject.FindProperty("model");
            box = serializedObject.FindProperty("box");

            if (productId.intValue == 0)
            {
                productId.intValue = GetNextProductId();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Product Details", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(productId, new GUIContent("Product ID"));
            }

            if (HasDuplicateProductId(productId.intValue))
            {
                EditorGUILayout.HelpBox("Duplicate Product ID detected. Click 'Fix' to resolve.", MessageType.Warning);
                if (GUILayout.Button("Fix Duplicate Product ID"))
                {
                    productId.intValue = GetNextProductId();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.PropertyField(name, new GUIContent("Product Name"));

            icon.objectReferenceValue = EditorGUILayout.ObjectField(
                new GUIContent("Product Icon"),
                icon.objectReferenceValue,
                typeof(Sprite),
                false
            );

            if (icon.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Product Icon is missing. Assign an icon or generate one automatically.", MessageType.Warning);
                if (GUILayout.Button("Generate Icon from Model"))
                {
                    GenerateIconFromModel();
                }
            }

            EditorGUILayout.PropertyField(priceInCents, new GUIContent("Price (in cents)"));
            EditorGUILayout.PropertyField(orderTime, new GUIContent("Order Time (seconds)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Category and Section", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(category, new GUIContent("Category"));
            EditorGUILayout.PropertyField(section, new GUIContent("Store Section"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Product Assets", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(model, new GUIContent("3D Model"));
            EditorGUILayout.PropertyField(box, new GUIContent("Product Box"));

            EditorGUILayout.Space();

            if (box.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No Product Box assigned.", MessageType.Warning);
            }
            else if (model.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No 3D Model assigned. Cannot calculate box fitting.", MessageType.Warning);
            }
            else
            {
                Box boxComponent = (Box)box.objectReferenceValue;
                if (boxComponent != null)
                {
                    Product product = (Product)target;
                    int quantity = product.GetBoxQuantity();

                    if (quantity > 0)
                    {
                        EditorGUILayout.HelpBox($"This box can fit {quantity} products.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Product is too large for this box. Reduce the product model size or use a larger box.", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Selected object is not a valid Product Box.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int GetNextProductId()
        {
            int maxId = 0;

            foreach (var product in products)
            {
                maxId = Mathf.Max(maxId, product.ProductID);
            }

            return maxId + 1;
        }

        private bool HasDuplicateProductId(int currentId)
        {
            if (currentId == 0) return false;

            int count = 0;

            foreach (var product in products)
            {
                if (product.ProductID == currentId)
                {
                    count++;
                    if (count > 1)
                        return true;
                }
            }

            return false;
        }

        private void GenerateIconFromModel()
        {
            if (model.objectReferenceValue == null)
            {
                Debug.LogError("Model is missing. Cannot generate an icon.");
                return;
            }

            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model.objectReferenceValue);
            modelInstance.transform.position = new Vector3(0, -1000, 0);
            modelInstance.transform.eulerAngles = new Vector3(0, 150, 0);

            Camera camera = new GameObject("TempCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.nearClipPlane = 0.01f;
            camera.orthographic = true;
            camera.transform.rotation = Quaternion.Euler(15f, 0, 0);

            Bounds bounds = modelInstance.GetComponent<Renderer>().bounds;

            // Transform bounds to camera space
            Vector3[] worldCorners = new Vector3[8];
            GetBoundsCorners(bounds, worldCorners);

            Matrix4x4 worldToCamera = camera.transform.worldToLocalMatrix;
            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            foreach (Vector3 corner in worldCorners)
            {
                Vector3 localCorner = worldToCamera.MultiplyPoint3x4(corner);
                min = Vector3.Min(min, localCorner);
                max = Vector3.Max(max, localCorner);
            }

            // Calculate the extents in camera space
            Vector3 extents = (max - min) * 0.5f;

            // Adjust orthographic size and position
            float verticalSize = extents.y;
            float horizontalSize = extents.x;
            camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
            // Ensures that the model doesn't appear cramped against the edges of the viewport.
            camera.orthographicSize *= 1.05f;

            // Adjust the camera position to center the object
            Vector3 boundsCenter = bounds.center;
            Vector3 offset = camera.transform.rotation * new Vector3(0, 0, -extents.z);
            camera.transform.position = boundsCenter + offset;
            // Ensures the camera is slightly farther from the object, reducing the risk of clipping.
            camera.transform.position -= camera.transform.forward;

            RenderTexture renderTexture = new RenderTexture(256, 256, 16)
            {
                antiAliasing = 8
            };

            camera.targetTexture = renderTexture;

            RenderTexture.active = renderTexture;
            camera.Render();

            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            string directory = "Assets/CheckoutFrenzy/Sprites/Products";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string productName = modelInstance.name;
            string path = Path.Combine(directory, productName + ".png");

            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            icon.objectReferenceValue = sprite;

            DestroyImmediate(modelInstance);
            DestroyImmediate(camera.gameObject);
            renderTexture.Release();

            Debug.Log("Icon generated and assigned successfully: " + path);
        }

        private void GetBoundsCorners(Bounds bounds, Vector3[] corners)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
            corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            corners[3] = center + new Vector3(extents.x, extents.y, -extents.z);
            corners[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
            corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            corners[6] = center + new Vector3(-extents.x, extents.y, extents.z);
            corners[7] = center + new Vector3(extents.x, extents.y, extents.z);
        }
    }
}
