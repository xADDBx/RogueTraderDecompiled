using System;
using Kingmaker.Enums;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class WeaponModelSizeSettings
{
	public float Tiny = 1f;

	public float Small = 1f;

	public float Medium = 1f;

	public float Large = 1f;

	public float Huge = 1f;

	public float Gargantuan = 1f;

	public float Colossal = 1f;

	public float GetCoeff(Size size)
	{
		return size switch
		{
			Size.Tiny => Tiny, 
			Size.Small => Small, 
			Size.Medium => Medium, 
			Size.Large => Large, 
			Size.Huge => Huge, 
			Size.Gargantuan => Gargantuan, 
			Size.Colossal => Colossal, 
			_ => 1f, 
		};
	}
}
