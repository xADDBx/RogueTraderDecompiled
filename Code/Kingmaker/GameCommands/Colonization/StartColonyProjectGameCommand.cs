using JetBrains.Annotations;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class StartColonyProjectGameCommand : GameCommand, IMemoryPackable<StartColonyProjectGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StartColonyProjectGameCommandFormatter : MemoryPackFormatter<StartColonyProjectGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StartColonyProjectGameCommand value)
		{
			StartColonyProjectGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartColonyProjectGameCommand value)
		{
			StartColonyProjectGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintColonyReference m_Colony;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintColonyProjectReference m_Project;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private StartColonyProjectGameCommand()
	{
	}

	[JsonConstructor]
	public StartColonyProjectGameCommand([NotNull] BlueprintColonyReference colony, [NotNull] BlueprintColonyProjectReference project)
	{
		m_Colony = colony;
		m_Project = project;
	}

	protected override void ExecuteInternal()
	{
		(Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == m_Colony.Get())?.Colony)?.StartProject(m_Project.Get());
	}

	static StartColonyProjectGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartColonyProjectGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartColonyProjectGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartColonyProjectGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartColonyProjectGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StartColonyProjectGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Colony);
		writer.WritePackable(in value.m_Project);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StartColonyProjectGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintColonyReference value2;
		BlueprintColonyProjectReference value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Colony;
				value3 = value.m_Project;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<BlueprintColonyReference>();
			value3 = reader.ReadPackable<BlueprintColonyProjectReference>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartColonyProjectGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.m_Colony;
				value3 = value.m_Project;
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
				goto IL_009a;
			}
		}
		value = new StartColonyProjectGameCommand
		{
			m_Colony = value2,
			m_Project = value3
		};
		return;
		IL_009a:
		value.m_Colony = value2;
		value.m_Project = value3;
	}
}
