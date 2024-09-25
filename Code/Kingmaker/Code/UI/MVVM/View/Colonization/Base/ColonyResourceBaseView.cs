using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization.Base;

public class ColonyResourceBaseView : ViewBase<ColonyResourceVM>, IWidgetView
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.BlueprintResource.Subscribe(delegate(BlueprintResource val)
		{
			m_Icon.sprite = val.Icon;
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int val)
		{
			m_Count.text = val.ToString();
		}));
		AddDisposable(m_Icon.SetTooltip(new TooltipTemplateColonyResource(base.ViewModel.BlueprintResource.Value, base.ViewModel.Count.Value)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyResourceVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyResourceVM;
	}
}
