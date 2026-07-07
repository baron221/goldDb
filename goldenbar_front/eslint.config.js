import eslintPluginVue from 'eslint-plugin-vue';
import eslintPluginTypeScript from '@typescript-eslint/eslint-plugin';
import eslintPluginImport from 'eslint-plugin-import';
import tsParser from '@typescript-eslint/parser';
import js from '@eslint/js';
import vueEslintParser from 'vue-eslint-parser';

export default [

  { ignores: ['node_modules/**', 'dist/**', '.vscode/*'] },

  js.configs.recommended,

  ...eslintPluginVue.configs['flat/essential'],

  ...eslintPluginTypeScript.configs['flat/recommended'],

  {
    'languageOptions': {
      'globals': {
        'browser': true,
        'node': true,
        'es2021': true,
        'process': 'readonly',
        'defineEmits': 'readonly',
        'defineProps': 'readonly',
        'defineExpose': 'readonly',
        'withDefaults': 'readonly',
        'WeixinJSBridge': 'readonly',
        'ElLoading': 'readonly',
        'ElMessage': 'readonly',
        'ElMessageBox': 'readonly',
        'ElNotification': 'readonly'
      },
      'parser': vueEslintParser,
      'parserOptions': {
        'parser': tsParser,
        'ecmaVersion': 'latest',
        'sourceType': 'module',
        'jsxPragma': 'React',
        'ecmaFeatures': {
          'jsx': true,
          'tsx': true
        }
      }
    },
    'plugins': {
      'vue': eslintPluginVue,
      '@typescript-eslint': eslintPluginTypeScript,
      'import': eslintPluginImport
    },
    'rules': {
      'vue/no-multiple-template-root': 'off',
      'vue/no-v-html': 'off',
      'vue/multi-word-component-names': 'off',
      'vue/multiline-html-element-content-newline': 'off',
      'accessor-pairs': 'error',
      'arrow-spacing': ['error', {
        'before': true,
        'after': true
      }],
      'block-spacing': ['error', 'always'],
      'brace-style': ['error', '1tbs', {
        'allowSingleLine': true
      }],
      'camelcase': ['off', {
        'properties': 'always'
      }],
      'comma-dangle': ['error', 'never'],
      'comma-spacing': ['error', {
        'before': false,
        'after': true
      }],
      'comma-style': ['error', 'last'],
      'constructor-super': 'error',
      'curly': ['error', 'multi-line'],
      'dot-location': ['error', 'property'],
      'eol-last': 'error',
      'eqeqeq': ['error', 'always', { 'null': 'ignore' }],
      'generator-star-spacing': ['error', {
        'before': true,
        'after': true
      }],
      'handle-callback-err': ['error', '^(err|error)$'],
      'indent': ['error', 2, {
        'SwitchCase': 1
      }],
      'jsx-quotes': ['error', 'prefer-single'],
      'key-spacing': ['error', {
        'beforeColon': false,
        'afterColon': true
      }],
      'keyword-spacing': ['error', {
        'before': true,
        'after': true
      }],
      'new-cap': ['error', {
        'newIsCap': true,
        'capIsNew': false
      }],
      'new-parens': 'error',
      'no-array-constructor': 'error',
      'no-caller': 'error',
      'no-console': 'off',
      'no-class-assign': 'error',
      'no-cond-assign': 'error',
      'no-const-assign': 'error',
      'no-control-regex': 'off',
      'no-delete-var': 'error',
      'no-dupe-args': 'error',
      'no-dupe-class-members': 'error',
      'no-dupe-keys': 'error',
      'no-duplicate-case': 'error',
      'no-empty-character-class': 'error',
      'no-empty-pattern': 'error',
      'no-eval': 'error',
      'no-ex-assign': 'error',
      'no-extend-native': 'error',
      'no-extra-bind': 'error',
      'no-extra-boolean-cast': 'error',
      'no-extra-parens': ['error', 'functions'],
      'no-fallthrough': 'error',
      'no-floating-decimal': 'error',
      'no-func-assign': 'error',
      'no-implied-eval': 'error',
      'no-inner-declarations': ['error', 'functions'],
      'no-invalid-regexp': 'error',
      'no-irregular-whitespace': 'error',
      'no-iterator': 'error',
      'no-label-var': 'error',
      'no-labels': ['error', {
        'allowLoop': false,
        'allowSwitch': false
      }],
      'no-lone-blocks': 'error',
      'no-mixed-spaces-and-tabs': 'error',
      'no-multi-spaces': 'error',
      'no-multi-str': 'error',
      'no-multiple-empty-lines': ['error', {
        'max': 1
      }],
      'no-native-reassign': 'error',
      'no-negated-in-lhs': 'error',
      'no-new-object': 'error',
      'no-new-require': 'error',
      'no-new-symbol': 'error',
      'no-new-wrappers': 'error',
      'no-obj-calls': 'error',
      'no-octal': 'error',
      'no-octal-escape': 'error',
      'no-path-concat': 'error',
      'no-proto': 'error',
      'no-redeclare': 'error',
      'no-regex-spaces': 'error',
      'no-return-assign': ['error', 'except-parens'],
      'no-self-assign': 'error',
      'no-self-compare': 'error',
      'no-sequences': 'error',
      'no-shadow-restricted-names': 'error',
      'no-spaced-func': 'error',
      'no-sparse-arrays': 'error',
      'no-this-before-super': 'error',
      'no-throw-literal': 'error',
      'no-trailing-spaces': 'error',
      'no-undef': 'error',
      'no-undef-init': 'error',
      'no-unexpected-multiline': 'error',
      'no-unmodified-loop-condition': 'error',
      'no-unneeded-ternary': ['error', {
        'defaultAssignment': false
      }],
      'no-unreachable': 'error',
      'no-unsafe-finally': 'error',
      'no-useless-call': 'error',
      'no-useless-computed-key': 'error',
      'no-useless-constructor': 'error',
      'no-useless-escape': 'off',
      'no-whitespace-before-property': 'error',
      'no-with': 'error',
      'one-var': ['error', {
        'initialized': 'never'
      }],
      'operator-linebreak': ['error', 'after', {
        'overrides': {
          '?': 'before',
          ':': 'before'
        }
      }],
      'padded-blocks': ['error', 'never'],
      'quotes': ['error', 'single', {
        'avoidEscape': true,
        'allowTemplateLiterals': true
      }],
      'semi': ['error', 'always'],
      'semi-spacing': ['error', {
        'before': false,
        'after': true
      }],
      'space-before-blocks': ['error', 'always'],
      'space-before-function-paren': 'off',
      'space-in-parens': ['error', 'never'],
      'space-infix-ops': 'error',
      'space-unary-ops': ['error', {
        'words': true,
        'nonwords': false
      }],
      'spaced-comment': ['error', 'always', {
        'markers': ['global', 'globals', 'eslint', 'eslint-disable', '*package', '!', ',']
      }],
      'template-curly-spacing': ['error', 'never'],
      'use-isnan': 'error',
      'valid-typeof': 'error',
      'vue/html-indent': ['error', 2],
      'vue/max-attributes-per-line': 'off',
      'vue/attribute-hyphenation': ['error', 'always'],
      'vue/order-in-components': 'error',
      'vue/singleline-html-element-content-newline': 'off',

      'wrap-iife': ['error', 'any'],
      'yield-star-spacing': ['error', 'both'],
      'yoda': ['error', 'never'],
      'prefer-const': 'error',
      'no-debugger': 'off',
      '@typescript-eslint/no-explicit-any': 'off',
      '@typescript-eslint/no-unused-vars': 'off',
      'no-unused-vars': 'off',
      'object-curly-spacing': ['error', 'always', {
        objectsInObjects: false
      }],
      'array-bracket-spacing': ['error', 'never']
    }
  }
];
