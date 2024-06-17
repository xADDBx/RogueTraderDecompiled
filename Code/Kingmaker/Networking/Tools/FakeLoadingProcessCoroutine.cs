using System;
using System.Collections;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Networking.Tools;

public class FakeLoadingProcessCoroutine : IDisposable
{
	private bool m_InProgress;

	public FakeLoadingProcessCoroutine(LoadingProcessTag processTag = LoadingProcessTag.None)
	{
		m_InProgress = true;
		LoadingProcess.Instance.StartLoadingProcess(Load(), null, processTag);
	}

	public void Dispose()
	{
		m_InProgress = false;
	}

	private IEnumerator Load()
	{
		while (m_InProgress)
		{
			yield return null;
		}
	}
}
