using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Metadata;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class EmbeddedSourceDebugInformation : CustomDebugInformation
{
	internal uint index;

	internal MetadataReader debug_reader;

	internal bool resolved;

	internal byte[] content;

	internal bool compress;

	public static Guid KindIdentifier = new Guid("{0E8A571B-6926-466E-B4AD-8AB04611F5FE}");

	public byte[] Content
	{
		get
		{
			if (!resolved)
			{
				Resolve();
			}
			return content;
		}
		set
		{
			content = value;
			resolved = true;
		}
	}

	public bool Compress
	{
		get
		{
			if (!resolved)
			{
				Resolve();
			}
			return compress;
		}
		set
		{
			compress = value;
			resolved = true;
		}
	}

	public override CustomDebugInformationKind Kind => CustomDebugInformationKind.EmbeddedSource;

	internal EmbeddedSourceDebugInformation(uint index, MetadataReader debug_reader)
		: base(KindIdentifier)
	{
		this.index = index;
		this.debug_reader = debug_reader;
	}

	public EmbeddedSourceDebugInformation(byte[] content, bool compress)
		: base(KindIdentifier)
	{
		resolved = true;
		this.content = content;
		this.compress = compress;
	}

	internal byte[] ReadRawEmbeddedSourceDebugInformation()
	{
		if (debug_reader == null)
		{
			throw new InvalidOperationException();
		}
		return debug_reader.ReadRawEmbeddedSourceDebugInformation(index);
	}

	private void Resolve()
	{
		if (!resolved)
		{
			if (debug_reader == null)
			{
				throw new InvalidOperationException();
			}
			Row<byte[], bool> row = debug_reader.ReadEmbeddedSourceDebugInformation(index);
			content = row.Col1;
			compress = row.Col2;
			resolved = true;
		}
	}
}
