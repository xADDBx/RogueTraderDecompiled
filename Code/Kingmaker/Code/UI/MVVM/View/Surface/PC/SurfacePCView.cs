using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Surface.PC;

public class SurfacePCView : SurfaceBaseView
{
	[SerializeField]
	private SurfaceStaticPartPCView m_StaticPartPCView;

	[SerializeField]
	private SurfaceDynamicPartPCView m_DynamicPartPCView;

	[SerializeField]
	private SurfaceCombatPartView m_CombatPartPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_StaticPartPCView.Initialize();
		m_DynamicPartPCView.Initialize();
		m_CombatPartPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_StaticPartPCView.Bind(base.ViewModel.StaticPartVM);
		m_DynamicPartPCView.Bind(base.ViewModel.DynamicPartVM);
		m_CombatPartPCView.Bind(base.ViewModel.CombatPartVM);
		base.BindViewImplementation();
	}
}
