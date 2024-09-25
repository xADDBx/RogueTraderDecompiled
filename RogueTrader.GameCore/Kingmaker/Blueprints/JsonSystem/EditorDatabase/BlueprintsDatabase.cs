namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase;

public class BlueprintsDatabase
{
	public static T LoadById<T>(string id) where T : SimpleBlueprint
	{
		return ResourcesLibrary.TryGetBlueprint(id) as T;
	}
}
