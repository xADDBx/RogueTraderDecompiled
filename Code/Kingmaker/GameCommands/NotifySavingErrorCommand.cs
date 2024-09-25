using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class NotifySavingErrorCommand : GameCommand, IMemoryPackable<NotifySavingErrorCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class NotifySavingErrorCommandFormatter : MemoryPackFormatter<NotifySavingErrorCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref NotifySavingErrorCommand value)
		{
			NotifySavingErrorCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref NotifySavingErrorCommand value)
		{
			NotifySavingErrorCommand.Deserialize(ref reader, ref value);
		}
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(WarningNotificationType.SavingError);
		});
	}

	static NotifySavingErrorCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<NotifySavingErrorCommand>())
		{
			MemoryPackFormatterProvider.Register(new NotifySavingErrorCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<NotifySavingErrorCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<NotifySavingErrorCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref NotifySavingErrorCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteObjectHeader(0);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref NotifySavingErrorCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		if (memberCount == 0)
		{
			if (value != null)
			{
				return;
			}
		}
		else
		{
			if (memberCount > 0)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(NotifySavingErrorCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new NotifySavingErrorCommand();
	}
}
