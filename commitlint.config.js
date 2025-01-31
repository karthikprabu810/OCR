// commitlint.config.js
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'subject-case': [2, 'always', ['sentence-case']], // enforce sentence case for commit message
    'type-enum': [
      2,
      'always',
      [
        'feat',    // A new feature
        'fix',     // A bug fix
        'chore',   // Routine tasks
        'docs',    // Documentation changes
        'style',   // Code style changes
        'refactor', // Code restructuring
        'test',    // Adding or modifying tests
        'build',   // Build system changes
        'ci',      // CI/CD changes
        'perf'     // Performance improvements
      ]
    ],

    // Enforce that the subject (description) should not be empty
    'subject-empty': [2, 'never'],

    // Enforce a non-empty scope in commit messages (optional but useful)
    'scope-empty': [2, 'never']
  },
};

