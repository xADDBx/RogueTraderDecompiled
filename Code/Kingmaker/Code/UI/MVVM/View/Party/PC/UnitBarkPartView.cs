using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.UI.MVVM.View.Bark.PC;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitBarkPartView : BarkBlockView<UnitBarkPartVM>
{
	[SerializeField]
	private GameObject m_BarkActiveIcon;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsBarkActive.CombineLatest(base.ViewModel.IsUnitOnScreen, (bool barkActive, bool unitOnScreen) => barkActive && !unitOnScreen).Subscribe(delegate(bool value)
		{
			FadeAnimator.PlayAnimation(value);
		}));
		AddDisposable(base.ViewModel.IsBarkActive.Subscribe(delegate(bool value)
		{
			m_BarkActiveIcon.SetActive(value);
		}));
	}
}
