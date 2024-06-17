namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal interface IAnimationClip
{
	void Sample(in PropertyBlock properties, float time);
}
