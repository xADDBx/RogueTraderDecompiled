using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("50e58cbba51c1cf41bdcca491ad559df")]
public class CommandInstantiatePrefab : CommandBase
{
	[SerializeField]
	[SerializeReference]
	private TransformEvaluator m_Placeholder;

	[SerializeField]
	private GameObject m_Prefab;

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[ConditionalHide("IsContinuous")]
	private float m_Lifetime;

	private bool m_Finished;

	private GameObject m_PrefabInstance;

	public override bool IsContinuous => m_Continuous;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = false;
		if (!m_PrefabInstance)
		{
			m_PrefabInstance = FxHelper.SpawnFxOnGameObject(m_Prefab, m_Placeholder.GetValue().gameObject);
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		m_Finished = !m_Continuous && time >= (double)m_Lifetime;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if ((bool)m_PrefabInstance)
		{
			FxHelper.Destroy(m_PrefabInstance);
			m_PrefabInstance = null;
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
	}

	public override string GetCaption()
	{
		string text = (m_Prefab ? m_Prefab.name : "");
		return "<b>Instantiate prefab:</b> " + (text ?? "???");
	}
}
