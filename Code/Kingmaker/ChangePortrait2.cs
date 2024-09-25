using UnityEngine;

namespace Kingmaker;

public class ChangePortrait2 : MonoBehaviour
{
	public Material matToChange;

	private Renderer rend;

	private MaterialPropertyBlock _propBlock;

	private void Start()
	{
		rend = GetComponent<MeshRenderer>();
		_propBlock = new MaterialPropertyBlock();
	}

	private void OnBecameVisible()
	{
		if (Game.Instance.Player.MainCharacterEntity.Portrait != null)
		{
			Sprite fullLengthPortrait = Game.Instance.Player.MainCharacterEntity.Portrait.FullLengthPortrait;
			_propBlock.SetTexture("_BaseMap", fullLengthPortrait.texture);
			rend.SetPropertyBlock(_propBlock);
		}
	}
}
