using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("e8314663525201148a07d32c9b075f3c")]
public class LocalMapMarker : EntityPartComponent<LocalMapMarkerPart, LocalMapMarkerSettings>
{
	public string GetDescription()
	{
		if (Settings.DescriptionUnit != null)
		{
			return Settings.DescriptionUnit.CharacterName;
		}
		if (Settings.Description != null)
		{
			return Settings.Description.String;
		}
		return "<???>";
	}
}
