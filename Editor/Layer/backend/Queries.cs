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
        memberships(input: { first: 100 }) {
          edges {
            node {
              __typename
              id
              workspace {
                __typename
                id
                name
                styles(input:{}) {
                  edges {
                    node {
                      id,
                      name,
                    }
                  }
                }
              }
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
      ... on Inference {
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
      workspaceId: ""_WORKSPACEID"",
      imageId: ""_IMAGEID""
    }) {
      __typename
      ... on RawImage {
        dataUri
      }
      ... on Error {
        type, code, message
      }
    }
  }
  ";
}