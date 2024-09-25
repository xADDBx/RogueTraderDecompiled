using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FeedbackPopup;

public class FeedbackPopupPCView : ViewBase<FeedbackPopupVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[Header("Items")]
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private FeedbackPopupItemPCView m_ItemPCView;

	protected override void BindViewImplementation()
	{
		m_Title.text = UIStrings.Instance.MainMenu.Feedback;
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		DrawEntities();
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Items.ToArray(), m_ItemPCView);
	}
}
