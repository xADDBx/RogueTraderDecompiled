using JetBrains.Annotations;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SelectionEntry : IMemoryPackable<SelectionEntry>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SelectionEntryFormatter : MemoryPackFormatter<SelectionEntry>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SelectionEntry value)
		{
			SelectionEntry.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SelectionEntry value)
		{
			SelectionEntry.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly BlueprintSelection Selection;

	[JsonProperty]
	public readonly int PathRank;

	[JsonProperty]
	public readonly BlueprintFeature Feature;

	[MemoryPackConstructor]
	public SelectionEntry([NotNull] BlueprintSelection selection, int pathRank, [NotNull] BlueprintFeature feature)
	{
		Selection = selection;
		PathRank = pathRank;
		Feature = feature;
	}

	static SelectionEntry()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SelectionEntry>())
		{
			MemoryPackFormatterProvider.Register(new SelectionEntryFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SelectionEntry[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SelectionEntry>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SelectionEntry? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteValue(in value.Selection);
		writer.WriteUnmanaged(in value.PathRank);
		writer.WriteValue(in value.Feature);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SelectionEntry? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintSelection value2;
		int value3;
		BlueprintFeature value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadValue<BlueprintSelection>();
				reader.ReadUnmanaged<int>(out value3);
				value4 = reader.ReadValue<BlueprintFeature>();
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.Feature;
				reader.ReadValue(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadValue(ref value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SelectionEntry), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
				value4 = null;
			}
			else
			{
				value2 = value.Selection;
				value3 = value.PathRank;
				value4 = value.Feature;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new SelectionEntry(value2, value3, value4);
	}
}
