using MemoryPack;

namespace Kingmaker.Networking.Settings;

public static class BaseSettingNetDataFormatterInitializer
{
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BaseSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new BaseSettingNetDataFormatter());
		}
	}
}
