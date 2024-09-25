using UnityEngine;

namespace Kingmaker.View.Spawners.Projectile;

public class RateBasedTokenGenerator
{
	private float m_Tokens;

	private float m_TokensPerSecondRate;

	public void Tick(float deltaTimeSec)
	{
		m_Tokens += deltaTimeSec * m_TokensPerSecondRate;
	}

	public void ChangeRate(float ratePerSec)
	{
		m_TokensPerSecondRate = Mathf.Max(ratePerSec, 0f);
	}

	public int ConsumeTokens()
	{
		int num = Mathf.FloorToInt(m_Tokens);
		m_Tokens -= num;
		return num;
	}
}
