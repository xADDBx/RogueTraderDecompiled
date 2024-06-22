using Kingmaker.Blueprints;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.Sound;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventAnswerPCView : BookEventAnswerView, IHasBlueprintInfo
{
	[SerializeField]
	private float m_DefaultFontSize = 18f;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Answer?.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_AnswerText.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (data.button == PointerEventData.InputButton.Left)
			{
				Confirm();
			}
		}));
		AddDisposable(m_AnswerText.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnPointerEnter();
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
			base.ViewModel?.PingAnswerHover(hover: true);
		}));
		AddDisposable(m_AnswerText.OnPointerExitAsObservable().Subscribe(delegate
		{
			base.ViewModel?.PingAnswerHover(hover: false);
		}));
		m_AnswerText.fontSize = m_DefaultFontSize * base.ViewModel.FontSizeMultiplier;
	}
}
