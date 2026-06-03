namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase;

public readonly struct BlueprintSearchResultItem
{
	public readonly string Guid;

	public readonly string Path;

	public readonly bool IsShadowDeleted;

	public readonly bool ContainsShadowDeletedBlueprints;

	public readonly bool ContainsObsoleteComponents;

	public BlueprintSearchResultItem(string guid, string path, bool isShadowDeleted, bool containsShadowDeletedBlueprints, bool containsObsoleteComponents)
	{
		this = default(BlueprintSearchResultItem);
		Guid = guid;
		Path = path;
		IsShadowDeleted = isShadowDeleted;
		ContainsShadowDeletedBlueprints = containsShadowDeletedBlueprints;
		ContainsObsoleteComponents = containsObsoleteComponents;
	}
}
