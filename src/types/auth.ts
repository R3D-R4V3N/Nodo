import type { Role } from '@prisma/client';

export interface SessionInfo {
	employeeId: number;
	role: Role;
};
