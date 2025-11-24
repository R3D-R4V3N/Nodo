import Router from '@koa/router';
import type { KoaContext, KoaRouter, ShopfloorSiteContext, ShopfloorSiteState } from '../types/koa';
import * as maintenanceService from '../service/maintenance';
import type { IdParams } from '../types/common';
import validate from '../core/validation';
import Joi from 'joi';
import type {
	GetAllMaintenanceResponse,
	getMaintenanceByEmployeeIdRequest,
	GetMaintenanceByEmployeeIdResponse,
	GetMaintenanceByIdResponse,
	GetMaintenanceByMachineIdResponse,
} from '../types/maintenance';
import { requireAuthentication } from '../core/auth';

/**
 * @swagger
 * tags:
 *   name: Maintenances
 *   description: Represents a maintenance
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     MaintenanceStatus:
 *       type: string
 *       enum: [Completed, Ongoing, Scheduled]
 * 
 *     Maintenance:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - datePlanned
 *             - startTime
 *             - productionStatus
 *           properties:
 *             datePlanned:
 *               type: string
 *               format: date-time
 *             startTime:
 *               type: string
 *               format: date-time
 *               nullable: true
 *             endTime:
 *               type: string
 *               format: date-time
 *               nullable: true
 *             reason:
 *               type: string
 *             maintenanceReport:
 *               type: string
 *               nullable: true
 *             comments:
 *               type: string
 *               nullable: true
 *             status:
 *               $ref: "#/components/schemas/MaintenanceStatus"
 *             machine:
 *               $ref: "#/components/schemas/Machine"
 *             technician:
 *               $ref: "#/components/schemas/Employee"
 *           example:
 *             id: 1
 *             datePlanned: "2025-04-01T14:30:00Z"
 *             startTime: "2025-04-01T15:00:00Z"
 *             endTime: "2025-04-01T17:30:00Z"
 *             reason: "Routine checkup"
 *             maintenanceReport: "All systems are functioning as expected"
 *             comments: "No issues found during inspection."
 *             status: "Completed"
 *             machine:
 *               id: 1
 *               code: "MACH-001"
 *             technician:
 *               id: 1
 *               firstName: "Bob"
 *               lastName: "Dupont"
 * 
 *     MaintenancesList:
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/Maintenance"
 * 
 *   parameters:
 *     maintenanceTypeQuery:
 *       in: query
 *       name: "type"
 *       required: false
 *       description: Filter by either 'past' or 'planned' maintenances
 *       schema:
 *         type: string
 *         enum: ["past", "planned"]
 */

/**
 * @swagger
 * /api/maintenances:
 *   get:
 *     summary: Get all maintenances
 *     tags:
 *      - Maintenances
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of maintenances
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MaintenancesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 */
const getAllMaintenances = async (ctx: KoaContext<GetAllMaintenanceResponse>) => {
	const sites = await maintenanceService.getAll();
	ctx.body = {
		items: sites,
	};
};
getAllMaintenances.validationScheme = null;

/**
 * @swagger
 * /api/maintenances/{id}:
 *   get:
 *     summary: Get a single maintenance
 *     tags:
 *      - Maintenances
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       200:
 *         description: List of maintenances
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/Maintenance"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getMaintenanceById = async (ctx: KoaContext<GetMaintenanceByIdResponse, IdParams>) => {
	const machine = await maintenanceService.getById(ctx.params.id);
	ctx.body = machine;
};
getMaintenanceById.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

/**
 * @swagger
 * /api/maintenances/employee/{id}:
 *   get:
 *     summary: Get all maintenances of a technician
 *     tags:
 *      - Maintenances
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/employeeIdParam"
 *       - $ref: "#/components/parameters/maintenanceTypeQuery"
 *     responses:
 *       200:
 *         description: List of maintenances
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MaintenancesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getMaintenanceByEmployeeId = async (
	ctx: KoaContext<GetMaintenanceByEmployeeIdResponse, getMaintenanceByEmployeeIdRequest>,
) => {
	const { type } = ctx.request.query as { type?: 'past' | 'planned' };
	const maintenances = await maintenanceService.getByEmployeeId(
		ctx.params.id === 'me' ? ctx.state.session.employeeId : ctx.params.id, type,
	);
	ctx.body = { items: maintenances };
};
getMaintenanceByEmployeeId.validationScheme = {
	params: {
		id: Joi.alternatives().try(
			Joi.number().integer().positive(),
			Joi.string().valid('me'),
		),
	},
	query: { type: Joi.string().valid('past', 'planned').optional() },
};

/**
 * @swagger
 * /api/maintenances/machine/{id}:
 *   get:
 *     summary: Get all maintenances of a machine
 *     tags:
 *      - Maintenances
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *       - $ref: "#/components/parameters/maintenanceTypeQuery"
 *     responses:
 *       200:
 *         description: List of maintenances
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/MaintenancesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getMaintenanceByMachineId = async (ctx: KoaContext<GetMaintenanceByMachineIdResponse, IdParams>) => {
	const { type } = ctx.request.query as { type?: 'past' | 'planned' };
	const maintenances = await maintenanceService.getByMachineId(ctx.params.id, type);
	ctx.body = { items: maintenances };
};
getMaintenanceByMachineId.validationScheme = {
	params: { id: Joi.number().integer().positive() },
	query: { type: Joi.string().valid('past', 'planned').optional() },
};

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({
		prefix: '/maintenances',
	});

	router.use(requireAuthentication);

	router.get('/', validate(getAllMaintenances.validationScheme), getAllMaintenances);
	router.get('/:id', validate(getMaintenanceById.validationScheme), getMaintenanceById);
	router.get('/machine/:id', validate(getMaintenanceByMachineId.validationScheme), getMaintenanceByMachineId);
	router.get('/employee/:id', validate(getMaintenanceByEmployeeId.validationScheme), getMaintenanceByEmployeeId);

	parent
		.use(router.routes())
		.use(router.allowedMethods());
};
