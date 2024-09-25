using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPortraitGameCommand : GameCommand, IMemoryPackable<CharGenSetPortraitGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetPortraitGameCommandFormatter : MemoryPackFormatter<CharGenSetPortraitGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintPortraitReference m_Blueprint;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly Guid m_CustomPortraitGuid;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetPortraitGameCommand([NotNull] BlueprintPortraitReference m_blueprint, Guid m_customPortraitGuid)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
		m_CustomPortraitGuid = m_customPortraitGuid;
	}

	public CharGenSetPortraitGameCommand([NotNull] BlueprintPortrait blueprint)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
		m_Blueprint = blueprint.ToReference<BlueprintPortraitReference>();
		PortraitData data = blueprint.Data;
		if (SavePacker.TryGetGuidFromPortrait(data, out m_CustomPortraitGuid))
		{
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Create] CustomPortrait '{data.CustomId}' -> '{m_CustomPortraitGuid}'");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetNewPortraitForSending(m_CustomPortraitGuid);
			}
		}
		else if (PhotonManager.Initialized)
		{
			PhotonManager.Instance.PortraitSyncer.ClearPortraitForSending();
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintPortrait blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetPortraitGameCommand] BlueprintPortrait was not found id=" + m_Blueprint.Guid);
			return;
		}
		if (m_CustomPortraitGuid != Guid.Empty)
		{
			string portraitId;
			bool flag = SavePacker.TryGetPortraitIdFromGuid(m_CustomPortraitGuid, out portraitId);
			PFLog.GameCommands.Log($"[CharGenSetPortraitGameCommand] [Execute] CustomPortrait '{m_CustomPortraitGuid}' -> '{portraitId}' found={flag}");
			if (PhotonManager.Initialized)
			{
				PhotonManager.Instance.PortraitSyncer.SetPortraitForReceiving(m_CustomPortraitGuid, blueprint, flag);
				if (!flag)
				{
					return;
				}
			}
			blueprint.Data = new PortraitData(portraitId);
		}
		EventBus.RaiseEvent(delegate(ICharGenPortraitHandler h)
		{
			h.HandleSetPortrait(blueprint);
		});
	}

	static CharGenSetPortraitGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPortraitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPortraitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortraitGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Blueprint);
		writer.WriteUnmanaged(in value.m_CustomPortraitGuid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPortraitReference value2;
		Guid value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintPortraitReference>();
				reader.ReadUnmanaged<Guid>(out value3);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_CustomPortraitGuid;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<Guid>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPortraitGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(Guid);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_CustomPortraitGuid;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<Guid>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetPortraitGameCommand(value2, value3);
	}
}
