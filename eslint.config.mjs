import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import stylistic from '@stylistic/eslint-plugin';
import jest from 'eslint-plugin-jest';

export default tseslint.config(
	eslint.configs.recommended,
	...tseslint.configs.recommended,
	{
		files: ['**/*.ts', '**/*.spec.ts'],
		ignores: ['build/'],
		plugins: {
			'@stylistic': stylistic,
		},
		rules: {
			'@stylistic/no-multiple-empty-lines': [
				'error',
				{
					max: 1,
					maxEOF: 1,
					maxBOF: 0,
				},
			],
			'@stylistic/indent': ['error', 'tab', { SwitchCase: 1 }],
			'@stylistic/quotes': ['error', 'single'],
			'@stylistic/semi': ['error', 'always'],
			'@stylistic/comma-dangle': ['error', 'always-multiline'],
			'@stylistic/max-len': [
				'error',
				{
					code: 130,
					tabWidth: 2,
				},
			],
			'@stylistic/arrow-parens': ['error', 'always'],
			'@stylistic/brace-style': ['error', '1tbs', { allowSingleLine: false }],
			'@stylistic/no-inner-declarations': 'off',
			'@typescript-eslint/no-explicit-any': 'off',
			'@typescript-eslint/consistent-type-imports': 'error',
			'@typescript-eslint/no-empty-object-type': 'off',
			'@typescript-eslint/no-unused-vars': ['error', { ignoreRestSiblings: true }],
		},
	},
	{
		files: ['**/*.spec.ts'],
		plugins: { jest },
	},
);
