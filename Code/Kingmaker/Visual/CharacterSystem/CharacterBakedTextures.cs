using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(menuName = "Character System/Baked Textures")]
public class CharacterBakedTextures : ScriptableObject
{
	public List<Texture2D> Albedo = new List<Texture2D>();

	public List<Texture2D> ColorRamps = new List<Texture2D>();

	public List<Texture2D> BakedTextures = new List<Texture2D>();
}
