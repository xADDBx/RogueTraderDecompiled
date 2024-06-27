using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class BinaryCustomDebugInformation : CustomDebugInformation
{
	private byte[] data;

	public byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public override CustomDebugInformationKind Kind => CustomDebugInformationKind.Binary;

	public BinaryCustomDebugInformation(Guid identifier, byte[] data)
		: base(identifier)
	{
		this.data = data;
	}
}
