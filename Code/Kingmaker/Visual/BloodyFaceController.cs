using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using UnityEngine;

namespace Kingmaker.Visual;

public class BloodyFaceController : IBloodSettingsHandler, ISubscriber, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitLifeStateChanged, EntitySubscriber>
{
	private const float DISSOLVE_WIDTH = 0.556f;

	private const float BLOOD_ENABLE_THRESHOLD_HP = 0.8f;

	private const float HP_RATIO_DIFFERENCE_THRESHOLD = 0.001f;

	private readonly AbstractUnitEntity m_EntityData;

	private readonly DissolveAnimationController m_DissolveController;

	private readonly Gradient m_BloodGradientAlive;

	private readonly Gradient m_BloodGradientDead;

	private readonly DissolveSettings m_BloodDissolveAnimation;

	private bool m_BloodIsVisibleCache;

	private float m_HpRatioCache = 1f;

	private bool m_IsDeadCache;

	private readonly AnimationCurve m_BloodFadeoutCurve;

	public bool IsDisposed { get; private set; }

	private bool IsBloodAllowed => (SettingsRoot.Game?.Main?.BloodOnCharacters?.GetValue()).GetValueOrDefault();

	public BloodyFaceController(AbstractUnitEntity entityData, DissolveAnimationController dissolveController)
	{
		if (entityData == null)
		{
			throw new NullReferenceException("BloodyFaceController: entityData for constructor should not be null!");
		}
		if (dissolveController == null)
		{
			throw new NullReferenceException("BloodyFaceController: dissolveController for constructor should not be null!");
		}
		m_EntityData = entityData;
		m_DissolveController = dissolveController;
		m_BloodDissolveAnimation = CreateDissolveSettings(m_EntityData);
		m_BloodGradientAlive = GetBloodColorGradient(m_EntityData, isDead: false);
		m_BloodGradientDead = GetBloodColorGradient(m_EntityData, isDead: true);
		m_BloodFadeoutCurve = BlueprintRoot.Instance.HitSystemRoot.GetBloodFadeoutCurve(entityData.SurfaceType);
		EventBus.Subscribe(this);
		IsDisposed = false;
	}

	public void InvalidateAnimationState()
	{
		if (IsBloodAllowed)
		{
			if (!m_DissolveController.Animations.Contains(m_BloodDissolveAnimation))
			{
				m_DissolveController.Animations.Add(m_BloodDissolveAnimation);
				m_DissolveController.InvalidateCache();
			}
		}
		else if (m_DissolveController.Animations.Contains(m_BloodDissolveAnimation))
		{
			m_DissolveController.Animations.Remove(m_BloodDissolveAnimation);
			m_DissolveController.InvalidateCache();
		}
	}

	private static DissolveSettings CreateDissolveSettings(AbstractUnitEntity entityData)
	{
		Texture2D bloodTexture = BlueprintRoot.Instance.HitSystemRoot.GetBloodTexture(entityData.SurfaceType);
		Vector2 bloodTextureTilingSize = BlueprintRoot.Instance.HitSystemRoot.GetBloodTextureTilingSize(entityData.SurfaceType);
		DissolveSettings dissolveSettings = new DissolveSettings();
		dissolveSettings.Texture = bloodTexture;
		dissolveSettings.Delay = 0f;
		dissolveSettings.Lifetime = 1f;
		dissolveSettings.LoopAnimation = true;
		dissolveSettings.TilingScale = bloodTextureTilingSize * GetSizeModifier(entityData.Blueprint);
		dissolveSettings.ColorOverLifetime = GetBloodColorGradient(entityData, isDead: false);
		dissolveSettings.DissolveOverLifetime = new AnimationCurve(new Keyframe(0f, 1f));
		dissolveSettings.DissolveWidthOverLifetime = new AnimationCurve(new Keyframe(0f, 0.556f));
		dissolveSettings.DissolveCutout = false;
		dissolveSettings.DissolveEmission = false;
		dissolveSettings.Layer = -9999;
		dissolveSettings.FadeOut = 1f;
		return dissolveSettings;
	}

	public void UpdateBloodValues(bool force = false)
	{
		if (!IsBloodAllowed)
		{
			return;
		}
		if (m_EntityData.IsDisposed)
		{
			Dispose();
			return;
		}
		PartHealth healthOptional = m_EntityData.GetHealthOptional();
		if (healthOptional != null)
		{
			int hitPointsLeft = healthOptional.HitPointsLeft;
			int maxHitPoints = healthOptional.MaxHitPoints;
			float num = (float)Mathf.Max(hitPointsLeft, 0) / (float)maxHitPoints;
			bool isDead = m_EntityData.IsDead;
			bool flag = IsBloodAllowed && num < 0.8f;
			if (force || m_BloodIsVisibleCache != flag || Math.Abs(num - m_HpRatioCache) > 0.001f || m_IsDeadCache != isDead)
			{
				m_BloodIsVisibleCache = flag;
				m_HpRatioCache = num;
				m_IsDeadCache = isDead;
				float time = 1.25f * num;
				float fadeOut = (m_BloodIsVisibleCache ? m_BloodFadeoutCurve.Evaluate(time) : 1f);
				m_BloodDissolveAnimation.FadeOut = fadeOut;
				m_BloodDissolveAnimation.ColorOverLifetime = (m_IsDeadCache ? m_BloodGradientDead : m_BloodGradientAlive);
			}
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		UpdateBloodValues();
	}

	public void HandleBloodSettingChanged()
	{
		InvalidateAnimationState();
		UpdateBloodValues(force: true);
	}

	private static float GetSizeModifier(BlueprintUnit unit)
	{
		return unit.Size switch
		{
			Size.Tiny => 1f, 
			Size.Small => 1f, 
			Size.Medium => 1f, 
			Size.Large => 0.75f, 
			Size.Huge => 0.6f, 
			Size.Gargantuan => 0.5f, 
			Size.Colossal => 0.5f, 
			_ => 1f, 
		};
	}

	private static Gradient GetBloodColorGradient(AbstractUnitEntity entityData, bool isDead)
	{
		Color bloodTypeColor = BlueprintRoot.Instance.HitSystemRoot.GetBloodTypeColor(entityData.SurfaceType, isDead);
		Gradient gradient = new Gradient();
		gradient.colorKeys = new GradientColorKey[1]
		{
			new GradientColorKey(bloodTypeColor, 0f)
		};
		return gradient;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
		if (m_DissolveController.Animations.Contains(m_BloodDissolveAnimation))
		{
			m_DissolveController.Animations.Remove(m_BloodDissolveAnimation);
			m_DissolveController.InvalidateCache();
		}
		IsDisposed = true;
	}
}
