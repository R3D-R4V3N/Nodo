export default {
	failOnErrors: true,
	apis: ['./src/rest/*.ts'],
	withCredentials: true,
	definition: {
		openapi: '3.0.0',
		info: {
			title: 'Shopfloor API',
			version: '0.1.0',
			description:
				'This is a CRUD API application made with Koa and documented with Swagger',
			license: {
				name: 'MIT',
				url: 'https://spdx.org/licenses/MIT.html',
			},
		},
		servers: [{ url: 'http://localhost:9000/' }],
	},
};