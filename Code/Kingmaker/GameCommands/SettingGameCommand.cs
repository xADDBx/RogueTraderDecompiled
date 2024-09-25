using System.Collections.Generic;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
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
	private readonly List<BaseSettingNetData> m_Settings;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SettingGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SettingGameCommand(List<BaseSettingNetData> m_settings)
	{
		m_Settings = m_settings;
	}

	protected override void ExecuteInternal()
	{
		SettingsController.Instance.RevertAllTempValues();
		foreach (BaseSettingNetData setting in m_Settings)
		{
			setting.ForceSet();
		}
		Game.Instance.UISettingsManager.OnSettingsApplied();
		EventBus.RaiseEvent(delegate(ISaveSettingsHandler h)
		{
			h.HandleSaveSettings();
		});
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
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
		if (!MemoryPackFormatterProvider.IsRegistered<List<BaseSettingNetData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BaseSettingNetData>());
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
		writer.WriteValue(in value.m_Settings);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SettingGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<BaseSettingNetData> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadValue<List<BaseSettingNetData>>();
			}
			else
			{
				value2 = value.m_Settings;
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
			value2 = ((value != null) ? value.m_Settings : null);
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
