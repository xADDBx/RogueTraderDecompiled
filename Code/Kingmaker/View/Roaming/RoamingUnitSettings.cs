using System;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.Roaming;

[Serializable]
public class RoamingUnitSettings
{
	[JsonProperty]
	public float Radius = 1f;

	[JsonProperty]
	public float MinIdleTime;

	[JsonProperty]
	public float MaxIdleTime;

	[NotNull]
	[JsonProperty]
	[SerializeField]
	[FormerlySerializedAs("IdleCutscenes")]
	private CutsceneReference[] m_IdleCutscenes = new CutsceneReference[0];

	[JsonProperty]
	public WalkSpeedType MovementType = WalkSpeedType.Walk;

	[JsonProperty]
	public float MovementSpeed;

	[JsonProperty]
	public bool Sleepless;

	public ReferenceArrayProxy<Cutscene> IdleCutscenes
	{
		get
		{
			BlueprintReference<Cutscene>[] idleCutscenes = m_IdleCutscenes;
			return idleCutscenes;
		}
	}

	public RoamingUnitSettings Copy()
	{
		return new RoamingUnitSettings
		{
			Radius = Radius,
			MinIdleTime = MinIdleTime,
			MaxIdleTime = MaxIdleTime,
			m_IdleCutscenes = m_IdleCutscenes.ToArray(),
			MovementType = MovementType,
			Sleepless = Sleepless,
			MovementSpeed = MovementSpeed
		};
	}

	public void SetCutscenes(CutsceneReference[] idleCutscenes)
	{
		m_IdleCutscenes = idleCutscenes.ToArray();
	}
}
