using System;
using Kingmaker.AreaLogic.Cutscenes;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class CutsceneReference : BlueprintReference<Cutscene>, IMemoryPackable<CutsceneReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CutsceneReferenceFormatter : MemoryPackFormatter<CutsceneReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CutsceneReference value)
		{
			CutsceneReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CutsceneReference value)
		{
			CutsceneReference.Deserialize(ref reader, ref value);
		}
	}

	static CutsceneReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CutsceneReference>())
		{
			MemoryPackFormatterProvider.Register(new CutsceneReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CutsceneReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CutsceneReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CutsceneReference? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.guid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CutsceneReference? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string text;
		if (memberCount == 1)
		{
			if (value != null)
			{
				text = value.guid;
				text = reader.ReadString();
				goto IL_0068;
			}
			text = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CutsceneReference), 1, memberCount);
				return;
			}
			text = ((value != null) ? value.guid : null);
			if (memberCount != 0)
			{
				text = reader.ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new CutsceneReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
