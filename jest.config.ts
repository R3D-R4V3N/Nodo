import type { Config } from 'jest';

const config: Config = {
	collectCoverageFrom: [
		'./src/service/**/*.ts',
		'./src/routes/**/*.ts',
	],
	coverageDirectory: '__tests__/coverage',

	// Indicates which provider should be used to instrument code for coverage
	coverageProvider: 'v8',

	// A preset that is used as a base for Jest's configuration
	preset: 'ts-jest',

	// The glob patterns Jest uses to detect test files
	testMatch: [
		'**/__tests__/**/?(*.)+(spec|test).[jt]s?(x)',
	],
};

export default config;