using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipCrewDollPCView : ViewBase<ShipCrewDollVM>
{
	[SerializeField]
	private RectTransform m_ModulesContainer;

	private Dictionary<ShipModuleType, ShipCrewDollModulePCView> m_ModuleViews;

	public void Initialize()
	{
		m_ModuleViews = new Dictionary<ShipModuleType, ShipCrewDollModulePCView>();
		ShipCrewDollModulePCView[] componentsInChildren = m_ModulesContainer.GetComponentsInChildren<ShipCrewDollModulePCView>();
		foreach (ShipCrewDollModulePCView shipCrewDollModulePCView in componentsInChildren)
		{
			m_ModuleViews[shipCrewDollModulePCView.ModuleType] = shipCrewDollModulePCView;
		}
	}

	protected override void BindViewImplementation()
	{
		foreach (ShipCrewDollModuleVM module in base.ViewModel.Modules)
		{
			m_ModuleViews.TryGetValue(module.ModuleType, out var value);
			value?.Bind(module);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
