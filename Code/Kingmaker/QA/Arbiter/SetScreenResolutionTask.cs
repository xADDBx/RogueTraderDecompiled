using System.Collections;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public class SetScreenResolutionTask : ArbiterTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	public SetScreenResolutionTask(ArbiterStartupParameters arguments, ArbiterTask parent)
		: base(parent)
	{
		m_Arguments = arguments;
	}

	protected override IEnumerator Routine()
	{
		int width = Arbiter.Root.Resolution.x;
		int height = Arbiter.Root.Resolution.y;
		string[] array = m_Arguments.ArbiterScreenResolution?.Split('x');
		if (array != null && array.Length == 2)
		{
			width = int.Parse(array[0]);
			height = int.Parse(array[1]);
		}
		Screen.SetResolution(Screen.width, Screen.height, fullscreen: false);
		yield return null;
		yield return null;
		Screen.SetResolution(width, height, Screen.fullScreenMode);
		yield return null;
		yield return null;
		if (Screen.currentResolution.width != width || Screen.currentResolution.height != height)
		{
			PFLog.Arbiter.Error($"Unable to set resolution to {width}x{height} " + $"(current is {Screen.currentResolution.width}x{Screen.currentResolution.height}). " + "Stop Arbiter due to critical issue");
			Application.Quit(-1);
		}
	}
}
