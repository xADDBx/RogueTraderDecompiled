using Kingmaker.EntitySystem.Persistence.Versioning;

public class EmptyUpgrader : JsonUpgraderBase
{
	public override bool WillUpgrade(string jsonName)
	{
		return jsonName == "player";
	}

	public override void Upgrade()
	{
	}
}
