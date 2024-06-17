using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public class SpaceCombatTorpedoPanelBaseView : ViewBase<SurfaceCombatUnitVM>
{
	[SerializeField]
	protected SpaceCombatHealthPartTextPCView m_SpaceCombatHealthPartTextPCView;

	[SerializeField]
	protected SpaceCombatHealthPartProgressPCView m_SpaceCombatHealthPartProgressPCView;

	[SerializeField]
	private TextMeshProUGUI m_TorpedoesCount;

	[SerializeField]
	private TextMeshProUGUI m_TorpedoSelfDestructTitle;

	[SerializeField]
	private TextMeshProUGUI m_TorpedoSelfDestructCounter;

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
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetTorpedoesCount()
	{
		int num = 0;
		PartHealth health = base.ViewModel.UnitUIWrapper.Health;
		if (health != null)
		{
			num = health.HitPointsLeft;
		}
		m_TorpedoesCount.text = "x" + num;
	}

	private void SetRoundsLeft()
	{
		int num = 0;
		Buff buff = base.ViewModel.Unit.Buffs?.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedTorpedoesBuff);
		if (buff != null)
		{
			num = buff.Rank;
		}
		m_TorpedoSelfDestructTitle.text = UIStrings.Instance.SpaceCombatTexts.TorpedoSelfDestruct.Text;
		m_TorpedoSelfDestructCounter.text = UIStrings.Instance.TurnBasedTexts.Rounds.Text + ": " + num;
	}
}
