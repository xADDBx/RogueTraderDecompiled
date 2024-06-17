using System;
using Kingmaker.Settings.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public abstract class UISettingsEntitySlider<TValue> : UISettingsEntityWithValueBase<TValue>, IUISettingsEntitySlider, IUISettingsEntityWithValueBase, IUISettingsEntityBase
{
	[SerializeField]
	[FormerlySerializedAs("ShowValueText")]
	private bool m_ShowValueText;

	[SerializeField]
	[FormerlySerializedAs("MinValue")]
	private float m_MinValue;

	[SerializeField]
	[FormerlySerializedAs("MaxValue")]
	private float m_MaxValue;

	[SerializeField]
	[FormerlySerializedAs("IsPercentage")]
	private bool m_IsPercentage;

	[SerializeField]
	[FormerlySerializedAs("ChangeDirection")]
	private bool m_ChangeDirection;

	public abstract bool IsInt { get; }

	public abstract float Step { get; }

	public bool ShowValueText => m_ShowValueText;

	public abstract int DecimalPlaces { get; }

	public float MinValue => m_MinValue;

	public float MaxValue => m_MaxValue;

	public bool IsPercentage => m_IsPercentage;

	public bool ChangeDirection => m_ChangeDirection;

	public override SettingsListItemType? Type => SettingsListItemType.Slider;

	public event Action<float> OnTempFloatValueChanged;

	public override void LinkSetting(SettingsEntity<TValue> setting)
	{
		base.LinkSetting(setting);
		Setting.OnTempValueChanged += delegate
		{
			this.OnTempFloatValueChanged?.Invoke(GetFloatTempValue());
		};
	}

	public abstract float GetFloatTempValue();

	public abstract void SetFloatTempValue(float value);
}
