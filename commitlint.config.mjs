export default {
  extends: ['@commitlint/config-conventional'],
  ignores: [
    (message) => /^Merge (branch|pull request|remote-tracking)/.test(message),
    (message) => /^Revert /.test(message)
  ],
  rules: {
    'header-max-length': [2, 'always', 100],
    'body-max-line-length': [0],
    'footer-max-line-length': [0],
    'subject-case': [0],
    'subject-full-stop': [0]
  }
};
