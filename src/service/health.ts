import config from 'config';
import packageJson from '../../package.json';

/**
 * Check if the server is healthy. Can be extended
 * with database connection check, etc.
 */
export const ping = () => ({ pong: true });

/**
 * Get the running server's information.
 */
export const getVersion = () => ({
	env: config.get<string>('env'),
	version: packageJson.version,
	name: packageJson.name,
});
