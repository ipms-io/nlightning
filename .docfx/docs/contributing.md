# Contributing to NLightning

We appreciate your interest in contributing to the NLightning project. Our goal is to facilitate contributions in a structured and efficient manner. Below are the guidelines for contributing to this project, which include but are not limited to reporting bugs, discussing the codebase, submitting fixes, proposing new features, and becoming a maintainer.

## Development Workflow on GitHub

Our project leverages GitHub for hosting code, issue tracking, feature requests, and pull request management.

## Adherence to GitHub Flow

To maintain codebase integrity and facilitate review processes, all code changes must be made through pull requests following the GitHub Flow. We encourage your pull requests under the following guidelines:

1. Fork the repository and create your branch from master.
2. For added code, ensure corresponding tests are added and pass.
3. Update documentation to reflect any changes to APIs.
4. Verify that the test suite passes.
5. Ensure your code adheres to the coding standards (runs dotnet format).
6. Submit your pull request.

## Branching Strategy for Pull Requests

Please initiate pull requests from branches other than master. This approach facilitates working on multiple issues simultaneously without conflict. We recommend naming branches with a prefix that indicates the type of contribution, such as feature/, bugfix/, followed by a short description of the contribution.

## Licensing and Contributions

Contributions are accepted under the MIT License, under which the project is licensed. By submitting code changes, you agree that your contributions will be licensed under the same terms. For any concerns, feel free to reach out to the maintainers.

## Coding Style Guidelines

Before submitting a pull request, you MUST execute `dotnet format` to ensure code style consistency. If you fail to run this command, the CI pipeline will fail, and your pull request will not be merged.

### Setting up pre-commit hook

To automatically format code before every commit, run the following command in your project directory:

```sh
echo -e '#!/bin/sh\n\ndotnet format\n\nif ! git diff --quiet; then\n    echo "Code formatting changes have been made. Please review and commit them."\n    exit 1\nfi' > .git/hooks/pre-commit && chmod +x .git/hooks/pre-commit
```

## Reporting Bugs via GitHub Issues

Bugs are tracked using GitHub issues. To report a bug, open a new issue with detailed information.

## Effective Bug Reporting

A well-documented bug report significantly aids in understanding and addressing the issue. Include the following in your report:

- A concise summary and/or background.
- Detailed steps to reproduce the issue.
    - Specify steps clearly.
    - Provide sample code if possible.
- Your expected outcome.
- The actual outcome.
- Any additional notes, such as potential causes or attempted solutions that failed.
- Thorough bug reports are highly valued and contribute significantly to the improvement of the project.