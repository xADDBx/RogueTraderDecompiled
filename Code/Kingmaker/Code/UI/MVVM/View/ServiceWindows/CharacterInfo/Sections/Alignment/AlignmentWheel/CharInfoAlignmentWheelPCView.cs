using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Biography;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public class CharInfoAlignmentWheelPCView : CharInfoComponentView<CharInfoAlignmentVM>, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[SerializeField]
	private ConvictionBarBaseView m_ConvictionBar;

	[FormerlySerializedAs("m_FaithGroup")]
	[Header("SoulMarks Groups")]
	[SerializeField]
	private CharInfoSoulMarkSectorView m_FaithSectorView;

	[FormerlySerializedAs("m_CorruptionGroup")]
	[SerializeField]
	private CharInfoSoulMarkSectorView m_CorruptionSectorView;

	[FormerlySerializedAs("m_HopeGroup")]
	[SerializeField]
	private CharInfoSoulMarkSectorView m_HopeSectorView;

	[Header("Background Groups")]
	[SerializeField]
	private GameObject m_MainCharGroup;

	[SerializeField]
	private GameObject m_CompanionGroup;

	[Header("Colors")]
	[SerializeField]
	private Color m_FaithColor;

	[SerializeField]
	private Color m_CorruptionColor;

	[SerializeField]
	private Color m_HopeColor;

	private bool m_OverrideColors;

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	public override void Initialize()
	{
		base.Initialize();
		SetupSectorColors();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsUnitPet.Subscribe(delegate
		{
			m_FadeAnimator.PlayAnimation(!base.ViewModel.IsUnitPet.Value);
		}));
		m_ConvictionBar.Bind(base.ViewModel.ConvictionBar);
		m_FaithSectorView.BindSection(base.ViewModel.FaithSector);
		m_CorruptionSectorView.BindSection(base.ViewModel.CorruptionSector);
		m_HopeSectorView.BindSection(base.ViewModel.HopeSector);
		CreateNavigation();
		base.BindViewImplementation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_FaithSectorView.Unbind();
		m_CorruptionSectorView.Unbind();
		m_HopeSectorView.Unbind();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		RefreshNavigation();
	}

	private void SetupSectorColors()
	{
		if (m_OverrideColors)
		{
			m_FaithSectorView.SetSectorColor(m_FaithColor);
			m_CorruptionSectorView.SetSectorColor(m_CorruptionColor);
			m_HopeSectorView.SetSectorColor(m_HopeColor);
		}
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		m_NavigationBehaviour.Clear();
		if (m_ConvictionBar is ConvictionBarConsoleView convictionBarConsoleView)
		{
			m_NavigationBehaviour.AddEntities(convictionBarConsoleView.GetEntities());
		}
		m_NavigationBehaviour.AddEntities(m_FaithSectorView.GetEntities());
		m_NavigationBehaviour.AddEntities(m_CorruptionSectorView.GetEntities());
		m_NavigationBehaviour.AddEntities(m_HopeSectorView.GetEntities());
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<FloatConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
