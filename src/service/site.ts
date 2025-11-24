import { Role } from '@prisma/client';
import ServiceError from '../core/serviceError';
import { prisma } from '../data/index';
import type { Site, SiteCreateInput, SiteWithMachines } from '../types/site';
import handleDBError from './_handleDBError';

const SITE_SELECT = {
	id: true,
	name: true,
	country: true,
	city: true,
	streetName: true,
	streetNumber: true,
	postalCode: true,
	latitude: true,
	longitude: true,
	isActive: true,
	supervisor: {
		select: {
			id: true,
			firstName: true,
			lastName: true,
		},
	},
};

const MACHINE_SELECT = {
	id: true,
	location: true,
	status: true,
	productionStatus: true,
};

export const getAll = async (employeeId: number, role: Role): Promise<Site[]> => {
	const authFilter = role === Role.Supervisor ? { supervisorId: employeeId } : {};
	const sites = await prisma.site.findMany({
		where: {
			isActive: true,
			...authFilter,
		},
		include: {
			_count: {
				select: { machines: true },
			},
			supervisor: {
				select: {
					id: true,
					firstName: true,
					lastName: true,
				},
			},
		},
	});

	return sites.map(({ _count, ...rest }) => ({
		...rest,
		machines: _count.machines,
	}));
};

export const getById = async (
	siteId: number,
	employeeId: number,
	role: Role,
): Promise<SiteWithMachines> => {

	const site = await prisma.site.findUnique({
		where: { id: siteId, AND: { isActive: true } },
		select: {
			...SITE_SELECT,
			machines: {
				where: role === Role.Technician ? { technicianId: employeeId } : {},
				select: MACHINE_SELECT,
			},
		},
	});

	if (!site) {
		throw ServiceError.notFound('Site not found');
	}

	const isAuthorized =
		role === Role.Technician && site.machines.length !== 0 ||
		role === Role.Supervisor && site.supervisor.id === employeeId ||
		role === Role.Manager || role === Role.Administrator;

	if (!isAuthorized) {
		throw ServiceError.forbidden('You have no access to this site');
	}

	return site;
};

export const create = async (site: SiteCreateInput /*, employeeId: number*/): Promise<Site> => {
	try {
		const createdSite = await prisma.site.create({
			data: site,
			select: SITE_SELECT,
		});

		// const actionName = 'Site created';
		// await prisma.log.create({
		// 	data: {
		// 		action: {
		// 			connectOrCreate: {
		// 				where: { name: actionName },
		// 				create: { name: actionName },
		// 			},
		// 		},
		// 		employee: {
		// 			connect: { id: employeeId },
		// 		},
		// 		site: {
		// 			connect: { id: createdSite.id },
		// 		},
		// 	},
		// });

		return { ...createdSite, machines: 0 };
	} catch (error) {
		throw handleDBError(error);
	}
};

export const update = async (id: number, site: SiteCreateInput /*, employeeId: number*/): Promise<Site> => {
	try {
		const updatedSite = await prisma.site.update({
			where: { id },
			data: site,
			include: {
				_count: {
					select: { machines: true },
				},
				supervisor: {
					select: {
						id: true,
						firstName: true,
						lastName: true,
					},
				},
			},
		});

		// const actionName = 'Site updated';
		// await prisma.log.create({
		// 	data: {
		// 		action: {
		// 			connectOrCreate: {
		// 				where: { name: actionName },
		// 				create: { name: actionName },
		// 			},
		// 		},
		// 		employee: {
		// 			connect: { id: employeeId },
		// 		},
		// 		site: {
		// 			connect: { id: updatedSite.id },
		// 		},
		// 		details: JSON.stringify(site),
		// 	},
		// });

		return {
			...updatedSite,
			machines: updatedSite._count.machines,
		};
	} catch (error) {
		throw handleDBError(error);
	}
};

export const remove = async (id: number /*, employeeId: number*/): Promise<void> => {
	try {
		/*const site = */await prisma.site.update({
			where: { id },
			data: { isActive: false },
		});

		// const actionName = 'Site removed';
		// await prisma.log.create({
		// 	data: {
		// 		action: {
		// 			connectOrCreate: {
		// 				where: { name: actionName },
		// 				create: { name: actionName },
		// 			},
		// 		},
		// 		employee: {
		// 			connect: { id: employeeId },
		// 		},
		// 		site: {
		// 			connect: { id: site.id },
		// 		},
		// 	},
		// });

	} catch (error) {
		throw handleDBError(error);
	}
};