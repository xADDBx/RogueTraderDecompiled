using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class BakedCharacter : ScriptableObject
{
	[Serializable]
	public class RendererDescription
	{
		public string Name;

		public string[] Bones;

		public string RootBone;

		public Mesh Mesh;

		public Material Material;

		public List<CharacterTextureDescription> Textures = new List<CharacterTextureDescription>();
	}

	public List<RendererDescription> RendererDescriptions = new List<RendererDescription>();
}
