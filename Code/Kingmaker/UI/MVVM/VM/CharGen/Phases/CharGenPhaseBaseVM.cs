using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases;

public abstract class CharGenPhaseBaseVM : SelectionGroupEntityVM
{
	public InfoSectionVM InfoVM;

	public InfoSectionVM SecondaryInfoVM;

	public readonly CharGenPhaseType PhaseType;

	public readonly ReactiveProperty<bool> IsCompleted = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReadOnlyReactiveProperty<bool> IsCompletedAndAvailable;

	protected readonly CharGenContext CharGenContext;

	private readonly StringReactiveProperty m_PhaseName = new StringReactiveProperty(string.Empty);

	private readonly BoolReactiveProperty m_IsInDetailedView = new BoolReactiveProperty(initialValue: false);

	[CanBeNull]
	private CharGenPhaseBaseVM m_NextPhase;

	private bool m_DetailedViewBinded;

	public readonly StringReactiveProperty OverrideConfirmHintLabel = new StringReactiveProperty(string.Empty);

	public int OrderPriority { get; }

	public bool HasPantograph { get; protected set; } = true;


	public bool HasPortrait { get; protected set; } = true;


	public bool CanInterruptChargen { get; protected set; }

	public CharacterDollPosition DollPosition { get; protected set; }

	public CharGenDollRoomType DollRoomType { get; protected set; }

	public ReactiveProperty<bool> ShowVisualSettings { get; } = new ReactiveProperty<bool>(initialValue: true);


	public StringReactiveProperty PhaseNextHint { get; } = new StringReactiveProperty(string.Empty);


	public virtual TooltipBaseTemplate NotCompletedReasonTooltip
	{
		get
		{
			if (!IsCompletedAndAvailable.Value)
			{
				return new TooltipTemplateSimple(UIStrings.Instance.CharGen.PhaseNotCompleted);
			}
			return null;
		}
	}

	public IReadOnlyReactiveProperty<bool> IsInDetailedView => m_IsInDetailedView;

	protected CharGenPhaseBaseVM(CharGenContext charGenContext, CharGenPhaseType type)
		: base(allowSwitchOff: false)
	{
		PhaseType = type;
		CharGenContext = charGenContext;
		OrderPriority = (int)type * 1000;
		m_PhaseName.Value = UIStrings.Instance.CharGen.GetPhaseName(type);
		IsCompletedAndAvailable = base.IsAvailable.And(IsCompleted).ToReadOnlyReactiveProperty();
		AddDisposable(IsCompletedAndAvailable.Subscribe(delegate(bool completed)
		{
			m_NextPhase?.UpdateAvailableState(completed);
			if (completed)
			{
				PhaseNextHint.Value = string.Empty;
			}
		}));
	}

	protected abstract bool CheckIsCompleted();

	public void UpdateAvailableState(bool previousIsCompleted)
	{
		SetAvailableState(previousIsCompleted);
		UpdateIsCompleted();
	}

	protected void UpdateIsCompleted()
	{
		IsCompleted.Value = CheckIsCompleted();
	}

	public void BeginDetailedView()
	{
		m_DetailedViewBinded = true;
		m_IsInDetailedView.Value = true;
		OnBeginDetailedView();
		UpdateIsCompleted();
	}

	public void EndDetailedView()
	{
		m_DetailedViewBinded = false;
	}

	public void ResetDetailedViewState()
	{
		m_IsInDetailedView.Value = m_DetailedViewBinded;
	}

	public void SetNextPhase(CharGenPhaseBaseVM phase)
	{
		if (phase != null)
		{
			m_NextPhase = phase;
			m_NextPhase.UpdateAvailableState(IsCompleted.Value);
		}
	}

	public virtual void InterruptChargen(Action onComplete)
	{
	}

	protected abstract void OnBeginDetailedView();

	protected override void DoSelectMe()
	{
	}
}
