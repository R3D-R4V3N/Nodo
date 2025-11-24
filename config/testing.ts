export default {
	log: {
		level: 'silly',
		disabled: false,
	},
	cors: {
		origins: ['http://localhost:9000'],
		maxAge: 3 * 60 * 60,
	},
	auth: {
		maxDelay: 0,
		jwt: {
			expirationInterval: 60 * 60, // s (1 hour)
			secret: 'eenveeltemoeilijksecretdatniemandooitzalradenandersisdesitegehacked',
		},
	},
};
