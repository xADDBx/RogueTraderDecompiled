using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[MemoryPackable(GenerateType.Object)]
public class GameElementData : IMemoryPackable<GameElementData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameElementDataFormatter : MemoryPackFormatter<GameElementData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameElementData value)
		{
			GameElementData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameElementData value)
		{
			GameElementData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public GameElementType Type { get; set; }

	[JsonProperty]
	public string BlueprintGuid { get; set; }

	[JsonProperty]
	public string BlueprintName { get; set; }

	[JsonProperty]
	public string Value { get; set; }

	[MemoryPackConstructor]
	public GameElementData()
	{
	}

	public GameElementData(GameElementType type, BlueprintScriptableObject blueprint, string value)
	{
		Type = type;
		Value = value;
		if (blueprint != null)
		{
			BlueprintName = blueprint.name;
			BlueprintGuid = blueprint.AssetGuid;
		}
	}

	static GameElementData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameElementData>())
		{
			MemoryPackFormatterProvider.Register(new GameElementDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameElementData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GameElementData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameElementType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<GameElementType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameElementData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		GameElementType value2 = value.Type;
		writer.WriteUnmanagedWithObjectHeader(4, in value2);
		writer.WriteString(value.BlueprintGuid);
		writer.WriteString(value.BlueprintName);
		writer.WriteString(value.Value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameElementData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		GameElementType value2;
		string blueprintGuid;
		string blueprintName;
		string value3;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.Type;
				blueprintGuid = value.BlueprintGuid;
				blueprintName = value.BlueprintName;
				value3 = value.Value;
				reader.ReadUnmanaged<GameElementType>(out value2);
				blueprintGuid = reader.ReadString();
				blueprintName = reader.ReadString();
				value3 = reader.ReadString();
				goto IL_00f5;
			}
			reader.ReadUnmanaged<GameElementType>(out value2);
			blueprintGuid = reader.ReadString();
			blueprintName = reader.ReadString();
			value3 = reader.ReadString();
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameElementData), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = GameElementType.Quest;
				blueprintGuid = null;
				blueprintName = null;
				value3 = null;
			}
			else
			{
				value2 = value.Type;
				blueprintGuid = value.BlueprintGuid;
				blueprintName = value.BlueprintName;
				value3 = value.Value;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<GameElementType>(out value2);
				if (memberCount != 1)
				{
					blueprintGuid = reader.ReadString();
					if (memberCount != 2)
					{
						blueprintName = reader.ReadString();
						if (memberCount != 3)
						{
							value3 = reader.ReadString();
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f5;
			}
		}
		value = new GameElementData
		{
			Type = value2,
			BlueprintGuid = blueprintGuid,
			BlueprintName = blueprintName,
			Value = value3
		};
		return;
		IL_00f5:
		value.Type = value2;
		value.BlueprintGuid = blueprintGuid;
		value.BlueprintName = blueprintName;
		value.Value = value3;
	}
}
