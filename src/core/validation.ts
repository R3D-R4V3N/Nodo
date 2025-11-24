// src/core/validation.ts
import type { Schema, SchemaLike } from 'joi';
import Joi from 'joi';
import type { KoaContext } from '../types/koa';
import type { Next } from 'koa';
import ServiceError from './serviceError';

const JOI_OPTIONS: Joi.ValidationOptions = {
	abortEarly: true, // stop when first error occured
	allowUnknown: false, // disallow unknown fields
	convert: true, // convert values to their types (number, Date, ...)
	presence: 'required', // default require all fields
};

type RequestValidationSchemeInput = Partial<
	Record<'params' | 'body' | 'query', SchemaLike>
>;
type RequestValidationScheme = Record<'params' | 'body' | 'query', Schema>;

const cleanupJoiError = (error: Joi.ValidationError) => {
	const errorDetails = error.details.reduce(
		(resultObj, { message, path, type }) => {
			const joinedPath = path.join('.') || 'value';
			if (!resultObj.has(joinedPath)) {
				resultObj.set(joinedPath, []);
			}

			resultObj.get(joinedPath).push({
				type,
				message,
			});

			return resultObj;
		},
		new Map(),
	);

	return Object.fromEntries(errorDetails);
};

const validate = (scheme: RequestValidationSchemeInput | null) => {
	const parsedSchema: RequestValidationScheme = {
		body: Joi.object(scheme?.body || {}),
		params: Joi.object(scheme?.params || {}),
		query: Joi.object(scheme?.query || {}),
	};

	return (ctx: KoaContext, next: Next) => {
		const errors = new Map();

		// path param validation
		const { error: paramsErrors, value: paramsValue } =
			parsedSchema.params.validate(ctx.params, JOI_OPTIONS);

		if (paramsErrors) {
			errors.set('params', cleanupJoiError(paramsErrors));
		} else {
			ctx.params = paramsValue;
		}

		// query param validation
		const { error: queryErrors, value: queryValue } = parsedSchema.query.validate(
			ctx.query,
			JOI_OPTIONS,
		);

		if (queryErrors) {
			errors.set('query', cleanupJoiError(queryErrors));
		} else {
			ctx.query = queryValue;
		}

		// Request body validation
		const { error: bodyErrors, value: bodyValue } = parsedSchema.body.validate(
			ctx.request.body,
			JOI_OPTIONS,
		);

		if (bodyErrors) {
			errors.set('body', cleanupJoiError(bodyErrors));
		} else {
			ctx.request.body = bodyValue;
		}

		if (errors.size > 0) {
			throw ServiceError.validationFailed('Validation failed, check details for more information',
				Object.fromEntries(errors));
		}

		return next();
	};
};

export default validate; 
