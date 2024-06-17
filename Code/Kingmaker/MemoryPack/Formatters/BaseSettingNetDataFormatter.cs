using Kingmaker.Networking.Settings;
using MemoryPack;

namespace Kingmaker.MemoryPack.Formatters;

[MemoryPackUnionFormatter(typeof(BaseSettingNetData))]
[MemoryPackUnion(0, typeof(BoolSettingNetData))]
[MemoryPackUnion(1, typeof(IntSettingNetData))]
[MemoryPackUnion(2, typeof(FloatSettingNetData))]
[MemoryPackUnion(3, typeof(EnumSettingNetData))]
public sealed class BaseSettingNetDataFormatter
{
}
