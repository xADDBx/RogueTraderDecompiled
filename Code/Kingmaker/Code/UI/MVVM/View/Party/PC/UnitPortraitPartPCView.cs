using Kingmaker.Code.UI.MVVM.VM.Party;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitPortraitPartPCView : ViewBase<UnitPortraitPartVM>
{
	[SerializeField]
	private Image m_DeadPortrait;

	[SerializeField]
	private Image m_LifePortrait;

	[SerializeField]
	private Image m_Cripple;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_DeadPortrait.sprite = value;
			m_LifePortrait.sprite = value;
		}));
		AddDisposable(base.ViewModel.IsDead.Subscribe(delegate(bool value)
		{
			m_DeadPortrait.gameObject.SetActive(value);
			m_LifePortrait.gameObject.SetActive(!value);
		}));
		AddDisposable(base.ViewModel.IsCrippled.Subscribe(delegate(bool value)
		{
			m_Cripple.gameObject.SetActive(value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
