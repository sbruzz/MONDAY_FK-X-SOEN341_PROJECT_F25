# Campus Events System - Contributing Guide

## Overview

Thank you for your interest in contributing to the Campus Events system! This guide will help you understand how to contribute effectively to the project.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Workflow](#development-workflow)
3. [Code Standards](#code-standards)
4. [Commit Guidelines](#commit-guidelines)
5. [Pull Request Process](#pull-request-process)
6. [Testing Requirements](#testing-requirements)
7. [Documentation Requirements](#documentation-requirements)

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Git
- SQLite (included with .NET)
- Code editor (Visual Studio, VS Code, or Rider)

### Setting Up Development Environment

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd MONDAY_FK-X-SOEN341_PROJECT_F25
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run database migrations:
   ```bash
   dotnet ef database update
   ```

4. Seed demo data (optional):
   - The application will automatically seed demo data on first run
   - Admin credentials: `admin@campusevents.com` / `Admin@123`

5. Run the application:
   ```bash
   dotnet run
   ```

---

## Development Workflow

### Branch Strategy

- `main`: Production-ready code
- `develop`: Integration branch for features
- Feature branches: `feature/feature-name`
- Bug fix branches: `fix/bug-description`

### Creating a Feature Branch

1. Create branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name
   ```

2. Make your changes

3. Commit with descriptive messages

4. Push and create pull request

### Code Review Process

1. All code must be reviewed before merging
2. Address review comments promptly
3. Ensure all tests pass
4. Update documentation as needed

---

## Code Standards

### Follow Existing Patterns

- Match the coding style of existing code
- Use established patterns and conventions
- Follow the [Code Style Guide](CODE_STYLE_GUIDE.md)

### Naming Conventions

- Classes: PascalCase (`EventService`)
- Methods: PascalCase (`GetEventAsync`)
- Variables: camelCase (`eventId`)
- Constants: PascalCase (`MaxPasswordLength`)

### Code Organization

- One class per file
- File name matches class name
- Group related functionality
- Use regions sparingly

### Documentation

- All public members must have XML documentation
- Include parameter descriptions
- Include return value descriptions
- Add code examples for complex methods

---

## Commit Guidelines

### Commit Message Format

```
<type>: <subject>

<body>

<footer>
```

### Commit Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Test additions or changes
- `chore`: Maintenance tasks

### Commit Message Examples

```
feat: Add carpool proximity matching

Implement proximity-based carpool matching using Haversine formula.
Adds distance calculation and nearby offer filtering.

Closes #123
```

```
fix: Resolve ticket validation race condition

Fix issue where concurrent ticket validations could cause
duplicate redemptions. Add database-level locking.

Fixes #456
```

### Commit Best Practices

- Write clear, descriptive commit messages
- Keep commits focused (one logical change per commit)
- Reference issue numbers when applicable
- Test before committing

---

## Pull Request Process

### Before Submitting

1. Ensure code follows style guide
2. All tests pass
3. Documentation is updated
4. No compiler warnings
5. Code is properly formatted

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Refactoring

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist
- [ ] Code follows style guide
- [ ] Documentation updated
- [ ] Tests pass
- [ ] No compiler warnings
```

### Review Process

1. Create pull request targeting `develop`
2. Wait for code review
3. Address review comments
4. Get approval from at least one reviewer
5. Merge after approval

---

## Testing Requirements

### Unit Tests

- Write unit tests for all new features
- Aim for high code coverage
- Test edge cases and error conditions
- Use descriptive test names

### Integration Tests

- Write integration tests for database operations
- Test complete workflows
- Clean up test data after tests

### Test Organization

- Place tests in `CampusEvents.Tests` project
- Mirror project structure in test project
- Group related tests

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test CampusEvents.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## Documentation Requirements

### Code Documentation

- All public classes, methods, properties must have XML documentation
- Include parameter descriptions
- Include return value descriptions
- Add examples for complex methods

### User Documentation

- Update user guide for new features
- Add screenshots for UI changes
- Update FAQ if needed

### Technical Documentation

- Update architecture docs for significant changes
- Update API documentation for new endpoints
- Update deployment guide for configuration changes

---

## Areas for Contribution

### High Priority

- Bug fixes
- Performance improvements
- Security enhancements
- Test coverage improvements

### Medium Priority

- New features (discuss first)
- Code refactoring
- Documentation improvements
- UI/UX enhancements

### Low Priority

- Code style improvements
- Comment improvements
- Minor optimizations

---

## Getting Help

### Questions

- Check existing documentation first
- Search existing issues
- Ask in team discussions
- Create an issue for bugs

### Reporting Bugs

Include:
- Steps to reproduce
- Expected vs actual behavior
- Environment details
- Error messages/logs
- Screenshots if applicable

### Suggesting Features

Include:
- Use case description
- Proposed solution
- Benefits
- Potential drawbacks

---

## Code of Conduct

### Be Respectful

- Be respectful of others' opinions
- Provide constructive feedback
- Accept feedback gracefully
- Focus on the code, not the person

### Be Professional

- Write clear, professional code
- Follow project conventions
- Document your work
- Test thoroughly

---

## Recognition

Contributors will be recognized in:
- Project documentation
- Release notes
- Contributor list

Thank you for contributing to the Campus Events system!

---

**Document Version**: 1.0
**Last Updated**: 2025-01-XX
**Maintained By**: Development Team

