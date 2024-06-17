namespace Kingmaker.Blueprints.Root;

public static class Root
{
	public static BlueprintRoot Common => BlueprintRoot.Instance;

	public static BlueprintWarhammerRoot WH => BlueprintRoot.Instance.WarhammerRoot;
}
