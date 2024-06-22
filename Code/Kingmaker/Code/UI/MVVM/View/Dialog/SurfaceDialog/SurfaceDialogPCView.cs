using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;

[RequireComponent(typeof(DialogColorsConfig))]
public class SurfaceDialogPCView : SurfaceDialogBaseView<DialogAnswerPCView>
{
	[SerializeField]
	private Image m_SpeakerHover;

	[SerializeField]
	private Image m_AnswererHover;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_SpeakerHover.OnPointerEnterAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowHideBigScreenshotSpeaker(state: true);
		}));
		AddDisposable(m_SpeakerHover.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowHideBigScreenshotSpeaker(state: false);
		}));
		AddDisposable(m_AnswererHover.OnPointerEnterAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowHideBigScreenshotAnswerer(state: true);
		}));
		AddDisposable(m_AnswererHover.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowHideBigScreenshotAnswerer(state: false);
		}));
		AddDisposable(base.ViewModel.SpeakerHasPortrait.Subscribe(m_SpeakerHover.transform.parent.gameObject.SetActive));
		AddDisposable(base.ViewModel.AnswererHasPortrait.Subscribe(m_AnswererHover.transform.parent.gameObject.SetActive));
	}

	protected override void OnPartsUpdating()
	{
		TooltipHelper.HideTooltip();
	}
}
