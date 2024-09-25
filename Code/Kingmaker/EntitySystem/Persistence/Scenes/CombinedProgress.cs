using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public class CombinedProgress
{
	private readonly IProgress<float> m_Out;

	private readonly List<float> m_Progresses = new List<float>();

	public CombinedProgress(IProgress<float> @out)
	{
		m_Out = @out;
	}

	public IProgress<float> CreateChild()
	{
		m_Progresses.Add(0f);
		int len = m_Progresses.Count;
		return new Progress<float>(delegate(float p)
		{
			OnReport(len - 1, p);
		});
	}

	private void OnReport(int idx, float arg)
	{
		m_Progresses[idx] = arg;
		float num = m_Progresses.Sum();
		m_Out?.Report(num / (float)m_Progresses.Count);
	}
}
