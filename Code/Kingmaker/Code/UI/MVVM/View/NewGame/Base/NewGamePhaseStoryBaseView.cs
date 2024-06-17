using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGamePhaseStoryBaseView : ViewBase<NewGamePhaseStoryVM>
{
	[Header("Common")]
	[SerializeField]
	private PantographView m_PantographView;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_MainStoryButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MainStoryButtonDescription;

	[SerializeField]
	private TextMeshProUGUI m_OtherModsAreComingSoonLabel;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	public PantographConfig PantographConfig { get; private set; }

	protected override void BindViewImplementation()
	{
		m_MainStoryButtonLabel.text = UIStrings.Instance.NewGameWin.MainStoryLabel;
		m_MainStoryButtonDescription.text = UIStrings.Instance.NewGameWin.MainStoryDescription;
		m_OtherModsAreComingSoonLabel.text = string.Concat("-// ", UIStrings.Instance.NewGameWin.OtherModsAreComingSoon, " //-");
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				ScrollToTop();
				m_PantographView.Show();
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Bind(PantographConfig);
				});
			}
			else
			{
				m_PantographView.Hide();
			}
		}));
		AddDisposable(m_PantographView);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetupPantographConfig()
	{
		List<Sprite> icons = new List<Sprite>();
		PantographConfig = new PantographConfig(m_MainStoryButtonLabel.transform.parent.transform, m_MainStoryButtonLabel.text, icons);
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}
}
