using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplatePetKeystone : TooltipBaseTemplate
{
	private RankEntrySelectionFeatureVM m_FeatureItemVM;

	private PetOwner m_PetOwnerComponent;

	private PetKeystoneInfoComponent m_PetKeystoneInfoComponent;

	private PetCharscreenUnitComponent m_PetCharscreenUnitComponent;

	private CalculatedPrerequisite m_CalculatedPrerequisite;

	private PrerequisitesList m_PrerequisitesList;

	private BaseUnitEntity m_PetEntityInternal;

	private BaseUnitEntity m_MasterUnit;

	public TooltipTemplatePetKeystone(RankEntrySelectionFeatureVM featureVM, BaseUnitEntity currentUnit)
	{
		m_FeatureItemVM = featureVM;
		m_MasterUnit = currentUnit;
		m_PetKeystoneInfoComponent = featureVM.Feature.GetComponent<PetKeystoneInfoComponent>();
		m_PrerequisitesList = featureVM.Feature.Prerequisites;
		if (m_PetKeystoneInfoComponent != null)
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					using (ContextData<UnitHelper.PreviewUnit>.Request())
					{
						m_PetEntityInternal = m_PetKeystoneInfoComponent.PetUnitReference.Get().CreateEntity();
					}
				}
			}
		}
		m_CalculatedPrerequisite = CalculatedPrerequisite.Calculate(m_PrerequisitesList, m_MasterUnit);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string title = m_FeatureItemVM.Feature.Name;
		if (m_PetKeystoneInfoComponent != null)
		{
			title = m_PetKeystoneInfoComponent.PetInfoTitleName;
		}
		return new List<ITooltipBrick>
		{
			new TooltipBrickTitle(title, TooltipTitleType.H2)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		CheckPrerequisites(list);
		list.Add(new TooltipBrickPetInfo(m_PetKeystoneInfoComponent, m_PetEntityInternal));
		CheckRecommendations(list);
		return list;
	}

	private void CheckPrerequisites(List<ITooltipBrick> result)
	{
		if (!m_PrerequisitesList.Meet(m_MasterUnit))
		{
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Pets.PetPrerequisitesBrickTitle, TooltipTitleType.H7));
			result.Add(new TooltipBrickPrerequisite(UIUtility.GetPrerequisiteEntries(m_CalculatedPrerequisite), oneFromList: true));
			result.Add(new TooltipBrickSpace(2f));
		}
	}

	public override IEnumerable<ITooltipBrick> GetHint(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>();
	}

	private void CheckRecommendations(List<ITooltipBrick> result)
	{
		if (m_PetKeystoneInfoComponent == null || m_PetKeystoneInfoComponent.RecommendedFeatures == null || m_PetKeystoneInfoComponent.RecommendedFeatures.Count == 0)
		{
			return;
		}
		result.Add(new TooltipBrickTitle(UIStrings.Instance.Pets.RavenRecommendationsTitle, TooltipTitleType.H7));
		foreach (PetRecommendedFeature item in m_PetKeystoneInfoComponent.RecommendedFeatures.OrderByDescending((PetRecommendedFeature f) => !f.NotRecommended))
		{
			result.Add(new TooltipBrickRecommendPaper(item.Feature.Get().LocalizedName, item.NotRecommended));
		}
	}
}
