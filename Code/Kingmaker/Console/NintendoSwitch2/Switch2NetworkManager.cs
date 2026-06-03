using System.Threading;
using System.Threading.Tasks;
using Core.Cheats;

namespace Kingmaker.Console.NintendoSwitch2;

public static class Switch2NetworkManager
{
	public readonly struct Token
	{
		public readonly long Length;

		public readonly byte[] Data;

		public static readonly Token Invalid;

		public Token(long length, byte[] data)
		{
			Length = length;
			Data = data;
		}
	}

	public static async Task EnsureConnectionAsync(CancellationToken token)
	{
	}

	public static bool HasToken()
	{
		return false;
	}

	public static async Task<Token> GetNetworkToken()
	{
		return Token.Invalid;
	}

	[Cheat(Name = "get_nsa_token_cheat")]
	public static async Task<string> GetTokenCheat()
	{
		await GetNetworkToken();
		return "Success";
	}
}
