using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;

public class DlcManagerTabModsBaseView : ViewBase<DlcManagerTabModsVM>
{
	[Header("Common")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected RectTransform m_BottomBlock;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_InstalledModsHeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_DiscoverModsLabel;

	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveAnyModsLabel;

	[SerializeField]
	protected OwlcatButton m_NexusModsButton;

	[SerializeField]
	private TextMeshProUGUI m_NexusModsLabel;

	[SerializeField]
	protected OwlcatButton m_SteamWorkshopButton;

	[SerializeField]
	private TextMeshProUGUI m_SteamWorkshopLabel;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		ScrollToTop();
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				ScrollToTop();
			}
		}));
		m_InstalledModsHeaderLabel.text = UIStrings.Instance.DlcManager.InstalledMods;
		m_YouDontHaveAnyModsLabel.text = UIStrings.Instance.DlcManager.YouDontHaveAnyMods;
		m_YouDontHaveAnyModsLabel.transform.parent.gameObject.SetActive(!base.ViewModel.HaveMods);
		if (base.ViewModel.SelectionGroup.EntitiesCollection.Any())
		{
			m_InfoView.Bind(base.ViewModel.InfoVM);
			base.ViewModel.SelectedEntity.Value?.ShowDescription(state: true);
		}
		SetBottomButtons();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetBottomButtons()
	{
		m_DiscoverModsLabel.text = UIStrings.Instance.DlcManager.DiscoverMoreMods;
		m_NexusModsLabel.text = UIStrings.Instance.DlcManager.NexusMods;
		m_SteamWorkshopButton.gameObject.SetActive(base.ViewModel.IsSteam.Value);
		if (base.ViewModel.IsSteam.Value)
		{
			m_SteamWorkshopLabel.text = UIStrings.Instance.DlcManager.SteamWorkshop;
		}
		SetBottomButtonsImpl();
	}

	protected virtual void SetBottomButtonsImpl()
	{
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
		m_InfoView.Or(null)?.ScrollRectExtended.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	protected void OpenNexusMods()
	{
		base.ViewModel.OpenNexusMods();
	}

	protected void OpenSteamWorkshop()
	{
		base.ViewModel.OpenSteamWorkshop();
	}
}
