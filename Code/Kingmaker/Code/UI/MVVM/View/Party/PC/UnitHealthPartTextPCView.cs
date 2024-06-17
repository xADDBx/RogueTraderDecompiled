using Kingmaker.Code.UI.MVVM.VM.Party;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitHealthPartTextPCView : ViewBase<UnitHealthPartVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Color m_DeathColor;

	[SerializeField]
	private Color m_NormalColor;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.HpText.Subscribe(delegate(string hp)
		{
			m_Label.text = hp;
			m_Label.color = (base.ViewModel.IsDead.Value ? m_DeathColor : m_NormalColor);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
