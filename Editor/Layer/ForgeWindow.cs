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
    string selectedStyleName = styles[workspaces[selectedWorkspace].name][selectedStyle].name;
    string selectedWorkspaceName = workspaces[selectedWorkspace].name;
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
            }
            //Refresh the AssetDatabase after saving the image
            AssetDatabase.Refresh();

            //Get relative path to the project
            string path = "Assets" + lastAssetSavedPath.Substring(Application.dataPath.Length);

            //Pick that image in the Project window
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            Selection.activeObject = obj;
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
    dynamic workspaceList = result.data.getMyUser.memberships.list;
    foreach (dynamic item in workspaceList)
    {
      string workspaceName = (string)item.workspace.name;
      workspaces.Add(new Workspace(item.workspace));
      dynamic styles = item.workspace.styles.list;
      List<Style> styleList = new List<Style>();
      foreach (dynamic style in styles)
      {
        styleList.Add(new Style(style));
      }
      this.styles.Add(workspaceName, styleList);
    }

  }

  public void generateAssets()
  {
    string workspaceId = workspaces[selectedWorkspace].id;
    string styleId = styles[workspaces[selectedWorkspace].name][selectedStyle].id;
    Debug.Log(workspaceId);

    GraphQL query = new GraphQL(token);
    dynamic result = query.generateAssets(prompt, workspaceId, styleId);
    dynamic images = result.data.generateImages.files;
    foreach (dynamic image in images)
    {
      Asset asset = new Asset(image, prompt);
      asset.loadImage();
      assets.Insert(0, asset);
    }

    Debug.Log(assets);
  }
}