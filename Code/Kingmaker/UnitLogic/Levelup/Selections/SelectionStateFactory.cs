using System;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.UnitLogic.Levelup.Selections.Ship;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UnitLogic.Levelup.Selections;

public static class SelectionStateFactory
{
	public static SelectionState Create([NotNull] LevelUpManager manager, [NotNull] BlueprintSelection selection, [NotNull] BlueprintPath path, int level)
	{
		if (!(selection is BlueprintSelectionFeature blueprint))
		{
			if (!(selection is BlueprintSelectionDoll blueprint2))
			{
				if (!(selection is BlueprintCharacterNameSelection blueprint3))
				{
					if (!(selection is BlueprintPortraitSelection blueprint4))
					{
						if (!(selection is BlueprintSelectionShip blueprint5))
						{
							if (!(selection is BlueprintVoiceSelection blueprint6))
							{
								if (selection is BlueprintGenderSelection blueprint7)
								{
									return new SelectionStateGender(manager, blueprint7, path, level);
								}
								throw new ArgumentOutOfRangeException(selection.GetType().ToString());
							}
							return new SelectionStateVoice(manager, blueprint6, path, level);
						}
						return new SelectionStateShip(manager, blueprint5, path, level);
					}
					return new SelectionStatePortrait(manager, blueprint4, path, level);
				}
				return new SelectionStateCharacterName(manager, blueprint3, path, level);
			}
			return new SelectionStateDoll(manager, blueprint2, path, level);
		}
		return new SelectionStateFeature(manager, blueprint, path, level);
	}
}
