using Kingmaker.Settings.Entities;
using MemoryPack;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

public abstract class TypedBaseSettingNetData<T> : BaseSettingNetData
{
	[JsonProperty]
	[MemoryPackInclude]
	protected readonly byte Index;

	[JsonProperty]
	[MemoryPackInclude]
	protected readonly T Value;

	[JsonConstructor]
	protected TypedBaseSettingNetData()
	{
	}

	[MemoryPackConstructor]
	protected TypedBaseSettingNetData(byte index, T value)
	{
		Index = index;
		Value = value;
	}

	public override void ForceSet()
	{
		((SettingsEntity<T>)PhotonManager.Settings.SettingsForSync[Index]).SetValueAndConfirm(Value);
	}
}
