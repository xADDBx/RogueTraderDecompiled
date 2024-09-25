using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("1aa0e70617f68494785d51510dd019c5")]
public class CommandCustomeCutsceneController : CommandBase
{
	[ValidateNotNull]
	[SerializeReference]
	public ArtCutsceneControllerEvaluator CustomeControllerEvaluator;

	private bool m_IsVfxComplete;

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_IsVfxComplete;
	}

	public override string GetCaption()
	{
		return $"CutsceneController {CustomeControllerEvaluator}";
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_IsVfxComplete = false;
		if (CustomeControllerEvaluator.TryGetValue(out var value))
		{
			value.OnRun(OnComplete);
			return;
		}
		PFLog.Default.Error("Не удалось найти CutsceneCamera на указанном объекте");
		m_IsVfxComplete = true;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		m_IsVfxComplete = true;
		if (CustomeControllerEvaluator.TryGetValue(out var value))
		{
			value.OnStop();
		}
		else
		{
			PFLog.Default.Error("Не удалось найти CutsceneCamera на указанном объекте");
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		m_IsVfxComplete = true;
		if (CustomeControllerEvaluator.TryGetValue(out var value))
		{
			value.OnRun(OnComplete);
		}
		else
		{
			PFLog.Default.Error("Не удалось найти CutsceneCamera на указанном объекте");
		}
	}

	private void OnComplete()
	{
		m_IsVfxComplete = true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
