using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("23cb10dc1b1a4f769142a2ef1747dd65")]
public class HasLastRedirectedAbilityGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		PartAbilityRedirect optional = this.GetTargetByType(Target).Parts.GetOptional<PartAbilityRedirect>();
		if (optional != null)
		{
			EntityFactRef<Ability> lastUsedAbility = optional.LastUsedAbility;
			if (!lastUsedAbility.IsEmpty)
			{
				return 1;
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has last redirected ability";
	}
}
