# Listenarr

Listenarr is an audiobook collection manager for Usenet and BitTorrent users.
It can monitor multiple RSS feeds for new audiobooks from your favorite authors
and will grab, sort, rename and tag them.

It is a fork of [Bookshelf](https://github.com/pennydreadful/bookshelf), which
is itself a revival of [Readarr](https://github.com/Readarr/Readarr), with all
ebook support removed:

- Only audio formats are handled (MP3, M4B, FLAC and other audio files).
  Ebook releases (EPUB, MOBI, AZW3, PDF) are rejected.
- Calibre integration, ebook tagging and the default eBook quality profile are
  gone. Existing databases are migrated and ebook-only quality profiles are
  removed.
- Indexers default to the Audio/Audiobook (3030) category.

Not to be confused with the unrelated
[Listenarrs/Listenarr](https://github.com/Listenarrs/Listenarr) project.

## Getting Started

The container listens on port 8787 and expects a volume mounted at `/config`.

    docker run -p 8787:8787 -v ~/.config/listenarr:/config ghcr.io/matthewcarberry/listenarr:hardcover

The `softcover` tags use [Goodreads](https://www.goodreads.com) as the metadata
provider and are backward-compatible with existing Readarr databases.

The `hardcover` tags use [Hardcover](https://hardcover.app/home) as a metadata
provider. This metadata is higher quality but isn't backward-compatible with
existing Readarr databases.

## Prowlarr

Listenarr keeps Readarr's API, so Prowlarr can manage its indexers. In Prowlarr
go to Settings > Apps, add a **Readarr** application pointing at Listenarr's
URL and API key, and set the sync categories to Audio/Audiobook (3030).

## Support

If you have a problem please file an issue or start a discussion.

## Features inherited from Bookshelf

- Native support for MyAnonaMouse without Prowlarr.
- Hardcover list import.
- Improved matching.
- Metadata is no longer cached locally.
- Servarr analytics removed.
- Supports selfhosted metadata (UI or `METADATA_URL` env var).

## License

This is a derivative work of [Bookshelf](https://github.com/pennydreadful/bookshelf),
[Readarr](https://github.com/Readarr/Readarr) and
[Prowlarr](https://github.com/Prowlarr/Prowlarr), which are licensed
[GPLv3](http://www.gnu.org/licenses/gpl.html). This project is therefore also
licensed under the terms of GPLv3.
