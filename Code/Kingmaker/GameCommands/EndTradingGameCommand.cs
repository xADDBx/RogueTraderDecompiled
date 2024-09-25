using Kingmaker.Code.UI.MVVM.VM.Vendor;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class EndTradingGameCommand : GameCommand, IMemoryPackable<EndTradingGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EndTradingGameCommandFormatter : MemoryPackFormatter<EndTradingGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EndTradingGameCommand value)
		{
			EndTradingGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EndTradingGameCommand value)
		{
			EndTradingGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public EndTradingGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		VendorHelper.Vendor.CancelSellCargoesDeal();
		VendorHelper.Vendor.EndTraiding();
	}

	static EndTradingGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EndTradingGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EndTradingGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EndTradingGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EndTradingGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EndTradingGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref EndTradingGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EndTradingGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new EndTradingGameCommand();
	}
}
