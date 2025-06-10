using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveLoadMenuSelectorBaseView : ViewBase<SelectionGroupRadioVM<SaveLoadMenuEntityVM>>
{
	[SerializeField]
	private SaveLoadMenuEntityBaseView m_SaveButton;

	[SerializeField]
	private SaveLoadMenuEntityBaseView m_LoadButton;

	[SerializeField]
	private GameObject m_Selector;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_SaveButton.Initialize();
			m_LoadButton.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_SaveButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((SaveLoadMenuEntityVM e) => e.Mode == SaveLoadMode.Save));
		m_LoadButton.Bind(base.ViewModel.EntitiesCollection.FindOrDefault((SaveLoadMenuEntityVM e) => e.Mode == SaveLoadMode.Load));
		AddDisposable(base.ViewModel.SelectedEntity.Subscribe(delegate(SaveLoadMenuEntityVM selectedEntity)
		{
			SaveLoadMenuEntityBaseView selectedButton = ((selectedEntity.Mode == SaveLoadMode.Save) ? m_SaveButton : m_LoadButton);
			AddDisposable(DelayedInvoker.InvokeInFrames(delegate
			{
				AddDisposable(UIUtility.CreateMoveXLensPosition(m_Selector.transform, selectedButton.transform.localPosition.x, 0.55f));
			}, 1));
		}));
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
	}
}
