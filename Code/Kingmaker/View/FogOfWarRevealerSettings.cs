using Kingmaker.Controllers;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.View;

public class FogOfWarRevealerSettings : UpdateableBehaviour
{
	private FogOfWarRevealer m_Revealer = new FogOfWarRevealer();

	public Texture2D MaskTexture;

	public float Radius;

	[Tooltip("Use Radius == GameConsts.FogOfWarVisionRadius (11.7m)")]
	public bool DefaultRadius;

	public bool RevealOnStart;

	public bool RevealAlways;

	[HideInInspector]
	public bool RevealManual;

	public Vector3 CachedPosition { get; private set; }

	public float CachedRadius { get; private set; }

	public FogOfWarRevealer Revealer => m_Revealer;

	public void Enable()
	{
		base.enabled = true;
	}

	public void Disable()
	{
		base.enabled = false;
	}

	private void Start()
	{
		if (RevealOnStart || RevealAlways)
		{
			FogOfWarControllerData.AddRevealer(base.transform);
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		UpdateRevealer();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		if (!RevealAlways)
		{
			FogOfWarControllerData.RemoveRevealer(base.transform);
		}
		m_Revealer?.OnDisabled();
	}

	private void OnDestroy()
	{
		if (RevealAlways)
		{
			FogOfWarControllerData.RemoveRevealer(base.transform);
		}
		if (m_Revealer != null)
		{
			m_Revealer.Dispose();
		}
	}

	public override void DoUpdate()
	{
		UpdateRevealer();
	}

	public static Vector2 CalculateHeightMinMax(float y)
	{
		FogOfWarSettings instance = FogOfWarSettings.Instance;
		if (instance == null)
		{
			return Vector2.zero;
		}
		float num = y + instance.ShadowCullingHeightOffset;
		return new Vector2(num, num + instance.ShadowCullingHeight);
	}

	private void UpdateRevealer()
	{
		FogOfWarSettings instance = FogOfWarSettings.Instance;
		if (instance != null)
		{
			Vector3 position = base.transform.position;
			m_Revealer.MaskTexture = MaskTexture;
			m_Revealer.Position = position;
			m_Revealer.Rotation = base.transform.rotation.eulerAngles.y;
			Vector3 lossyScale = base.transform.lossyScale;
			m_Revealer.Scale = new Vector2(lossyScale.x * 0.5f, lossyScale.z * 0.5f);
			m_Revealer.HeightMinMax = CalculateHeightMinMax(position.y);
			m_Revealer.MaskTexture = MaskTexture;
			m_Revealer.Radius = instance.RevealerInnerRadius;
			if (DefaultRadius)
			{
				m_Revealer.Range = instance.RevealerOutterRadius;
			}
			else
			{
				m_Revealer.Range = Radius + instance.BorderWidth;
			}
			m_Revealer.Range += instance.BorderOffset;
		}
	}

	public void UpdateRadiusAndPosition()
	{
		Transform transform = base.transform;
		CachedPosition = transform.position;
		CachedRadius = (DefaultRadius ? 22f : ((Radius < 0.01f) ? (transform.localScale.x / 2f) : Radius));
	}
}
