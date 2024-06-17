using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("43f1ee69992dc1848b97e623487f6442")]
public class BlueprintFeatureReplaceSpellbook : BlueprintFeature
{
	[SerializeField]
	[FormerlySerializedAs("Spellbook")]
	private BlueprintSpellbookReference m_Spellbook;

	public BlueprintSpellbook Spellbook => m_Spellbook?.Get();
}
