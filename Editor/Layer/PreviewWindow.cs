// A preview window shows an asset in full width/height
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PreviewWindow: EditorWindow {

  public Asset asset;

  void OnGUI()
  {
    if (asset != null)
    {
      // Get existing window width
      float windowWidth = EditorGUIUtility.currentViewWidth;
      // Draw image
      GUILayout.BeginHorizontal();
      GUILayout.Label(asset.texture, GUILayout.Width(windowWidth), GUILayout.Height(windowWidth));
      GUILayout.EndHorizontal();
    }
  }

}  