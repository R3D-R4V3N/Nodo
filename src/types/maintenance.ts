import type { Entity, ListResponse } from './common';
import type { Machine } from './machine';
import type { Employee } from './employee';
import type { MaintenanceStatus } from '@prisma/client';

export interface Maintenance extends Entity {
	datePlanned: Date;
	startTime: Date | null;
	endTime: Date | null;
	reason: string;
	maintenanceReport: string | null;
	comments: string | null;
	status: MaintenanceStatus;
	machine: Pick<Machine, 'id' | 'code'>;
	technician: Pick<Employee, 'id' | 'firstName' | 'lastName'>;
}

export interface getMaintenanceByEmployeeIdRequest {
	id: number | 'me';
}

export interface GetAllMaintenanceResponse extends ListResponse<Maintenance> { }
export interface GetMaintenanceByIdResponse extends Maintenance { }
export interface getMaintenanceByMachineIdResponse extends ListResponse<MaintenanceByMachineId> { }
export interface MaintenanceByMachineId extends Omit<Maintenance, 'machine'> { }
export interface GetMaintenanceByMachineIdResponse extends GetAllMaintenanceResponse { }
export interface GetMaintenanceByEmployeeIdResponse extends GetAllMaintenanceResponse { }