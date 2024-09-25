using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[Serializable]
public class FamiliarSettings
{
	[SerializeField]
	[ValidateNotNull]
	private HideFamiliarSettings m_HideSettings = new HideFamiliarSettings();

	[SerializeField]
	[ValidateNotNull]
	private FollowerSettings m_FollowerSettings = new FollowerSettings();

	[SerializeField]
	private bool m_FadeOnDestroy;

	[SerializeField]
	private ConditionsChecker m_SpawnOnLocationCondition;

	public HideFamiliarSettings HideFamiliarSettings => m_HideSettings;

	public FollowerSettings FollowerSettings => m_FollowerSettings;

	public bool FadeOnDestroy => m_FadeOnDestroy;

	public ConditionsChecker SpawnOnLocationCondition => m_SpawnOnLocationCondition;

	public Entity.ViewHandlingOnDisposePolicyType ViewHandlingOnDisposePolicyType
	{
		get
		{
			if (!m_FadeOnDestroy)
			{
				return Entity.ViewHandlingOnDisposePolicyType.Destroy;
			}
			return Entity.ViewHandlingOnDisposePolicyType.FadeOutAndDestroy;
		}
	}
}
