using System.Collections.Generic;

namespace Kingmaker.Twitch.DropsService.Model;

public class ClaimDropsResponseBody : Dictionary<ClaimResultStatus, List<string>>
{
}
