namespace Core.Cheats;

public struct KnownObjectsInfo
{
	public string Version { get; set; }

	public CheatMethodInfo[] Methods { get; set; }

	public CheatPropertyInfo[] Variables { get; set; }

	public string[] Externals { get; set; }
}
