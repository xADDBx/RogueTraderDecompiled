using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(menuName = "Character System/Colors Profile")]
public class CharacterColorsProfile : ScriptableObject
{
	[FormerlySerializedAs("PrimaryRamps")]
	public List<Texture2D> Ramps = new List<Texture2D>();
}
