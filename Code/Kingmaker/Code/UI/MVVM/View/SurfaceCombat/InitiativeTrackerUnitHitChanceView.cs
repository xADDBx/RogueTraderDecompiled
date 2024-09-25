using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class InitiativeTrackerUnitHitChanceView : ViewBase<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private bool m_HasAttackParams;

	[SerializeField]
	private GameObject m_AttacksContainer;

	[SerializeField]
	private bool m_HasDefenceParams = true;

	[SerializeField]
	private GameObject m_DefenceContainer;

	[Header("Sum Hit Chance")]
	[SerializeField]
	[ConditionalShow("m_HasAttackParams")]
	private GameObject m_SumChanceBlock;

	[SerializeField]
	[ConditionalShow("m_HasAttackParams")]
	private TextMeshProUGUI m_SumChanceLabel;

	[Header("Burst")]
	[ConditionalShow("m_HasAttackParams")]
	[SerializeField]
	private GameObject m_BurstBlock;

	[SerializeField]
	[ConditionalShow("m_HasAttackParams")]
	private TextMeshProUGUI m_BurstIndexLabel;

	[Header("Hit")]
	[ConditionalShow("m_HasAttackParams")]
	[SerializeField]
	private GameObject m_HitBlock;

	[SerializeField]
	[ConditionalShow("m_HasAttackParams")]
	private TextMeshProUGUI m_HitLabel;

	[Header("Push")]
	[SerializeField]
	[ConditionalShow("m_HasAttackParams")]
	private GameObject m_PushBlock;

	[Header("Dodge")]
	[SerializeField]
	[ConditionalShow("m_HasDefenceParams")]
	private GameObject m_DodgeBlock;

	[SerializeField]
	[ConditionalShow("m_HasDefenceParams")]
	private TextMeshProUGUI m_DodgeLabel;

	[Header("Parry")]
	[ConditionalShow("m_HasDefenceParams")]
	[SerializeField]
	private GameObject m_ParryBlock;

	[SerializeField]
	[ConditionalShow("m_HasDefenceParams")]
	private TextMeshProUGUI m_ParryLabel;

	[Header("Cover")]
	[SerializeField]
	[ConditionalShow("m_HasDefenceParams")]
	private GameObject m_CoverBlock;

	[SerializeField]
	[ConditionalShow("m_HasDefenceParams")]
	private TextMeshProUGUI m_CoverLabel;

	protected override void BindViewImplementation()
	{
		m_HasAttackParams = false;
		m_AttacksContainer.Or(null)?.SetActive(m_HasAttackParams);
		m_DefenceContainer.Or(null)?.SetActive(m_HasDefenceParams);
		AddDisposable(base.ViewModel.IsVisibleTrigger.CombineLatest(base.ViewModel.HasHit, base.ViewModel.IsCaster, (bool isVisible, bool hasHit, bool isCaster) => isVisible && (hasHit || isCaster)).ObserveLastValueOnLateUpdate().Subscribe(base.gameObject.SetActive));
		if (m_HasAttackParams)
		{
			AddDisposable(base.ViewModel.HitChance.Subscribe(delegate(float value)
			{
				m_SumChanceBlock.gameObject.SetActive(base.ViewModel.IsCaster.Value && !base.ViewModel.HitAlways.Value);
				m_SumChanceLabel.text = UIUtilityTexts.GetPercentString(value);
			}));
			AddDisposable(base.ViewModel.BurstIndex.ObserveLastValueOnLateUpdate().Subscribe(delegate(int val)
			{
				m_BurstBlock.gameObject.SetActive(val > 1 && base.ViewModel.IsCaster.Value);
				m_BurstIndexLabel.text = $"{base.ViewModel.BurstIndex.Value}";
			}));
			AddDisposable(base.ViewModel.InitialHitChance.Subscribe(delegate(float value)
			{
				m_HitBlock.gameObject.SetActive(base.ViewModel.IsCaster.Value && !base.ViewModel.HitAlways.Value);
				m_HitLabel.text = UIUtilityTexts.GetPercentString(value);
			}));
			AddDisposable(base.ViewModel.CanPush.ObserveLastValueOnLateUpdate().Subscribe(delegate(bool val)
			{
				m_PushBlock.gameObject.SetActive(val);
			}));
		}
		if (m_HasDefenceParams)
		{
			AddDisposable(base.ViewModel.DodgeChance.ObserveLastValueOnLateUpdate().Subscribe(delegate(float val)
			{
				m_DodgeBlock.gameObject.SetActive(val > 0f);
				m_DodgeLabel.text = UIUtilityTexts.GetPercentString(base.ViewModel.DodgeChance.Value);
			}));
			AddDisposable(base.ViewModel.ParryChance.ObserveLastValueOnLateUpdate().Subscribe(delegate(float val)
			{
				m_ParryBlock.gameObject.SetActive(val > 0f);
				m_ParryLabel.text = UIUtilityTexts.GetPercentString(base.ViewModel.ParryChance.Value);
			}));
			AddDisposable(base.ViewModel.CoverChance.ObserveLastValueOnLateUpdate().Subscribe(delegate(float val)
			{
				m_CoverBlock.gameObject.SetActive(val > 0f);
				m_CoverLabel.text = UIUtilityTexts.GetPercentString(base.ViewModel.CoverChance.Value);
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}
