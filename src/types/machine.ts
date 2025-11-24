import type { Employee } from './employee';
import type { Site } from './site';
import type { Entity, ListResponse } from './common';
import type { MachineStatus, ProductionStatus } from '@prisma/client';
import type { Product } from './product';

export interface Machine extends Entity {
	code: string;
	location: string;
	product: Product;
	uptime: Date | null;
	site: Pick<Site, 'id' | 'name'>;
	technician: Pick<Employee, 'id' | 'firstName' | 'lastName'> | null;
	status: MachineStatus;
	productionStatus: ProductionStatus;
}

export interface MachinePreview extends Pick<
	Machine, 'id' | 'location' | 'status' | 'productionStatus'
> { }

export interface GetMachineByIdResponse extends Machine { }
export interface GetMachinesResponse extends ListResponse<MachinePreview> { }

export interface StartStopMachineInput extends Pick<Machine, 'status'> { }
export interface StartStopMachineOutput extends Pick<Machine, 'id' | 'status' | 'uptime'> { }

export interface StartStopMachineRequest extends StartStopMachineInput { }
export interface StartStopMachineResponse extends StartStopMachineOutput { }
