using MemoryPack;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.NoGenerate)]
public abstract class BaseSettingNetData
{
	public abstract void ForceSet();
}
