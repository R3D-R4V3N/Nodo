import config from 'config';
import bodyParser from 'koa-bodyparser';
import koaCors from '@koa/cors';
import koaHelmet from 'koa-helmet';
import type { KoaApplication } from '../types/koa';
import { getLogger } from './logging';
import ServiceError from './serviceError';
import { koaSwagger } from 'koa2-swagger-ui';
import swaggerJsdoc from 'swagger-jsdoc';
import swaggerOptions from '../../swagger.config';

const CORS_ORIGINS = config.get<string[]>('cors.origins');
const CORS_MAX_AGE = config.get<number>('cors.maxAge');
const NODE_ENV = config.get<string>('env');

const isDevelopment = NODE_ENV === 'development';

export default function installMiddlewares(app: KoaApplication) {

	// allows proxies
	app.proxy = true;

	// Logging Middleware
	app.use(async (ctx, next) => {
		getLogger().info(`⏩ ${ctx.method} ${ctx.url}`);

		const getStatusEmoji = () => {
			if (ctx.status >= 500) return '💀';
			if (ctx.status >= 400) return '❌';
			if (ctx.status >= 300) return '🔀';
			if (ctx.status >= 200) return '✅';
			return '🔄';
		};

		await next();

		getLogger().info(
			`${getStatusEmoji()} ${ctx.method} ${ctx.status} ${ctx.url}`,
		);
	});

	// Error Handling Middleware
	app.use(async (ctx, next) => {
		try {
			await next();
		} catch (error: any) {
			getLogger().error('Error occured while handling a request', { error });

			let statusCode = error.status || 500;
			const errorBody = {
				code: error.code || 'INTERNAL_SERVER_ERROR',
				message: NODE_ENV !== 'production' ? error.message : 'Unexpected error occurred. Please try again later.',
				details: error.details,
				stack: NODE_ENV !== 'production' ? error.stack : undefined,
			};

			if (error instanceof ServiceError) {
				errorBody.message = error.message;

				if (error.isNotFound) {
					statusCode = 404;
				}

				if (error.isValidationFailed) {
					statusCode = 400;
				}

				if (error.isUnauthorized) {
					statusCode = 401;
				}

				if (error.isForbidden) {
					statusCode = 403;
				}

				if (error.isConflict) {
					statusCode = 409;
				}
			}

			ctx.status = statusCode;
			ctx.body = errorBody;
		}
	});

	app.use(bodyParser());

	app.use(
		koaCors({
			origin: (ctx) => {
				const requestOrigin = ctx.request.header.origin!;
				if (CORS_ORIGINS.includes(requestOrigin)) {
					return requestOrigin;
				}
				return ''; // Return an empty string to indicate the origin is not allowed
			},
			allowHeaders: ['Accept', 'Content-Type', 'Authorization', 'Cookie'],
			credentials: true,
			maxAge: CORS_MAX_AGE,
		}),
	);

	app.use(
		koaHelmet({
			contentSecurityPolicy: !isDevelopment,
		}),
	);

	const spec = swaggerJsdoc(swaggerOptions) as Record<string, unknown>;

	app.use(
		koaSwagger({
			routePrefix: '/swagger',
			specPrefix: '/swagger.json',
			exposeSpec: true,
			swaggerOptions: { spec },
		}),
	);

	// 404 Handler
	app.use(async (ctx, next) => {
		await next();

		if (ctx.status === 404) {
			ctx.status = 404;
			ctx.body = {
				code: 'NOT_FOUND',
				message: `Unknown resource: ${ctx.url}`,
			};
		}
	});
}
