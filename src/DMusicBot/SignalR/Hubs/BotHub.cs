using Lavalink4NET;
using Microsoft.AspNetCore.SignalR;

namespace DMusicBot.SignalR.Hubs;

public class BotHub : Hub
{
    public async Task GetQueueAsync(IAudioService audioService, ulong guildId)
    {
        // var queue = await AudioService.GetQueueAsync(guildId).ConfigureAwait(false);
        // await Clients.Caller.SendAsync("Queue", queue).ConfigureAwait(false);
    }
}