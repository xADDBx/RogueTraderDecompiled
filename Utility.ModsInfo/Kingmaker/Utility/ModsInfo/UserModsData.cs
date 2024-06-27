using System.Collections.Generic;

namespace Kingmaker.Utility.ModsInfo;

public class UserModsData
{
	private static UserModsData s_Instance;

	public List<ModInfo> UsedMods = new List<ModInfo>();

	public bool ExternalUmmUsed;

	public static UserModsData Instance => s_Instance ?? (s_Instance = new UserModsData());

	public bool PlayingWithMods
	{
		get
		{
			if (ExternalUmmUsed)
			{
				return true;
			}
			if (UsedMods != null && UsedMods.Count > 0)
			{
				return true;
			}
			return false;
		}
	}
}
