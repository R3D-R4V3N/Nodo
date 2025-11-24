import type { ParameterizedContext } from 'koa';
import type Application from 'koa';
import type Router from '@koa/router';
import type { SessionInfo } from './auth';
import type Joi from 'joi';

export interface ShopfloorSiteState {
	session: SessionInfo;
}

export interface ShopfloorSiteContext<
	Params = unknown,
	RequestBody = unknown,
	Query = unknown,
> {
	request: {
		body: RequestBody,
		query: Query,
	},
	params: Params,
};

export type KoaContext<
	ResponseBody = unknown,
	Params = unknown,
	RequestBody = unknown,
	Query = unknown,
> = ParameterizedContext<
	ShopfloorSiteState,
	ShopfloorSiteContext<Params, RequestBody, Query>,
	ResponseBody
>;

export interface KoaApplication extends Application<ShopfloorSiteState, ShopfloorSiteContext> { }

export interface KoaRouter extends Router<ShopfloorSiteState, ShopfloorSiteContext> { }

// Voeg dit nieuwe type toe voor validatie
export interface ValidatedRoute<
	ResponseBody = unknown,
	Params = unknown,
	RequestBody = unknown,
	Query = unknown,
> {
	(ctx: KoaContext<ResponseBody, Params, RequestBody, Query>): Promise<void>;
	validationScheme?: {
		params?: Joi.Schema;
		query?: Joi.Schema;
		body?: Joi.Schema;
	};
}