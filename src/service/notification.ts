import ServiceError from '../core/serviceError';
import { prisma } from '../data';
import type { 
	Notification, 
	NotificationPreview,
	UpdateNotificationStatusInput,
	UpdateNotificationStatusResponse,
} from '../types/notification';
import handleDBError from './_handleDBError';

const NOTIFICATION_SELECT = {
	id: true,
	title: true,
	message: true,
	status: true,
	createdAt: true,
	employee: {
		select: {
			id: true,
			firstName: true,
			lastName: true,
		},
	},
	machine: {
		select: {
			id: true,
			code: true,
		},
	},
	site: {
		select: {
			id: true,
			name: true,
		},
	},
	maintenance: {
		select: {
			id: true,
			datePlanned: true,
		},
	},
};

const NOTIFICATION_PREVIEW_SELECT = {
	id: true,
	title: true,
	message: true,
	status: true,
	createdAt: true,
};

export const getAll = async (employeeId: number): Promise<NotificationPreview[]> => {
	return prisma.notification.findMany({
		where: { employeeId },
		select: NOTIFICATION_PREVIEW_SELECT,
		orderBy: { createdAt: 'desc' },
	});
};

export const getById = async (id: number, employeeId: number): Promise<Notification> => {
	const notification = await prisma.notification.findUnique({
		where: { id },
		select: NOTIFICATION_SELECT,
	});

	if (!notification) {
		throw ServiceError.notFound('Notification not found');
	}

	if (notification.employee.id !== employeeId) {
		throw ServiceError.forbidden('You have no access to this notification');
	}

	return notification;
};

export const updateStatus = async (
	id: number,
	{ status }: UpdateNotificationStatusInput,
	employeeId: number,
): Promise<UpdateNotificationStatusResponse> => {
	const notification = await prisma.notification.findUnique({
		where: { id },
		select: { employeeId: true },
	});

	if (!notification) {
		throw ServiceError.notFound('Notification not found');
	}

	if (notification.employeeId !== employeeId) {
		throw ServiceError.forbidden('You have no access to this notification');
	}

	try {
		return prisma.notification.update({
			where: { id },
			data: { status },
			select: { id: true, status: true },
		});
	} catch (error) {
		throw handleDBError(error);
	}
};

export const create = async (
	employeeId: number,
	title: string,
	message: string,
	options?: {
		machineId?: number | null; 
		siteId?: number | null; 
		maintenanceId?: number | null; 
	},
): Promise<void> => {
	try {
		await prisma.notification.create({
			data: {
				title,
				message,
				status: 'New',
				employeeId,
				machineId: options?.machineId ?? null,
				siteId: options?.siteId ?? null, 
				maintenanceId: options?.maintenanceId ?? null, 
			},
		});
	} catch (error) {
		throw handleDBError(error);
	}
};