using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class InventoryDollAdditionalStatsPCView : CharInfoComponentView<InventoryDollAdditionalStatsVM>
{
	[Header("Deflection")]
	[SerializeField]
	private TextMeshProUGUI m_DeflectionLabel;

	[SerializeField]
	private TextMeshProUGUI m_DeflectionValue;

	[SerializeField]
	protected OwlcatMultiButton m_DeflectionTooltip;

	[Header("Absorption")]
	[SerializeField]
	private TextMeshProUGUI m_AbsorptionLabel;

	[SerializeField]
	private TextMeshProUGUI m_AbsorptionValue;

	[SerializeField]
	protected OwlcatMultiButton m_AbsorptionTooltip;

	[Header("Dodge")]
	[SerializeField]
	private TextMeshProUGUI m_DodgeLabel;

	[SerializeField]
	private TextMeshProUGUI m_DodgeValue;

	[SerializeField]
	protected OwlcatMultiButton m_DodgeTooltip;

	[Header("Dodge Penetration")]
	[SerializeField]
	private TextMeshProUGUI m_DodgePenetrationLabel;

	[SerializeField]
	private TextMeshProUGUI m_DodgePenetrationValue;

	[SerializeField]
	protected OwlcatMultiButton m_DodgePenetrationTooltip;

	[Header("Resolve")]
	[SerializeField]
	private TextMeshProUGUI m_ResolveLabel;

	[SerializeField]
	private TextMeshProUGUI m_ResolveValue;

	[SerializeField]
	protected OwlcatMultiButton m_ResolveTooltip;

	[Header("Parry")]
	[SerializeField]
	private TextMeshProUGUI m_ParryLabel;

	[SerializeField]
	private TextMeshProUGUI m_ParryValue;

	[SerializeField]
	protected OwlcatMultiButton m_ParryTooltip;

	private List<OwlcatSelectable> m_StatsBlocks = new List<OwlcatSelectable>();

	public override void Initialize()
	{
		base.Initialize();
		m_StatsBlocks = new List<OwlcatSelectable> { m_DeflectionTooltip, m_DodgeTooltip, m_AbsorptionTooltip, m_ResolveTooltip, m_DodgePenetrationTooltip, m_ParryTooltip };
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetValues();
		SetLabels();
		SetTooltips();
		SetSounds();
	}

	private void SetValues()
	{
		AddDisposable(base.ViewModel.ArmorAbsorption.Subscribe(delegate(string value)
		{
			m_AbsorptionValue.text = value;
		}));
		AddDisposable(base.ViewModel.ArmorDeflection.Subscribe(delegate(string value)
		{
			m_DeflectionValue.text = value;
		}));
		AddDisposable(base.ViewModel.Dodge.Subscribe(delegate(string value)
		{
			m_DodgeValue.text = value;
		}));
		AddDisposable(base.ViewModel.DodgeReduction.Subscribe(delegate(string value)
		{
			m_DodgePenetrationValue.text = value;
		}));
		AddDisposable(base.ViewModel.Resolve.Subscribe(delegate(string value)
		{
			m_ResolveValue.text = value;
		}));
		AddDisposable(base.ViewModel.Parry.Subscribe(delegate(string value)
		{
			m_ParryValue.text = value;
		}));
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_ResolveTooltip.gameObject.SetActive(!u.IsPet);
		}));
	}

	private void SetLabels()
	{
		if ((bool)m_DeflectionLabel)
		{
			m_DeflectionLabel.text = UIStrings.Instance.CharacterSheet.ArmorDeflection;
		}
		if ((bool)m_AbsorptionLabel)
		{
			m_AbsorptionLabel.text = UIStrings.Instance.CharacterSheet.ArmorAbsorption;
		}
		if ((bool)m_DodgeLabel)
		{
			m_DodgeLabel.text = UIStrings.Instance.CharacterSheet.Dodge;
		}
		if ((bool)m_DodgePenetrationLabel)
		{
			m_DodgePenetrationLabel.text = UIStrings.Instance.CharacterSheet.DodgeReduction;
		}
		if ((bool)m_ResolveLabel)
		{
			m_ResolveLabel.text = UIStrings.Instance.CharacterSheet.Resolve;
		}
		if ((bool)m_ParryLabel)
		{
			m_ParryLabel.text = UIStrings.Instance.CombatTexts.Parried;
		}
	}

	private void SetTooltips()
	{
		TooltipConfig config = new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true);
		AddDisposable(m_DeflectionTooltip.SetTooltip(base.ViewModel.DeflectionTooltip));
		AddDisposable(m_DodgeTooltip.SetTooltip(base.ViewModel.DodgeTooltip));
		AddDisposable(m_AbsorptionTooltip.SetTooltip(base.ViewModel.AbsorptionTooltip));
		AddDisposable(m_ResolveTooltip.SetGlossaryTooltip("Resolve", config));
		AddDisposable(m_DodgePenetrationTooltip.SetGlossaryTooltip("DodgeReduction", config));
		AddDisposable(m_ParryTooltip.SetGlossaryTooltip("Parry", config));
	}

	private void SetSounds()
	{
		UISounds.ButtonSoundsEnum hoverSound = (Game.Instance.IsControllerGamepad ? UISounds.ButtonSoundsEnum.PaperComponentSound : UISounds.ButtonSoundsEnum.NoSound);
		m_StatsBlocks.ForEach(delegate(OwlcatSelectable s)
		{
			UISounds.Instance.SetClickSound(s, UISounds.ButtonSoundsEnum.NoSound);
			UISounds.Instance.SetHoverSound(s, hoverSound);
		});
	}
}
