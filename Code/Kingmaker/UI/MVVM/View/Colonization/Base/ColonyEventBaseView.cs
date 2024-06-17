using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyEventBaseView : ViewBase<ColonyEventVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[FormerlySerializedAs("m_Name")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_MechanicString;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected OwlcatButton m_Button;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string val)
		{
			m_NameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.IsColonyManagement.Subscribe(SetMechanicString));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetMechanicString(bool isColonyManagement)
	{
		m_MechanicString.text = (isColonyManagement ? UIStrings.Instance.ColonyEventsTexts.NeedsVisitMechanicString.Text : UIStrings.Instance.ColonyEventsTexts.NeedsResolveMechanicString.Text);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyEventVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyEventVM;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public bool CanInteract()
	{
		return true;
	}

	public bool CanShowTooltip()
	{
		return true;
	}

	public void Interact()
	{
		base.ViewModel.HandleColonyEvent();
	}

	public void ShowTooltip()
	{
		this.ShowTooltip(base.ViewModel.Tooltip.Value);
	}
}
