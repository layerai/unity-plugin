using UnityEngine;
using UnityEditor;
using UnityEditor.VSAttribution.Layer;

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
            Application.OpenURL(Constants.CreateAccessTokenUrl);
        }

        // Show a button to save the access token
        if (GUILayout.Button("Save"))
        {
            EditorPrefs.SetString(Constants.AccessTokenKey, accessToken);
            
            // Get my simple user id
            GraphQL graphQL = new GraphQL(accessToken);
            dynamic user = graphQL.getMySimpleUser();

            // Check for error
            if (user.data.getMyUser.__typename == "Error")
            {
                ShowNotification(new GUIContent("Invalid access token"));
                GUIUtility.ExitGUI();
                return;
            }
            string uid = (string) user.data.getMyUser.id;
            VSAttribution.SendAttributionEvent("login", "layer", uid);

            // Show notification that the access token was saved
            ShowNotification(new GUIContent("Access token saved"));
        }

        // Show help text and a button goes to the documentation url
        GUILayout.Label("Need help?", EditorStyles.boldLabel);
        GUILayout.Label("Check out the documentation for more information on how to use the Layer");
        if (GUILayout.Button("Documentation"))
        {
            Application.OpenURL(Constants.DocumentationUrl);
        }
    }
}