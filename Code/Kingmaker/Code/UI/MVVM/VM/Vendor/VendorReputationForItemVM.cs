using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorReputationForItemVM : VirtualListElementVMBase
{
	public ItemsItemOrigin Type;

	public Sprite TypeIcon;

	public string TypeLabel;

	public int ReputationCost;

	public TooltipTemplateCargo Tooltip;

	public VendorReputationForItemVM(ItemsItemOrigin type, int reputationCost)
	{
		ReputationCost = reputationCost;
		TypeLabel = UIStrings.Instance.CargoTexts.GetLabelByOrigin(type);
		TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(type);
		Tooltip = new TooltipTemplateCargo(type);
	}

	protected override void DisposeImplementation()
	{
	}
}
