using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.UI.MVVM.View.GroupChanger.PC;

public class GroupChangerPartyCharacterPCView : GroupChangerCharacterBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClick();
		}));
	}

	protected override void SetState(bool isInParty, bool isLock)
	{
		base.gameObject.SetActive(isInParty || isLock);
	}
}
