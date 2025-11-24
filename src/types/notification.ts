import type { Employee } from './employee';
import type { Site } from './site';
import type { Machine } from './machine';
import type { Maintenance } from './maintenance';
import type { Entity, ListResponse } from './common';
import type { NotificationStatus } from '@prisma/client';

export interface Notification extends Entity {
	title: string;
	message: string;
	status: NotificationStatus;
	createdAt: Date;
	employee: Pick<Employee, 'id' | 'firstName' | 'lastName'>;
	machine?: Pick<Machine, 'id' | 'code'> | null; 
	site?: Pick<Site, 'id' | 'name'> | null;
	maintenance?: Pick<Maintenance, 'id' | 'datePlanned'> | null; 
}

export interface NotificationPreview extends Pick<
	Notification, 
    'id' | 'title' | 'message' | 'status' | 'createdAt'
> { }

export interface GetNotificationByIdResponse extends Notification { }
export interface GetNotificationsResponse extends ListResponse<NotificationPreview> { }

export interface UpdateNotificationStatusInput {
	status: NotificationStatus;
}

export interface UpdateNotificationStatusResponse extends Pick<
	Notification, 
    'id' | 'status'
> { }