using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("5892c65f961d47f7b67f04073017fc22")]
public abstract class ReplaceStat : MechanicEntityFactComponentDelegate, IHashable
{
	protected enum Attributes
	{
		Unknown,
		WarhammerBallisticSkill,
		WarhammerWeaponSkill,
		WarhammerStrength,
		WarhammerToughness,
		WarhammerAgility,
		WarhammerIntelligence,
		WarhammerPerception,
		WarhammerWillpower,
		WarhammerFellowship,
		WarhammerMedicae
	}

	[SerializeField]
	[Tooltip("Рестрикшн не предотвращает добавление оверрайда юниту. Он проверяется при каждой попытке использовать этот оверрайд и не использует его, если рестрикшн не пройден.")]
	private RestrictionCalculator m_RestrictionCalculator;

	[SerializeField]
	protected StatType m_OriginalStat;

	[SerializeField]
	protected Attributes m_NewAttribute;

	[SerializeField]
	protected bool m_OnlyIfHigher;

	[SerializeField]
	[ShowIf("m_OnlyIfHigher")]
	protected Attributes m_PreviousAttributeToCompare;

	public RestrictionCalculator RestrictionCalculator => m_RestrictionCalculator;

	protected StatType PreviousAttributeToCompare => GetStat(m_PreviousAttributeToCompare);

	public StatType OriginalStat => m_OriginalStat;

	protected static StatType GetStat(Attributes attribute)
	{
		return attribute switch
		{
			Attributes.WarhammerBallisticSkill => StatType.WarhammerBallisticSkill, 
			Attributes.WarhammerWeaponSkill => StatType.WarhammerWeaponSkill, 
			Attributes.WarhammerStrength => StatType.WarhammerStrength, 
			Attributes.WarhammerToughness => StatType.WarhammerToughness, 
			Attributes.WarhammerAgility => StatType.WarhammerAgility, 
			Attributes.WarhammerIntelligence => StatType.WarhammerIntelligence, 
			Attributes.WarhammerPerception => StatType.WarhammerPerception, 
			Attributes.WarhammerWillpower => StatType.WarhammerWillpower, 
			Attributes.WarhammerFellowship => StatType.WarhammerFellowship, 
			Attributes.WarhammerMedicae => StatType.SkillMedicae, 
			Attributes.Unknown => StatType.Unknown, 
			_ => throw new ArgumentOutOfRangeException("attribute", attribute, null), 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
