using System;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static readonly Dictionary<string, Quality> _audioExtensions;

        static MediaFileExtensions()
        {
            _audioExtensions = new Dictionary<string, Quality>(StringComparer.OrdinalIgnoreCase)
            {
                { ".flac", Quality.FLAC },
                { ".ape", Quality.FLAC },
                { ".wavpack", Quality.FLAC },
                { ".wav", Quality.FLAC },
                { ".alac", Quality.FLAC },
                { ".mp2", Quality.MP3 },
                { ".mp3", Quality.MP3 },
                { ".wma", Quality.MP3 },
                { ".m4a", Quality.MP3 },
                { ".m4p", Quality.MP3 },
                { ".m4b", Quality.M4B },
                { ".aac", Quality.MP3 },
                { ".mp4a", Quality.MP3 },
                { ".ogg", Quality.MP3 },
                { ".oga", Quality.MP3 },
                { ".vorbis", Quality.MP3 },
            };
        }

        public static HashSet<string> AudioExtensions => new HashSet<string>(_audioExtensions.Keys, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> AllExtensions => new HashSet<string>(_audioExtensions.Keys, StringComparer.OrdinalIgnoreCase);

        public static Quality GetQualityForExtension(string extension)
        {
            if (_audioExtensions.ContainsKey(extension))
            {
                return _audioExtensions[extension];
            }

            return Quality.Unknown;
        }
    }
}
