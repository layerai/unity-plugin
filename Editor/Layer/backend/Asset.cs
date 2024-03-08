using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Networking;



public class Asset
{
  public string id;
  public string url;
  public string workspaceId;

  public string prompt;
  public Texture2D texture;

  public Asset(dynamic asset, string prompt, string workspaceId)
  {
    this.id = asset.id;
    this.url = asset.url;
    this.workspaceId = workspaceId;
    this.prompt = prompt;
  }

  public void removeBackground()
  {
    string token = EditorPrefs.GetString(Constants.AccessTokenKey, "");
    GraphQL query = new GraphQL(token);
    dynamic result = query.removeBackground(this.id, this.workspaceId);
    string dataUri = result.data.removeBackground.dataUri;
    string base64Image = dataUri.Replace("data:image/png;base64,", "");
    byte[] bytes = System.Convert.FromBase64String(base64Image);
    this.texture = new Texture2D(1, 1);
    this.texture.LoadImage(bytes);
  }

  public void loadImage()
  {
    // Load image from url as texture
    UnityWebRequest www = UnityWebRequestTexture.GetTexture(this.url);
    UnityWebRequestAsyncOperation op = www.SendWebRequest();

    op.completed += (aop) =>
    {
      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.Log(www.result);
      }
      else
      {
        this.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
      }
    };
  }
}