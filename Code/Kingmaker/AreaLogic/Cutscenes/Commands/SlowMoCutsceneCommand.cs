using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("dfc23a92cb484f3eb72ed916a224bcce")]
public class SlowMoCutsceneCommand : CommandBase
{
	public bool DisableSlowMo;

	[HideIf("DisableSlowMo")]
	public float SlowMoFactor = 0.1f;

	[SerializeReference]
	[HideIf("DisableSlowMo")]
	public AbstractUnitEvaluator[] Targets;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!DisableSlowMo)
		{
			if (Targets.Empty())
			{
				EventBus.RaiseEvent(delegate(ISlowMoCutsceneHandler h)
				{
					h.AddUnitToNormalTimeline(null);
				});
				SlowMoController.SlowMoFactor = SlowMoFactor;
				return;
			}
			AbstractUnitEvaluator[] targets = Targets;
			foreach (AbstractUnitEvaluator target in targets)
			{
				EventBus.RaiseEvent(delegate(ISlowMoCutsceneHandler h)
				{
					h.AddUnitToNormalTimeline(target.GetValue());
				});
				SlowMoController.SlowMoFactor = SlowMoFactor;
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISlowMoCutsceneHandler h)
			{
				h.OffSlowMo();
			});
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		EventBus.RaiseEvent(delegate(ISlowMoCutsceneHandler h)
		{
			h.OffSlowMo();
		});
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}
}
