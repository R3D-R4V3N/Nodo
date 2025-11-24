import Router from '@koa/router';
import type { KoaContext, KoaRouter, ShopfloorSiteContext, ShopfloorSiteState } from '../types/koa';
import type {
	GetMachinesResponse,
	GetMachineByIdResponse,
	StartStopMachineResponse,
	StartStopMachineRequest,
} from '../types/machine';
import * as machineService from '../service/machine';
import * as machineReportService from '../service/machineReport';
import type { IdParams } from '../types/common';
import validate from '../core/validation';
import Joi from 'joi';
import { MachineStatus } from '@prisma/client';
import { requireAuthentication } from '../core/auth';
import type { getMachineDailyReportByMachineIdResponse } from '../types/machineReport';

/**
 * @swagger
 * tags:
 *   name: Machines
 *   description: Represents a machine
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     MachineStatus:
 *       type: string
 *       enum: [Running, Maintenance, Stopped, Ready]
 * 
 *     ProductionStatus:
 *       type: string
 *       enum: [Healthy, Critical, Failing]
 * 
 *     Product:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - info
 *           properties:
 *             info:
 *               type: string
 * 
 *     Machine:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - location
 *             - status
 *             - productionStatus
 *           properties:
 *             location:
 *               type: string
 *             status:
 *               $ref: "#/components/schemas/MachineStatus"
 *             productionStatus:
 *               $ref: "#/components/schemas/ProductionStatus"
 *           example:
 *             id: 1
 *             location: "Assembly Line 1"
 *             status: "Running"
 *             productionStatus: "Healthy"
 * 
 *     MachinesList:
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/Machine"
 *
 *     MachineDetail:
 *       allOf:
 *         - $ref: "#/components/schemas/Machine"
 *         - type: object
 *           required:
 *             - code
 *             - product
 *             - site
 *           properties:
 *             code:
 *               type: "string"
 *             uptime:
 *               type: "string"
 *               format: date-time
 *               nullable: true
 *             product:
 *               $ref: "#/components/schemas/Product"
 *             site:
 *               $ref: "#/components/schemas/Site"
 *             technician:
 *               $ref: "#/components/schemas/Employee"
 *               nullable: true
 *
 *           example:
 *             id: 1
 *             code: "MACH-001"
 *             location: "Assembly Line 1"
 *             status: "Running"
 *             productionStatus: "Healthy"
 *             product:
 *               id: 1
 *               info: "European Widgets - Model A"
 *             site:
 *               id: 1
 *               name: "Berlin Plant"
 *             technician:
 *               id: 1
 *               firstName: "Bob"
 *               lastName: "Dupont"
 *
 *   requestBodies:
 *     MachineStatus:
 *       description: The new status for the machine
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               status:
 *                 $ref: "#/components/schemas/MachineStatus"
 */

/**
 * @swagger
 * /api/machines:
 *   get:
 *     summary: Get all machines
 *     tags:
 *      - Machines
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of machines
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MachinesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 */
const getAllMachines = async (ctx: KoaContext<GetMachinesResponse>) => {
	const machines = await machineService.getAll(
		ctx.state.session.employeeId,
		ctx.state.session.role,
	);
	ctx.body = {
		items: machines,
	};
};
getAllMachines.validationScheme = {
	query: {
		site: Joi.number().integer().positive().optional(),
	},
};

/**
 * @swagger
 * /api/machines/{id}:
 *   get:
 *     summary: Get a single machine
 *     tags:
 *      - Machines
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       200:
 *         description: The requested machine
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MachineDetail"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getMachineById = async (ctx: KoaContext<GetMachineByIdResponse, IdParams>) => {
	const machine = await machineService.getById(
		ctx.params.id,
		ctx.state.session.employeeId,
		ctx.state.session.role,
	);
	ctx.body = machine;
};
getMachineById.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

/**
 * @swagger
 * /api/machines/{id}/reports:
 *   get:
 *     summary: Get all daily reports of a single machine
 *     tags:
 *      - Machines
 *      - MachineReports
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       200:
 *         description: List of daily reports of the requested machine
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MachineDailyReportsList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getDailyReport = async (ctx: KoaContext<getMachineDailyReportByMachineIdResponse, IdParams>) => {
	const reports = await machineReportService.getByMachineId(ctx.params.id);
	ctx.body = {
		items: reports,
	};
};
getDailyReport.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

/**
 * @swagger
 * /api/machines/{id}:
 *   patch:
 *     summary: Start or stop the machine
 *     tags:
 *      - Machines
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     requestBody:
 *       $ref: "#/components/requestBodies/MachineStatus"
 *     responses:
 *       200:
 *         description: The updated status and uptime
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 status:
 *                   $ref: "#/components/schemas/MachineStatus"
 *                 uptime:
 *                   type: string
 *                   format: date-time
 *                   nullable: true
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const startStopMachine = async (ctx: KoaContext<StartStopMachineResponse, IdParams, StartStopMachineRequest>) => {
	const status = await machineService.startStopMachine(
		ctx.params.id,
		ctx.request.body,
		ctx.state.session.employeeId,
		ctx.state.session.role,
	);
	ctx.status = 200;
	ctx.body = status;
};
startStopMachine.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
	body: {
		status: Joi.string().valid(MachineStatus.Running, MachineStatus.Stopped),
	},
};

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({ prefix: '/machines' });

	router.use(requireAuthentication);

	router.get('/', validate(getAllMachines.validationScheme), getAllMachines);
	router.get('/:id', validate(getMachineById.validationScheme), getMachineById);
	router.get('/:id/reports', validate(getDailyReport.validationScheme), getDailyReport);
	router.patch('/:id', validate(startStopMachine.validationScheme), startStopMachine);

	parent
		.use(router.routes())
		.use(router.allowedMethods());
};
