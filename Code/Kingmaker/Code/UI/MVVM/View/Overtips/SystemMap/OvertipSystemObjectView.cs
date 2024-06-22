using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;

public abstract class OvertipSystemObjectView : BaseOvertipView<OvertipEntitySystemObjectVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, ISystemMapOvertip
{
	[SerializeField]
	protected OwlcatButton m_SystemObjectButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_SystemObjectName;

	[Header("States Icons")]
	[SerializeField]
	private RectTransform m_StateIconCanvas;

	[SerializeField]
	private Image m_PoiStateIcon;

	[Header("Overtip Size Control")]
	[SerializeField]
	private RectTransform m_CanvasRectTransform;

	[Header("Noise")]
	[SerializeField]
	private Image m_NoiseAroundImage;

	[SerializeField]
	private Sprite[] m_RandomNoises;

	[Header("Top Information")]
	[SerializeField]
	private TextMeshProUGUI m_TopInformation;

	[Header("Other")]
	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	[SerializeField]
	private List<Image> m_AdditionalTargetPingImages;

	private MapObjectView m_SystemObject;

	private bool m_Visible;

	private IDisposable m_PingDelay;

	protected override bool CheckVisibility => m_Visible;

	public bool Visible => m_Visible;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_TargetPingEntity.CanvasGroup != null)
		{
			m_TargetPingEntity.CanvasGroup.alpha = 0f;
		}
		m_TargetPingEntity.DisappearAnimation();
		m_SystemObject = base.ViewModel.SystemMapObject.View;
		MapObjectEntity data = m_SystemObject.Data;
		m_Visible = (data is ArtificialObjectEntity || data is AsteroidEntity) && m_SystemObject.Data.IsInGame;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ObservableExtensions.Subscribe(m_SystemObjectButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.RequestVisit();
		}));
		SetSystemObjectIconStates();
		AddDisposable(base.ViewModel.IsScanned.Subscribe(delegate
		{
			SetSystemObjectName();
		}));
		SetOvertipSize();
		AddDisposable(base.ViewModel.CoopPingEntity.Subscribe(delegate((NetPlayer player, Entity entity) value)
		{
			PingEntity(value.player, value.entity);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_PingDelay?.Dispose();
		m_PingDelay = null;
	}

	public void SetFocus(bool value)
	{
		m_SystemObjectButton.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.isActiveAndEnabled)
		{
			return base.CanvasGroup.alpha > 0f;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public void UnfocusButton()
	{
		m_SystemObjectButton.OnPointerExit();
	}

	private void SetSystemObjectIconStates()
	{
		AddDisposable(base.ViewModel.IsPoi.Subscribe(delegate(bool state)
		{
			m_StateIconCanvas.gameObject.SetActive(state);
			m_PoiStateIcon.gameObject.SetActive(state);
			if (state)
			{
				string text = string.Join(Environment.NewLine, base.ViewModel.PoiNamesList);
				AddDisposable(m_PoiStateIcon.SetHint(string.Concat(UIStrings.Instance.SystemMap.PoIsDetected, ":\n", text)));
			}
		}));
	}

	public void SetSystemObjectName()
	{
		m_SystemObjectName.text = ((!base.ViewModel.IsScanned.Value) ? "???" : (string.IsNullOrWhiteSpace(base.ViewModel.SystemMapObject.View.Data.Blueprint.Name) ? "Empty Name" : base.ViewModel.SystemMapObject.View.Data.Blueprint.Name));
		m_SystemObjectName.color = Game.Instance.BlueprintRoot.UIConfig.OvertipSystemObjectColors.GetColorByState(GetState());
	}

	private OvertipSystemObjectState GetState()
	{
		StarSystemObjectView component = m_SystemObject.gameObject.GetComponent<StarSystemObjectView>();
		bool flag = component.Data.IsFullyExplored && (component.Data.IsScanned ^ component.Data.Blueprint.IsScannedOnStart);
		if (component.Data.Blueprint.IsQuestObject)
		{
			if (!flag)
			{
				return OvertipSystemObjectState.QuestActive;
			}
			return OvertipSystemObjectState.QuestInactive;
		}
		if (!flag)
		{
			return OvertipSystemObjectState.DefaultActive;
		}
		return OvertipSystemObjectState.DefaultInactive;
	}

	private void SetOvertipSize()
	{
		m_NoiseAroundImage.sprite = m_RandomNoises[UnityEngine.Random.Range(0, m_RandomNoises.Length)];
		m_TopInformation.text = GetTopInformationText();
		float num = UnityEngine.Random.Range(15f, 25f);
		m_TopInformation.transform.parent.transform.rotation *= Quaternion.Euler(0f, 0f, num);
		m_TopInformation.transform.rotation *= Quaternion.Euler(0f, 0f, 0f - num);
		SphereCollider component = m_SystemObject.gameObject.GetComponent<SphereCollider>();
		if (!(component == null))
		{
			Vector3 size = component.bounds.size;
			m_CanvasRectTransform.sizeDelta = new Vector2(size.x * 12f, size.y * 12f);
		}
	}

	private string GetTopInformationText()
	{
		string text = "UNKNOWN";
		if (Game.Instance.CurrentMode != GameModeType.StarSystem || !(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap))
		{
			return text;
		}
		string text2 = ((!(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap blueprintStarSystemMap)) ? text : UIUtility.ArabicToRoman(blueprintStarSystemMap.RomeSystemNumber));
		string objectTypeString = GetObjectTypeString();
		string text3 = UnityEngine.Random.Range(100, 1000).ToString();
		return "Notitium collectionis:\n// RES //\n" + text2 + "-" + objectTypeString + "-" + text3;
	}

	private string GetObjectTypeString()
	{
		Dictionary<Type, string> dictionary = new Dictionary<Type, string>
		{
			{
				typeof(BlueprintAnomalyFact),
				"ALF"
			},
			{
				typeof(BlueprintArtificialObject),
				"LOC"
			},
			{
				typeof(BlueprintAsteroid),
				"LAP"
			},
			{
				typeof(BlueprintCloud),
				"PRA"
			},
			{
				typeof(BlueprintComet),
				"LAP"
			},
			{
				typeof(BlueprintStar),
				"IGN"
			}
		};
		string text = "UNKNOWN";
		Type type = base.ViewModel.SystemMapObject.Blueprint?.GetType();
		string value = text;
		if (type != null && dictionary.TryGetValue(type, out value))
		{
			return value;
		}
		return text;
	}

	private void PingEntity(NetPlayer player, Entity entity)
	{
		m_PingDelay?.Dispose();
		if (entity != base.ViewModel.SystemMapObject)
		{
			m_TargetPingEntity.DisappearAnimation();
			return;
		}
		int index = player.Index - 1;
		Image component = m_TargetPingEntity.GetComponent<Image>();
		Color currentColor = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[index];
		if (component != null)
		{
			component.color = currentColor;
		}
		if (m_AdditionalTargetPingImages != null && m_AdditionalTargetPingImages.Any())
		{
			m_AdditionalTargetPingImages.ForEach(delegate(Image i)
			{
				i.color = currentColor;
			});
		}
		m_TargetPingEntity.AppearAnimation();
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntity.DisappearAnimation();
		}, 7.5f);
	}
}
