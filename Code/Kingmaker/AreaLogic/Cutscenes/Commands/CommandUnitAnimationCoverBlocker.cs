using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("4394fb3648d14b5e95ab4111fc3fcff7")]
public class CommandUnitAnimationCoverBlocker : CommandBase
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool CoverAvailable;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		UnitAnimationManager animationManager = Unit.GetValue().View.AnimationManager;
		if (!(animationManager == null))
		{
			animationManager.InCutsceneCoverAvailable = CoverAvailable;
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}
}
