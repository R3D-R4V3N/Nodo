import type { Entity, ListResponse } from './common';
import type { Machine } from './machine';

export interface MachineDailyReport extends Entity {
	machine: Pick<Machine, 'id'>;
	date: Date;
	amountProduced: number;
	uptime: number;
	downtime: number;
	scrap: number;
}

export interface GetMachineMetricRequest {
	machineId: number;
	metric: 'amountProduced' | 'uptime' | 'downtime' | 'scrap';
}

export type GetMachineMetricResponse = {
	machineId: number;
	metric: 'amountProduced' | 'uptime' | 'downtime' | 'scrap';
	days: number;
	labels: string[];
	values: number[];
};

// omit metric from getmachinerequest
export interface getAllMetricsForMachine extends Omit<GetMachineMetricRequest, 'metric'> { }
export interface getAllMetricsForSiteRequest {
	siteId: number;
}
export interface getAllMetricsForSiteResponse {
	siteId: number;
	days: number;
	metrics: {};
}

export interface getAllMetricsForMachineResponse {
	machineId: number;
	days: number;
	metrics: {};
}

export interface getAllMachineDailyReportResponse extends ListResponse<MachineDailyReport> { }
export interface getMachineDailyReportByMachineIdResponse extends ListResponse<MachineDailyReport> { }
