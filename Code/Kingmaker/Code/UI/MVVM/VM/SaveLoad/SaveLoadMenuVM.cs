using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveLoadMenuVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly SelectionGroupRadioVM<SaveLoadMenuEntityVM> SelectionGroup;

	private List<SaveLoadMenuEntityVM> m_EntitiesList;

	private readonly ReactiveProperty<SaveLoadMenuEntityVM> m_CurrentEntity = new ReactiveProperty<SaveLoadMenuEntityVM>();

	private readonly IReactiveProperty<SaveLoadMode> m_CurrentMode;

	private readonly List<SaveLoadMode> m_ModeList;

	public readonly ReactiveProperty<bool> HasFewEntities = new ReactiveProperty<bool>();

	public SaveLoadMenuVM(IReactiveProperty<SaveLoadMode> currentMode, List<SaveLoadMode> modeList)
	{
		m_CurrentMode = currentMode;
		m_ModeList = modeList;
		CreateEntities();
		AddDisposable(SelectionGroup = new SelectionGroupRadioVM<SaveLoadMenuEntityVM>(m_EntitiesList, m_CurrentEntity));
		AddDisposable(m_CurrentEntity.Subscribe(delegate(SaveLoadMenuEntityVM e)
		{
			m_CurrentMode.Value = e?.Mode ?? SaveLoadMode.Load;
		}));
	}

	private void CreateEntities()
	{
		m_EntitiesList = new List<SaveLoadMenuEntityVM>();
		foreach (SaveLoadMode mode in m_ModeList)
		{
			SaveLoadMenuEntityVM saveLoadMenuEntityVM = new SaveLoadMenuEntityVM(mode);
			AddDisposable(saveLoadMenuEntityVM);
			m_EntitiesList.Add(saveLoadMenuEntityVM);
			if (mode == m_CurrentMode.Value)
			{
				m_CurrentEntity.Value = saveLoadMenuEntityVM;
			}
		}
		HasFewEntities.Value = m_EntitiesList.Count > 1;
	}

	protected override void DisposeImplementation()
	{
	}
}
