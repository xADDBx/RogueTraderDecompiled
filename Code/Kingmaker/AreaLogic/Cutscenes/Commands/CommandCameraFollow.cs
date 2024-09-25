using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("dd3e509d1d5d21c4b851a4d373822c4c")]
public class CommandCameraFollow : CommandBase
{
	[SerializeReference]
	public PositionEvaluator Target;

	public float OverrideRubberband = 3f;

	private float? m_OldRubberband;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Target.GetValue();
		m_OldRubberband = CameraRig.Instance.ScrollRubberBand;
		CameraRig.Instance.ScrollRubberBand = OverrideRubberband;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (Target != null && Target.TryGetValue(out var value))
		{
			CameraRig.Instance.ScrollTo(value);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (m_OldRubberband.HasValue)
		{
			CameraRig.Instance.ScrollRubberBand = m_OldRubberband.Value;
		}
		m_OldRubberband = null;
	}

	public override string GetCaption()
	{
		return "Camera follows " + Target;
	}
}
