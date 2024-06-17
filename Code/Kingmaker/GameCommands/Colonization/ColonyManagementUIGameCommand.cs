using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class ColonyManagementUIGameCommand : GameCommand, IMemoryPackable<ColonyManagementUIGameCommand>, IMemoryPackFormatterRegister
{
	public class Context : ContextFlag<Context>
	{
	}

	[Preserve]
	private sealed class ColonyManagementUIGameCommandFormatter : MemoryPackFormatter<ColonyManagementUIGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ColonyManagementUIGameCommand value)
		{
			ColonyManagementUIGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ColonyManagementUIGameCommand value)
		{
			ColonyManagementUIGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly ServiceWindowsType m_NextWindow;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private ColonyManagementUIGameCommand()
	{
	}

	[MemoryPackConstructor]
	public ColonyManagementUIGameCommand(ServiceWindowsType m_nextWindow)
	{
		m_NextWindow = m_nextWindow;
	}

	protected override void ExecuteInternal()
	{
		using (ContextData<Context>.Request())
		{
			ServiceWindowsType nextWindow = (GameCommandPlayer.GetPlayerOrEmpty().IsLocal ? m_NextWindow : ServiceWindowsType.ColonyManagement);
			EventBus.RaiseEvent(delegate(ICommandServiceWindowUIHandler h)
			{
				h.HandleOpenWindowOfType(nextWindow);
			});
		}
	}

	static ColonyManagementUIGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyManagementUIGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ColonyManagementUIGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyManagementUIGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ColonyManagementUIGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ServiceWindowsType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<ServiceWindowsType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ColonyManagementUIGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_NextWindow);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ColonyManagementUIGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ServiceWindowsType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<ServiceWindowsType>(out value2);
			}
			else
			{
				value2 = value.m_NextWindow;
				reader.ReadUnmanaged<ServiceWindowsType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ColonyManagementUIGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_NextWindow : ServiceWindowsType.None);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<ServiceWindowsType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new ColonyManagementUIGameCommand(value2);
	}
}
