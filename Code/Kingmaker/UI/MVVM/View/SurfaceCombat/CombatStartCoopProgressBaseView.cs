using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.SurfaceCombat;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SurfaceCombat;

public class CombatStartCoopProgressBaseView : ViewBase<CombatStartCoopProgressVM>
{
	[SerializeField]
	private List<CombatStartCoopProgressBaseItemView> m_Items;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsActive.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.TotalProgress.Subscribe(delegate(int value)
		{
			for (int j = 0; j < m_Items.Count; j++)
			{
				m_Items[j].gameObject.SetActive(j < value);
			}
		}));
		AddDisposable(base.ViewModel.CurrentProgress.Subscribe(delegate(int value)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				m_Items[i].SetActive(i < value);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
