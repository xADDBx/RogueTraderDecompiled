using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker;

[ExecuteInEditMode]
public class PropertyBlockSplineNurgl : MonoBehaviour
{
	private MaterialPropertyBlock _propBlock;

	private Renderer _renderer;

	private float mbulgeWidth;

	[Header("Width of Bulge")]
	[Range(0f, 30f)]
	public float bulgeWidth = 10f;

	private float mBuldgePower;

	[Header("Buldge power")]
	[Range(0f, 1f)]
	public float BuldgePower = 0.1f;

	private float mTimeOffset;

	[Header("TimeOffset")]
	[Range(0f, 3f)]
	public float TimeOffset;

	[Header("TileX")]
	public float TileX = 1f;

	[Header("TileY")]
	public float TileY = 1f;

	[Header("Speed")]
	[Range(0f, 10f)]
	public float Speed = 5f;

	[Header("Invert")]
	public bool InvertBool;

	private void Start()
	{
		_propBlock = new MaterialPropertyBlock();
		_renderer = GetComponent<Renderer>();
		mbulgeWidth = Shader.PropertyToID("_BulgeWidth");
		mBuldgePower = Shader.PropertyToID("_BuldgePower");
		mTimeOffset = Shader.PropertyToID("_TimeOffset");
		if (_propBlock != null)
		{
			_renderer.GetPropertyBlock(_propBlock);
			_propBlock.SetFloat(ShaderProps._BulgeWidth, bulgeWidth);
			_propBlock.SetFloat(ShaderProps._BuldgePower, BuldgePower);
			_propBlock.SetFloat(ShaderProps._TimeOffset, TimeOffset);
			_propBlock.SetFloat(ShaderProps._TileX, TileX);
			_propBlock.SetFloat(ShaderProps._TileY, TileY);
			_propBlock.SetFloat(ShaderProps._Speed, Speed);
			_propBlock.SetFloat(ShaderProps._Invert, InvertBool ? 1f : 0f);
			_renderer.SetPropertyBlock(_propBlock);
		}
	}
}
