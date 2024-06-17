using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public sealed class FilmGrainLookupParameter : VolumeParameter<FilmGrainLookup>
{
	public FilmGrainLookupParameter(FilmGrainLookup value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
