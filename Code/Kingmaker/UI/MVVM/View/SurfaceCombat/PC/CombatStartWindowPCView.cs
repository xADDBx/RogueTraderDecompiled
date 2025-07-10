using Kingmaker.Code.UI.MVVM.View.EtudeCounter.PC;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SurfaceCombat.PC;

public class CombatStartWindowPCView : CombatStartWindowView
{
	[SerializeField]
	private EtudeCounterPCView m_EtudeCounterView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_EtudeCounterView == null)
		{
			m_EtudeCounterView = Object.FindObjectOfType<EtudeCounterPCView>();
		}
		if (m_EtudeCounterView != null)
		{
			m_EtudeCounterView.SetEtudeCounterVisible(value: false);
		}
		AddDisposable(m_StartBattleButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.StartBattle();
			if (m_EtudeCounterView != null)
			{
				m_EtudeCounterView.SetEtudeCounterVisible(value: true);
			}
		}));
	}
}
