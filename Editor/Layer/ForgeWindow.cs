using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ForgeWindow : EditorWindow
{
  string prompt = "";

  string token = "";

  string lastAssetSavedPath = "";

  int selectedWorkspace = 0;
  int selectedStyle = 0;
  

  List<Asset> assets = new List<Asset>();

  public Vector2 scrollPosition = Vector2.zero;

  bool showPosition = true;
  List<Workspace> workspaces = new List<Workspace>();
  Dictionary<string, List<Style>> styles = new Dictionary<string, List<Style>>();

  // Add menu named "My Window" to the Window menu
  [MenuItem("Window/Layer AI/Generate Assets")]
  static void Init()
  {
    // Get existing open window or if none, make a new one:
    ForgeWindow window = (ForgeWindow)EditorWindow.GetWindow(typeof(ForgeWindow));
    window.Show();
  }

  void OnGUI()
  {
    GUILayout.BeginVertical();
    if (styles == null || styles.Count == 0)
    {
      GUILayout.Label("No styles found. Please check your personal access token!");
      GUILayout.EndVertical();
      return;
    }
    
    // Show a small question icon button that links to the documentation on the right
    GUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    // Settings button
    if (GUILayout.Button("Settings", GUILayout.Width(70)))
    {
      Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
      window.Show();
    }

    if (GUILayout.Button("Help", GUILayout.Width(50)))
    {
      Application.OpenURL(Constants.DocumentationUrl);
    }


    GUILayout.EndHorizontal();

    string selectedWorkspaceName = workspaces[selectedWorkspace].name;
    string selectedStyleName = styles[selectedWorkspaceName][selectedStyle].name;

    showPosition = EditorGUILayout.Foldout(showPosition, showPosition ? "Workspace Settings" : selectedWorkspaceName + " - "+ selectedStyleName );
    if (showPosition)
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Workspace", EditorStyles.boldLabel, GUILayout.Width(150));
      string[] workspaces = this.workspaces.ConvertAll<string>(workspace => workspace.name).ToArray();
      selectedWorkspace = EditorGUILayout.Popup(selectedWorkspace, workspaces);
      GUILayout.EndHorizontal();


      GUILayout.BeginHorizontal();
      GUILayout.Label("Style", EditorStyles.boldLabel, GUILayout.Width(150));
      List<Style> styleList = styles[workspaces[selectedWorkspace]];
      string[] styleNames = styleList.ConvertAll<string>(style => style.name).ToArray();
      selectedStyle = EditorGUILayout.Popup(selectedStyle, styleNames);
      GUILayout.EndHorizontal();
    }

    GUILayout.BeginHorizontal();
    GUILayout.Label("Prompt", EditorStyles.boldLabel, GUILayout.Width(150));
    prompt = EditorGUILayout.TextArea(prompt, GUILayout.Height(50));
    GUILayout.EndHorizontal();

    if (GUILayout.Button("Forge", GUILayout.Height(40)))
    {
      generateAssets();
    }

    //Show scroll view, always show vertical
    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

    // Display assets as images in the UI in a 4x4 grid
    int columns = 4;
    int rows = Mathf.CeilToInt(assets.Count / (float)columns);
    for (int i = 0; i < rows; i++)
    {
      GUILayout.BeginHorizontal();
      for (int j = 0; j < columns; j++)
      {
        int index = i * columns + j;
        if (index < assets.Count)
        {
          Asset asset = assets[index];
          // Show image as texture and a button below the image to save the image
          GUILayout.BeginVertical();
          // Get existing window width
          float windowWidth = EditorGUIUtility.currentViewWidth-40;
          // Calculate thumbnail width: window width divided by number of columns minus space for spacing between the images
          float thumbnailWidth = (windowWidth / columns);
          // Draw image, when clicked, show preview
          if (GUILayout.Button(asset.texture, GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailWidth)))
          {
            // Show preview
            PreviewWindow window = (PreviewWindow)EditorWindow.GetWindow(typeof(PreviewWindow));
            window.asset = asset;
            window.Show();
          }


          GUILayout.BeginHorizontal();
          if (GUILayout.Button("Remove BG"))
          {
            asset.removeBackground();
          }
          if (GUILayout.Button("Save"))
          {

            lastAssetSavedPath = EditorUtility.SaveFilePanel("Save Image", "", asset.prompt, "png");

            if (lastAssetSavedPath.Length != 0)
            {
              System.IO.File.WriteAllBytes(lastAssetSavedPath, asset.texture.EncodeToPNG());
              //Refresh the AssetDatabase after saving the image
              AssetDatabase.Refresh();

              //Get relative path to the project
              string path = "Assets" + lastAssetSavedPath.Substring(Application.dataPath.Length);

              //Pick that image in the Project window
              Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
              Selection.activeObject = obj;
              GUIUtility.ExitGUI();
            }
          }
          GUILayout.EndHorizontal();
          GUILayout.EndVertical();
        }
      }
      GUILayout.EndHorizontal();
    }
    GUILayout.EndScrollView();


    GUILayout.EndVertical();

  }

  void OnEnable()
  {
    token = EditorPrefs.GetString(Constants.AccessTokenKey, "");

    GraphQL query = new GraphQL(token);
    dynamic result = query.getMyUser();
    if (result.data.getMyUser.__typename != "User")
    {
      EditorPrefs.SetString(Constants.AccessTokenKey, "");
      EditorUtility.DisplayDialog("Error", "Invalid token. Check your token at Window > Layer AI > Settings", "Ok");
      return;
    }
    dynamic workspaceList = result.data.getMyUser.memberships.edges;
    foreach (dynamic workspaceItem in workspaceList)
    {
      dynamic workspace = workspaceItem.node.workspace;
      dynamic workspaceStyles = workspaceItem.node.workspace.styles.edges;

      string workspaceName = (string)workspace.name;

      if (workspaceStyles.Count > 0) {
        workspaces.Add(new Workspace(workspace));
      }

      List<Style> styleList = new List<Style>();
      foreach (dynamic styleItem in workspaceStyles)
      {
        dynamic style = styleItem.node;
        styleList.Add(new Style(style));
      }

      if (styleList.Count > 0) {
        this.styles.Add(workspaceName, styleList);
      }
    }
  }

  public void generateAssets()
  {
    string workspaceId = workspaces[selectedWorkspace].id;
    string styleId = styles[workspaces[selectedWorkspace].name][selectedStyle].id;

    GraphQL query = new GraphQL(token);
    dynamic result = query.generateAssets(prompt, workspaceId, styleId);
    dynamic images = result.data.generateImages.files;
    foreach (dynamic image in images)
    {
      Asset asset = new Asset(image, prompt, workspaceId);
      asset.loadImage();
      assets.Insert(0, asset);
    }
  }
}