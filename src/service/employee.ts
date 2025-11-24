import { verifyPassword } from '../core/password';
import ServiceError from '../core/serviceError';
import { prisma } from '../data/index';
import type { Employee, PublicEmployee } from '../types/employee';
import jwt from 'jsonwebtoken';
import { getLogger } from '../core/logging';
import { generateJWT, verifyJWT } from '../core/jwt';
import type { SessionInfo } from '../types/auth';
import type { Role } from '@prisma/client';

const makeExposedEmployee = ({ passwordHash, ...publicFields }: Employee): PublicEmployee => ({
	...publicFields,
});

export const getAll = async (): Promise<PublicEmployee[]> => {
	const employees = await prisma.employee.findMany();
	return employees.map(makeExposedEmployee);
};

export const getById = async (id: number): Promise<PublicEmployee> => {
	const employee = await prisma.employee.findUnique({ where: { id } });

	if (!employee) {
		throw ServiceError.notFound('Employee not found');
	}

	return makeExposedEmployee(employee);
};

export const checkRole = (role: Role, allowed: Role[]): void => {
	const hasPermission = allowed.includes(role);

	if (!hasPermission) {
		throw ServiceError.forbidden(
			'You are not allowed to view this part of the application',
		);
	}
};

export const login = async (
	email: string,
	password: string,
): Promise<string> => {
	const employee = await prisma.employee.findUnique({ where: { email }});

	if (!employee) {
		throw ServiceError.unauthorized('The given email and password do not match');
	}

	if (!employee.passwordHash) {
		// TODO: is 401 oke voor nieuwe accounts zonder wachtwoord?
		throw ServiceError.unauthorized('The given email and password do not match');
	}

	if (!employee.isActive) {
		throw ServiceError.forbidden('Account is disabled');
	}

	const passwordValid = await verifyPassword(password, employee.passwordHash);

	if (!passwordValid) {
		throw ServiceError.unauthorized('The given email and password do not match');
	}

	return await generateJWT(employee);
};

export const checkAndParseSession = async (authHeader?: string): Promise<SessionInfo> => {
	if (!authHeader) {
		throw ServiceError.unauthorized('You need to be signed in');
	}

	if (!authHeader.startsWith('Bearer ')) {
		throw ServiceError.unauthorized('Invalid authentication token');
	}

	const authToken = authHeader.substring(7); // drop 'Bearer ' to get token

	try {
		const { role, sub } = await verifyJWT(authToken);

		return {
			employeeId: Number(sub),
			role,
		};
	} catch (error: any) {
		getLogger().error(error.message, { error });

		if (error instanceof jwt.TokenExpiredError) {
			throw ServiceError.unauthorized('The token has expired');
		} else if (error instanceof jwt.JsonWebTokenError) {
			throw ServiceError.unauthorized(`Invalid authentication token: ${error.message}`);
		} else {
			throw ServiceError.unauthorized(error.message);
		}
	}
};
