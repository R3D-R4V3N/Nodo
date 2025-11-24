export default {
	log: {
		level: 'info',
		disabled: false,
	},
	cors: {
		origins: ['https://two025-react-gent17.onrender.com'],
		maxAge: 3 * 60 * 60,
	},
	auth: {
		jwt: {
			expirationInterval: 60 * 60,
		},
	},
};
