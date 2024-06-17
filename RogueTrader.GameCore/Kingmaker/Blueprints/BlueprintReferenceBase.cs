using System;
using Kingmaker.Blueprints.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintReferenceBase : IEquatable<BlueprintReferenceBase>, IReferenceBase, IMemoryPackable<BlueprintReferenceBase>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintReferenceBaseFormatter : MemoryPackFormatter<BlueprintReferenceBase>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintReferenceBase value)
		{
			BlueprintReferenceBase.Deserialize(ref reader, ref value);
		}
	}

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	[MemoryPackInclude]
	protected string guid;

	private BlueprintScriptableObject Cached { get; set; }

	[MemoryPackIgnore]
	[JsonIgnore]
	public string Guid => guid;

	[MemoryPackConstructor]
	protected BlueprintReferenceBase()
	{
	}

	public BlueprintScriptableObject GetBlueprint()
	{
		if (Cached == null)
		{
			Cached = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintScriptableObject;
		}
		return Cached;
	}

	public bool IsEmpty()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return !GetBlueprint();
		}
		return true;
	}

	public static TRef CreateTyped<TRef>(BlueprintScriptableObject bp) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public bool Equals(BlueprintReferenceBase other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return guid == other.guid;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((BlueprintReferenceBase)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (guid == null)
		{
			return 0;
		}
		return guid.GetHashCode();
	}

	public void ReadGuidFromJson(string value)
	{
		guid = value;
	}

	public static TRef CreateCopy<TRef>(TRef source) where TRef : BlueprintReferenceBase, new()
	{
		return new TRef
		{
			guid = source.guid
		};
	}

	static BlueprintReferenceBase()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintReferenceBase>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintReferenceBaseFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintReferenceBase[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintReferenceBase>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintReferenceBase? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintReferenceBase? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintReferenceBase), 1, memberCount);
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
		value = new BlueprintReferenceBase
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}
