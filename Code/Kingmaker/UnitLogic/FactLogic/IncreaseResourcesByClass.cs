using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("56737ee5357759b4885a4222547b218c")]
public class IncreaseResourcesByClass : UnitFactComponentDelegate, IResourceAmountBonusHandler<EntitySubscriber>, IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private BlueprintAbilityResourceReference m_Resource;

	[SerializeField]
	private BlueprintCharacterClassReference m_CharacterClass;

	[SerializeField]
	private BlueprintArchetypeReference m_Archetype;

	public StatType Stat;

	public int BaseValue;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	[CanBeNull]
	public BlueprintCharacterClass CharacterClass => m_CharacterClass?.Get();

	[CanBeNull]
	public BlueprintArchetype Archetype => m_Archetype?.Get();

	public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
	{
		if (resource != Resource)
		{
			return;
		}
		int num = base.Owner.Stats.GetStat<ModifiableValueAttributeStat>(Stat)?.Bonus ?? 0;
		bonus += BaseValue + num;
		if (CharacterClass != null)
		{
			int classLevel = base.Owner.Progression.GetClassLevel(CharacterClass);
			bonus += classLevel;
		}
		if (Archetype == null)
		{
			return;
		}
		foreach (ClassData @class in base.Owner.Progression.Classes)
		{
			if (@class.CharacterClass != CharacterClass && @class.Archetypes.HasItem(Archetype))
			{
				bonus += @class.Level;
				break;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
