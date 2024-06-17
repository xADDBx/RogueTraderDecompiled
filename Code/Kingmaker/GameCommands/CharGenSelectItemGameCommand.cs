using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSelectItemGameCommand : GameCommand, IMemoryPackable<CharGenSelectItemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSelectItemGameCommandFormatter : MemoryPackFormatter<CharGenSelectItemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSelectItemGameCommand value)
		{
			CharGenSelectItemGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly FeatureGroup m_FeatureGroup;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintFeatureReference m_BlueprintFeature;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSelectItemGameCommand(FeatureGroup m_featureGroup, [NotNull] BlueprintFeatureReference m_blueprintFeature)
	{
		if (m_blueprintFeature == null)
		{
			throw new ArgumentNullException("m_blueprintFeature");
		}
		m_FeatureGroup = m_featureGroup;
		m_BlueprintFeature = m_blueprintFeature;
	}

	public CharGenSelectItemGameCommand(FeatureGroup featureGroup, [NotNull] BlueprintFeature feature)
		: this(featureGroup, feature.ToReference<BlueprintFeatureReference>())
	{
		if (feature == null)
		{
			throw new ArgumentNullException("feature");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintFeature blueprintFeature = m_BlueprintFeature.Get();
		if (blueprintFeature == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectItemGameCommand] BlueprintFeature not found #" + m_BlueprintFeature.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenSelectItemHandler h)
		{
			h.HandleSelectItem(m_FeatureGroup, blueprintFeature);
		});
	}

	static CharGenSelectItemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSelectItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSelectItemGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FeatureGroup>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<FeatureGroup>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSelectItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_FeatureGroup);
		writer.WritePackable(in value.m_BlueprintFeature);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSelectItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		FeatureGroup value2;
		BlueprintFeatureReference value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				value3 = reader.ReadPackable<BlueprintFeatureReference>();
			}
			else
			{
				value2 = value.m_FeatureGroup;
				value3 = value.m_BlueprintFeature;
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				reader.ReadPackable(ref value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSelectItemGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = FeatureGroup.None;
				value3 = null;
			}
			else
			{
				value2 = value.m_FeatureGroup;
				value3 = value.m_BlueprintFeature;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<FeatureGroup>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSelectItemGameCommand(value2, value3);
	}
}
