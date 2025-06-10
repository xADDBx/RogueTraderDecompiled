using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Tasks;
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

	protected override IEnumerator<ArbiterTask> Routine()
	{
		float startTime = Time.unscaledTime;
		while (IsWaitingForCutscene(startTime))
		{
			float num = Time.unscaledTime - startTime;
			base.Status = $"Wait for cut scene... ({num:g})";
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
		}
	}

	private bool IsWaitingForCutscene(float startTime)
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			if (Time.unscaledTime - startTime > 10f)
			{
				ArbiterService.Logger.Log("Force stop locked cutscenes");
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
