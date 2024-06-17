using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public sealed class CharInfoAlignmentAbilitySlotVM : CharInfoComponentVM
{
	public enum SlotState
	{
		Active,
		Inactive,
		Locked
	}

	private readonly BlueprintSoulMark m_BlueprintSoulMark;

	private readonly SoulMarkDirection m_Direction;

	private readonly int m_SoulMarkTier;

	private readonly bool m_Initialized;

	private bool m_IsLocked;

	private SoulMarkDirection? m_MainDirection;

	public TooltipBaseTemplate Tooltip { get; private set; }

	public SlotState CurrentSlotState { get; private set; }

	public CharInfoAlignmentAbilitySlotVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, BlueprintSoulMark soulMark, SoulMarkDirection direction, int soulMarkTier)
		: base(unit)
	{
		m_BlueprintSoulMark = soulMark;
		m_Direction = direction;
		m_SoulMarkTier = soulMarkTier;
		m_Initialized = true;
		RefreshData();
	}

	protected override void RefreshData()
	{
		if (!m_Initialized)
		{
			return;
		}
		base.RefreshData();
		if (m_IsLocked)
		{
			CurrentSlotState = SlotState.Locked;
		}
		else
		{
			EntityFact entityFact = Unit.Value.Facts.List.FirstOrDefault((EntityFact f) => f.Blueprint == m_BlueprintSoulMark);
			CurrentSlotState = ((entityFact == null) ? SlotState.Inactive : SlotState.Active);
		}
		Tooltip = new TooltipTemplateSoulMarkFeature(Unit.Value, m_Direction, m_SoulMarkTier, m_MainDirection);
	}

	public void UpdateMainDirection(SoulMarkDirection? direction)
	{
		m_MainDirection = direction;
		m_IsLocked = direction.HasValue && direction.Value != m_Direction && m_SoulMarkTier > 2;
		RefreshData();
	}

	protected override void DisposeImplementation()
	{
	}
}
