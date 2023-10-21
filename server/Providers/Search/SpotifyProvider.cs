using System.Globalization;
using AutoMapper.Internal;
using melodiy.server.Dtos.Search;
using melodiy.server.Dtos.Song;
using server.Dtos.Artist;
using server.Models;
using SpotifyAPI.Web;

namespace melodiy.server.Providers.Search
{
    public class SpotifyProvider : ISearchProvider
    {
        public SpotifyClientConfig DefaultConfig;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAudioProvider _audioProvider;

        public SpotifyProvider(IConfiguration configuration, DataContext context, IMapper mapper, IAudioProvider audioProvider)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _audioProvider = audioProvider;

            string spotifyClientId = _configuration.GetSection("AppSettings:SpotifyClientId").Value ?? throw new Exception("SpotifyClientId is null!");
            string spotifyClientSecret = _configuration.GetSection("AppSettings:SpotifyClientSecret").Value ?? throw new Exception("SpotifyClientSecret is null!");
            DefaultConfig = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(spotifyClientId, spotifyClientSecret));
        }

        public async Task<SearchResults> Search(string term, int limit)
        {
            SpotifyClient spotify = new(DefaultConfig);
            // SearchResponse results = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Artist | SearchRequest.Types.Album | SearchRequest.Types.Track, term));

            //Searches for only Artists, Albums and Tracks
            SearchResponse results = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Artist | SearchRequest.Types.Album | SearchRequest.Types.Track, term)
            {
                Type = SearchRequest.Types.Artist | SearchRequest.Types.Album | SearchRequest.Types.Track,
                Query = term,
                Limit = limit
            });


            //No Results so we can return an empty result;
            SearchResults pipedResults = new();
            if (results == null)
            {
                return new();
            }

            if (results.Tracks.Items != null)
            {
                pipedResults.Songs = await ParseTracks(results.Tracks.Items);
            }

            if (results.Artists.Items != null)
            {
                pipedResults.Artists = await ParseArtists(results.Artists.Items);
            }

            return pipedResults;
        }

        private async Task<List<GetSongResponse>> ParseTracks(List<FullTrack> tracks)
        {
            List<Song> _insertSongs = new();
            List<string> spotifyIds = new(); //List of all songs fetched regardless if they have already exist.

            //TODO: Move to song service?
            //Converts Spotify API results into DB serialised Song Track.
            for (int i = 0; i < tracks.Count; i++)
            {
                FullTrack track = tracks[i];
                DateTime releaseDate = GetReleaseDate(track.Album.ReleaseDate, track.Album.ReleaseDatePrecision);

                //Check if it already exists.
                //TODO: Filter out list before instead of individually checking (Less DB calls)
                Song? dbSong = await _context.Songs.SingleOrDefaultAsync(s => s.SpotifyId == track.Id);
                if (dbSong != null)
                {
                    spotifyIds.Add(track.Id);
                    continue;
                }


                List<string> artists = track.Artists.ConvertAll(a => a.Name);
                try
                {
                    //To Make searches quicker and reduce uneeded API calls the audio stream is fetched whenever the individual song
                    //is requested (usually when it is played by a user).
                    //This will throw an error if no video is found
                    // YoutubeVideo video = await _audioProvider.Find(track.Name, artists, track.DurationMs);
                    // _ = TimeSpan.TryParseExact(video.Duration, @"m\:ss", null, out TimeSpan videoDuration);

                    Console.WriteLine(releaseDate);
                    _ = _insertSongs.TryAdd(new Song
                    {
                        Title = track.Name,
                        Artist = track.Artists[0].Name, //TODO: Update to include multiple artists ?
                        Album = track.Album.Name,
                        CoverPath = track.Album.Images[0].Url,
                        Duration = track.DurationMs,
                        Provider = ProviderType.External,
                        SpotifyId = track.Id,
                        // YoutubeId = video.Id,
                        ReleaseDate = releaseDate.ToUniversalTime(),
                    });
                    spotifyIds.Add(track.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored Out");
                    Console.WriteLine(ex.Message);
                }

            }

            try
            {
                await _context.Songs.BulkInsertAsync(_insertSongs, options =>
                {
                    options.InsertIfNotExists = true;
                    options.ColumnPrimaryKeyExpression = s => s.SpotifyId;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            //Pointless as we don't care about getting the newest "createdAt" variable for searched results.
            List<Song> dbSongs = await _context.Songs.Where(s => spotifyIds.Contains(s.SpotifyId!)).ToListAsync();
            return dbSongs.Select(_mapper.Map<GetSongResponse>).ToList();
        }

        private async Task<List<GetArtistResponse>> ParseArtists(List<FullArtist> artists)
        {
            List<Artist> _insertArtists = new();
            List<string> spotifyIds = new(); //List of all artists fetched regardless if they have already exist.

            //TODO: Move to song service?
            //Converts Spotify API results into DB serialised Song Track.
            for (int i = 0; i < artists.Count; i++)
            {
                FullArtist artist = artists[i];

                //Check if it already exists.
                //TODO: Filter out list before instead of individually checking (Less DB calls)
                Artist? dbArtist = await _context.Artists.SingleOrDefaultAsync(a => a.SpotifyId == artist.Id);
                if (dbArtist != null)
                {
                    spotifyIds.Add(artist.Id);
                    continue;
                }

                try
                {
                    _ = _insertArtists.TryAdd(new Artist
                    {
                        Name = artist.Name,
                        CoverPath = artist.Images[0].Url,
                        Verified = true, //Any artist from the spotify API is "official"
                        SpotifyId = artist.Id,
                    });
                    spotifyIds.Add(artist.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored Out");
                    Console.WriteLine(ex.Message);
                }
            }

            try
            {
                await _context.Artists.BulkInsertAsync(_insertArtists, options =>
                {
                    options.InsertIfNotExists = true;
                    options.ColumnPrimaryKeyExpression = s => s.SpotifyId;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            //Pointless as we don't care about getting the newest "createdAt" variable for searched results.
            List<Artist> dbArtists = await _context.Artists.Where(s => spotifyIds.Contains(s.SpotifyId!)).ToListAsync();
            return dbArtists.Select(_mapper.Map<GetArtistResponse>).ToList();
        }

        // public async Task<FullArtist> Artist(string id)
        // {

        // }
        //Converts YYYY-MM-DD (2004-01-01 || 2004-01)  to a DateTime Variable.
        private static DateTime GetReleaseDate(string releaseDate, string releaseDatePrecision)
        {
            //Some results are only accurate to the month or year in this case we default to the start of the period.
            if (releaseDatePrecision == "month")
            {
                releaseDate += "-01";
            }
            else if (releaseDatePrecision == "year")
            {
                releaseDate += "-01-01";
            }

            DateTime result = DateTime.ParseExact(releaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return result;
        }
    }
}