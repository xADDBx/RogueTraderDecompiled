using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Formations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[TypeId("14f1034cf5e8fd244b84ab701a39fade")]
public class FormationsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class AutoFormationSettings
	{
		public float SpaceX = 2f;

		public float SpaceY = 2f;
	}

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("PredefinedFormations")]
	private BlueprintPartyFormationReference[] m_PredefinedFormations = new BlueprintPartyFormationReference[0];

	[SerializeField]
	private BlueprintFollowersFormationReference m_FollowersFormation;

	public float FormationsScale = 1f;

	public float MinSpaceFactor = 0.5f;

	public AutoFormationSettings AutoFormation;

	public FollowersFormation FollowersFormation => m_FollowersFormation;

	public ReferenceArrayProxy<BlueprintPartyFormation> PredefinedFormations
	{
		get
		{
			BlueprintReference<BlueprintPartyFormation>[] predefinedFormations = m_PredefinedFormations;
			return predefinedFormations;
		}
	}
}
