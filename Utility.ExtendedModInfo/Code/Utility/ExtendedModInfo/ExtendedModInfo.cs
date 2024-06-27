namespace Code.Utility.ExtendedModInfo;

public class ExtendedModInfo
{
	public string Id;

	public string DisplayName;

	public string Author;

	public string Version;

	public string Description;

	public bool IsUmmMod;

	public bool UpdateRequired;

	public bool Enabled;

	public string Path;

	public bool HasSettings;

	public override string ToString()
	{
		return "Mod Info: id " + Id + ", DisplayName " + DisplayName + ", Author " + Author + ", " + $"Version {Version}, Description {Description}, IsUMM {IsUmmMod}, " + $"Enabled {Enabled}, HasSettings {HasSettings}, " + $"Path {Path}, HasSettings {HasSettings}";
	}
}
