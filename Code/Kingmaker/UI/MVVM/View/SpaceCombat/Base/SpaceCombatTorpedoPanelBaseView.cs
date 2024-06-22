using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public class SpaceCombatTorpedoPanelBaseView : ViewBase<SurfaceCombatUnitVM>
{
	[Header("Views")]
	[SerializeField]
	protected SpaceCombatHealthPartTextPCView m_SpaceCombatHealthPartTextPCView;

	[SerializeField]
	protected SpaceCombatHealthPartProgressPCView m_SpaceCombatHealthPartProgressPCView;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_TorpedoesCount;

	[SerializeField]
	private TextMeshProUGUI m_TorpedoSelfDestructTitle;

	[SerializeField]
	private TextMeshProUGUI m_TorpedoSelfDestructCounter;

	[Header("Icons")]
	[SerializeField]
	private Sprite TorpedoesIcon;

	[SerializeField]
	private Sprite StarfightersIcon;

	[SerializeField]
	private Image IconImage;

	private bool IsTorpedoes => base.ViewModel.Unit.Buffs?.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedTorpedoesBuff) != null;

	public void Initialize()
	{
		m_SpaceCombatHealthPartProgressPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitHealthPartVM.Subscribe(delegate(UnitHealthPartVM h)
		{
			m_SpaceCombatHealthPartTextPCView.Bind(h);
			m_SpaceCombatHealthPartProgressPCView.Bind(h);
		}));
		SetTorpedoesCount();
		SetRoundsLeft();
		SetIcon();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetTorpedoesCount()
	{
		int num = ((!IsTorpedoes) ? (base.ViewModel.Unit.Blueprint.GetComponent<StarshipTeamController>()?.UnitsAlive(base.ViewModel.UnitAsBaseUnitEntity) ?? GetCountFromHealth()) : GetCountFromHealth());
		m_TorpedoesCount.text = "x" + num;
	}

	private int GetCountFromHealth()
	{
		return base.ViewModel.UnitUIWrapper.Health?.HitPointsLeft ?? 0;
	}

	private void SetRoundsLeft()
	{
		int num = 0;
		BlueprintBuff blueprint = (IsTorpedoes ? BlueprintRoot.Instance.SystemMechanics.SummonedTorpedoesBuff : BlueprintRoot.Instance.SystemMechanics.SummonedStarfigthersBuff);
		Buff buff = base.ViewModel.Unit.Buffs?.GetBuff(blueprint);
		if (buff != null)
		{
			num = buff.Rank;
		}
		string text = (IsTorpedoes ? UIStrings.Instance.SpaceCombatTexts.TorpedoSelfDestruct.Text : UIStrings.Instance.CargoTexts.Fuel.Text);
		m_TorpedoSelfDestructTitle.text = text;
		m_TorpedoSelfDestructCounter.text = UIStrings.Instance.TurnBasedTexts.Rounds.Text + ": " + num;
	}

	private void SetIcon()
	{
		Sprite sprite = (IsTorpedoes ? TorpedoesIcon : StarfightersIcon);
		IconImage.sprite = sprite;
	}
}
