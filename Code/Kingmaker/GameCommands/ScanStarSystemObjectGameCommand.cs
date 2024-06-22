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

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_FinishScan;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private ScanStarSystemObjectGameCommand()
	{
	}

	[JsonConstructor]
	public ScanStarSystemObjectGameCommand(StarSystemObjectEntity starSystemObject, bool finishScan)
	{
		m_StarSystemObjectRef = starSystemObject;
		m_FinishScan = finishScan;
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectEntity entity = m_StarSystemObjectRef.Entity;
		if (entity == null)
		{
			return;
		}
		if (m_FinishScan)
		{
			FinishScan(entity);
			return;
		}
		EventBus.RaiseEvent(entity, delegate(IScanStarSystemObjectHandler h)
		{
			h.HandleStartScanningStarSystemObject();
		});
	}

	private static void FinishScan(StarSystemObjectEntity entity)
	{
		if (!entity.IsScanned)
		{
			entity.Scan();
			entity.PlayBarkBanter();
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
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_StarSystemObjectRef);
		writer.WriteUnmanaged(in value.m_FinishScan);
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
		bool value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_StarSystemObjectRef;
				value3 = value.m_FinishScan;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
				goto IL_00a1;
			}
			value2 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
			reader.ReadUnmanaged<bool>(out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ScanStarSystemObjectGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<StarSystemObjectEntity>);
				value3 = false;
			}
			else
			{
				value2 = value.m_StarSystemObjectRef;
				value3 = value.m_FinishScan;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a1;
			}
		}
		value = new ScanStarSystemObjectGameCommand
		{
			m_StarSystemObjectRef = value2,
			m_FinishScan = value3
		};
		return;
		IL_00a1:
		value.m_StarSystemObjectRef = value2;
		value.m_FinishScan = value3;
	}
}
