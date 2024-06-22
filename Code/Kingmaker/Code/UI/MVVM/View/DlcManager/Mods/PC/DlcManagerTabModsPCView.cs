using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.PC;

public class DlcManagerTabModsPCView : DlcManagerTabModsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabModsModSelectorPCView m_ModSelectorPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ModSelectorPCView.Bind(base.ViewModel.SelectionGroup);
	}

	protected override void SetBottomButtonsImpl()
	{
		AddDisposable(m_NexusModsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OpenNexusMods();
		}));
		if (base.ViewModel.IsSteam.Value)
		{
			AddDisposable(m_SteamWorkshopButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				OpenSteamWorkshop();
			}));
		}
	}
}
