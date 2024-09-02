using System;
using UnityEngine.Sprites;

namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(ParticleSystem))]
[AddComponentMenu("UI/Effects/Extensions/UI Particle System")]
public class UIParticleSystem : MaskableGraphic
{
	public Texture particleTexture;

	public Sprite particleSprite;

	private Transform _transform;

	private ParticleSystem _particleSystem;

	private ParticleSystem.Particle[] _particles;

	private UIVertex[] _quad = new UIVertex[4];

	private Vector4 _uv = Vector4.zero;

	private ParticleSystem.TextureSheetAnimationModule _textureSheetAnimation;

	private int _textureSheetAnimationFrames;

	private Vector2 _textureSheedAnimationFrameSize;

	public override Texture mainTexture
	{
		get
		{
			if ((bool)particleTexture)
			{
				return particleTexture;
			}
			if ((bool)particleSprite)
			{
				return particleSprite.texture;
			}
			return null;
		}
	}

	protected bool Initialize()
	{
		if (_transform == null)
		{
			_transform = base.transform;
		}
		if (_particleSystem == null)
		{
			_particleSystem = GetComponent<ParticleSystem>();
			if (_particleSystem == null)
			{
				return false;
			}
			ParticleSystemRenderer particleSystemRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
			if (particleSystemRenderer == null)
			{
				particleSystemRenderer = _particleSystem.gameObject.AddComponent<ParticleSystemRenderer>();
			}
			Material sharedMaterial = particleSystemRenderer.sharedMaterial;
			if ((bool)sharedMaterial && sharedMaterial.HasProperty("_MainTex"))
			{
				particleTexture = sharedMaterial.mainTexture;
			}
			Material material = new Material(Shader.Find("UI/Particles/Hidden"));
			if (Application.isPlaying)
			{
				particleSystemRenderer.material = material;
			}
			ParticleSystem.MainModule main = _particleSystem.main;
			main.scalingMode = ParticleSystemScalingMode.Hierarchy;
			_particles = null;
		}
		if (_particles == null)
		{
			_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
		}
		if ((bool)particleTexture)
		{
			_uv = new Vector4(0f, 0f, 1f, 1f);
		}
		else if ((bool)particleSprite)
		{
			_uv = DataUtility.GetOuterUV(particleSprite);
		}
		_textureSheetAnimation = _particleSystem.textureSheetAnimation;
		_textureSheetAnimationFrames = 0;
		_textureSheedAnimationFrameSize = Vector2.zero;
		if (_textureSheetAnimation.enabled)
		{
			_textureSheetAnimationFrames = _textureSheetAnimation.numTilesX * _textureSheetAnimation.numTilesY;
			_textureSheedAnimationFrameSize = new Vector2(1f / (float)_textureSheetAnimation.numTilesX, 1f / (float)_textureSheetAnimation.numTilesY);
		}
		return true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (!Initialize())
		{
			base.enabled = false;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		int particles = _particleSystem.GetParticles(_particles);
		for (int i = 0; i < particles; i++)
		{
			ParticleSystem.Particle particle = _particles[i];
			Vector2 vector = ((_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local) ? particle.position : _transform.InverseTransformPoint(particle.position));
			float num = (0f - particle.rotation) * (MathF.PI / 180f);
			float f = num + MathF.PI / 2f;
			Color32 currentColor = particle.GetCurrentColor(_particleSystem);
			float num2 = particle.GetCurrentSize(_particleSystem) * 0.5f;
			if (_particleSystem.main.scalingMode == ParticleSystemScalingMode.Shape)
			{
				vector /= base.canvas.scaleFactor;
			}
			Vector4 uv = _uv;
			if (_textureSheetAnimation.enabled)
			{
				float num3 = 1f - particle.remainingLifetime / particle.startLifetime;
				num3 = Mathf.Repeat(num3 * (float)_textureSheetAnimation.cycleCount, 1f);
				int num4 = 0;
				switch (_textureSheetAnimation.animation)
				{
				case ParticleSystemAnimationType.WholeSheet:
					num4 = Mathf.FloorToInt(num3 * (float)_textureSheetAnimationFrames);
					break;
				case ParticleSystemAnimationType.SingleRow:
				{
					num4 = Mathf.FloorToInt(num3 * (float)_textureSheetAnimation.numTilesX);
					int rowIndex = _textureSheetAnimation.rowIndex;
					num4 += rowIndex * _textureSheetAnimation.numTilesX;
					break;
				}
				}
				num4 %= _textureSheetAnimationFrames;
				uv.x = (float)(num4 % _textureSheetAnimation.numTilesX) * _textureSheedAnimationFrameSize.x;
				uv.y = (float)Mathf.FloorToInt(num4 / _textureSheetAnimation.numTilesX) * _textureSheedAnimationFrameSize.y;
				uv.z = uv.x + _textureSheedAnimationFrameSize.x;
				uv.w = uv.y + _textureSheedAnimationFrameSize.y;
			}
			_quad[0] = UIVertex.simpleVert;
			_quad[0].color = currentColor;
			_quad[0].uv0 = new Vector2(uv.x, uv.y);
			_quad[1] = UIVertex.simpleVert;
			_quad[1].color = currentColor;
			_quad[1].uv0 = new Vector2(uv.x, uv.w);
			_quad[2] = UIVertex.simpleVert;
			_quad[2].color = currentColor;
			_quad[2].uv0 = new Vector2(uv.z, uv.w);
			_quad[3] = UIVertex.simpleVert;
			_quad[3].color = currentColor;
			_quad[3].uv0 = new Vector2(uv.z, uv.y);
			if (num == 0f)
			{
				Vector2 vector2 = new Vector2(vector.x - num2, vector.y - num2);
				Vector2 vector3 = new Vector2(vector.x + num2, vector.y + num2);
				_quad[0].position = new Vector2(vector2.x, vector2.y);
				_quad[1].position = new Vector2(vector2.x, vector3.y);
				_quad[2].position = new Vector2(vector3.x, vector3.y);
				_quad[3].position = new Vector2(vector3.x, vector2.y);
			}
			else
			{
				Vector2 vector4 = new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * num2;
				Vector2 vector5 = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num2;
				_quad[0].position = vector - vector4 - vector5;
				_quad[1].position = vector - vector4 + vector5;
				_quad[2].position = vector + vector4 + vector5;
				_quad[3].position = vector + vector4 - vector5;
			}
			vh.AddUIVertexQuad(_quad);
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			_particleSystem.Simulate(Time.unscaledDeltaTime, withChildren: false, restart: false);
			SetAllDirty();
		}
	}
}
