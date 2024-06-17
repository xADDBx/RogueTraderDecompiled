using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[TypeId("65cf33ed832812849867702df5c642fe")]
public class BlueprintSpellsTable : BlueprintScriptableObject
{
	[NotNull]
	public SpellsLevelEntry[] Levels = new SpellsLevelEntry[0];

	public int? GetCount(int classLevel, int spellLevel)
	{
		if (classLevel < 0 || classLevel >= Levels.Length)
		{
			return null;
		}
		SpellsLevelEntry spellsLevelEntry = Levels[classLevel];
		if (spellLevel < 0 || spellLevel >= spellsLevelEntry.Count.Length)
		{
			return null;
		}
		return spellsLevelEntry.Count[spellLevel];
	}
}
