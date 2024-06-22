using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public struct RequestBugReportMessage : IMemoryPackable<RequestBugReportMessage>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RequestBugReportMessageFormatter : MemoryPackFormatter<RequestBugReportMessage>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RequestBugReportMessage value)
		{
			RequestBugReportMessage.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RequestBugReportMessage value)
		{
			RequestBugReportMessage.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackInclude]
	public readonly string Id;

	[MemoryPackInclude]
	public readonly string Text;

	[MemoryPackInclude]
	public readonly string Type;

	[MemoryPackConstructor]
	public RequestBugReportMessage([NotNull] string id, [CanBeNull] string text, [CanBeNull] string type)
	{
		Id = id;
		Text = text;
		Type = type;
	}

	static RequestBugReportMessage()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RequestBugReportMessage>())
		{
			MemoryPackFormatterProvider.Register(new RequestBugReportMessageFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RequestBugReportMessage[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RequestBugReportMessage>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RequestBugReportMessage value)
	{
		writer.WriteObjectHeader(3);
		writer.WriteString(value.Id);
		writer.WriteString(value.Text);
		writer.WriteString(value.Type);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RequestBugReportMessage value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(RequestBugReportMessage);
			return;
		}
		string id;
		string text;
		string type;
		if (memberCount == 3)
		{
			id = reader.ReadString();
			text = reader.ReadString();
			type = reader.ReadString();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RequestBugReportMessage), 3, memberCount);
				return;
			}
			id = null;
			text = null;
			type = null;
			if (memberCount != 0)
			{
				id = reader.ReadString();
				if (memberCount != 1)
				{
					text = reader.ReadString();
					if (memberCount != 2)
					{
						type = reader.ReadString();
						_ = 3;
					}
				}
			}
		}
		value = new RequestBugReportMessage(id, text, type);
	}
}
