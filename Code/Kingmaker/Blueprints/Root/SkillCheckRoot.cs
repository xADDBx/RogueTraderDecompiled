using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("eb0f154cbe024b708d154713d974f2f0")]
public class SkillCheckRoot : BlueprintScriptableObject
{
	[SerializeField]
	private DamageSkillCheckRootReference m_DamageSkillCheckRoot;

	[SerializeField]
	private DebuffSkillCheckRootReference m_DebuffSkillCheckRoot;

	public DamageSkillCheckRoot DamageSkillCheckRoot => m_DamageSkillCheckRoot;

	public DebuffSkillCheckRoot DebuffSkillCheckRoot => m_DebuffSkillCheckRoot;
}
