using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.Progression.Paths;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSelectCareerPathGameCommand : GameCommand, IMemoryPackable<CharGenSelectCareerPathGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSelectCareerPathGameCommandFormatter : MemoryPackFormatter<CharGenSelectCareerPathGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSelectCareerPathGameCommand value)
		{
			CharGenSelectCareerPathGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintCareerPathReference m_CareerPathRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPathReference m_careerPathRef)
	{
		m_CareerPathRef = m_careerPathRef;
	}

	public CharGenSelectCareerPathGameCommand([NotNull] BlueprintCareerPath careerPath)
		: this(careerPath.ToReference<BlueprintCareerPathReference>())
	{
		if (careerPath == null)
		{
			throw new ArgumentNullException("careerPath");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintCareerPath blueprintCareerPath = m_CareerPathRef.Get();
		if (blueprintCareerPath == null)
		{
			PFLog.GameCommands.Error("[CharGenSelectCareerPathGameCommand] BlueprintCareerPath not found #" + m_CareerPathRef.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenCareerPathHandler h)
		{
			h.HandleCareerPath(blueprintCareerPath);
		});
	}

	static CharGenSelectCareerPathGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectCareerPathGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSelectCareerPathGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSelectCareerPathGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSelectCareerPathGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSelectCareerPathGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_CareerPathRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSelectCareerPathGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintCareerPathReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintCareerPathReference>();
			}
			else
			{
				value2 = value.m_CareerPathRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSelectCareerPathGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_CareerPathRef : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSelectCareerPathGameCommand(value2);
	}
}
