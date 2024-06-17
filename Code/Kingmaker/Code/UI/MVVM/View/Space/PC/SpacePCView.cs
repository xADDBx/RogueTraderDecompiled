using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class SpacePCView : SpaceBaseView
{
	[SerializeField]
	private SpaceStaticPartPCView m_StaticPartPCView;

	[SerializeField]
	private SpaceDynamicPartPCView m_DynamicPartPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_StaticPartPCView.Initialize();
		m_DynamicPartPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_StaticPartPCView.Bind(base.ViewModel.StaticPartVM);
		m_DynamicPartPCView.Bind(base.ViewModel.DynamicPartVM);
		base.BindViewImplementation();
	}
}
