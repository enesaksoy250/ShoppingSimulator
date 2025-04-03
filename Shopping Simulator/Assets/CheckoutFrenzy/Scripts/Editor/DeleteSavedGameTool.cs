using UnityEngine;
using UnityEditor;
using System.IO;

namespace CryingSnow.CheckoutFrenzy
{
    public class DeleteSavedGameTool
    {
        [MenuItem("Tools/Checkout Frenzy/Delete Saved Game")]
        public static void DeleteSavedGame()
        {
            // Get the persistent data path
            string persistentDataPath = Application.persistentDataPath;

            // Confirm action with the user
            if (EditorUtility.DisplayDialog(
                "Delete Saved Game",
                "Are you sure you want to delete the saved game file?",
                "Yes, Delete",
                "Cancel"))
            {
                try
                {
                    // Check if the directory exists
                    if (Directory.Exists(persistentDataPath))
                    {
                        // Delete all files in the directory
                        foreach (var file in Directory.GetFiles(persistentDataPath))
                        {
                            File.Delete(file);
                        }

                        // Provide feedback to the user
                        EditorUtility.DisplayDialog("Success", "Saved game file has been deleted.", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "No saved game file found to delete.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error deleting saved game file: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", "An error occurred while deleting the file. Check the Console for details.", "OK");
                }
            }
        }
    }
}
