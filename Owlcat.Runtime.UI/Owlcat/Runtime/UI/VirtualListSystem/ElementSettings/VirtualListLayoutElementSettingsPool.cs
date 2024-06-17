using System.Collections.Generic;

namespace Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;

public static class VirtualListLayoutElementSettingsPool
{
	private static Stack<VirtualListLayoutElementSettings> s_Pool = new Stack<VirtualListLayoutElementSettings>();

	public static VirtualListLayoutElementSettings GetCopy(VirtualListLayoutElementSettings settings)
	{
		if (settings == VirtualListLayoutElementSettings.None)
		{
			return VirtualListLayoutElementSettings.None;
		}
		VirtualListLayoutElementSettings virtualListLayoutElementSettings = ((s_Pool.Count != 0) ? s_Pool.Pop() : new VirtualListLayoutElementSettings());
		virtualListLayoutElementSettings.CopyFrom(settings);
		return virtualListLayoutElementSettings;
	}

	public static void ReturnSettings(VirtualListLayoutElementSettings settings)
	{
		if (settings != VirtualListLayoutElementSettings.None)
		{
			s_Pool.Push(settings);
		}
	}
}
