import { prisma } from '../data';
import type { MachineDailyReport } from '../types/machineReport';
import ServiceError from '../core/serviceError';

const MACHINE_REPORT_SELECT = {
	id: true,
	date: true,
	amountProduced: true,
	uptime: true,
	downtime: true,
	scrap: true,
	machine: {
		select: {
			id: true,
		},
	},
};

export const getAll = async (): Promise<MachineDailyReport[]> => {
	const machineReports = await prisma.machineDailyReport.findMany({
		select: MACHINE_REPORT_SELECT,
	});

	if (!machineReports) {
		throw ServiceError.notFound('No machine reports found');
	}

	return machineReports;
};

export const getByMachineId = async (machineId: number): Promise<MachineDailyReport[]> => {
	const machineReports = await prisma.machineDailyReport.findMany({
		where: { machineId },
		select: MACHINE_REPORT_SELECT,
	});

	if (!machineReports) {
		throw ServiceError.notFound('No machine reports found');
	}

	return machineReports;
};

export const getMetricForLastNDays = async (
	machineId: number,
	days: number,
	metric: 'amountProduced' | 'uptime' | 'downtime' | 'scrap',
) => {
	const startDate = new Date();
	startDate.setDate(startDate.getDate() - days);

	const reports = await prisma.machineDailyReport.findMany({
		where: {
			machineId,
			date: {
				gte: startDate,
			},
		},
		select: {
			date: true,
			[metric]: true, // Dynamically select the requested metric
		},
		orderBy: {
			date: 'asc',
		},
	});

	return reports.map((report) => {
		if (!report.date) {
			throw ServiceError.internalServerError('Date is undefined in report');
		}

		// Format the date only if it is defined
		const formattedDate = (report.date as Date).toISOString().split('T')[0];

		return {
			date: formattedDate,
			value: report[metric],
		};
	});
};

export const getAllMetricsForMachine = async (
	machineId: number,
	days: number,
) => {
	const startDate = new Date();
	startDate.setDate(startDate.getDate() - days);

	const reports = await prisma.machineDailyReport.findMany({
		where: {
			machineId,
			date: {
				gte: startDate,
			},
		},
		select: {
			date: true,
			amountProduced: true,
			uptime: true,
			downtime: true,
			scrap: true,
		},
		orderBy: {
			date: 'asc',
		},
	});
	const labels = reports.map((report) => {
		if (!report.date) {
			throw ServiceError.internalServerError('Date is undefined in report');
		}
		return (report.date as Date).toISOString().split('T')[0];
	});

	const series = [
		{
			name: 'Amount Produced',
			data: reports.map((report) => report.amountProduced),
		},
		{
			name: 'Uptime',
			data: reports.map((report) => report.uptime),
		},
		{
			name: 'Downtime',
			data: reports.map((report) => report.downtime),
		},
		{
			name: 'Scrap',
			data: reports.map((report) => report.scrap),
		},
	];

	return { labels, series };
};

export const getAllMetricsForSite = async (
	siteId: number,
	days: number,
) => {
	const startDate = new Date();
	startDate.setDate(startDate.getDate() - days);

	const reports = await prisma.machineDailyReport.findMany({
		where: {
			machine: {
				siteId, // Assuming machines have a `siteId` field
			},
			date: {
				gte: startDate,
			},
		},
		select: {
			machineId: true,
			date: true,
			amountProduced: true,
			uptime: true,
			downtime: true,
			scrap: true,
		},
		orderBy: {
			date: 'asc',
		},
	});

	return reports.map((report) => {
		if (!report.date) {
			throw ServiceError.internalServerError('Date is undefined in report');
		}

		const formattedDate = (report.date as Date).toISOString().split('T')[0];

		return {
			machineId: report.machineId,
			date: formattedDate,
			amountProduced: report.amountProduced,
			uptime: report.uptime,
			downtime: report.downtime,
			scrap: report.scrap,
		};
	});
};