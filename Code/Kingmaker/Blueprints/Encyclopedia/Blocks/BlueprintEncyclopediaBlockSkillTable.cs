using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Encyclopedia.Blocks;

public class BlueprintEncyclopediaBlockSkillTable : BlueprintEncyclopediaBlock, IBlockText, IBlockSkillTable, IBlock
{
	[SerializeField]
	[FormerlySerializedAs("Class")]
	private BlueprintCharacterClassReference m_Class;

	public BlueprintCharacterClass Class => m_Class?.Get();

	public string GetText()
	{
		return Class.Description;
	}

	public override string ToString()
	{
		return "Skill Table";
	}
}
