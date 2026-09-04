# Contributing to AbpGuessGame

Thank you for contributing to AbpGuessGame. This project follows a small, strict set of standards to keep the codebase consistent and interview-ready.

## Quick start

- Fork the repository and create a branch from `dev`.
- Name branches: `feature/<short-description>`, `fix/<short-description>`, or `chore/<short-description>`.
- Open a pull request (PR) against the `dev` branch when your change is ready.

## Branches and PRs

- Base branch for day-to-day development: `dev`.
- PRs must target `dev`.
- PR titles: `TYPE(scope): short description` (e.g. `feat(api): add start game endpoint`).
- Include a short description and testing steps in the PR body.
- Small, focused PRs are preferred.

## Code style and formatting

This repository enforces the rules in `.editorconfig`. Configure your IDE to respect it before committing:

- UTF-8, CRLF, final newline
- Spaces for indentation, 4 for C# files
- Max line length 120 for code

C# conventions (from `.editorconfig`):

- File-scoped namespaces where suggested by .editorconfig
- Do not use `var` for built-in types unless the rule is explicitly changed in `.editorconfig`
- New line before open brace for all members (`csharp_new_line_before_open_brace = all`)

Run `dotnet format` or ensure your IDE formats files according to `.editorconfig` before pushing.

## Commit messages

Follow conventional style in brief form:

- `feat:` — new feature
- `fix:` — bug fix
- `chore:` — non-code changes (docs, CI)

Example: `feat(api): implement Guess result DTO and controller`.

## Tests

- Unit tests are required for domain logic. Domain unit tests are mandatory before merging features that change game logic.
- Test projects follow naming: `AbpGuessGame.*.Tests`.
- Run tests locally: `dotnet test --no-build --verbosity minimal`.

## Pull request checklist

- [ ] Code compiles and builds locally (`dotnet build`).
- [ ] All unit tests pass (`dotnet test`).
- [ ] New public API or behavior documented in `docs/steps/*`.
- [ ] `.editorconfig` rules preserved; no formatting/whitespace violations.
- [ ] No secrets or credentials committed.
- [ ] PR includes testing steps and expected behavior.

## Review guidelines

- Reviewers check design, tests, logging, and security considerations.
- Use explicit comments to request changes.
- Approve only when tests and the checklist items are satisfied.

## CI

- CI runs `dotnet restore`, `dotnet build`, and `dotnet test`.
- PRs must pass CI before merge.

## Security and secrets

- Never commit secrets, credentials, or connection strings.
- Use environment variables on CI and hosting platforms.

## Documentation

- Every implementation step gets a markdown under `docs/steps/` describing goal, tasks, logs, security notes, and done criteria.

## Formatting enforcement

- CI includes a formatting check. If formatting fails, run `dotnet format` or fix per `.editorconfig`.

## Contact

If a contribution is unclear, open an issue to discuss design before investing large effort.
