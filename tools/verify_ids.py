# -*- coding: utf-8 -*-
"""Checks every winget id in catalog.py against the official winget source index.

    python3 tools/verify_ids.py

Downloads the index winget itself uses (a few MB), so it needs network access.
Exits non-zero if any id in the catalogue no longer exists upstream.
"""
import io
import os
import sqlite3
import sys
import tempfile
import urllib.request
import zipfile

import catalog

SOURCE_URL = "https://cdn.winget.microsoft.com/cache/source2.msix"


def load_known_ids():
    with urllib.request.urlopen(SOURCE_URL) as response:
        payload = response.read()

    with tempfile.TemporaryDirectory() as workdir:
        with zipfile.ZipFile(io.BytesIO(payload)) as archive:
            archive.extract("Public/index.db", workdir)

        connection = sqlite3.connect(os.path.join(workdir, "Public", "index.db"))
        try:
            return {row[0].lower() for row in connection.execute("select id from packages")}
        finally:
            connection.close()


def main():
    known = load_known_ids()
    missing = []

    for _name_en, _name_ru, _glyph, apps in catalog.CATEGORIES:
        for app in apps:
            ids = [app["winget"]] if app["kind"] == "app" else [v[1] for v in app["variants"]]
            for winget_id in ids:
                if winget_id and winget_id.lower() not in known:
                    missing.append((winget_id, app["title"]))

    if missing:
        print("These ids are no longer in the winget source:")
        for winget_id, title in missing:
            print("  {0}  ({1})".format(winget_id, title))
        return 1

    print("All winget ids in the catalogue exist upstream.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
