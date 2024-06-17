using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class LoadAreaGameCommand : GameCommand, IMemoryPackable<LoadAreaGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LoadAreaGameCommandFormatter : MemoryPackFormatter<LoadAreaGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LoadAreaGameCommand value)
		{
			LoadAreaGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintAreaEnterPointReference m_EnterPoint;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly AutoSaveMode m_AutoSaveMode;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public LoadAreaGameCommand([NotNull] BlueprintAreaEnterPointReference m_enterPoint, AutoSaveMode m_autoSaveMode)
	{
		m_EnterPoint = m_enterPoint;
		m_AutoSaveMode = m_autoSaveMode;
		if (m_EnterPoint == null)
		{
			throw new ArgumentNullException("m_enterPoint");
		}
		if ((BlueprintAreaEnterPoint)m_EnterPoint == null)
		{
			throw new NullReferenceException("EnterPoint was not found! " + m_EnterPoint.Guid);
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = m_EnterPoint;
		if (blueprintAreaEnterPoint == null)
		{
			PFLog.GameCommands.Log("[LoadAreaGameCommand] EnterPoint was not found! " + m_EnterPoint.Guid);
		}
		else
		{
			Game.Instance.LoadArea(blueprintAreaEnterPoint, m_AutoSaveMode);
		}
	}

	static LoadAreaGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LoadAreaGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new LoadAreaGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LoadAreaGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LoadAreaGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AutoSaveMode>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<AutoSaveMode>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LoadAreaGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_EnterPoint);
		writer.WriteUnmanaged(in value.m_AutoSaveMode);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LoadAreaGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintAreaEnterPointReference value2;
		AutoSaveMode value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintAreaEnterPointReference>();
				reader.ReadUnmanaged<AutoSaveMode>(out value3);
			}
			else
			{
				value2 = value.m_EnterPoint;
				value3 = value.m_AutoSaveMode;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<AutoSaveMode>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LoadAreaGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = AutoSaveMode.None;
			}
			else
			{
				value2 = value.m_EnterPoint;
				value3 = value.m_AutoSaveMode;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<AutoSaveMode>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new LoadAreaGameCommand(value2, value3);
	}
}
