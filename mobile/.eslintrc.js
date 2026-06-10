module.exports = {
  root: true,
  extends: '@react-native/eslint-config/recommended',
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
    ecmaFeatures: {
      jsx: true,
    },
  },
  env: {
    es6: true,
    node: true,
    jest: true,
  },
  rules: {
    'react-native/no-inline-styles': 'warn',
    'react-hooks/exhaustive-deps': 'warn',
  },
};
