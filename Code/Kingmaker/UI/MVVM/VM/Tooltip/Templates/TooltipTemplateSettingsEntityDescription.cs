using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSettingsEntityDescription : TooltipBaseTemplate
{
	private readonly IUISettingsEntityBase m_SettingsEntity;

	private readonly string m_OwnTitle;

	private readonly string m_OwnDescription;

	public TooltipTemplateSettingsEntityDescription(IUISettingsEntityBase entity, string groupTitle = null, string ownDescription = null)
	{
		m_SettingsEntity = entity;
		m_OwnTitle = groupTitle;
		m_OwnDescription = ownDescription;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickTitle((m_SettingsEntity != null) ? ((string)m_SettingsEntity.Description) : ((!string.IsNullOrWhiteSpace(m_OwnTitle)) ? m_OwnTitle : string.Empty), TooltipTitleType.H1),
			new TooltipBrickText(m_OwnDescription ?? ((m_SettingsEntity != null) ? ((string)m_SettingsEntity.TooltipDescription) : string.Empty))
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_SettingsEntity == null)
		{
			return list;
		}
		List<BlueprintEncyclopediaPageReference> encyclopediaDescription = m_SettingsEntity.EncyclopediaDescription;
		if (!encyclopediaDescription.Any())
		{
			return list;
		}
		foreach (BlueprintEncyclopediaPage item in from page in encyclopediaDescription
			select page?.Get() into blPage
			where blPage != null
			select blPage)
		{
			list.Add(new TooltipBrickTitle(item.GetTitle()));
			list.Add(new TooltipBrickText(UIUtilityCreateEncyclopediaTooltipDescription.CreateSettingsTooltipDescription(item)));
		}
		return list;
	}
}
