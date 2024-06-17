using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("95b599bbf82e498b9c31dbdd4293b338")]
public class DamageSkillCheckRoot : BlueprintScriptableObject
{
	public DamageCRPair[] DamageCRPair = new DamageCRPair[0];

	public void DealDamage(BaseUnitEntity user, bool isCriticalFail)
	{
		DamageCRPair damageCRPair = DamageCRPair.FirstOrDefault((DamageCRPair x) => x.CR == Game.Instance.CurrentlyLoadedArea.GetCR()) ?? DamageCRPair.FirstOrDefault();
		DamageDescription damageDescription = new DamageDescription
		{
			Bonus = ((!isCriticalFail) ? 1 : 2) * damageCRPair.Damage.Calculate(user.Context) * ((!isCriticalFail) ? 1 : 2)
		};
		RuleDealDamage ruleDealDamage = new RuleDealDamage(user, user, damageDescription.CreateDamage())
		{
			DisableGameLog = true
		};
		ruleDealDamage.DisableFxAndSound = false;
		using (ContextData<PartHealthExtension.IgnoreWoundThreshold>.Request())
		{
			Rulebook.Trigger(ruleDealDamage);
		}
	}
}
