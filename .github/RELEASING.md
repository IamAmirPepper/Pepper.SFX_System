# Releasing

For maintainers. Users don't need any of this; they read `README.md`.

## How a release goes out

1. In the private `SFX_System` repo, `release_dev_package.ps1 <version>` and then `release_free_package.ps1 <version>`
   put the new version into this repo.
2. In GitHub Desktop: look over the changes, commit them as "Pepper.SFX <version>", tag that commit `v<version>`,
   and push. The tag goes up with it.
3. The tag starts the **Release** workflow (`.github/workflows/release.yml`). It checks the release, then publishes
   the GitHub Release "Pepper.SFX <version>". Its notes are the install line, then that version's section of
   `CHANGELOG.md`. A version with a `-` in it (`3.2.0-beta.1`) is marked a pre-release.
4. If a check fails, nothing is published, and GitHub emails you about the failed run. Those emails are set under
   GitHub > Settings > Notifications > Actions; keep the failed-workflow emails on.

## What it checks

- `package.json` is named `com.pepper.sfx`, and its version is the tag without the `v`.
- Each guide in `Documentation~` says that version on the `**Version:**` line at its top, without any `-dev.N`.
- `CHANGELOG.md` has a `## <version>` section, and it isn't empty.
- `LICENSE.txt` has nothing left in square brackets (a blank like `[date]` that was never filled in), and
  `Third-Party Notices.txt` is there.
- Both DLLs and the reverb plugin are there, with their `.meta` files, and the DLLs' guids haven't changed: Runtime
  `185cca1de5fe9184da36f5ffa2232159`, Editor `350c266aa26477146a19f969cf09e021`. Every free user's project points
  at those; a new guid breaks them all.
- The notes fit in GitHub's limit of 125,000 characters.

## Running it by hand

Actions tab > **Release** > **Run workflow**. Type the tag (`v3.1.1`) and:

- leave **Dry run** ticked (the default) to check the release and see its notes on the run's page. Nothing is
  published.
- untick it to publish the Release. If that tag already has a Release, it's updated with the notes from the tag;
  there's never a second one. The tag must already be on GitHub.

If the tag isn't on GitHub yet, a dry run checks the branch picked under "Use workflow from" instead, as if it were
tagged. That's a way to check a release before you tag it.

## When a check fails

The run's page lists each problem. Nothing was published.

1. Fix it: in the private repo (then run the release scripts again), or here if the problem is only in this repo.
2. Commit the fix.
3. If you pushed the tag only minutes ago, nobody has installed it yet, so you can move the tag to the fixed commit:
   delete the tag on GitHub (Code > Tags > the tag > Delete) and in GitHub Desktop, tag the fixed commit again, and
   push. The workflow runs again. If the tag has been out for longer, leave it and release the next version instead:
   users pin versions by tag, so a tag must never change under them.

## Changing a Release's notes

The notes come from `CHANGELOG.md` at the tag. To fix a typo in a published Release, edit it on GitHub (Releases >
the version > Edit). Running the workflow again for that tag puts the `CHANGELOG.md` text back.

## Keep in mind

- `release_free_package.ps1` keeps everything already in this repo, `.github/` included, when it copies a release
  in.
- Unity ignores folders whose names start with a dot, so `.github/` never shows up in users' projects. Keep notes like
  this one in here: a file at the repo root without a `.meta` file makes Unity warn in every user's project.
