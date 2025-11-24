import Koa from 'koa';

import { getLogger } from './core/logging';
import installMiddlewares from './core/installMiddlewares';
import { initializeData, shutdownData } from './data';
import installRest from './rest/index';
import type {
	KoaApplication,
	ShopfloorSiteContext,
	ShopfloorSiteState,
} from './types/koa';

export interface Server {
	getApp(): KoaApplication;
	start(): Promise<void>;
	stop(): Promise<void>;
}

export default async function createServer(): Promise<Server> {
	const app = new Koa<ShopfloorSiteState, ShopfloorSiteContext>();

	installMiddlewares(app);
	await initializeData();
	installRest(app);

	return {
		getApp() {
			return app;
		},

		start() {
			return new Promise<void>((resolve) => {
				app.listen(9000, () => {
					getLogger().info('🚀 Server listening on http://localhost:9000');
					resolve();
				});
			});
		},

		async stop() {
			app.removeAllListeners();
			await shutdownData();
			getLogger().info('Goodbye! 👋');
		},
	};
}