using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorSelectingWindowBaseView : ViewBase<VendorSelectingWindowVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoFactionReputationItemPCView m_FactionReputationItemPCView;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_FadeAnimator.AppearAnimation();
		DrawEntities();
		m_Header.text = UIStrings.Instance.Vendor.ChooseVendorForTrade;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(OnCloseClick));
	}

	protected override void DestroyViewImplementation()
	{
		Close();
	}

	protected void OnCloseClick()
	{
		if (!(Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value || UINetUtility.IsControlMainCharacterWithWarning())
		{
			Close();
			EventBus.RaiseEvent(delegate(IBeginSelectingVendorHandler h)
			{
				h.HandleExitSelectingVendor();
			});
		}
	}

	protected virtual void Close()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			Game.Instance.GameCommandQueue.CloseScreen(IScreenUIHandler.ScreenType.VendorSelecting, (Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value);
		});
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.FactionItems.ToArray(), m_FactionReputationItemPCView);
	}
}
