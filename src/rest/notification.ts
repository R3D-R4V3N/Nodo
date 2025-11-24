import Router from '@koa/router';
import type { 
	KoaContext, 
	KoaRouter, 
	ShopfloorSiteContext, 
	ShopfloorSiteState, 
} from '../types/koa';
import type { 
	GetNotificationsResponse,
	GetNotificationByIdResponse,
	UpdateNotificationStatusResponse,
	UpdateNotificationStatusInput,
} from '../types/notification';
import * as notificationService from '../service/notification';
import type { IdParams } from '../types/common';
import validate from '../core/validation';
import Joi from 'joi';
import { NotificationStatus } from '@prisma/client';
import { requireAuthentication } from '../core/auth';

/**
 * @swagger
 * tags:
 *   name: Notifications
 *   description: Represents a notification
 */

/**
 * @swagger
 * components:
 *   schemas:
 * 
 *     NotificationStatus:
 *       type: string
 *       enum: [New, Unread, Read]
 * 
 *     NotificationPreview:
 *       allOf:
 *         - $ref: "#/components/schemas/Base"
 *         - type: object
 *           required:
 *             - title
 *             - message
 *             - createdAt
 *             - status
 *           properties:
 *             title:
 *               type: string
 *             message:
 *               type: string
 *             createdAt:
 *               type: string
 *               format: date-time
 *             status:
 *               $ref: "#/components/schemas/NotificationStatus"
 * 
 *     Notification:
 *       allOf:
 *         - $ref: "#/components/schemas/NotificationPreview"
 *         - type: object
 *           required:
 *             - employee
 *           properties:
 *             employee:
 *               $ref: "#/components/schemas/Employee"
 *             machine:
 *               $ref: "#/components/schemas/Machine"
 *               nullable: true
 *             site:
 *               $ref: "#/components/schemas/Site"
 *               nullable: true
 *             maintenance:
 *               $ref: "#/components/schemas/Maintenance"
 *               nullable: true
 * 
 *     NotificationsList:
 *       type: object
 *       required:
 *         - items
 *       properties:
 *         items:
 *           type: array
 *           items:
 *             $ref: "#/components/schemas/Notification"
 *
 *   requestBodies:
 *     NotificationStatus:
 *       description: The new status for the notification
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               status:
 *                 $ref: "#/components/schemas/NotificationStatus"
 */

/**
 * @swagger
 * /notifications:
 *   get:
 *     summary: Get all notifications for the logged-in user
 *     tags:
 *       - Notifications
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of notifications
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/NotificationsList"
 *       400:
 *         $ref: '#/components/responses/400BadRequest'
 *       401:
 *         $ref: "#/components/responses/401Unauthorized"
 */
const getNotifications = async (ctx: KoaContext<GetNotificationsResponse>) => {
	const notifications = await notificationService.getAll(
		ctx.state.session.employeeId,
	);
	ctx.body = {
		items: notifications,
	};
};

/**
 * @swagger
 * /notifications/{id}:
 *   get:
 *     summary: Get a single notification
 *     tags:
 *       - Notifications
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     responses:
 *       200:
 *         description: The requested notification
 *         content:
 *           application/json:
 *             schema:
 *               $ref: "#/components/schemas/Notification"
 *       401:
 *         $ref: "#/components/responses/401Unauthorized"
 *       404:
 *         $ref: "#/components/responses/404NotFound"
 */
const getNotificationById = async (ctx: KoaContext<GetNotificationByIdResponse, IdParams>) => {
	const notification = await notificationService.getById(
		ctx.params.id,
		ctx.state.session.employeeId,
	);
	ctx.body = notification;
};
getNotificationById.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
};

/**
 * @swagger
 * /notifications/{id}:
 *   patch:
 *     summary: Update the status of a notification
 *     tags:
 *       - Notifications
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - $ref: "#/components/parameters/idParam"
 *     requestBody:
 *       $ref: "#/components/requestBodies/NotificationStatus"
 *     responses:
 *       200:
 *         description: The id and new status of the notification
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 id:
 *                   type: integer
 *                   minimum: 1
 *                 status:
 *                   $ref: "#/components/schemas/NotificationStatus"
 *       400:
 *         $ref: "#/components/responses/400BadRequest"
 *       401:
 *         $ref: "#/components/responses/401Unauthorized"
 *       404:
 *         $ref: "#/components/responses/404NotFound"
 */
const updateNotificationStatus = async (
	ctx: KoaContext<UpdateNotificationStatusResponse, IdParams, UpdateNotificationStatusInput>,
) => {
	const updated = await notificationService.updateStatus(
		ctx.params.id,
		ctx.request.body,
		ctx.state.session.employeeId,
	);
	ctx.body = updated;
};
updateNotificationStatus.validationScheme = {
	params: {
		id: Joi.number().integer().positive(),
	},
	body: {
		status: Joi.string().valid(...Object.values(NotificationStatus)),
	},
};

export default (parent: KoaRouter) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({ 
		prefix: '/notifications', 
	});

	router.use(requireAuthentication);

	router.get('/', getNotifications);
	router.get('/:id', validate(getNotificationById.validationScheme), getNotificationById);
	router.patch(
		'/:id', 
		validate(updateNotificationStatus.validationScheme), 
		updateNotificationStatus,
	);

	parent
		.use(router.routes())
		.use(router.allowedMethods());
};