import { Role } from '@prisma/client';
import { MachineStatus } from '@prisma/client';
import ServiceError from '../core/serviceError';
import { prisma } from '../data';
import type { Machine, MachinePreview, StartStopMachineInput, StartStopMachineOutput } from '../types/machine';

const MACHINE_SELECT = {
	id: true,
	site: {
		select: {
			id: true,
			name: true,
		},
	},
	code: true,
	location: true,
	product: true,
	status: true,
	productionStatus: true,
	uptime: true,
	technician: {
		select: {
			id: true,
			firstName: true,
			lastName: true,
		},
	},
};

const MACHINE_PREVIEW_SELECT = {
	id: true,
	location: true,
	status: true,
	productionStatus: true,
};

export const getAll = async (employeeId: number, role: Role): Promise<MachinePreview[]> => {
	let authFilter;
	if (role === Role.Technician) {
		authFilter = { technicianId: employeeId };
	} else if (role === Role.Supervisor) {
		const sites = await prisma.site.findMany({
			where: { supervisorId: employeeId },
			select: { id: true },
		});
		authFilter = { siteId: { in: sites.flatMap((s) => s.id) } };
	} else {
		authFilter = {};
	}
	return prisma.machine.findMany({
		where: authFilter,
		select: MACHINE_PREVIEW_SELECT,
	});
};

export const getById = async (id: number, employeeId: number, role: Role): Promise<Machine> => {
	const machine = await prisma.machine.findUnique({
		where: { id },
		select: MACHINE_SELECT,
	});

	if (!machine) {
		throw ServiceError.notFound('Machine not found');
	}

	let authorized;
	if (role === Role.Technician) {
		authorized = machine.technician?.id === employeeId;
	} else if (role === Role.Supervisor) {
		const site = await prisma.site.findFirst({
			where: { id: machine.site.id },
			select: { supervisorId: true },
		});
		authorized = site?.supervisorId === employeeId;
	} else {
		authorized = true;
	}

	if (!authorized) {
		throw ServiceError.forbidden('You have no access to this machine');
	}

	return machine;
};

export const startStopMachine = async (
	id: number,
	{ status }: StartStopMachineInput,
	employeeId: number,
	role: Role,
): Promise<StartStopMachineOutput> => {
	let uptime: Date | null;
	// let actionName: string;

	const machine = await prisma.machine.findUnique({
		where: { id },
		select: {
			status: true,
			site: { select: { supervisorId: true } },
			technicianId: true,
		},
	});

	if (!machine) {
		throw ServiceError.notFound('Machine not found');
	}

	const isAuthorized =
		// isAuthorized if... technician van deze machine
		role === Role.Technician && machine.technicianId === employeeId ||
		// isAuthorized if... supervisor van de site van deze machine
		role === Role.Supervisor && machine.site?.supervisorId === employeeId;

	if (!isAuthorized) {
		throw ServiceError.forbidden('You have no access to this machine');
	}

	// Map of allowed transitions for each status
	const allowedTransitions: Record<MachineStatus, MachineStatus[]> = {
		[MachineStatus.Ready]: [MachineStatus.Running, MachineStatus.Stopped],
		[MachineStatus.Stopped]: [MachineStatus.Running, MachineStatus.Maintenance],
		[MachineStatus.Running]: [MachineStatus.Stopped],
		[MachineStatus.Maintenance]: [MachineStatus.Ready],
	};

	// check if status change is allowed in the above map
	if (!allowedTransitions[machine.status]?.includes(status)) {
		throw ServiceError.conflict(
			`Cannot change machine status from '${machine.status}' to '${status}'`,
		);
	}

	// update uptime if machine is started or stopped
	if (status === MachineStatus.Running) {
		uptime = new Date();
		// actionName = 'Machine started';
	} else {
		uptime = null;
		// actionName = 'Machine stopped';
	}

	return prisma.machine.update({
		where: { id },
		data: { status, uptime },
		select: {
			id: true,
			status: true,
			uptime: true,
		},
	});

	// try {
	// 	const updatedMachine = await prisma.machine.update({
	// 		where: { id },
	// 		data: { status, uptime },
	// 		select: {
	// 			id: true,
	// 			status: true,
	// 			uptime: true,
	// 		},
	// 	});

	// 	await prisma.log.create({
	// 		data: {
	// 			action: {
	// 				connectOrCreate: {
	// 					where: { name: actionName },
	// 					create: { name: actionName },
	// 				},
	// 			},
	// 			employee: {
	// 				connect: { id: employeeId },
	// 			},
	// 			machine: {
	// 				connect: { id: id },
	// 			},
	// 		},
	// 	});

	// 	return updatedMachine;
	// } catch (error) {
	// 	throw handleDBError(error);
	// }
};
