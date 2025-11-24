export default {
	auth: {
		maxDelay: 5000,
		argon: {
			hashLength: 32,
			timeCost: 6,
			memoryCost: 2 ** 17,
		},
		jwt: {
			audience: 'shopfloor.hogent.be',
			issuer: 'shopfloor.hogent.be',
		},
	},
};
