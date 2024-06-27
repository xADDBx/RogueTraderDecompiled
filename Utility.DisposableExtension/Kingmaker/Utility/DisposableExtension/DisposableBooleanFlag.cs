using System;
using UniRx;

namespace Kingmaker.Utility.DisposableExtension;

public class DisposableBooleanFlag
{
	private bool m_Retained;

	public IDisposable Retain()
	{
		m_Retained = true;
		return Disposable.Create(delegate
		{
			m_Retained = false;
		});
	}

	public static implicit operator bool(DisposableBooleanFlag flag)
	{
		return flag.m_Retained;
	}
}
