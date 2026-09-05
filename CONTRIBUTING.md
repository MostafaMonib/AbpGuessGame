# Contributing to AbpGuessGame

Thank you for contributing to **AbpGuessGame**.

The project follows a consistent development workflow focused on maintainability, clean architecture, automated testing, and secure software delivery.

## Getting Started

Clone the repository:

[AbpGuessGame GitHub Repository](https://github.com/MostafaMonib/AbpGuessGame)

```bash
git clone https://github.com/MostafaMonib/AbpGuessGame.git
cd AbpGuessGame
```

Install the required dependencies and follow the setup instructions in [`README.md`](./README.md).

## Development Workflow

The primary development branch is:

```text
dev
```

Create a dedicated branch for each change.

### Branch naming

```text
feature/<short-description>
fix/<short-description>
chore/<short-description>
refactor/<short-description>
test/<short-description>
docs/<short-description>
```

Examples:

```text
feature/game-history
fix/duplicate-guess
refactor/game-domain
test/game-aggregate
docs/api-documentation
```

Pull requests should target `dev`.

---

## Commit Messages

Use concise Conventional Commit-style messages.

Examples:

```text
feat(game): add guess history endpoint
fix(game): prevent duplicate guesses
refactor(domain): extract binary search strategy
test(game): add winning game scenarios
docs(api): document guess endpoints
chore(build): update dependencies
```

Recommended prefixes:

| Prefix     | Purpose                                    |
| ---------- | ------------------------------------------ |
| `feat`     | New functionality                          |
| `fix`      | Bug fix                                    |
| `refactor` | Code restructuring without behavior change |
| `test`     | Tests                                      |
| `docs`     | Documentation                              |
| `chore`    | Build, tooling or maintenance              |

---

## Code Style

The repository uses `.editorconfig` to maintain consistent formatting.

Before committing:

```bash
dotnet format
```

Follow the existing project conventions for:

* Naming
* Namespaces
* Dependency injection
* Async APIs
* Nullable reference types
* Entity Framework Core mappings
* ABP application services
* Domain entities and aggregates

Avoid introducing infrastructure concerns into the domain layer.

---

## Architecture Rules

Contributions must preserve the application's layered architecture.

The dependency direction should remain:

```text
HttpApi.Host
    ↓
HttpApi
    ↓
Application
    ↓
Domain
    ↓
Domain.Shared
```

Infrastructure components such as EF Core should not introduce dependencies from the domain toward the database or HTTP layer.

Business rules should be implemented in the domain model where appropriate.

Application services should coordinate use cases rather than duplicate domain rules.

---

## Domain Changes

Changes to game behavior must include appropriate domain tests.

Examples include:

* Guess validation
* Higher/lower rules
* Winning conditions
* Best-score calculation
* Game state transitions
* Duplicate-guess behavior
* Idempotency
* Binary-search behavior

Domain tests should remain deterministic and should not require a live database.

---

## API Changes

When changing public API behavior:

* Update application contracts and DTOs consistently.
* Preserve authorization requirements.
* Validate input on the server.
* Avoid exposing internal domain entities directly.
* Ensure sensitive properties such as the active secret number are not returned.
* Update API documentation when the contract changes.
* Add or update API/integration tests.

---

## Database Changes

Database schema changes must include an Entity Framework Core migration.

Example:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/AbpGuessGame.EntityFrameworkCore \
  --startup-project src/AbpGuessGame.HttpApi.Host
```

Review migrations before committing them.

Do not commit:

* Production database credentials
* Connection strings containing secrets
* Database dumps containing sensitive data

---

## Logging and Observability

Application logs should use structured logging.

Where applicable, operations should preserve:

```text
CorrelationId
UserId
Operation
ApplicationLayer
```

Do not log:

* Passwords
* Access tokens
* Cookies
* Connection strings
* Password request bodies
* Active game secrets

Business behavior should not depend on log messages.

---

## Security

Security-sensitive changes require particular attention to:

* Authentication
* Authorization
* Input validation
* CORS
* Rate limiting
* Secret handling
* SQL injection prevention
* Information disclosure
* Concurrency
* Error handling

Never commit credentials, API keys, certificates, private keys, or other secrets.

Use environment variables or the hosting platform's secure configuration mechanism.

---

## Testing

Run backend tests:

```bash
dotnet test
```

Run frontend tests:

```bash
npm --prefix react test
```

Before opening a pull request, ensure:

```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Frontend tests should also pass when frontend code is modified.

Changes to business behavior should include tests covering the affected rules.

---

## Pull Requests

Pull requests should:

* Target `dev`.
* Contain one focused change where possible.
* Explain the problem and solution.
* Include relevant tests.
* Mention database migrations when applicable.
* Describe API contract changes.
* Identify security or configuration changes.
* Avoid unrelated formatting or refactoring.

### Pull request checklist

* [ ] Code builds successfully.
* [ ] Backend tests pass.
* [ ] Frontend tests pass when applicable.
* [ ] Formatting is compliant.
* [ ] Database migrations are included when required.
* [ ] API contracts/documentation are updated when required.
* [ ] No secrets are committed.
* [ ] Security implications have been reviewed.
* [ ] Logging does not expose sensitive information.
* [ ] Changes respect the layered architecture.

---

## Review Guidelines

Reviewers should evaluate:

1. Correctness
2. Domain integrity
3. Architecture and dependency boundaries
4. Security
5. Test coverage
6. Database consistency
7. API behavior
8. Observability
9. Maintainability
10. Performance where relevant

A pull request should not be approved when required tests, migrations, security controls, or architecture constraints are incomplete.

---

## Documentation

Technical documentation should be updated when behavior or architecture changes.

The main technical reference is:

```text
DOCUMENTATION.md
```

Implementation-specific documentation can be maintained under:

```text
docs/
```

Documentation should explain the current behavior of the system rather than describe future plans.

---

## Questions and Design Discussions

For significant architectural or domain changes, open an issue or discussion before implementing a large change.

The goal is to keep the system intentionally simple, maintainable, testable, and consistent with its DDD/ABP architecture.
