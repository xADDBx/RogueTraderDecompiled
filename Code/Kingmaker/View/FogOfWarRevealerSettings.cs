using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.View;

[KnowledgeDatabaseID("f2c3a550a6cdcc24aa46cc6f188beac8")]
public class FogOfWarRevealerSettings : MonoBehaviour, IUpdatable
{
	private FogOfWarRevealer m_Revealer = new FogOfWarRevealer();

	[CanBeNull]
	private EntityViewBase m_EntityView;

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

	public Vector3 Position
	{
		get
		{
			if (!(m_EntityView != null) || m_EntityView.Data == null)
			{
				return base.transform.position;
			}
			return m_EntityView.Data.Position;
		}
	}

	public float Orientation
	{
		get
		{
			if (!(m_EntityView != null) || !(m_EntityView.Data is MechanicEntity mechanicEntity))
			{
				return base.transform.rotation.eulerAngles.y;
			}
			return mechanicEntity.Orientation;
		}
	}

	public Vector3 Scale => base.transform.lossyScale;

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

	private void OnEnable()
	{
		Game.Instance?.CustomUpdateController.Add(this);
		m_EntityView = GetComponentInChildren<EntityViewBase>();
		UpdateRevealer();
	}

	private void OnDisable()
	{
		try
		{
			if (!RevealAlways)
			{
				FogOfWarControllerData.RemoveRevealer(base.transform);
			}
			m_Revealer?.OnDisabled();
		}
		finally
		{
			Game.Instance?.CustomUpdateController.Remove(this);
		}
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

	void IUpdatable.Tick(float delta)
	{
		UpdateRevealer();
	}

	private void UpdateRevealer()
	{
		FogOfWarSettings instance = FogOfWarSettings.Instance;
		if (instance != null)
		{
			Vector3 position = Position;
			m_Revealer.MaskTexture = MaskTexture;
			m_Revealer.Position = position;
			m_Revealer.Rotation = Orientation;
			Vector3 scale = Scale;
			m_Revealer.Scale = new Vector2(scale.x * 0.5f, scale.z * 0.5f);
			m_Revealer.HeightMinMax = instance.CalculateHeightMinMax(position.y);
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
