using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class EndOfGameView : ViewBase<EndOfGameVM>
{
	[SerializeField]
	private EndOfGameObject m_EndOfGameObject;

	protected override void BindViewImplementation()
	{
		m_EndOfGameObject.Show();
	}

	protected override void DestroyViewImplementation()
	{
		m_EndOfGameObject.Hide();
	}
}
