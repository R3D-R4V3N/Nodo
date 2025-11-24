export default {
	log: {
		level: 'silly',
		disabled: false,
	},
	cors: {
		origins: ['http://localhost:9000', 'http://localhost:5173'],
		maxAge: 3 * 60 * 60,
	},
	auth: {
		jwt: {
			expirationInterval: 60 * 60, // s (1 hour)
			secret: 'eenveeltemoeilijksecretdatniemandooitzalradenandersisdesitegehacked',
		},
	},
};
