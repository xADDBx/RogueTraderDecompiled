using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Solvers;

[TypeId("05e4936346aaee54694e3766e125eda9")]
public class TutorialSolverSpellWithDescriptor : TutorialSolverSpellOrUsableItem
{
	public SpellDescriptorWrapper SpellDescriptors;

	[InfoBox(Text = "If NeedAllDescriptors is true, only buff that has all listed flags will trigger")]
	public bool NeedAllDescriptors;

	[InfoBox(Text = "To prevent including self healing spells or buffs")]
	public bool ExcludeOnlySelfTarget;

	protected override int GetPriority(AbilityData ability)
	{
		return GetSpellPriority(ability.Blueprint);
	}

	protected override int GetPriority(ItemEntity item)
	{
		return -1;
	}

	private int GetSpellPriority(BlueprintAbility ability)
	{
		if (ExcludeOnlySelfTarget && ability.CanTargetSelf && !ability.CanTargetFriends && !ability.CanTargetEnemies && !ability.CanTargetPoint)
		{
			return -1;
		}
		if (NeedAllDescriptors ? ability.SpellDescriptor.HasAllFlags(SpellDescriptors.Value) : ability.SpellDescriptor.HasAnyFlag(SpellDescriptors.Value))
		{
			return 1;
		}
		return -1;
	}
}
