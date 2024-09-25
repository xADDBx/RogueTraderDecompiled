using System;
using System.Threading.Tasks;
using Kingmaker.Networking.Player;

namespace Kingmaker.Networking.Platforms.User;

public interface IPlatformUser
{
	string NickName { get; }

	PlayerAvatar LargeIcon { get; }

	Task Initialize();

	void GetLargeIcon(string userId, Action<PlayerAvatar> callback);
}
