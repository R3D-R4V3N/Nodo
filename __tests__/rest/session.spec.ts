import type { Agent } from 'supertest';
import { prisma } from '../../src/data';
import withServer from '../helpers/withServer';
import type { Employee } from '../../src/types/employee';
import { EmployeeFactory } from '../helpers/mock';

describe('Sessions', () => {

	let supertest: Agent;

	withServer((s) => supertest = s);

	describe('POST /api/sessions', () => {

		const url = '/api/sessions';
		const password = 'password';

		let activeEmployee: Employee;
		let deactivatedEmployee: Employee;
		let newEmployee: Employee;

		beforeAll(async () => {
			const employeeFactory = await EmployeeFactory.withPassword(password);

			activeEmployee = await prisma.employee.create({
				data: employeeFactory.create({
					id: 1,
					isActive: true,
					passwordHash: employeeFactory.passwordHash,
				}),
			});

			deactivatedEmployee = await prisma.employee.create({
				data: employeeFactory.create({
					id: 2,
					isActive: false,
					passwordHash: employeeFactory.passwordHash,
				}),
			});

			newEmployee = await prisma.employee.create({
				data: employeeFactory.create({
					id: 3,
					isActive: false,
					passwordHash: null,
				}),
			});
		});

		afterAll(async () => {
			await prisma.employee.deleteMany({
				where: { id: { in: [1, 2, 3] } },
			});
		});

		it('should 200 and return the token when succesfully logged in', async () => {
			const response = await supertest.post(url)
				.send({
					email: activeEmployee.email,
					password,
				});

			expect(response.statusCode).toBe(200);
			expect(response.body.token).toBeTruthy();
		});

		it('should 403 for deactivated employee with correct password', async () => {
			const response = await supertest.post(url)
				.send({
					email: deactivatedEmployee.email,
					password,
				});

			expect(response.statusCode).toBe(403);
			expect(response.body).toMatchObject({
				code: 'FORBIDDEN',
				message: 'Account is disabled',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 403 for deactivated employee with wrong password', async () => {
			const response = await supertest.post(url)
				.send({
					email: deactivatedEmployee.email,
					password: 'wrong',
				});

			expect(response.statusCode).toBe(403);
			expect(response.body).toMatchObject({
				code: 'FORBIDDEN',
				message: 'Account is disabled',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 401 for new employee', async () => {
			const response = await supertest.post(url)
				.send({
					email: newEmployee.email,
					password,
				});

			expect(response.statusCode).toBe(401);
			expect(response.body).toMatchObject({
				code: 'UNAUTHORIZED',
				message: 'The given email and password do not match',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 401 with wrong email', async () => {
			const response = await supertest.post(url)
				.send({
					email: 'wrong@example.com',
					password,
				});

			expect(response.statusCode).toBe(401);
			expect(response.body).toMatchObject({
				code: 'UNAUTHORIZED',
				message: 'The given email and password do not match',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 401 with wrong password', async () => {
			const response = await supertest.post(url)
				.send({
					email: activeEmployee.email,
					password: 'wrong',
				});

			expect(response.statusCode).toBe(401);
			expect(response.body).toMatchObject({
				code: 'UNAUTHORIZED',
				message: 'The given email and password do not match',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 400 with invalid email', async () => {
			const response = await supertest.post(url)
				.send({
					email: 'invalid',
					password,
				});

			expect(response.statusCode).toBe(400);
			expect(response.body.code).toBe('VALIDATION_FAILED');
			expect(response.body.details.body).toHaveProperty('email');
		});

		it('should 400 when no password given', async () => {
			const response = await supertest.post(url)
				.send({ email: activeEmployee.email });

			expect(response.statusCode).toBe(400);
			expect(response.body.code).toBe('VALIDATION_FAILED');
			expect(response.body.details.body).toHaveProperty('password');
		});

		it('should 400 when no email given', async () => {
			const response = await supertest.post(url)
				.send({ password });

			expect(response.statusCode).toBe(400);
			expect(response.body.code).toBe('VALIDATION_FAILED');
			expect(response.body.details.body).toHaveProperty('email');
		});
	});
});
