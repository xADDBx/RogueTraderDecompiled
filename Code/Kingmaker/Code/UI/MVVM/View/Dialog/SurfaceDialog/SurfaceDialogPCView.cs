using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;

[RequireComponent(typeof(DialogColorsConfig))]
public class SurfaceDialogPCView : SurfaceDialogBaseView<DialogAnswerPCView>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
	}

	protected override void OnPartsUpdating()
	{
		TooltipHelper.HideTooltip();
	}
}
