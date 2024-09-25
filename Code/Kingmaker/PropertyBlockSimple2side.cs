using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker;

[ExecuteInEditMode]
public class PropertyBlockSimple2side : MonoBehaviour
{
	private MaterialPropertyBlock _propBlock;

	private Renderer _renderer;

	private float mHeight;

	[Header("Cut Height Bottom")]
	public float mHeight2;

	private float mHeight3;

	[Header("Cut Height Top")]
	public float mHeight4;

	private int _upDown1;

	[Header("Invert")]
	public bool _upDown2;

	[Header("Cut only from Bottom")]
	public bool _OnlyBottom;

	[Header("Use Main Object Height")]
	public bool _bObjectHeight;

	public GameObject _mObject;

	private void Start()
	{
		_propBlock = new MaterialPropertyBlock();
		_renderer = GetComponent<Renderer>();
		mHeight = Shader.PropertyToID("_FloatHeight");
		mHeight3 = Shader.PropertyToID("_FloatHeight2");
		_upDown1 = Shader.PropertyToID("_UpDown");
		if (_propBlock != null)
		{
			_renderer.GetPropertyBlock(_propBlock);
			if (_bObjectHeight)
			{
				_propBlock.SetFloat(ShaderProps._FloatHeight, mHeight2 + _mObject.transform.position.y);
				_propBlock.SetFloat(ShaderProps._FloatHeight2, mHeight4 + _mObject.transform.position.y);
			}
			else
			{
				_propBlock.SetFloat(ShaderProps._FloatHeight, mHeight2);
				_propBlock.SetFloat(ShaderProps._FloatHeight2, mHeight4);
			}
			if (_upDown2)
			{
				_propBlock.SetInt(ShaderProps._UpDown, 1);
			}
			else
			{
				_propBlock.SetInt(ShaderProps._UpDown, 0);
			}
			if (_OnlyBottom)
			{
				_propBlock.SetInt(ShaderProps._OnlyBottom, 1);
			}
			else
			{
				_propBlock.SetInt(ShaderProps._OnlyBottom, 0);
			}
			_renderer.SetPropertyBlock(_propBlock);
		}
	}
}
