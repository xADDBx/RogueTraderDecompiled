using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyCreateJoinPartBaseView : ViewBase<NetLobbyVM>
{
	[Space]
	[SerializeField]
	private CanvasGroup m_ShowHideLobbyIcon;

	[Space]
	[SerializeField]
	protected TMP_InputField m_LobbyCodeInputField;

	[SerializeField]
	private TextMeshProUGUI m_LobbyCodeInputFieldPlaceholder;

	[Space]
	[SerializeField]
	protected OwlcatDropdown m_RegionDropdown;

	[SerializeField]
	private GameObject m_RegionWaiting;

	[SerializeField]
	private TextMeshProUGUI m_RegionHeader;

	[Space]
	[SerializeField]
	private TextMeshProUGUI m_VersionText;

	[SerializeField]
	private TextMeshProUGUI m_VersionHeader;

	[SerializeField]
	private TextMeshProUGUI m_NeedSameRegionAndCoopVerDescription;

	[SerializeField]
	protected OwlcatDropdown m_JoinableUserTypesDropdown;

	[SerializeField]
	private TextMeshProUGUI m_JoinableUserTypesLabel;

	[SerializeField]
	protected OwlcatDropdown m_InvitableUserTypesDropdown;

	[SerializeField]
	private TextMeshProUGUI m_InvitableUserTypesLabel;

	protected readonly BoolReactiveProperty ShowLobbyCode = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty IsInCreateJoinPart = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty ReadyToJoin = new BoolReactiveProperty();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_LobbyCodeInputFieldPlaceholder.text = UIStrings.Instance.NetLobbyTexts.JoinLobbyCodePlaceholder;
		m_VersionHeader.text = UIStrings.Instance.NetLobbyTexts.CoopVer;
		m_RegionHeader.text = UIStrings.Instance.NetLobbyTexts.RegionHeader;
		m_NeedSameRegionAndCoopVerDescription.text = UIStrings.Instance.NetLobbyTexts.NeedSameRegionAndCoopVer;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsInRoom.CombineLatest(base.ViewModel.NetGameCurrentState, base.ViewModel.ReadyToHostOrJoin, (bool inRoom, NetGame.State state, bool ready) => new { inRoom, state, ready }).Subscribe(value =>
		{
			bool isConnectingNetGameCurrentState = base.ViewModel.IsConnectingNetGameCurrentState;
			IsInCreateJoinPart.Value = !value.inRoom && !isConnectingNetGameCurrentState && value.ready;
			base.gameObject.SetActive(!value.inRoom && !isConnectingNetGameCurrentState && value.ready);
		}));
		AddDisposable(base.ViewModel.HasCodeForLobby.CombineLatest(base.ViewModel.RegionDropdownVM, (bool lobbyCode, OwlcatDropdownVM region) => lobbyCode && region != null).Subscribe(delegate(bool value)
		{
			ReadyToJoin.Value = value;
		}));
		AddDisposable(base.ViewModel.RegionDropdownVM.Subscribe(delegate(OwlcatDropdownVM value)
		{
			m_RegionWaiting.gameObject.SetActive(value == null);
			m_RegionDropdown.gameObject.SetActive(value != null);
			m_RegionDropdown.Bind(value);
		}));
		AddDisposable(ShowLobbyCode.Subscribe(delegate(bool value)
		{
			m_ShowHideLobbyIcon.alpha = (value ? 1f : 0.25f);
			m_LobbyCodeInputField.contentType = ((!value) ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard);
			m_LobbyCodeInputField.ForceLabelUpdate();
		}));
		m_LobbyCodeInputField.text = string.Empty;
		AddDisposable(m_LobbyCodeInputField.ObserveEveryValueChanged((TMP_InputField f) => f.text).Subscribe(delegate(string t)
		{
			base.ViewModel.SetLobbyCode(t);
		}));
		AddDisposable(base.ViewModel.Version.Subscribe(delegate(string value)
		{
			m_VersionText.text = value;
		}));
		AddDisposable(m_VersionText.SetHint(UIStrings.Instance.NetLobbyTexts.CoopVerTooltip));
		SetUserTypesDropdowns();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetUserTypesDropdowns()
	{
		bool flag = base.ViewModel.JoinableUserTypesDropdownVM != null;
		m_JoinableUserTypesDropdown.gameObject.SetActive(flag);
		if (flag)
		{
			m_JoinableUserTypesLabel.text = UIStrings.Instance.NetLobbyTexts.JoinableUserTypesLabel;
			m_JoinableUserTypesDropdown.Bind(base.ViewModel.JoinableUserTypesDropdownVM);
			m_JoinableUserTypesDropdown.SetIndex((int)base.ViewModel.CurrentJoinableUserType.Value);
			AddDisposable(m_JoinableUserTypesDropdown.Index.Subscribe(base.ViewModel.SetJoinableUserType));
		}
		bool flag2 = base.ViewModel.InvitableUserTypesDropdownVM != null;
		m_InvitableUserTypesDropdown.gameObject.SetActive(flag2);
		if (flag2)
		{
			m_InvitableUserTypesLabel.text = UIStrings.Instance.NetLobbyTexts.InvitableUserTypesLabel;
			m_InvitableUserTypesDropdown.Bind(base.ViewModel.InvitableUserTypesDropdownVM);
			m_InvitableUserTypesDropdown.SetIndex((int)base.ViewModel.CurrentInvitableUserType.Value);
			AddDisposable(m_InvitableUserTypesDropdown.Index.Subscribe(base.ViewModel.SetInvitableUserType));
		}
	}
}
