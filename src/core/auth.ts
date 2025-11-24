import type { Next } from 'koa';
import type { KoaContext } from '../types/koa';
import * as employeeService from '../service/employee';
import config from 'config';
import type { Role } from '@prisma/client';

const AUTH_MAX_DELAY = config.get<number>('auth.maxDelay');

export const authDelay = async (_: KoaContext, next: Next) => {
	await new Promise((resolve) => {
		const delay = Math.round(Math.random() * AUTH_MAX_DELAY);
		setTimeout(resolve, delay);
	});
	return next();
};

export const requireAuthentication = async (ctx: KoaContext, next: Next) => {
	const { authorization } = ctx.headers;

	ctx.state.session = await employeeService.checkAndParseSession(authorization);

	return next();
};

export const makeAllowRoles = (allowed: Role[]) => async (ctx: KoaContext, next: Next) => {
	const { role } = ctx.state.session;

	employeeService.checkRole(role, allowed);

	return next();
};
