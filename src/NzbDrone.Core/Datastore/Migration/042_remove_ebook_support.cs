using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(042)]
    public class remove_ebook_support : NzbDroneMigrationBase
    {
        // PDF, MOBI, EPUB, AZW3
        private static readonly HashSet<int> EbookQualities = new () { 1, 2, 3, 4 };

        // Unknown Audio, MP3, M4B, FLAC
        private static readonly List<int> AudioQualities = new () { 13, 10, 12, 11 };

        protected override void MainDbUpgrade()
        {
            Delete.Column("IsCalibreLibrary").FromTable("RootFolders");
            Delete.Column("CalibreSettings").FromTable("RootFolders");
            Delete.Column("CalibreId").FromTable("BookFiles");

            Execute.Sql("DELETE FROM \"QualityDefinitions\" WHERE \"Quality\" IN (1, 2, 3, 4)");

            Execute.WithConnection(RemoveEbookQualitiesFromProfiles);
        }

        private void RemoveEbookQualitiesFromProfiles(IDbConnection conn, IDbTransaction tran)
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileItem42>>(new QualityIntConverter()));

            var profiles = conn.Query<Profile42>("SELECT \"Id\", \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"", transaction: tran).ToList();

            var usedProfileIds = conn.Query<int>("SELECT \"QualityProfileId\" FROM \"Authors\" WHERE \"QualityProfileId\" IS NOT NULL", transaction: tran)
                .Concat(conn.Query<int>("SELECT \"DefaultQualityProfileId\" FROM \"RootFolders\"", transaction: tran))
                .Concat(conn.Query<int>("SELECT \"ProfileId\" FROM \"ImportLists\"", transaction: tran))
                .ToHashSet();

            var updatedProfiles = new List<Profile42>();
            var deletedProfiles = new List<Profile42>();

            foreach (var profile in profiles)
            {
                profile.Items = RemoveEbookItems(profile.Items);

                if (!profile.Items.Any(i => i.Allowed))
                {
                    // Profile only allowed ebook qualities
                    if (!usedProfileIds.Contains(profile.Id))
                    {
                        deletedProfiles.Add(profile);
                        continue;
                    }

                    foreach (var item in profile.Items.Where(i => AudioQualities.Contains(i.Quality)))
                    {
                        item.Allowed = true;
                    }
                }

                if (!IsValidCutoff(profile))
                {
                    var allowed = profile.Items.Where(i => i.Allowed).ToList();
                    var cutoff = allowed.LastOrDefault();

                    profile.Cutoff = cutoff == null ? 0 : cutoff.Id > 0 ? cutoff.Id : cutoff.Quality;
                }

                updatedProfiles.Add(profile);
            }

            conn.Execute("UPDATE \"QualityProfiles\" SET \"Cutoff\" = @Cutoff, \"Items\" = @Items WHERE \"Id\" = @Id", updatedProfiles, transaction: tran);
            conn.Execute("DELETE FROM \"QualityProfiles\" WHERE \"Id\" = @Id", deletedProfiles, transaction: tran);
        }

        private static List<ProfileItem42> RemoveEbookItems(List<ProfileItem42> items)
        {
            var result = new List<ProfileItem42>();

            foreach (var item in items)
            {
                if (item.Items.Any())
                {
                    item.Items = RemoveEbookItems(item.Items);

                    if (item.Items.Any())
                    {
                        result.Add(item);
                    }
                }
                else if (!EbookQualities.Contains(item.Quality))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static bool IsValidCutoff(Profile42 profile)
        {
            return profile.Items.Any(i => i.Allowed && (i.Id > 0 ? i.Id == profile.Cutoff : i.Quality == profile.Cutoff));
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
            public int Id { get; set; }
            public string Name { get; set; }
            public int Quality { get; set; }
            public bool Allowed { get; set; }
            public List<ProfileItem42> Items { get; set; } = new ();
        }
    }
}
