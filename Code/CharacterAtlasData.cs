using System;
using System.Collections.Generic;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAtlas", menuName = "Character System/CharacterAtlasData", order = 1)]
public class CharacterAtlasData : ScriptableObject
{
	public enum m_AtlasResolution
	{
		x64 = 0x40,
		x128 = 0x80,
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400,
		x2048 = 0x800,
		x4096 = 0x1000,
		x8192 = 0x2000
	}

	[Serializable]
	public class BodyPartCoords
	{
		[Serializable]
		public class GpuCoords
		{
			public int x;

			public int y;

			public int z;

			public int w;

			public GpuCoords(int x, int y, int z, int w)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.w = w;
			}
		}

		[SerializeField]
		[LongAsEnum(typeof(BodyPartType))]
		public long bodyPart;

		[SerializeField]
		public RectInt textureRectCoords;

		[SerializeField]
		public Color color = new Color(0f, 1f, 0f, 0.2f);

		[HideInInspector]
		public GpuCoords gpuCoords;
	}

	[SerializeField]
	public m_AtlasResolution targetResolution = m_AtlasResolution.x2048;

	[SerializeField]
	public List<BodyPartCoords> BodyPartsCoords;

	private void OnValidate()
	{
		foreach (BodyPartCoords bodyPartsCoord in BodyPartsCoords)
		{
			if (bodyPartsCoord.textureRectCoords.x < 0)
			{
				bodyPartsCoord.textureRectCoords.x = 0;
			}
			if (bodyPartsCoord.textureRectCoords.y < 0)
			{
				bodyPartsCoord.textureRectCoords.y = 0;
			}
			if (bodyPartsCoord.textureRectCoords.width < 0)
			{
				bodyPartsCoord.textureRectCoords.width = 0;
			}
			if (bodyPartsCoord.textureRectCoords.height < 0)
			{
				bodyPartsCoord.textureRectCoords.height = 0;
			}
			bodyPartsCoord.gpuCoords = new BodyPartCoords.GpuCoords(bodyPartsCoord.textureRectCoords.x, (int)(targetResolution - bodyPartsCoord.textureRectCoords.y - bodyPartsCoord.textureRectCoords.height), bodyPartsCoord.textureRectCoords.x + bodyPartsCoord.textureRectCoords.width, (int)(targetResolution - bodyPartsCoord.textureRectCoords.y));
		}
	}
}
