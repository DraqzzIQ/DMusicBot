using Discord.Interactions;
using DMusicBot.Models;
using DMusicBot.Services;
using DMusicBot.Util;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DMusicBot.Modules;
public sealed class WebPlayerModule(IAudioService audioService, ILogger<SkipModule> logger, IDbService dbService) : BaseModule(audioService, logger)
{
    private readonly IDbService _dbService = dbService;
    
    /// <summary>
    ///     Generates a URL with auth token to the web player.
    /// </summary>
    /// <returns>a task that represents the asynchronous operation</returns>
    [SlashCommand("web-player", description: "Skips the current track", runMode: RunMode.Async)]
    public async Task WebPlayer()
    {
        await DeferAsync().ConfigureAwait(false);
        
        await _dbService.RemoveAllMatchingAuthTokensAsync(new AuthModel {GuildId = Context.Guild.Id, UserId = Context.User.Id}).ConfigureAwait(false);
        
        AuthModel auth = new()
        {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            Token = SecureGuidGenerator.CreateCryptographicallySecureGuid()
        };
        
        await _dbService.AddAuthTokenAsync(auth).ConfigureAwait(false);
        
        await FollowupAsync("auth token: " + auth.Token).ConfigureAwait(false);
    }
}