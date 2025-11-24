import Router from '@koa/router';
import type { KoaContext, KoaRouter, ShopfloorSiteContext, ShopfloorSiteState } from '../types/koa';
import * as machineReportService from '../service/machineReport';
import type {
	getAllMachineDailyReportResponse,
	getAllMetricsForMachine,
	getAllMetricsForMachineResponse,
	getAllMetricsForSiteRequest,
	getAllMetricsForSiteResponse,
	GetMachineMetricRequest,
	GetMachineMetricResponse,
} from '../types/machineReport';
import Joi from 'joi';

/**
 * @swagger
 * tags:
 *   name: MachineReports
 *   description: Represents KPI reports of machines
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     MachineDailyReport:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - machine
 *             - date
 *             - amountProduced
 *             - uptime
 *             - downtime
 *             - scrap
 *           properties:
 *             machine:
 *               type: integer
 *               minimum: 1
 *             date:
 *               type: string
 *               format: date
 *             amountProduced:
 *               type: integer
 *               minimum: 0
 *             uptime:
 *               type: integer
 *               minimum: 0
 *             downtime:
 *               type: integer
 *               minimum: 0
 *             scrap:
 *               type: integer
 *               minimum: 0
 * 
 *     MachineDailyReportsList:
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/MachineDailyReport"
 * 
 *     MetricType:
 *       type: string
 *       enum: [amountProduced, uptime, downtime, scrap]
 * 
 *     MachineMetric:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - machineId
 *             - metric
 *             - days
 *             - labels
 *             - values
 *           properties:
 *             machineId:
 *               type: integer
 *               mininum: 1
 *             metric:
 *               $ref: "#/components/schemas/MetricType"
 *             days:
 *               type: integer
 *               minimum: 1
 *             labels:
 *               type: array
 *               items:
 *                 type: string
 *             values:
 *               type: array
 *               items:
 *                 type: number
 */

/**
 * @swagger
 * /api/machinereports:
 *   get:
 *     summary: Get all daily machine reports
 *     tags:
 *      - MachineReports
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of machine reports
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MachineDailyReportsList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 */
const getAllMachineDailyReports = async (ctx: KoaContext<getAllMachineDailyReportResponse>) => {
	const reports = await machineReportService.getAll();
	ctx.body = {
		items: reports,
	};
};
getAllMachineDailyReports.validationScheme = null;

/**
 * @swagger
 * /api/{machineId}/metric/{metric}?timeframe={timeframe}:
 *   get:
 *     summary: Get a single metric of a machine for the last X days
 *     tags:
 *      - MachineReports
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - name: machineId
 *         in: path
 *         required: true
 *         description: ID of the machine
 *         schema:
 *           type: integer
 *           minimum: 1
 *       - name: metric
 *         in: path
 *         required: true
 *         description: Type of metric
 *         schema:
 *           $ref: "#/components/schemas/MetricType"
 *       - name: timeframe
 *         in: query
 *         required: true
 *         description: Choose how many days
 *         schema:
 *           type: integer
 *           minimum: 1
 *     responses:
 *       200:
 *         description: List of daily reports of the requested machine
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MachineMetric"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getMachineMetric = async (ctx: KoaContext<GetMachineMetricResponse, GetMachineMetricRequest>) => {
	const { machineId, metric } = ctx.params;
	const days = Number(ctx.query.timeframe);

	const data = await machineReportService.getMetricForLastNDays(
		Number(machineId),
		days,
		metric,
	);

	// Extract the labels and values from the data returned by the service
	const labels = data.map((entry) => entry.date || 'Unknown date');
	const values = data.map((entry) => entry.value ?? 0);

	// Respond with the formatted data
	ctx.body = {
		machineId: Number(machineId),
		metric,
		days,
		labels,
		values,
	};
};
getMachineMetric.validationScheme = Joi.object({
	machineId: Joi.number().integer().required().min(1),
	metric: Joi.string().valid('amountProduced', 'uptime', 'downtime', 'scrap').required(),
	timeframe: Joi.number().integer().min(1).required(),
});

const getAllMetricsForMachine = async (ctx: KoaContext<getAllMetricsForMachineResponse, getAllMetricsForMachine>) => {
	const { machineId } = ctx.params;
	const days = Number(ctx.query.timeframe || 7); // Default to 7 days if not provided

	const data = await machineReportService.getAllMetricsForMachine(Number(machineId), days);

	ctx.body = {
		machineId: Number(machineId),
		days,
		metrics: data,
	};
};
//! error when I try to add a validation scheme

// Property 'validationScheme' does not exist on type 
// '(ctx: KoaContext<getAllMetricsForMachineResponse, getAllMetricsForMachine>) => Promise<void>'.ts(2339)

/* getAllMetricsForMachine.validationScheme = {
	params: {
		machineId: Joi.number().integer().required().min(1),
	},
};  */

const getAllMetricsForSite = async (ctx: KoaContext<getAllMetricsForSiteResponse, getAllMetricsForSiteRequest>) => {
	const { siteId } = ctx.params;
	const days = Number(ctx.query.timeframe || 7); // Default to 7 days if not provided

	const data = await machineReportService.getAllMetricsForSite(Number(siteId), days);

	ctx.body = {
		siteId: Number(siteId),
		days,
		metrics: data,
	};
};
//! error when I try to add a validation scheme

// Property 'validationScheme' does not exist on type 
// '(ctx: KoaContext<getAllMetricsForSiteRequest, getAllMetricsForSiteResponse>) => Promise<void>'.

/* getAllMetricsForSite.validationScheme = Joi.object({
	siteId: Joi.number().integer().required().min(1),
	timeframe: Joi.number().integer().min(1).optional(),
}); */

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({
		prefix: '/machinereports',
	});

	router.get('/:machineId/metrics/:metric', getMachineMetric);
	router.get('/:machineId/metrics', getAllMetricsForMachine);
	router.get('/site/:siteId/metrics', getAllMetricsForSite);

	parent.use(router.routes()).use(router.allowedMethods());
};
