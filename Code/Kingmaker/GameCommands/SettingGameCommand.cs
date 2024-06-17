using Kingmaker.Networking.Settings;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SettingGameCommand : GameCommand, IMemoryPackable<SettingGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SettingGameCommandFormatter : MemoryPackFormatter<SettingGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SettingGameCommand value)
		{
			SettingGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SettingGameCommand value)
		{
			SettingGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BaseSettingNetData m_Setting;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SettingGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SettingGameCommand(BaseSettingNetData m_setting)
	{
		m_Setting = m_setting;
	}

	protected override void ExecuteInternal()
	{
		m_Setting.ForceSet();
		Game.Instance.GameCommandQueue.SaveSettings();
	}

	static SettingGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SettingGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SettingGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SettingGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SettingGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SettingGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteValue(in value.m_Setting);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SettingGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BaseSettingNetData value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadValue<BaseSettingNetData>();
			}
			else
			{
				value2 = value.m_Setting;
				reader.ReadValue(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SettingGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Setting : null);
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SettingGameCommand(value2);
	}
}
