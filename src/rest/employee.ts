import Router from '@koa/router';
import Joi from 'joi';
import validate from '../core/validation';
import * as employeeService from '../service/employee';
import type { ShopfloorSiteState, ShopfloorSiteContext, KoaContext, KoaRouter } from '../types/koa';
import type { getAllEmployeeResponse, getEmployeeByIdResponse, getEmployeeRequest } from '../types/employee';
import { makeAllowRoles, requireAuthentication } from '../core/auth';
import { Role } from '@prisma/client';
import type { Next } from 'koa';

/**
 * @swagger
 * tags:
 *   name: Employees
 *   description: Represents an employee
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     Role:
 *       type: string
 *       enum: [Technician, Supervisor, Manager, Administrator]
 * 
 *     Employee:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - firstName
 *             - lastName
 *             - dateOfBirth
 *             - country
 *             - city
 *             - postalCode
 *             - streetName
 *             - streetNumber
 *             - email
 *             - role
 *             - isActive
 *           properties:
 *             firstName:
 *               type: string
 *             lastName:
 *               type: string
 *             dateOfBirth:
 *               type: string
 *               format: date
 *             country:
 *               type: string
 *             city:
 *               type: string
 *             postalCode:
 *               type: string
 *             streetName:
 *               type: string
 *             streetNumber:
 *               type: string
 *             email:
 *               type: string
 *               format: email
 *             role:
 *               $ref: "#/components/schemas/Role"
 *             isActive:
 *               type: boolean
 *             phoneNumber:
 *               type: string
 *               nullable: true
 *             passwordHash:
 *               type: string
 *               nullable: true
 *           example:
 *             id: 1
 *             firstName: "Alice"
 *             lastName: "Müller"
 *             dateOfBirth: "1980-04-10"
 *             country: "Germany"
 *             city: "Berlin"
 *             streetName: "Friedrichstraße"
 *             streetNumber: "1A"
 *             postalCode: "10115"
 *             email: "alice.mueller@example.com"
 *             phoneNumber: "+49 30 123456"
 *             isActive: true
 *             role: "Supervisor"
 * 
 *     EmployeesList:
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/Employee" 
 *
 */

const checkUserId = (ctx: KoaContext<unknown, getEmployeeRequest>, next: Next) => {
	const { employeeId, role } = ctx.state.session;
	const { id } = ctx.params;

	if (id !== 'me' && id !== employeeId && role != Role.Administrator) {
		console.log(role);
		return ctx.throw(
			403,
			'You are not allowed to view this employee\'s information',
			{ code: 'FORBIDDEN' },
		);
	}
	return next();
};

/**
 * @swagger
 * /api/employees:
 *   get:
 *     summary: Get all employees
 *     tags:
 *      - Employees
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of employees
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/EmployeesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 */
const getAllEmployees = async (ctx: KoaContext<getAllEmployeeResponse>) => {
	const employees = await employeeService.getAll();
	ctx.body = {
		items: employees,
	};
};
getAllEmployees.validationScheme = null;

/**
 * @swagger
 * /api/employees/{id}:
 *   get:
 *     summary: Get a single employee
 *     description: Get a single employee by their id or your own information if you use 'me' as the id
 *     tags:
 *      - Employees
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/employeeIdParam"
 *     responses:
 *       200:
 *         description: The requested employee
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/Employee"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getEmployeeById = async (ctx: KoaContext<getEmployeeByIdResponse, getEmployeeRequest>) => {
	ctx.body = await employeeService.getById(
		ctx.params.id === 'me' ? ctx.state.session.employeeId : ctx.params.id,
	);
};
getEmployeeById.validationScheme = {
	params: {
		id: Joi.alternatives().try(
			Joi.number().integer().positive(),
			Joi.string().valid('me'),
		),
	},
};

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({ prefix: '/employees' });

	const requireAdmin = makeAllowRoles([Role.Administrator]);

	router.use(requireAuthentication);

	router.get('/', requireAdmin, validate(getAllEmployees.validationScheme), getAllEmployees);
	router.get('/:id', validate(getEmployeeById.validationScheme), checkUserId, getEmployeeById);

	parent
		.use(router.routes())
		.use(router.allowedMethods());
};
