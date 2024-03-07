using System.Net;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GraphQL
{
  private string token;

  //Constructor
  public GraphQL(string token)
  {
    this.token = token;
  }

  private static string ENDPOINT = "https://api.app.layer.ai/";

  public object getMySimpleUser() 
  {
    return call(
        "getMyUser",
        new JObject(),
        Queries.getMySimpleUser);
  }

  public object getMyUser()
  {
    return call(
        "getMyUser",
        new JObject(),
        Queries.getMyUser);
  }

  public object generateAssets(string prompt, string workspaceId, string styleId) {
    string query = Queries.generateImages;
    query = query.Replace("_PROMPT", prompt);
    query = query.Replace("_WORKSPACEID", workspaceId);
    query = query.Replace("_STYLEID", styleId);

    return call(
        "generateImages",
        new JObject(),
        query);
  }

  public object removeBackground(string imageId, string workspaceId) {
    string query = Queries.removeBackground;
    query = query.Replace("_IMAGEID", imageId);
    query = query.Replace("_WORKSPACEID", workspaceId);

    return call(
        "removeBackground",
        new JObject(),
        query);
  }


  private object call(string operationName, JObject variables, string query)
  {
    // Make a request to the url endpoint with json string and bearer authozartion
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GraphQL.ENDPOINT);
    request.Method = "POST";
    request.Headers["Authorization"] = "Bearer " + token;
    request.ContentType = "application/json";
    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
    {
      JObject json = new JObject();
      json.Add("operationName", operationName);
      json.Add("variables", variables);
      json.Add("query", query);
      streamWriter.Write(json.ToString());
    }

    var httpResponse = (HttpWebResponse)request.GetResponse();
    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
    {
      var result = streamReader.ReadToEnd();
      return JsonConvert.DeserializeObject(result);
    }
  }
}
