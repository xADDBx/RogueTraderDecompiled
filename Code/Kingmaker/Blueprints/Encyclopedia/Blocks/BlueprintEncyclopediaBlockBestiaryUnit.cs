using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Encyclopedia.Blocks;

public class BlueprintEncyclopediaBlockBestiaryUnit : BlueprintEncyclopediaBlock
{
	public Sprite FullImage;

	[SerializeField]
	[FormerlySerializedAs("Unit")]
	private BlueprintUnitReference m_Unit;

	public BlueprintUnit Unit => m_Unit?.Get();

	public override string ToString()
	{
		return "Bestiary Unit";
	}
}
