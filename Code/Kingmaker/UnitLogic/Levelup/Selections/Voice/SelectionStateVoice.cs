using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Visual.Sound;

namespace Kingmaker.UnitLogic.Levelup.Selections.Voice;

public class SelectionStateVoice : SelectionState
{
	private BlueprintUnitAsksList m_Asks;

	public SelectionStateVoice([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection blueprint, [NotNull] BlueprintPath path, int pathRank)
		: base(manager, blueprint, path, pathRank)
	{
		TrySetDefaultCustomAsks();
	}

	protected override bool IsMadeInternal()
	{
		return true;
	}

	protected override bool IsValidInternal()
	{
		return true;
	}

	protected override bool CanSelectAnyInternal()
	{
		return true;
	}

	public void SelectVoice(BlueprintUnitAsksList asks)
	{
		m_Asks = asks;
		NotifySelectionChanged();
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		if (m_Asks != null)
		{
			unit.Asks.SetCustom(m_Asks);
		}
	}

	protected override void InvalidateInternal()
	{
	}

	private bool TrySetDefaultCustomAsks()
	{
		BaseUnitEntity previewUnit = base.Manager.PreviewUnit;
		if (previewUnit != null && previewUnit.Asks?.List == null)
		{
			BlueprintCharGenRoot charGenRoot = BlueprintRoot.Instance.CharGenRoot;
			int index = ((previewUnit.Gender == Gender.Male) ? charGenRoot.MaleVoiceDefaultId : charGenRoot.FemaleVoiceDefaultId);
			m_Asks = charGenRoot.Voices.ElementAt(index);
			return true;
		}
		return false;
	}
}
