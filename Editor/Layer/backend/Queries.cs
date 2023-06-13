public class Queries
{

  // getMyUser query
  public static string getMySimpleUser = @"
  query getMyUser {
    getMyUser {
      __typename
      ... on User {
        id
      }
    }
  }
  ";

  // getMyUser query
  public static string getMyUser = @"
  query getMyUser {
    getMyUser {
      __typename
      ... on User {
        name
        email
        memberships(input: { pagination: { limit: 50 } }) {
          list {
            workspace {
              id,
              name,
              styles(input:{}) { list {id, name}}
            }
          }
        }
      }
    }
  }
  ";

  public static string generateImages = @"
  mutation generateImages {
    generateImages (input:{
      prompt: ""_PROMPT"",
      workspaceId: ""_WORKSPACEID"",
      styleId: ""_STYLEID""
    }) {
      
      __typename
      ... on StyleInference {
        id,
        files {id, url}
      }
      ... on Error {
        type, code, message
      }
    }
  }
  ";

  public static string removeBackground = @"
  mutation removeBackground {
    removeBackground (input:{
      imageId:""_IMAGEID"",
    }) {
      
      __typename
      ... on RawImage {
        base64
      }
      ... on Error {
        type, code, message
      }
    }
  }
  ";
}