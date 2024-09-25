using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Solvers;

[ClassInfoBox("`t|SolutionItemOrAbility`")]
[TypeId("cd45c852c67c42b79c4b66cac41e15d2")]
public abstract class TutorialSolverSpellOrUsableItem : TutorialSolver
{
	public bool AllowItems = true;

	protected abstract int GetPriority(AbilityData ability);

	protected abstract int GetPriority(ItemEntity item);

	public sealed override bool Solve(TutorialContext context)
	{
		AbilityData abilityData = FindSuitableSpell();
		if (abilityData != null)
		{
			if (abilityData.SourceItem != null)
			{
				context[TutorialContextKey.SolutionItem] = abilityData.SourceItem;
			}
			else
			{
				context[TutorialContextKey.SolutionAbility] = abilityData;
			}
			return true;
		}
		ItemEntity itemEntity = FindSuitableItem();
		if (itemEntity != null)
		{
			context[TutorialContextKey.SolutionItem] = itemEntity;
			return true;
		}
		return false;
	}

	[CanBeNull]
	private AbilityData FindSuitableSpell()
	{
		AbilityData result = null;
		float num = -1f;
		IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
		while (enumerator.MoveNext())
		{
			AbilityData current = enumerator.Current;
			if (!(current == null) && (AllowItems || current.SourceItem == null))
			{
				float num2 = GetPriority(current);
				if (current.IsAvailable && num2 >= 0f && num2 > num)
				{
					result = current;
					num = num2;
				}
			}
		}
		return result;
	}

	private ItemEntity FindSuitableItem()
	{
		if (!AllowItems)
		{
			return null;
		}
		ItemEntity result = null;
		float num = -1f;
		foreach (ItemEntity item2 in Game.Instance.Player.Inventory.Items)
		{
			if (item2.HoldingSlot == null && item2.Blueprint is BlueprintItemEquipmentUsable item)
			{
				float num2 = (TutorialSolverHelper.PartyCanUseItem(item) ? GetPriority(item2) : (-1));
				if (num2 >= 0f && num2 > num)
				{
					result = item2;
					num = num2;
				}
			}
		}
		return result;
	}
}
