import type { Entity, ListResponse } from './common';
import type { Role } from '@prisma/client';

export interface Employee extends Entity {
	firstName: string;
	lastName: string;
	dateOfBirth: Date;
	country: string;
	city: string;
	postalCode: string;
	streetName: string;
	streetNumber: string;
	email: string;
	phoneNumber: string | null;
	role: Role;
	passwordHash: string | null;
	isActive: boolean;
}

export interface PublicEmployee extends Omit<Employee, 'passwordHash'> { }

export interface getAllEmployeeResponse extends ListResponse<PublicEmployee> { }
export interface getEmployeeByIdResponse extends PublicEmployee { }
export interface getEmployeeRequest {
	id: number | 'me';
}

export interface LoginRequest {
	email: string;
	password: string;
}

export interface LoginResponse {
	token: string;
}
