using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("1637d939b9b84f66bc941445aa7454db")]
public class CheckRateOfFireGetter : PropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private int m_MinValue;

	[SerializeField]
	private int m_MaxValue;

	public bool ReturnRateOfFire;

	protected override int GetBaseValue()
	{
		int rateOfFire = this.GetAbility().RateOfFire;
		if (ReturnRateOfFire)
		{
			return rateOfFire;
		}
		if ((rateOfFire > m_MaxValue && m_MaxValue >= 1) || rateOfFire < m_MinValue)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (ReturnRateOfFire)
		{
			return "Rate of Fire";
		}
		return "Check if Rate of Fire is in range";
	}
}
