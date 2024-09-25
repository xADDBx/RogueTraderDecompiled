using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class ComputeBufferUtils
{
	public static ComputeBuffer SetComputeBufferData<T>(ComputeBuffer buffer, NativeArray<T> data, int count, string bufferName) where T : struct
	{
		if (buffer == null || !buffer.IsValid() || buffer.count < count)
		{
			buffer?.Release();
			if (count > 0)
			{
				buffer = new ComputeBuffer(count, Marshal.SizeOf<T>());
				buffer.name = bufferName;
			}
		}
		if (count > 0)
		{
			buffer.SetData(data, 0, 0, count);
		}
		return buffer;
	}

	public static ComputeBuffer SetComputeBufferData<T>(ComputeBuffer buffer, T[] data, int count, string bufferName) where T : struct
	{
		if (buffer == null || !buffer.IsValid() || buffer.count < count)
		{
			buffer?.Release();
			if (count > 0)
			{
				buffer = new ComputeBuffer(count, Marshal.SizeOf<T>());
				buffer.name = bufferName;
			}
		}
		if (count > 0)
		{
			buffer.SetData(data, 0, 0, count);
		}
		return buffer;
	}

	public static ComputeBuffer SetComputeBufferData<T>(ComputeBuffer buffer, List<T> data, int count, string bufferName) where T : struct
	{
		if (buffer == null || !buffer.IsValid() || buffer.count < count)
		{
			buffer?.Release();
			if (count > 0)
			{
				buffer = new ComputeBuffer(count, Marshal.SizeOf<T>());
				buffer.name = bufferName;
			}
		}
		if (count > 0)
		{
			buffer.SetData(data, 0, 0, count);
		}
		return buffer;
	}

	public static ComputeBuffer SetSize(ComputeBuffer buffer, Type type, int size, string name)
	{
		if (buffer == null || !buffer.IsValid() || buffer.count != size)
		{
			buffer?.Release();
			if (size > 0)
			{
				buffer = new ComputeBuffer(size, Marshal.SizeOf(type));
				buffer.name = name;
			}
		}
		return buffer;
	}
}
