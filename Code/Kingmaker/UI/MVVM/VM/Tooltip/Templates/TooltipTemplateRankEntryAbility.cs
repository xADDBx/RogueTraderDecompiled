using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRankEntryAbility : TooltipTemplateAbility
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly IReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	private readonly RankEntrySelectionVM m_Owner;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit;

	protected override MechanicEntity PreviewEntity => m_PreviewUnit.Value;

	private CalculatedPrerequisite Prerequisite => m_SelectionState.Value?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, (BaseUnitEntity)Caster);

	public TooltipTemplateRankEntryAbility(BlueprintAbility blueprintAbility, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, RankEntrySelectionVM owner, BaseUnitEntity caster, ReactiveProperty<BaseUnitEntity> previewUnit)
		: base(blueprintAbility, null, caster)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Owner = owner;
		m_PreviewUnit = previewUnit;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = base.GetBody(type).ToList();
		if (Prerequisite != null)
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			list.Add(new TooltipBrickPrerequisite(UIUtility.GetPrerequisiteEntries(Prerequisite)));
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type != 0 && Game.Instance.IsControllerMouse && Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature.RankEntryUtils.HasPrerequisiteFooter(Prerequisite, m_Owner))
		{
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.PrerequisitesFooter, TooltipTitleType.H6);
		}
	}

	protected override void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.PreviewUnit>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					EntityFact entityFact = null;
					if (m_PreviewUnit.Value.Facts.Get<Ability>(BlueprintAbility) == null)
					{
						entityFact = m_PreviewUnit.Value.AddFact(BlueprintAbility);
					}
					base.AddDescription(bricks, type);
					if (entityFact != null)
					{
						m_PreviewUnit.Value.Facts.Remove(entityFact);
					}
				}
			}
		}
	}
}
