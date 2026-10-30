using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        public static Quality Unknown => new Quality(0, "Unknown");
        public static Quality MP3 => new Quality(10, "MP3");
        public static Quality FLAC => new Quality(11, "FLAC");
        public static Quality M4B => new Quality(12, "M4B");
        public static Quality UnknownAudio => new Quality(13, "Unknown Audio");

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                UnknownAudio,
                MP3,
                M4B,
                FLAC
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)      { Weight = 1, MinSize = 0, MaxSize = 350, GroupWeight = 1 },
                new QualityDefinition(Quality.UnknownAudio) { Weight = 50, MinSize = 0, MaxSize = 350, GroupWeight = 50 },
                new QualityDefinition(Quality.MP3)      { Weight = 100, MinSize = 0, MaxSize = 350, GroupWeight = 100 },
                new QualityDefinition(Quality.M4B)          { Weight = 105, MinSize = 0, MaxSize = 350, GroupWeight = 105 },
                new QualityDefinition(Quality.FLAC)         { Weight = 110, MinSize = 0, MaxSize = null, GroupWeight = 110 },
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;

        public static Quality FindById(int id)
        {
            // Ids 1-4 were the retired ebook qualities (PDF, MOBI, EPUB, AZW3)
            if (id >= 0 && id <= 4)
            {
                return Unknown;
            }
            else if (id > AllLookup.Length)
            {
                throw new ArgumentException("ID does not match a known quality", nameof(id));
            }

            var quality = AllLookup[id];

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", nameof(id));
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
