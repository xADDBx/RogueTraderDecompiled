using Code.Utility.ExtendedModInfo;

namespace Kingmaker.Modding;

public static class OwlcatModificationExtension
{
	public static ExtendedModInfo ToExtendedModInfo(this OwlcatModification mod)
	{
		return new ExtendedModInfo
		{
			Id = mod.UniqueName,
			DisplayName = mod.Manifest.DisplayName,
			Author = mod.Manifest.Author,
			Description = mod.Manifest.Description,
			Version = mod.Manifest.Version,
			IsUmmMod = false,
			UpdateRequired = false,
			Enabled = mod.Enabled,
			HasSettings = (mod.OnDrawGUI != null),
			Path = mod.Path
		};
	}
}
