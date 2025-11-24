import type { Entity, ListResponse } from './common';
import type { Employee } from './employee';
import type { MachinePreview } from './machine';

export interface Site extends Entity {
	name: string;
	country: string;
	city: string;
	streetName: string;
	streetNumber: string;
	postalCode: string;
	latitude: string;
	longitude: string;
	isActive: boolean;
	machines: number;
	supervisor: Pick<Employee, 'id' | 'firstName' | 'lastName'>;
}

export interface SiteWithMachines extends Omit<Site, 'machines'> {
	machines: MachinePreview[];
}

export interface SiteCreateInput {
	name: string;
	country: string;
	city: string;
	streetName: string;
	streetNumber: string;
	postalCode: string;
	latitude: string;
	longitude: string;
	isActive: boolean;
	supervisorId: number;
}

export interface UpdateSiteInput extends SiteCreateInput { }
export interface CreateSiteRequest extends SiteCreateInput { }
export interface CreateSiteResponse extends Site { }
export interface UpdateSiteRequest extends UpdateSiteInput { }
export interface UpdateSiteResponse extends Site { }

export interface GetSiteByIdResponse extends SiteWithMachines { }
export interface GetAllSitesResponse extends ListResponse<Site> { }
