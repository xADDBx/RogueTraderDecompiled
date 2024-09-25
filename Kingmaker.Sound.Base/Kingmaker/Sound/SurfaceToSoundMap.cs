using System.Collections.Generic;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker.Sound;

[CreateAssetMenu(fileName = "SurfaceToSoundMap", menuName = "Sound/SurfaceToSoundMap", order = 1)]
public class SurfaceToSoundMap : ScriptableObject
{
	public List<TextureToSoundSurface> TextureToSoundSurfaceMap = new List<TextureToSoundSurface>();

	public Dictionary<Texture2D, SurfaceType> SurfaceSoundMap = new Dictionary<Texture2D, SurfaceType>();

	public void SetSoundType(Texture2D texture, SurfaceType surfaceSound)
	{
		TextureToSoundSurfaceMap.RemoveAll((TextureToSoundSurface x) => x.Texture == texture);
		TextureToSoundSurfaceMap.Add(new TextureToSoundSurface
		{
			Texture = texture,
			SurfaceSoundType = surfaceSound
		});
		UpdateDictionary();
	}

	public void UpdateDictionary()
	{
		SurfaceSoundMap.Clear();
		for (int i = 0; i < TextureToSoundSurfaceMap.Count; i++)
		{
			TextureToSoundSurface textureToSoundSurface = TextureToSoundSurfaceMap[i];
			if (!SurfaceSoundMap.ContainsKey(textureToSoundSurface.Texture))
			{
				SurfaceSoundMap.Add(textureToSoundSurface.Texture, textureToSoundSurface.SurfaceSoundType);
			}
		}
	}
}
