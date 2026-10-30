using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class remove_ebook_supportFixture : MigrationTest<remove_ebook_support>
    {
        private static string EbookItems()
        {
            return new[]
            {
                new { quality = 0, allowed = false, items = new object[0] },
                new { quality = 1, allowed = false, items = new object[0] },
                new { quality = 2, allowed = true, items = new object[0] },
                new { quality = 3, allowed = true, items = new object[0] },
                new { quality = 4, allowed = true, items = new object[0] },
                new { quality = 13, allowed = false, items = new object[0] },
                new { quality = 10, allowed = false, items = new object[0] },
                new { quality = 12, allowed = false, items = new object[0] },
                new { quality = 11, allowed = false, items = new object[0] }
            }.ToJson();
        }

        private static string SpokenItems()
        {
            return new[]
            {
                new { quality = 0, allowed = false, items = new object[0] },
                new { quality = 1, allowed = false, items = new object[0] },
                new { quality = 2, allowed = false, items = new object[0] },
                new { quality = 3, allowed = false, items = new object[0] },
                new { quality = 4, allowed = false, items = new object[0] },
                new { quality = 13, allowed = true, items = new object[0] },
                new { quality = 10, allowed = true, items = new object[0] },
                new { quality = 12, allowed = true, items = new object[0] },
                new { quality = 11, allowed = true, items = new object[0] }
            }.ToJson();
        }

        [Test]
        public void should_remove_ebook_qualities_and_unused_ebook_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 1,
                    Name = "eBook",
                    Cutoff = 2,
                    Items = EbookItems(),
                    UpgradeAllowed = false
                });

                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 2,
                    Name = "Spoken",
                    Cutoff = 10,
                    Items = SpokenItems(),
                    UpgradeAllowed = false
                });

                c.Insert.IntoTable("QualityDefinitions").Row(new { Quality = 3, Title = "EPUB" });
                c.Insert.IntoTable("QualityDefinitions").Row(new { Quality = 10, Title = "MP3" });
            });

            var profiles = db.Query<Profile42>("SELECT \"Id\", \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"");

            profiles.Should().HaveCount(1);

            var spoken = profiles.Single();
            spoken.Name.Should().Be("Spoken");
            spoken.Cutoff.Should().Be(10);
            spoken.Items.Select(i => i.Quality).Should().Equal(0, 13, 10, 12, 11);

            db.Query<QualityDefinition42>("SELECT \"Quality\" FROM \"QualityDefinitions\"").Select(d => d.Quality).Should().Equal(10);
        }

        [Test]
        public void should_switch_used_ebook_profile_to_audio_qualities()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 1,
                    Name = "eBook",
                    Cutoff = 2,
                    Items = EbookItems(),
                    UpgradeAllowed = false
                });

                c.Insert.IntoTable("RootFolders").Row(new
                {
                    Path = "/books",
                    Name = "Books",
                    DefaultMetadataProfileId = 1,
                    DefaultQualityProfileId = 1,
                    DefaultMonitorOption = 0,
                    IsCalibreLibrary = false
                });
            });

            var profiles = db.Query<Profile42>("SELECT \"Id\", \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"");

            profiles.Should().HaveCount(1);

            var profile = profiles.Single();
            profile.Items.Select(i => i.Quality).Should().Equal(0, 13, 10, 12, 11);
            profile.Items.Where(i => i.Allowed).Select(i => i.Quality).Should().Equal(13, 10, 12, 11);
            profile.Cutoff.Should().Be(11);
        }

        public class QualityDefinition42
        {
            public int Quality { get; set; }
        }

        public class Profile42
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Cutoff { get; set; }
            public List<ProfileItem42> Items { get; set; }
        }

        public class ProfileItem42
        {
            public int Quality { get; set; }
            public bool Allowed { get; set; }
        }
    }
}
