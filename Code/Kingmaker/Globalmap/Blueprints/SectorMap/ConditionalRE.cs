using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.SectorMap;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.SectorMap;

[Serializable]
public class ConditionalRE
{
	[SerializeField]
	public ConditionsChecker Conditions;

	[SerializeField]
	private BlueprintDialogReference m_RandomEncounter;

	[SerializeField]
	public List<SectorMapPassageEntity.PassageDifficulty> Difficulties;

	public BlueprintDialog RandomEncounter => m_RandomEncounter?.Get();
}
