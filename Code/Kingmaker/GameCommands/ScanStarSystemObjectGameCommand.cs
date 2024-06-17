using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ScanStarSystemObjectGameCommand : GameCommand, IMemoryPackable<ScanStarSystemObjectGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ScanStarSystemObjectGameCommandFormatter : MemoryPackFormatter<ScanStarSystemObjectGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ScanStarSystemObjectGameCommand value)
		{
			ScanStarSystemObjectGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ScanStarSystemObjectGameCommand value)
		{
			ScanStarSystemObjectGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<StarSystemObjectEntity> m_StarSystemObjectRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private ScanStarSystemObjectGameCommand()
	{
	}

	[JsonConstructor]
	public ScanStarSystemObjectGameCommand(StarSystemObjectEntity starSystemObject)
	{
		m_StarSystemObjectRef = starSystemObject;
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectEntity entity = m_StarSystemObjectRef.Entity;
		if (entity != null)
		{
			EventBus.RaiseEvent(entity, delegate(IScanStarSystemObjectHandler h)
			{
				h.HandleStartScanningStarSystemObject();
			});
		}
	}

	static ScanStarSystemObjectGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ScanStarSystemObjectGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ScanStarSystemObjectGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ScanStarSystemObjectGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ScanStarSystemObjectGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ScanStarSystemObjectGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_StarSystemObjectRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ScanStarSystemObjectGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<StarSystemObjectEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_StarSystemObjectRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ScanStarSystemObjectGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_StarSystemObjectRef : default(EntityRef<StarSystemObjectEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new ScanStarSystemObjectGameCommand
		{
			m_StarSystemObjectRef = value2
		};
		return;
		IL_0070:
		value.m_StarSystemObjectRef = value2;
	}
}
