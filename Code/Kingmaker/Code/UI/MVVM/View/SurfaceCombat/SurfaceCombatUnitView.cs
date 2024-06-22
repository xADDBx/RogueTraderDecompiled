using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class SurfaceCombatUnitView<TCombatUnitVM> : ViewBase<TCombatUnitVM> where TCombatUnitVM : SurfaceCombatUnitVM
{
	[SerializeField]
	protected OwlcatMultiButton Button;

	[Header("PortraitZone")]
	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_CharacetrPortraitZone;

	[Header("Difficulty")]
	[SerializeField]
	private GameObject m_DifficultyBlock;

	[SerializeField]
	private TextMeshProUGUI m_DifficultyText;

	[Header("No Portrait Zone")]
	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_NoPortraitZone;

	[Header("Health")]
	[SerializeField]
	protected UnitHealthPartTextPCView UnitHealthPartTextPCView;

	[SerializeField]
	protected UnitHealthPartProgressPCView UnitHealthPartProgressPCView;

	[Header("Name")]
	[ConditionalShow("HasNameView")]
	[SerializeField]
	protected TextMeshProUGUI m_NameNormal;

	[FormerlySerializedAs("m_UnitBuffPartPCView")]
	[Header("Buffs")]
	[SerializeField]
	protected UnitBuffPartPCView UnitBuffPartPCView;

	[Header("Markers")]
	public bool HasMarkers = true;

	[ConditionalShow("HasMarkers")]
	[SerializeField]
	protected GameObject m_WillLossTurn;

	[ConditionalShow("HasMarkers")]
	[SerializeField]
	protected GameObject m_HasLossControlEffectsIcon;

	[ConditionalShow("HasMarkers")]
	[SerializeField]
	protected GameObject m_UnableToActNormaly;

	private UIUtilityUnit.UnitFractionViewMode m_FractionViewMode;

	private bool m_IsInit;

	[HideInInspector]
	public bool WillBeReused { get; set; }

	public void Initialize()
	{
		if (!m_IsInit)
		{
			if ((bool)m_NameNormal)
			{
				m_NameNormal.text = string.Empty;
			}
			m_CharacetrPortraitZone.Hide();
			m_NoPortraitZone.Hide();
			DoInitialize();
			m_IsInit = true;
		}
	}

	protected abstract void DoInitialize();

	protected override void BindViewImplementation()
	{
		InternalBind();
	}

	protected virtual void InternalBind()
	{
		if (!base.ViewModel.HasUnit)
		{
			return;
		}
		SetupName();
		SetupPortrait();
		SetupDifficulty();
		UnitBuffPartPCView.Bind(base.ViewModel.UnitBuffs);
		if (HasMarkers)
		{
			AddDisposable(base.ViewModel.IsEnemy.Subscribe(delegate
			{
				SetupPortrait();
			}));
			AddDisposable(base.ViewModel.IsUnableToAct.Subscribe(delegate
			{
				SetupUnableToAct();
			}));
			AddDisposable(base.ViewModel.WillNotTakeTurn.Subscribe(delegate
			{
				SetupUnableToAct();
			}));
			AddDisposable(base.ViewModel.HasControlLossEffects.Subscribe(delegate
			{
				SetupUnableToAct();
			}));
		}
		AddDisposable(base.ViewModel.UnitHealthPartVM.Subscribe(delegate(UnitHealthPartVM h)
		{
			UnitHealthPartTextPCView.Bind(h);
			UnitHealthPartProgressPCView.Bind(h);
		}));
		if (Button != null)
		{
			AddDisposable(Button.OnLeftDoubleClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.HandleUnitClick(isDoubleClick: true);
			}));
			AddDisposable(Button.OnSingleLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.HandleUnitClick();
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		if (m_NameNormal != null)
		{
			m_NameNormal.text = string.Empty;
		}
		if (!WillBeReused)
		{
			m_NoPortraitZone.Hide();
			m_CharacetrPortraitZone.Hide();
		}
		UILog.ViewUnbinded("InitiativeTrackerUnitNPCView");
	}

	private void SetupUnableToAct()
	{
		m_WillLossTurn.gameObject.SetActive(base.ViewModel.WillNotTakeTurn.Value);
		m_HasLossControlEffectsIcon.gameObject.SetActive(base.ViewModel.HasControlLossEffects.Value && !base.ViewModel.WillNotTakeTurn.Value);
		m_UnableToActNormaly.gameObject.SetActive(base.ViewModel.IsUnableToAct.Value && !base.ViewModel.WillNotTakeTurn.Value);
	}

	private void SetupPortrait()
	{
		if (base.ViewModel.UsedSubtypeIcon)
		{
			m_CharacetrPortraitZone.Hide();
			m_NoPortraitZone.SetUnit(base.ViewModel.Unit);
		}
		else
		{
			m_CharacetrPortraitZone.SetUnit(base.ViewModel.Unit);
			m_NoPortraitZone.Hide();
		}
		if (base.ViewModel.IsEnemy.Value)
		{
			m_FractionViewMode = UIUtilityUnit.UnitFractionViewMode.Enemy;
		}
		else if (base.ViewModel.IsPlayer.Value)
		{
			m_FractionViewMode = UIUtilityUnit.UnitFractionViewMode.Companion;
		}
		else
		{
			m_FractionViewMode = UIUtilityUnit.UnitFractionViewMode.Friend;
		}
		Button.Or(null)?.SetActiveLayer(m_FractionViewMode.ToString());
	}

	private void SetupDifficulty()
	{
		if (!(m_DifficultyBlock == null))
		{
			AddDisposable(base.ViewModel.ShowDifficulty.Subscribe(m_DifficultyBlock.SetActive));
			m_DifficultyText.text = UIUtility.ArabicToRoman(base.ViewModel.UnitUIWrapper.Difficulty);
		}
	}

	private void SetupName()
	{
		if (m_NameNormal != null)
		{
			m_NameNormal.text = base.ViewModel.DisplayName;
		}
	}
}
