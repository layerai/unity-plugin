using UnityEngine;
using UnityEditor;

public class Settings : EditorWindow
{
    string accessToken = "Paste your access token here";
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Layer AI/Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
        window.Show();
        
    }

    // Fetch access token before showing dialog
    void OnEnable()
    {
        accessToken = EditorPrefs.GetString(Constants.AccessTokenKey, accessToken);
    }
    

    void OnGUI()
    {
        // Show the access token field
        GUILayout.Label("Access Token", EditorStyles.boldLabel);
        accessToken = EditorGUILayout.TextField("Access Token", accessToken);
        
        // Show how to create access token in the UI
        if (GUILayout.Button("Create your access token"))
        {
            Application.OpenURL("https://app.layer.ai/settings/tokens");
        }

        // Show a button to save the access token
        if (GUILayout.Button("Save"))
        {
            EditorPrefs.SetString(Constants.AccessTokenKey, accessToken);
            // Show notification that the access token was saved
            ShowNotification(new GUIContent("Access token saved"));
        }
    }
}