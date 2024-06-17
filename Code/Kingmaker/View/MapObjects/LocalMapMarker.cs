namespace Kingmaker.View.MapObjects;

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
