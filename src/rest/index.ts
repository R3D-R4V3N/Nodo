import Router from '@koa/router';
import installHealthRoutes from './health';
import installSiteRoutes from './site';
import installEmployeeRoutes from './employee';
import installSessionRoutes from './session';
import installMachineRoutes from './machine';
import installMaintenanceRoutes from './maintenance';
import installMachineReportRoutes from './machineReport';
import installNotificationRoutes from './notification';
import type { ShopfloorSiteContext, ShopfloorSiteState, KoaApplication } from '../types/koa';

/**
 * @swagger
 * components:
 *   schemas:
 *     Base:
 *       required:
 *         - id
 *       properties:
 *         id:
 *           type: integer
 *           format: "int32"
 *   parameters:
 *     idParam:
 *       in: path
 *       name: id
 *       description: Id of the item to fetch/update/delete
 *       required: true
 *       schema:
 *         type: integer
 *         format: "int32"
 *     employeeIdParam:
 *       in: path
 *       name: id
 *       description: Id of the employee to fetch/update/delete. Use 'me' to reference yourself
 *       required: true
 *       schema:
 *         oneOf:
 *           - type: integer
 *             format: int32
 *             minimum: 1
 *           - type: string
 *             enum: ["me"]
 *   securitySchemes:
 *     bearerAuth: # arbitrary name for the security scheme
 *       type: http
 *       scheme: bearer
 *       bearerFormat: JWT # optional, arbitrary value for documentation purposes
 *   responses:
 *     400BadRequest:
 *       description: You provided invalid data
 *     401Unauthorized:
 *       description: You need to be authenticated to access this resource
 *     403Forbidden:
 *       description: You don't have access to this resource
 *     404NotFound:
 *       description: The requested resource could not be found
 */

export default (app: KoaApplication) => {
	const router = new Router<ShopfloorSiteState, ShopfloorSiteContext>({
		prefix: '/api',
	});

	installHealthRoutes(router);
	installSiteRoutes(router);
	installEmployeeRoutes(router);
	installSessionRoutes(router);
	installMachineRoutes(router);
	installMaintenanceRoutes(router);
	installMachineReportRoutes(router);
	installNotificationRoutes(router);

	app
		.use(router.routes())
		.use(router.allowedMethods());
};
