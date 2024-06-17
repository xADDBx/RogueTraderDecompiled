using System;
using System.Collections;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public class WaitForCutsceneTask : ArbiterTask
{
	private const int s_CutsceneSkipMaxTime = 10;

	public WaitForCutsceneTask(ArbiterTask parent)
		: base(parent)
	{
	}

	protected override IEnumerator Routine()
	{
		float startTime = Time.unscaledTime;
		while (IsWaitingForCutscene(startTime))
		{
			float num = Time.unscaledTime - startTime;
			base.Status = $"Wait for cut scene... ({num:F})";
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
		}
		Game.Instance.DialogController.StopDialog();
	}

	private bool IsWaitingForCutscene(float startTime)
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			if (Time.unscaledTime - startTime > 10f)
			{
				PFLog.Arbiter.Log("Force stop locked cutscenes");
				foreach (CutscenePlayerData item in Game.Instance.State.Cutscenes.ToTempList())
				{
					if (item.Cutscene.LockControl)
					{
						item.Stop();
					}
				}
				return true;
			}
			Game.Instance.GameCommandQueue.SkipCutscene();
			return true;
		}
		return false;
	}
}
