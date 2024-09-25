using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base.DlcList;
using Kingmaker.UI.MVVM.VM.NetLobby.DlcList;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC.DlcList;

public class NetLobbyDlcListPCView : NetLobbyDlcListBaseView
{
	[SerializeField]
	private NetLobbyDlcListDlcEntityPCView m_DlcEntityPCViewPrefab;

	[SerializeField]
	private OwlcatButton m_DlcListButton;

	[SerializeField]
	private TextMeshProUGUI m_DlcListButtonButtonText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DlcListButtonButtonText.text = UIStrings.Instance.CommonTexts.CloseWindow;
		AddDisposable(m_DlcListButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.CloseWindow();
		}));
	}

	protected override void DrawDlcsImpl()
	{
		base.DrawDlcsImpl();
		NetLobbyDlcListDlcEntityVM[] array = base.ViewModel.Dlcs.ToArray();
		if (array.Any())
		{
			m_DlcsWidgetList.DrawEntries(array, m_DlcEntityPCViewPrefab);
		}
	}
}
