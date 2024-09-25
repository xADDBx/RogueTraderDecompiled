using System;
using System.Collections.Generic;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class SurfaceServiceRequestPool : IDisposable
{
	private readonly Stack<SurfaceServiceRequest> m_Stack = new Stack<SurfaceServiceRequest>();

	private bool m_Disposed;

	public SurfaceServiceRequest Get()
	{
		if (m_Disposed)
		{
			throw new ObjectDisposedException("SurfaceServiceRequestPool");
		}
		if (m_Stack.TryPop(out var result))
		{
			return result;
		}
		return new SurfaceServiceRequest();
	}

	public void Release(SurfaceServiceRequest value)
	{
		if (m_Disposed)
		{
			throw new ObjectDisposedException("SurfaceServiceRequestPool");
		}
		if (m_Disposed)
		{
			value.Dispose();
			return;
		}
		value.Clear();
		m_Stack.Push(value);
	}

	public void Dispose()
	{
		if (m_Disposed)
		{
			return;
		}
		foreach (SurfaceServiceRequest item in m_Stack)
		{
			item.Dispose();
		}
		m_Stack.Clear();
		m_Disposed = true;
	}
}
