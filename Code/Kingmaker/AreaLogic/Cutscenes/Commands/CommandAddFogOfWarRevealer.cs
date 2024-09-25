using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("72655d3c035284e418bff927ec00c094")]
public class CommandAddFogOfWarRevealer : CommandBase
{
	[SerializeField]
	[SerializeReference]
	private TransformEvaluator m_Revealer;

	private Transform m_EvaluatedRevealer;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_EvaluatedRevealer = m_Revealer.GetValue();
		if ((bool)m_EvaluatedRevealer)
		{
			FogOfWarControllerData.AddRevealer(m_EvaluatedRevealer);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (m_EvaluatedRevealer != null)
		{
			FogOfWarControllerData.RemoveRevealer(m_EvaluatedRevealer);
			m_EvaluatedRevealer = null;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "<b>Fog of war revealer</b>";
	}
}
