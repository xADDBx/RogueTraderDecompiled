using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class CustomMarshalInfo : MarshalInfo
{
	internal Guid guid;

	internal string unmanaged_type;

	internal TypeReference managed_type;

	internal string cookie;

	public Guid Guid
	{
		get
		{
			return guid;
		}
		set
		{
			guid = value;
		}
	}

	public string UnmanagedType
	{
		get
		{
			return unmanaged_type;
		}
		set
		{
			unmanaged_type = value;
		}
	}

	public TypeReference ManagedType
	{
		get
		{
			return managed_type;
		}
		set
		{
			managed_type = value;
		}
	}

	public string Cookie
	{
		get
		{
			return cookie;
		}
		set
		{
			cookie = value;
		}
	}

	public CustomMarshalInfo()
		: base(NativeType.CustomMarshaler)
	{
	}
}
