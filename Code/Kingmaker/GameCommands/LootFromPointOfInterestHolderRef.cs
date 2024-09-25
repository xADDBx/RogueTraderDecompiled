using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class LootFromPointOfInterestHolderRef : IMemoryPackable<LootFromPointOfInterestHolderRef>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LootFromPointOfInterestHolderRefFormatter : MemoryPackFormatter<LootFromPointOfInterestHolderRef>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LootFromPointOfInterestHolderRef value)
		{
			LootFromPointOfInterestHolderRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LootFromPointOfInterestHolderRef value)
		{
			LootFromPointOfInterestHolderRef.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintPointOfInterestReference m_PoiRef;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<StarSystemObjectEntity> m_OwnerRef;

	[MemoryPackIgnore]
	public LootFromPointOfInterestHolder LootHolder
	{
		get
		{
			if (m_OwnerRef.Entity == null)
			{
				return null;
			}
			BlueprintPointOfInterest poi = m_PoiRef?.Get();
			if (poi != null)
			{
				return m_OwnerRef.Entity.LootHolder.FirstOrDefault((LootFromPointOfInterestHolder x) => x?.Point?.Blueprint == poi);
			}
			return null;
		}
	}

	[MemoryPackConstructor]
	private LootFromPointOfInterestHolderRef()
	{
	}

	[JsonConstructor]
	public LootFromPointOfInterestHolderRef(EntityRef<StarSystemObjectEntity> entityRef, BlueprintPointOfInterestReference poiRef)
	{
		m_OwnerRef = entityRef;
		m_PoiRef = poiRef;
	}

	public LootFromPointOfInterestHolderRef(EntityRef<StarSystemObjectEntity> entityRef, LootFromPointOfInterestHolder lootHolder)
	{
		m_OwnerRef = entityRef;
		m_PoiRef = lootHolder.Point?.Blueprint?.ToReference<BlueprintPointOfInterestReference>();
	}

	public bool Equals(LootFromPointOfInterestHolderRef other)
	{
		if (other != null && m_OwnerRef.Equals(other.m_OwnerRef))
		{
			return m_PoiRef.Equals(other.m_PoiRef);
		}
		return false;
	}

	static LootFromPointOfInterestHolderRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LootFromPointOfInterestHolderRef>())
		{
			MemoryPackFormatterProvider.Register(new LootFromPointOfInterestHolderRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LootFromPointOfInterestHolderRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LootFromPointOfInterestHolderRef>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LootFromPointOfInterestHolderRef? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_PoiRef);
		writer.WritePackable(in value.m_OwnerRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LootFromPointOfInterestHolderRef? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPointOfInterestReference value2;
		EntityRef<StarSystemObjectEntity> value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_PoiRef;
				value3 = value.m_OwnerRef;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_00a0;
			}
			value2 = reader.ReadPackable<BlueprintPointOfInterestReference>();
			value3 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LootFromPointOfInterestHolderRef), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(EntityRef<StarSystemObjectEntity>);
			}
			else
			{
				value2 = value.m_PoiRef;
				value3 = value.m_OwnerRef;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a0;
			}
		}
		value = new LootFromPointOfInterestHolderRef
		{
			m_PoiRef = value2,
			m_OwnerRef = value3
		};
		return;
		IL_00a0:
		value.m_PoiRef = value2;
		value.m_OwnerRef = value3;
	}
}
