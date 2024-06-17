using Kingmaker.Code.UI.MVVM.VM.Party;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class SpaceCombatHealthPartTextPCView : ViewBase<UnitHealthPartVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.HpText.Subscribe(delegate(string hp)
		{
			m_Label.text = hp;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
