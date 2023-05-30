
public class Workspace
{
  public string id;
  public string name;

  public Workspace(dynamic workspace)
  {
    this.id = workspace.id;
    this.name = workspace.name;
  }
}
