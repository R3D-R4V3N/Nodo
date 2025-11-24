// src/rest/session.ts
import Router from '@koa/router';
import Joi from 'joi';
import validate from '../core/validation';
import * as employeeService from '../service/employee';
import type {
	KoaContext,
	KoaRouter,
	ShopfloorSiteContext,
	ShopfloorSiteState,
} from '../types/koa';
import type { LoginResponse, LoginRequest } from '../types/employee';
import { authDelay } from '../core/auth';

/**
 * @swagger
 * tags:
 *   name: Sessions
 *   description: User session management
 */

/**
 * @swagger
 * /api/sessions:
 *   post:
 *     summary: Try to login
 *     tags:
 *      - Sessions
 *     requestBody:
 *       description: The credentials of the user to login
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               email:
 *                 type: string
 *                 format: email
 *               password:
 *                 type: string
 *     responses:
 *       200:
 *         description: A JWT token
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 token:
 *                   type: string
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 */

const login = async (ctx: KoaContext<LoginResponse, void, LoginRequest>) => {
	const { email, password } = ctx.request.body;
	const token = await employeeService.login(email, password);

	ctx.status = 200;
	ctx.body = { token };
};
login.validationScheme = {
	body: {
		email: Joi.string().email(),
		password: Joi.string(),
	},
};

export default function installSessionRouter(parent: KoaRouter) {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({
		prefix: '/sessions',
	});

	router.post(
		'/',
		authDelay,
		validate(login.validationScheme),
		login,
	);

	parent.use(router.routes()).use(router.allowedMethods());
}
