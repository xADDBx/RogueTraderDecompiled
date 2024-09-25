using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Controllers.SpaceCombat;

public class SpaceCombatBackgroundComposerController : IController, IAreaHandler, ISubscriber, IAdditiveAreaSwitchHandler
{
	private GameObject m_BackgroundComposer;

	public void OnAreaBeginUnloading()
	{
		if ((bool)m_BackgroundComposer)
		{
			BackgroundComposerDestroy();
		}
	}

	public void OnAreaDidLoad()
	{
		BackgroundComposerInstance();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		if ((bool)m_BackgroundComposer)
		{
			BackgroundComposerDestroy();
		}
	}

	public void OnAdditiveAreaDidActivated()
	{
		BackgroundComposerInstance();
	}

	private void BackgroundComposerInstance()
	{
		if (BlueprintRoot.Instance.BackgroundComposer != null && m_BackgroundComposer == null)
		{
			m_BackgroundComposer = Object.Instantiate(BlueprintRoot.Instance.BackgroundComposer);
			Game.Instance.DynamicRoot.Add(m_BackgroundComposer.transform);
		}
		else
		{
			PFLog.TechArt.Error("BackgroundCameraSpaceCombatComposer prefab not found in BlueprintRoot -> BackgroundComposer or somebody try instantiate BackgroundComposer at twice");
		}
	}

	private void BackgroundComposerDestroy()
	{
		Object.Destroy(m_BackgroundComposer);
		m_BackgroundComposer = null;
	}
}
