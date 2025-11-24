import type supertest from 'supertest';

const login = async (
	supertest: supertest.Agent,
	email: string,
	password: string,
): Promise<string> => {
	const response = await supertest.post('/api/sessions').send({
		email: email,
		password: password,
	});

	if (response.statusCode !== 200) {
		throw new Error(response.body.message || 'Unknown error occured');
	}

	return `Bearer ${response.body.token}`;
};

export const loginById = async (supertest: supertest.Agent, id: number): Promise<string> => {
	return login(supertest, `${id}@test.com`, 'password');
};
