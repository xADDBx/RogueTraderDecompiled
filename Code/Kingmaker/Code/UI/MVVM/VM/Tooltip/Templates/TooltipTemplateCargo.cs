using System.Collections.Generic;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCargo : TooltipBaseTemplate
{
	private readonly string m_Title;

	private readonly string m_Description;

	private readonly string m_TypeLabel;

	private readonly Sprite m_TypeIcon;

	private readonly CargoSlotVM m_CargoSlotVM;

	public TooltipTemplateCargo(BlueprintCargo blueprintCargo)
	{
		if (blueprintCargo != null)
		{
			m_Title = blueprintCargo.Name;
			m_Description = blueprintCargo.Description;
			ItemsItemOrigin originType = blueprintCargo.OriginType;
			m_TypeLabel = UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType);
			m_TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(originType);
		}
	}

	public TooltipTemplateCargo(ItemsItemOrigin origin)
	{
		m_TypeLabel = UIStrings.Instance.CargoTexts.GetLabelByOrigin(origin);
		m_TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(origin);
	}

	public TooltipTemplateCargo(CargoSlotVM cargoSlotVM)
		: this(cargoSlotVM?.CargoEntity?.Blueprint)
	{
		m_CargoSlotVM = cargoSlotVM;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_TypeLabel,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_Title,
			TextParams = new TextFieldParams()
		};
		yield return new TooltipBrickIconPattern(m_TypeIcon, null, titleValues, secondaryValues);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		bool hasDescription = !string.IsNullOrEmpty(m_Description);
		CargoSlotVM cargoSlotVM = m_CargoSlotVM;
		if (cargoSlotVM != null && !cargoSlotVM.IsEmpty)
		{
			yield return new TooltipBricksGroupStart();
			yield return new TooltipBrickCargoCapacity(m_CargoSlotVM);
			yield return new TooltipBricksGroupEnd();
		}
		if (hasDescription)
		{
			yield return new TooltipBrickText(m_Description, TooltipTextType.Paragraph);
		}
	}
}
