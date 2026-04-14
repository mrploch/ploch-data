# Code Review Guidelines

This document outlines guidelines for reviewing code changes, focusing on aspects that might be missed by automated tools.

## Security Vulnerabilities

When reviewing code, look for these potential security issues:

- **Injection vulnerabilities**: SQL, Command, LDAP, XPath, or other injection flaws
- **Authentication issues**: Weak authentication mechanisms, hardcoded credentials
- **Authorization problems**: Missing or incorrect permission checks
- **Sensitive data exposure**: Unencrypted sensitive data, improper handling of secrets
- **Insecure cryptographic implementations**: Weak algorithms, improper key management
- **CSRF/XSS vulnerabilities**: Missing CSRF tokens, unescaped user input
- **Insecure deserialization**: Deserializing untrusted data without proper validation
- **Dependency vulnerabilities**: Outdated libraries with known security issues
- **Insecure file operations**: Path traversal vulnerabilities, unsafe file handling
- **Race conditions**: Time-of-check to time-of-use (TOCTOU) bugs

## Hard-to-Notice Bugs

Pay special attention to these subtle issues:

- **Off-by-one errors**: Boundary conditions in loops and array accesses
- **Null pointer dereferences**: Missing null checks before accessing objects
- **Resource leaks**: Unclosed files, connections, or other resources
- **Concurrency issues**: Race conditions, deadlocks, improper synchronization
- **Exception handling**: Swallowed exceptions, overly broad catch blocks
- **State management**: Incorrect state transitions, missing state validation
- **Edge cases**: Handling of empty collections, extreme values, or special inputs
- **Floating-point precision issues**: Equality comparisons with floating-point values
- **Internationalization bugs**: Locale-dependent operations, character encoding issues
- **Logical errors**: Incorrect boolean expressions, misplaced parentheses

## Unintended Code

Look for code that was likely not intended to be committed:

- **Debug print statements**: Console.log, System.out.println, print, etc.
- **Commented-out code**: Large blocks of commented code without explanation
- **TODO/FIXME comments**: Especially those indicating incomplete work
- **Test or mock data**: Hardcoded test values in production code
- **Temporary workarounds**: Code marked as temporary or with "hack" comments
- **Gibberish or placeholder text**: Random characters, "asdf", "test123", etc.
- **Development configuration**: Local paths, development API keys
- **Disabled functionality**: Commented-out method calls or conditionals
- **Debugging flags**: Enabled debug modes or verbose logging
- **Incomplete refactoring**: Partially renamed variables or methods

## Code Style Issues

Review for these code style problems:

- **Inconsistent naming conventions**: Mixed camelCase/snake_case, inconsistent prefixes
- **Poor code organization**: Overly long methods or classes, poor separation of concerns
- **Duplicated code**: Copy-pasted logic that should be refactored
- **Magic numbers/strings**: Unexplained literals that should be constants
- **Misleading comments**: Comments that don't match the actual code behavior
- **Inconsistent formatting**: Mixed indentation, line length violations
- **Poor variable names**: Cryptic or overly abbreviated identifiers
- **Excessive nesting**: Deeply nested conditionals or loops
- **Unused imports/variables**: Dead code that should be removed
- **Overly complex expressions**: Code that's difficult to understand at a glance

## Additional Concerns

Other issues to watch for:

- **Performance problems**: Inefficient algorithms, unnecessary computations
- **Maintainability issues**: Code that's difficult to modify or extend
- **Accessibility concerns**: UI changes that might affect accessibility
- **Backwards compatibility**: Breaking changes to public APIs
- **Error handling**: Missing or inappropriate error handling
- **Documentation**: Missing or outdated documentation
- **Test coverage**: Insufficient test coverage for new or modified code
- **Dependency management**: Unnecessary or conflicting dependencies
- **Configuration issues**: Hardcoded configuration that should be externalized
- **Compliance concerns**: Code that might violate legal or regulatory requirements
