using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class AreaTransitionSettings
{
	[SerializeField]
	private BlueprintAreaEnterPointReference m_AreaEnterPoint;

	[SerializeField]
	private BlueprintAreaTransitionReference m_Blueprint;

	[SerializeField]
	private BlueprintUnlockableFlagReference m_VisibilityFlag;

	[SerializeField]
	private BlueprintEtudeReference m_VisibilityEtude;

	[SerializeField]
	public int TooltipIndex;

	[SerializeField]
	private bool m_OverrideProximityDistance;

	[SerializeField]
	[ShowIf("m_OverrideProximityDistance")]
	private float m_ProximityDistance;

	[SerializeField]
	[Tooltip("If the point leads to a global map and the player comes right back, the enter point nearest to this exit point will be used instead of the default one.")]
	public bool ReturnToNearestEnterPoint = true;

	[SerializeField]
	[HideIf("ReturnToNearestEnterPoint")]
	[Tooltip("If the point leads to a global map and the player comes right back, the specified enter point will be used instead of the default one.")]
	public bool ReturnToSpecificEnterPoint;

	[SerializeField]
	[ShowIf("ReturnToSpecificEnterPoint")]
	[Tooltip("The enter point to use if this exit point leads to a global map and the player comes right back.")]
	private BlueprintAreaEnterPointReference m_ReturnToEnterPoint;

	public bool AddMapMarker = true;

	public AutoSaveMode AutoSaveMode = AutoSaveMode.BeforeExit;

	public bool SuppressLoot;

	public bool EnableInCombat;

	public BlueprintAreaEnterPoint AreaEnterPoint
	{
		get
		{
			return m_AreaEnterPoint.Get();
		}
		set
		{
			m_AreaEnterPoint = value.ToReference<BlueprintAreaEnterPointReference>();
		}
	}

	public float ProximityDistance
	{
		get
		{
			if (!m_OverrideProximityDistance)
			{
				return 5f;
			}
			return m_ProximityDistance;
		}
	}

	public BlueprintAreaTransition Blueprint => m_Blueprint?.Get();

	public BlueprintUnlockableFlag VisibilityFlag => m_VisibilityFlag?.Get();

	public BlueprintEtude VisibilityEtude => m_VisibilityEtude?.Get();

	public BlueprintAreaEnterPoint ReturnToEnterPoint => m_ReturnToEnterPoint?.Get();
}
