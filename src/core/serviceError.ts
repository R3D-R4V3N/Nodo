const NOT_FOUND = 'NOT_FOUND';
const VALIDATION_FAILED = 'VALIDATION_FAILED';
const UNAUTHORIZED = 'UNAUTHORIZED';
const FORBIDDEN = 'FORBIDDEN';
const INTERNAL_SERVER_ERROR = 'INTERNAL_SERVER_ERROR';
const CONFLICT = 'CONFLICT';

export default class ServiceError extends Error {
	code: string;
	details?: any;

	constructor(code: string, message: string, details?: any) {
		super(message);
		this.code = code;
		this.name = 'ServiceError';
		this.details = details;
	}

	static notFound(message: string, details?: any) {
		return new ServiceError(NOT_FOUND, message, details);
	}

	static validationFailed(message: string, details?: any) {
		return new ServiceError(VALIDATION_FAILED, message, details);
	}

	static unauthorized(message: string, details?: any) {
		return new ServiceError(UNAUTHORIZED, message, details);
	}

	static forbidden(message: string, details?: any) {
		return new ServiceError(FORBIDDEN, message, details);
	}

	static internalServerError(message: string, details?: any) {
		return new ServiceError(INTERNAL_SERVER_ERROR, message, details);
	}

	static conflict(message: string, details?: any) {
		return new ServiceError(CONFLICT, message, details);
	}

	get isNotFound(): boolean {
		return this.code === NOT_FOUND;
	}

	get isValidationFailed(): boolean {
		return this.code === VALIDATION_FAILED;
	}

	get isUnauthorized(): boolean {
		return this.code === UNAUTHORIZED;
	}

	get isForbidden(): boolean {
		return this.code === FORBIDDEN;
	}

	get isInternalServerError(): boolean {
		return this.code === INTERNAL_SERVER_ERROR;
	}

	get isConflict(): boolean {
		return this.code === CONFLICT;
	}
}