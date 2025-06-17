# KBlog
My personal blog website for sharing programming knowledge, developed with Angular and .NET Core.

## Contributing

This project follows the GitHub Flow methodology. I appreciate your contributions! To ensure a smooth process, please adhere to the following guidelines.

### 1. Workflow Summary

* The `main` branch is the primary branch and should always be stable and deployable.
* **Do not commit directly to `main`**.
* All new work (features, bug fixes, enhancements, etc.) must be done on a separate **feature branch** created from the latest `main`.
* Once your work on the feature branch is complete and tested, open a **Pull Request (PR)** to merge your changes into `main`.
* Your PR will be reviewed (if applicable), and automated checks (CI builds, tests) must pass.
* After approval and successful checks, the feature branch will be merged into `main`.
* The `main` branch can then be deployed.

### 2. Branch Naming Conventions

When creating a new branch from `main`, please use the following naming convention:

* **`feature/<descriptive-name>`**: For new features (e.g., `feature/user-authentication`, `feature/article-search`).
* **`fix/<descriptive-name>`**: For bug fixes (e.g., `fix/login-form-validation`, `fix/api-null-reference`).
* **`chore/<descriptive-name>`**: For routine tasks, refactoring, or updates that don't add features or fix bugs (e.g., `chore/update-dependencies`, `chore/refactor-data-service`).
* **`docs/<descriptive-name>`**: For changes to documentation (e.g., `docs/update-readme-setup-guide`).

Names should be in **kebab-case** (all lowercase with hyphens as separators).

### 3. Pull Request (PR) Requirements

* **Create PRs against `main`**: Ensure your feature branch is up-to-date with `main` before creating a PR. Rebase if necessary to resolve conflicts.
* **Clear Title**: The PR title should succinctly describe the change (e.g., "feat: Implement user registration API", "fix: Resolve article display bug on mobile").
* **Detailed Description**: In the PR description, please provide:
    * A summary of the changes and the problem being solved.
    * How the changes were implemented.
    * Steps to test or verify the changes (if applicable).
    * Links to any relevant issues (e.g., "Closes #42").
* **Self-Review**: Review your own code for potential issues before submitting the PR.
* **Keep PRs Focused**: Aim for small, atomic PRs that address a single concern. This makes them easier and faster to review and merge.
* **Ensure CI Checks Pass**: All automated checks (builds, tests, linters) configured for the repository must pass before a PR can be merged.

### 4. Commit Message Guidelines

I encourage clear, concise, and informative commit messages. Following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification is highly recommended as it helps in standardizing messages and can automate changelog generation.

**Format:**

`<type>(<optional scope>): <description>`

**Common `<type>` examples:**

* `feat`: A new feature (e.g., `feat: Add comment moderation for admin`)
* `fix`: A bug fix (e.g., `fix: Correct pagination error on article list`)
* `docs`: Documentation only changes (e.g., `docs: Update API endpoint examples`)
* `style`: Code style changes (formatting, white-space, etc.) (e.g., `style: Format code with Prettier`)
* `refactor`: Code changes that neither fix a bug nor add a feature (e.g., `refactor(auth): Simplify token generation logic`)
* `perf`: Performance improvements (e.g., `perf: Optimize image loading on homepage`)
* `test`: Adding or correcting tests (e.g., `test: Add unit tests for article service`)
* `chore`: Routine tasks, build process changes, etc. (e.g., `chore: Update Angular to v18.1.0`)
* `ci`: Changes to CI configuration files and scripts (e.g., `ci: Adjust GitHub Actions workflow timeout`)

**Example Commit Message:**

feat(api): Implement endpoint for fetching user profile

    Added GET /api/users/profile endpoint
    Returns current authenticated user's profile information
    Includes basic error handling for unauthorized access

<!-- end list -->

* Use the imperative mood in the subject line (e.g., "Add feature" not "Added feature").
* The subject line should be concise (ideally under 72 characters).
* Provide more details in the commit body if the change is complex.
