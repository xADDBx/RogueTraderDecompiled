using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class SurfaceCombatCurrentUnitView : SurfaceCombatUnitView<SurfaceCombatUnitVM>
{
	[Header("Action points")]
	[SerializeField]
	private LampSliderPrediction m_BluePrediction;

	[SerializeField]
	private TextMeshProUGUI m_BlueLabel;

	[SerializeField]
	private TextMeshProUGUI m_BlueCostLabel;

	[SerializeField]
	private LampSliderPrediction m_YellowPrediction;

	[SerializeField]
	private TextMeshProUGUI m_YellowLabel;

	[SerializeField]
	private TextMeshProUGUI m_YellowCostLabel;

	[Header("Move animator")]
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[Header("Frame")]
	[SerializeField]
	private Image m_Frame;

	[SerializeField]
	private Sprite m_FriendFrame;

	[SerializeField]
	private Sprite m_EnemyFrame;

	[SerializeField]
	private GameObject m_Aquila;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	protected override void DoInitialize()
	{
		m_MoveAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		if (!base.ViewModel.HasUnit)
		{
			SetActive(state: false);
			return;
		}
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CanBeShowed.Subscribe(SetActive));
		AddDisposable(base.ViewModel.ActionPointVM.Subscribe(delegate(ActionPointsVM points)
		{
			m_Disposable.Clear();
			if (points != null && base.ViewModel.IsPlayer.Value)
			{
				m_BluePrediction.gameObject.SetActive(value: true);
				m_YellowPrediction.gameObject.SetActive(value: true);
				m_Disposable.Add(points.BlueAP.Subscribe(delegate(float v)
				{
					m_BlueLabel.SetText(v.ToString());
				}));
				m_Disposable.Add(points.YellowAP.Subscribe(delegate(float v)
				{
					m_YellowLabel.SetText(v.ToString());
				}));
				m_Disposable.Add(m_BluePrediction.Bind(points.MaxBlueAP, points.BlueAP, points.PredictedBlueAP, isYellowPoints: false));
				m_Disposable.Add(m_YellowPrediction.Bind(points.MaxYellowAP, points.YellowAP, points.PredictedYellowAP, isYellowPoints: true));
				if (m_BlueCostLabel != null)
				{
					m_Disposable.Add(points.PredictedBlueAP.Subscribe(SetBlueCostAP));
				}
				if (m_YellowCostLabel != null)
				{
					m_Disposable.Add(points.PredictedYellowAP.Subscribe(SetYellowCostAP));
				}
			}
			else
			{
				m_BluePrediction.gameObject.SetActive(value: false);
				m_YellowPrediction.gameObject.SetActive(value: false);
				m_BluePrediction.Dispose();
				m_YellowPrediction.Dispose();
			}
		}));
		SetupFrame(base.ViewModel.IsEnemy.Value);
	}

	private void SetupFrame(bool isEnemy)
	{
		m_Frame.sprite = (isEnemy ? m_EnemyFrame : m_FriendFrame);
		m_Aquila.Or(null)?.SetActive(!isEnemy);
	}

	public void SetActive(bool state)
	{
		if (state)
		{
			base.gameObject.SetActive(value: true);
			m_MoveAnimator.AppearAnimation();
		}
		else
		{
			m_MoveAnimator.DisappearAnimation();
		}
	}

	private void SetBlueCostAP(float value)
	{
		float num = base.ViewModel.ActionPointVM.Value.BlueAP.Value - value;
		bool flag = num > 0f;
		m_BlueCostLabel.EnsureComponent<CanvasGroup>().alpha = (flag ? 1f : 0f);
		m_BlueLabel.EnsureComponent<CanvasGroup>().alpha = (flag ? 0f : 1f);
		m_BlueCostLabel.SetText("-" + num);
	}

	private void SetYellowCostAP(float value)
	{
		float num = base.ViewModel.ActionPointVM.Value.YellowAP.Value - value;
		bool flag = num > 0f;
		m_YellowCostLabel.EnsureComponent<CanvasGroup>().alpha = (flag ? 1f : 0f);
		m_YellowLabel.EnsureComponent<CanvasGroup>().alpha = (flag ? 0f : 1f);
		m_YellowCostLabel.SetText("-" + num);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Disposable.Clear();
		m_MoveAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
