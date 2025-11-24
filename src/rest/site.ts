import Router from '@koa/router';
import Joi from 'joi';
import validate from '../core/validation';
import * as siteService from '../service/site';
import type { ShopfloorSiteState, ShopfloorSiteContext, KoaContext, KoaRouter } from '../types/koa';
import type { IdParams } from '../types/common';
import type { GetAllSitesResponse, GetSiteByIdResponse, UpdateSiteRequest, UpdateSiteResponse } from '../types/site';
import { makeAllowRoles, requireAuthentication } from '../core/auth';
import { Role } from '@prisma/client';

/**
 * @swagger
 * tags:
 *   name: Sites
 *   description: Represents a site
 */

/**
 * @swagger
 * components:
 *   schemas:
 *     Site:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - name
 *             - country
 *             - city
 *             - streetName
 *             - streetNumber
 *             - postalCode
 *             - latitude
 *             - longitude
 *             - isActive
 *             - machines
 *             - supervisor
 *           properties:
 *             name:
 *               type: string
 *             country:
 *               type: string
 *             city:
 *               type: string
 *             streetName:
 *               type: string
 *             streetNumber:
 *               type: string
 *             postalCode:
 *               type: string
 *             latitude:
 *               type: string
 *             longitude:
 *               type: string
 *             isActive:
 *               type: boolean
 *             machines:
 *               type: integer
 *               minimum: 0
 *             supervisor:
 *               $ref: "#/components/schemas/Employee"
 *           example:
 *             id: 1
 *             name: "Berlin Plant"
 *             country: "Germany"
 *             city: "Berlin"
 *             streetName: "Unter den Linden"
 *             streetNumber: "10"
 *             postalCode: "10117"
 *             latitude: "52.517197"
 *             longitude: "13.391195"
 *             isActive: true
 *             machines: 3
 *             supervisor:
 *               id: 1
 *               firstName: "Alice"
 *               lastName: "Müller"
 * 
 *     SitesList:
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/Site" 
 *
 *     SiteWithMachines:
 *       allOf:
 *         - $ref: "#/components/schemas/Site"
 *         - type: object
 *           properties:
 *             machines:
 *               type: array
 *               items:
 *                 $ref: "#/components/schemas/Machine"
 * 
 * 
 *   requestBodies:
 *     Site:
 *       description: The site info to save
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               name:
 *                 type: string
 *               country:
 *                 type: string
 *               city:
 *                 type: string
 *               streetName:
 *                 type: string
 *               streetNumber:
 *                 type: string
 *               postalCode:
 *                 type: string
 *               latitude:
 *                 type: string
 *               longitude:
 *                 type: string
 *               isActive:
 *                 type: boolean
 *               supervisorId:
 *                 type: integer
 *                 minimum: 1
 *             required:
 *               - name
 *               - country
 *               - city
 *               - streetName
 *               - streetNumber
 *               - postalCode
 *               - latitude
 *               - longitude
 *               - isActive
 *               - supervisorId
 */

/**
 * @swagger
 * /api/sites:
 *   get:
 *     summary: Get all sites
 *     tags:
 *       - Sites
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of sites
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/SitesList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 */
const getAllSites = async (ctx: KoaContext<GetAllSitesResponse>) => {
	const sites = await siteService.getAll(
		ctx.state.session.employeeId,
		ctx.state.session.role,
	);
	ctx.body = {
		items: sites,
	};
};
getAllSites.validationScheme = null;

/**
 * @swagger
 * /api/sites/{id}:
 *   get:
 *     summary: Get a single site with its machines
 *     tags:
 *       - Sites
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       200:
 *         description: The requested site with its machines
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/SiteWithMachines"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const getSiteById = async (ctx: KoaContext<GetSiteByIdResponse, IdParams>) => {
	ctx.body = await siteService.getById(
		ctx.params.id,
		ctx.state.session.employeeId,
		ctx.state.session.role,
	);
};
getSiteById.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

/**
 * @swagger
 * /api/sites/{id}:
 *   put:
 *     summary: Update an existing site
 *     tags:
 *       - Sites
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     requestBody:
 *       $ref: "#/components/requestBodies/Site"
 *     responses:
 *       200:
 *         description: The updated site
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/Site"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const updateSite = async (ctx: KoaContext<UpdateSiteResponse, IdParams, UpdateSiteRequest>) => {
	ctx.body = await siteService.update(ctx.params.id, ctx.request.body /*, ctx.state.session.employeeId*/);
	ctx.status = 200;
};
updateSite.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
	body: {
		name: Joi.string().required(),
		country: Joi.string().required(),
		city: Joi.string().required(),
		streetName: Joi.string().required(),
		streetNumber: Joi.string().required(),
		postalCode: Joi.string().required(),
		latitude: Joi.string().required(),
		longitude: Joi.string().required(),
		isActive: Joi.boolean().required(),
		supervisorId: Joi.number().integer().positive().required(),
	},
};

/**
 * @swagger
 * /api/sites:
 *   post:
 *     summary: Create a new site
 *     tags:
 *       - Sites
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       $ref: "#/components/requestBodies/Site"
 *     responses:
 *       200:
 *         description: The created site
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/Site"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 */
const createSite = async (ctx: KoaContext<UpdateSiteResponse, null, UpdateSiteRequest>) => {
	ctx.body = await siteService.create(ctx.request.body /*, ctx.state.session.employeeId*/);
	ctx.status = 201;
};
createSite.validationScheme = {
	body: {
		name: Joi.string().required(),
		country: Joi.string().required(),
		city: Joi.string().required(),
		streetName: Joi.string().required(),
		streetNumber: Joi.string().required(),
		postalCode: Joi.string().required(),
		latitude: Joi.string().required(),
		longitude: Joi.string().required(),
		isActive: Joi.boolean().required(),
		supervisorId: Joi.number().integer().positive().required(),
	},
};

/**
 * @swagger
 * /api/sites/{id}:
 *   delete:
 *     summary: Delete a site
 *     tags:
 *       - Sites
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       204:
 *         description: No response, the delete was successful
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: '#/components/responses/401Unauthorized'
 *       403:
 *         $ref: '#/components/responses/403Forbidden'
 *       404:
 *         $ref: '#/components/responses/404NotFound'
 */
const deleteSite = async (ctx: KoaContext<void, IdParams>) => {
	await siteService.remove(ctx.params.id /*, ctx.state.session.employeeId*/);
	ctx.status = 204;
};
deleteSite.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({ prefix: '/sites' });

	const requireManager = makeAllowRoles([Role.Manager, Role.Administrator]);
	const requireSupervisor = makeAllowRoles([Role.Supervisor, Role.Manager, Role.Administrator]);

	router.use(requireAuthentication);

	router.get('/',
		validate(getAllSites.validationScheme),
		requireSupervisor,
		getAllSites,
	);
	router.get('/:id', validate(getSiteById.validationScheme), getSiteById);

	router.post(
		'/',
		requireManager,
		validate(createSite.validationScheme),
		createSite,
	);
	router.put(
		'/:id',
		requireManager,
		validate(updateSite.validationScheme),
		updateSite,
	);
	router.delete('/:id',
		requireManager,
		validate(deleteSite.validationScheme),
		deleteSite,
	);

	parent
		.use(router.routes())
		.use(router.allowedMethods());
};
