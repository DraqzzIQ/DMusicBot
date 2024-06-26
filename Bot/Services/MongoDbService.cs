using DMusicBot.Models;
using MongoDB.Driver;

namespace DMusicBot.Services;

public class MongoDbService : IDbService
{
    private readonly string? _connectionString = Config.DbConnectionString;
    private readonly IMongoCollection<PlaylistModel> _playlistCollection;
    public MongoDbService()
    {
        if (_connectionString is null)
        {
            throw new ArgumentNullException(nameof(_connectionString));
        }
        MongoClient client = new(_connectionString);
        IMongoDatabase database = client.GetDatabase("music-bot");
        _playlistCollection = database.GetCollection<PlaylistModel>("playlists");
    }
    
    public async Task<bool> PlaylistExistsAsync(ulong guildId, string name)
    {
        return await _playlistCollection.Find(p => p.GuildId == guildId && p.Name == name).AnyAsync();
    }
    
    public async Task<bool> TrackExistsInPlaylistAsync(ulong guildId, string playlistName, string trackName)
    {
        PlaylistModel playlist = await GetPlaylistAsync(guildId, playlistName);
        return playlist.Tracks.Any(t => t.Title == trackName);
    }
    
    public async Task<PlaylistModel> GetPlaylistAsync(ulong guildId, string name)
    {
        return await _playlistCollection.Find(p => p.GuildId == guildId && p.Name == name).FirstOrDefaultAsync();
    }
    
    public async Task<PlaylistModel> CreatePlaylistAsync(ulong userId, ulong guildId, string name, bool publicPlaylist)
    {
        PlaylistModel playlist = new()
        {
            Name = name,
            OwnerId = userId,
            GuildId = guildId,
            IsPublic = publicPlaylist,
            Tracks = []
        };
        await _playlistCollection.InsertOneAsync(playlist);
        return playlist;
    }
    
    public async Task DeletePlaylistAsync(ulong guildId, string name)
    {
        await _playlistCollection.DeleteOneAsync(p => p.GuildId == guildId && p.Name == name);
    }

    public async Task DeletePlaylistAsync(PlaylistModel playlist)
    {
        await _playlistCollection.DeleteOneAsync(p => p.GuildId == playlist.GuildId && p.Name == playlist.Name);
    }
    
    public async Task AddTrackToPlaylistAsync(ulong guildId, string name, TrackModel track)
    {
        await _playlistCollection.UpdateOneAsync(p => p.GuildId == guildId && p.Name == name, Builders<PlaylistModel>.Update.Push(p => p.Tracks, track));
    }
    
    public async Task AddTracksToPlaylistAsync(ulong guildId, string name, IEnumerable<TrackModel> tracks)
    {
        await _playlistCollection.UpdateOneAsync(p => p.GuildId == guildId && p.Name == name, Builders<PlaylistModel>.Update.PushEach(p => p.Tracks, tracks));
    }
    
    public async Task RemoveTrackFromPlaylistAsync(ulong guildId, string playlistName, string trackName)
    {
        await _playlistCollection.UpdateOneAsync(p => p.GuildId == guildId && p.Name == playlistName, Builders<PlaylistModel>.Update.PullFilter(p => p.Tracks, s => s.Title == trackName));
    }
    
    public async Task<List<PlaylistModel>> GetPlaylistsAsync(ulong guildId)
    {
        return await _playlistCollection.Find(p => p.GuildId == guildId).ToListAsync();
    }
    
    public async Task SetPlaylistPublicAsync(ulong guildId, string name, bool isPublic)
    {
        await _playlistCollection.UpdateOneAsync(p => p.GuildId == guildId && p.Name == name, Builders<PlaylistModel>.Update.Set(p => p.IsPublic, isPublic));
    }
    
    public async Task UpdatePlaylistAsync(PlaylistModel playlist)
    {
        await _playlistCollection.ReplaceOneAsync(p => p.GuildId == playlist.GuildId && p.Name == playlist.Name, playlist);
    }
    
    public async Task<List<PlaylistModel>> FindMatchingPlaylistsAsync(ulong guildId, string query)
    {
        return await _playlistCollection.Find(p => p.GuildId == guildId && p.Name.ToLower().Contains(query.ToLower())).ToListAsync();
    }
    
    public async Task<List<TrackModel>> FindMatchingTracksForPlaylistAsync(ulong guildId, string playlistName, string query)
    {
        PlaylistModel playlist = await GetPlaylistAsync(guildId, playlistName);
        return playlist.Tracks.Where(t => t.Title.ToLower().Contains(query.ToLower())).ToList();
    }
}