using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.Canvases;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Assets.AreaLogic.Cutscenes.Commands;

[TypeId("54800cc505f54d84b945c6bf9a4008d4")]
public class CommandCustomLoadingScreen : CommandBase
{
	[ValidateNotNull]
	[AssetPicker("")]
	public CanvasAnimation Prefab;

	public float ShowTime = 1f;

	public ActionList Actions;

	private float m_TimeLeft;

	private bool m_Finished;

	private CanvasAnimation m_Instance;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping;
		if (!skipping)
		{
			m_TimeLeft = ShowTime;
			m_Instance = SimpleBlueprint.Instantiate(Prefab);
			LoadingProcess.Instance.ShowManualLoadingScreen(m_Instance);
		}
		else
		{
			Actions.Run();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_Instance == null;
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		m_Finished = true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (m_Instance == null)
		{
			return;
		}
		switch (m_Instance.GetLoadingScreenState())
		{
		case LoadingScreenState.ShowAnimation:
		case LoadingScreenState.HideAnimation:
			break;
		case LoadingScreenState.Shown:
			m_TimeLeft -= Time.unscaledDeltaTime;
			if (!m_Finished && m_TimeLeft <= 0f)
			{
				Actions.Run();
				LoadingProcess.Instance.HideManualLoadingScreen();
				m_Finished = true;
			}
			break;
		case LoadingScreenState.Hidden:
			Object.Destroy(m_Instance);
			m_Instance = null;
			break;
		}
	}
}
