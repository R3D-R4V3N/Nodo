import ServiceError from '../core/serviceError';
import { prisma } from '../data';
import type { Maintenance } from '../types/maintenance';

const MAINTENANCE_SELECT = {
	id: true,
	datePlanned: true,
	startTime: true,
	endTime: true,
	reason: true,
	maintenanceReport: true,
	comments: true,
	status: true,
	machine: {
		select: {
			id: true,
			code: true,
		},
	},
	technician: {
		select: {
			id: true,
			firstName: true,
			lastName: true,
		},
	},
};

export const getAll = async (): Promise<Maintenance[]> => {
	return prisma.maintenance.findMany({
		select: MAINTENANCE_SELECT,
	});
};

export const getById = async (id: number): Promise<Maintenance> => {
	const maintenance = await prisma.maintenance.findUnique({
		where: { id },
		select: MAINTENANCE_SELECT,
	});

	if (!maintenance) {
		throw ServiceError.notFound('Maintenance not found');
	}

	return maintenance;
};

export const getByEmployeeId = async (
	employeeId: number,
	type?: 'past' | 'planned',
): Promise<Maintenance[]> => {

	const where: any = { technicianId: employeeId };

	if (type === 'past') {
		where.AND = [
			{ datePlanned: { lt: new Date() } },
			{ endTime: { not: null } },
		];
	} else if (type === 'planned') {
		where.OR = [
			{ datePlanned: { gt: new Date() } },
			{ datePlanned: { lt: new Date() }, startTime: null },
		];
	}

	return prisma.maintenance.findMany({ where, select: MAINTENANCE_SELECT });
};

export const getByMachineId = async (machineId: number, type?: 'past' | 'planned'): Promise<Maintenance[]> => {
	const where: any = { machineId };

	if (type === 'past') {
		where.AND = [
			{ datePlanned: { lt: new Date() } },
			{ endTime: { not: null } },
		];
	} else if (type === 'planned') {
		where.OR = [
			{ datePlanned: { gt: new Date() } },
			{ datePlanned: { lt: new Date() }, startTime: null },
		];
	}

	return prisma.maintenance.findMany({ where, select: MAINTENANCE_SELECT });
};
