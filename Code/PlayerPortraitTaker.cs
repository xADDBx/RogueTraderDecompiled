using Kingmaker;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

public class PlayerPortraitTaker : MonoBehaviour
{
	public enum PortraitSize
	{
		Small,
		Half,
		Full
	}

	[SerializeField]
	private MeshRenderer m_MeshRenderer;

	public int MatIndex;

	private Sprite m_Portrait;

	public PortraitSize portraitSize;

	public void TakePortret()
	{
		switch (portraitSize)
		{
		case PortraitSize.Small:
			m_Portrait = Game.Instance.Player.MainCharacterEntity.Portrait.SmallPortrait;
			break;
		case PortraitSize.Half:
			m_Portrait = Game.Instance.Player.MainCharacterEntity.Portrait.HalfLengthPortrait;
			break;
		case PortraitSize.Full:
			m_Portrait = Game.Instance.Player.MainCharacterEntity.Portrait.FullLengthPortrait;
			break;
		}
		m_MeshRenderer.materials[MatIndex].SetTexture(ShaderProps._BaseMap, m_Portrait.texture);
	}
}
