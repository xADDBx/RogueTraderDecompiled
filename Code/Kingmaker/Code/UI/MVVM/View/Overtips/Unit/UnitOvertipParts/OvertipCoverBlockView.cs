using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.View.Covers;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipCoverBlockView : ViewBase<OvertipCoverBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Image m_Icon;

	[Header("State sprites")]
	[SerializeField]
	private Sprite m_None;

	[SerializeField]
	private Sprite m_Half;

	[SerializeField]
	private Sprite m_Full;

	[SerializeField]
	private Sprite m_Invisible;

	[HideInInspector]
	public BoolReactiveProperty IsVisible = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.NeedCover.CombineLatest(base.ViewModel.CoverType, (bool need, LosCalculations.CoverType coverType) => new { need, coverType }).Subscribe(value =>
		{
			IsVisible.Value = value.need && value.coverType != LosCalculations.CoverType.None;
		}));
		AddDisposable(IsVisible.Subscribe(delegate(bool value)
		{
			m_CanvasGroup.alpha = (value ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.CoverType.Subscribe(delegate(LosCalculations.CoverType value)
		{
			switch (value)
			{
			case LosCalculations.CoverType.None:
				m_Icon.sprite = m_None;
				break;
			case LosCalculations.CoverType.Half:
				m_Icon.sprite = m_Half;
				break;
			case LosCalculations.CoverType.Full:
				m_Icon.sprite = m_Full;
				break;
			case LosCalculations.CoverType.Invisible:
				m_Icon.sprite = m_Invisible;
				break;
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
