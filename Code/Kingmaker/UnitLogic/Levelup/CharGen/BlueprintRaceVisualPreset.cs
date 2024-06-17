using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.CharGen;

[TypeId("11a828b07c9145c484d200ea9581541a")]
public class BlueprintRaceVisualPreset : BlueprintScriptableObject
{
	public Race RaceId;

	[ValidateNotNull]
	public Skeleton MaleSkeleton;

	[ValidateNotNull]
	public Skeleton FemaleSkeleton;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Skin")]
	private KingmakerEquipmentEntityReference m_Skin;

	public KingmakerEquipmentEntity Skin => m_Skin?.Get();
}
