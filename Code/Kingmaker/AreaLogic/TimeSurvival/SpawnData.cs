using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
public class SpawnData
{
	[SerializeField]
	private BlueprintUnitsListReference m_UnitsList;

	[SerializeField]
	private BlueprintSpawnersListReference m_SpawnersList;

	private BlueprintSpawnersList m_CachedSpawnersList;

	[ValidatePositiveNumber]
	[Tooltip("Units will be spawned from this list starting FROM this round")]
	public int RoundFrom;

	[ValidatePositiveNumber]
	[Tooltip("Units will be spawned from this list until this round met")]
	public int RoundTo;

	public List<BlueprintUnit> UnitsList => m_UnitsList.Get().GetBlueprintUnits();

	public BlueprintSpawnersList SpawnersList => m_CachedSpawnersList ?? (m_CachedSpawnersList = m_SpawnersList.Get());
}
