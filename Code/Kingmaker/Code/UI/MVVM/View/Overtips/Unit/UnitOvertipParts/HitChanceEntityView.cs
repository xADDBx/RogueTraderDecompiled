using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class HitChanceEntityView : ViewBase<HitChanceEntityVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_Index;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		double num = Math.Round(base.ViewModel.Chance, 0);
		m_Text.text = (base.ViewModel.IsLast ? $"{num}%" : $"{num}/");
	}

	public void SetColor(Color color)
	{
		m_Text.color = color;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel viewModel)
	{
		Bind(viewModel as HitChanceEntityVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is HitChanceEntityVM;
	}
}
