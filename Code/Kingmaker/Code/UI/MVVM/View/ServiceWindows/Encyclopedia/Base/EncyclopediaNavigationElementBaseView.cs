using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaNavigationElementBaseView : ViewBase<EncyclopediaNavigationElementVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private Image m_UncommitedPlanetsBigIcon;

	[SerializeField]
	private Image m_UncommitedPlanetsLittleIcon;

	[SerializeField]
	private bool m_isBigImage;

	[SerializeField]
	private Sprite m_PantographUncommittedIcon;

	public MonoBehaviour MonoBehaviour => this;

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	private PantographConfig PantographConfig { get; set; }

	protected override void BindViewImplementation()
	{
		UISounds.Instance.SetHoverSound(m_MultiButton, Game.Instance.IsControllerGamepad ? UISounds.ButtonSoundsEnum.PaperComponentSound : UISounds.ButtonSoundsEnum.NormalSound);
		m_Label.text = base.ViewModel.Title;
		m_Label.alignment = ((base.ViewModel.Title.Length > 1) ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center);
		AddDisposable(base.ViewModel.IsAvailablePage.Subscribe(base.gameObject.SetActive));
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
		AddDisposable(base.ViewModel.IsUncommitedPlanetsLittleIcon.Subscribe(delegate(bool value)
		{
			if (!value || m_isBigImage)
			{
				m_UncommitedPlanetsLittleIcon.gameObject.SetActive(value: false);
			}
			else
			{
				m_UncommitedPlanetsLittleIcon.gameObject.SetActive(value: true);
				AddDisposable(m_UncommitedPlanetsLittleIcon.SetHint(UIStrings.Instance.EncyclopediaTexts.EncyclopediaNeedReportToAdministratumHint));
			}
		}));
		AddDisposable(base.ViewModel.IsUncommitedPlanetsBigIcon.Subscribe(delegate(bool value)
		{
			if (!value || !m_isBigImage)
			{
				m_UncommitedPlanetsBigIcon.gameObject.SetActive(value: false);
			}
			else
			{
				m_UncommitedPlanetsBigIcon.gameObject.SetActive(value: true);
				AddDisposable(m_UncommitedPlanetsBigIcon.SetHint(UIStrings.Instance.EncyclopediaTexts.EncyclopediaNeedReportToAdministratumHint));
			}
		}));
		AddDisposable(m_MultiButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			SelectPage();
		}));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private void OnSelected(bool value)
	{
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		if (value && base.ViewModel.Page is BlueprintEncyclopediaChapter && base.ViewModel.IsAvailablePage.Value)
		{
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((EncyclopediaNavigationElementVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is EncyclopediaNavigationElementVM;
	}

	private void SetupPantographConfig()
	{
		List<Sprite> list = new List<Sprite>();
		if (base.ViewModel.IsUncommitedPlanetsBigIcon.Value)
		{
			list.Add(m_PantographUncommittedIcon);
		}
		PantographConfig = new PantographConfig(base.transform, m_Label.text, list);
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public void SelectPage()
	{
		base.ViewModel.SelectPage();
	}

	public bool HaveChilds()
	{
		if (base.ViewModel.ChildsVM.Count > 0)
		{
			return base.ViewModel.ChildsVM != null;
		}
		return false;
	}
}
