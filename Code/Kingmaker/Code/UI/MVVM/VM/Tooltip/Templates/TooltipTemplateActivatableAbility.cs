using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Items;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateActivatableAbility : TooltipBaseTemplate
{
	private ActivatableAbility m_Ability;

	public readonly BlueprintActivatableAbility BlueprintActivatableAbility;

	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly string m_Description;

	public TooltipTemplateActivatableAbility(ActivatableAbility ability)
	{
		if (ability != null)
		{
			m_Name = ability.Name;
			m_Icon = ability.Icon;
			m_Description = ability.Description;
			BlueprintActivatableAbility = ability.Blueprint;
			m_Ability = ability;
		}
	}

	public TooltipTemplateActivatableAbility(BlueprintActivatableAbility activatableAbility)
	{
		if (activatableAbility != null)
		{
			m_Name = activatableAbility.Name;
			m_Icon = activatableAbility.Icon;
			m_Description = activatableAbility.Description;
			BlueprintActivatableAbility = activatableAbility;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickEntityHeader(m_Name, m_Icon, hasUpgrade: false);
		yield return new TooltipBrickSeparator();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(m_Description, TooltipTextType.Paragraph);
		ITooltipBrick sourceBrick = null;
		if ((bool)m_Ability?.SourceAbilityBlueprint)
		{
			sourceBrick = new TooltipBrickFeature(m_Ability.SourceAbilityBlueprint);
		}
		if (m_Ability?.SourceFact != null)
		{
			sourceBrick = new TooltipBrickFeature(m_Ability.SourceFact);
		}
		if (m_Ability?.SourceItem != null)
		{
			ItemEntity itemEntity = (ItemEntity)m_Ability.SourceItem;
			sourceBrick = new TooltipBrickIconAndName(itemEntity.Icon, itemEntity.Name);
		}
		if (sourceBrick != null)
		{
			yield return new TooltipBrickSeparator();
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2);
			yield return sourceBrick;
		}
	}
}
