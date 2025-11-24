import type supertest from 'supertest';
import withServer from '../helpers/withServer';
import { loginById } from '../helpers/login';
import testAuthHeader from '../helpers/testAuthHeader';
import { MachineStatus, ProductionStatus } from '@prisma/client';
import { prisma } from '../../src/data';
import { faker } from '@faker-js/faker';

const data = {
	products: [
		{
			id: 1,
			info: 'example product info',
		},
	],
	sites: [
		{
			id: 1,
			supervisorId: 201,
			name: 'Paris Facility',
			country: 'France',
			city: 'Paris',
			streetName: 'Champs-Élysées',
			streetNumber: '50',
			postalCode: '75008',
			latitude: '48.869931',
			longitude: '2.357042',
			isActive: true,
		},
		{
			id: 2,
			supervisorId: 201,
			name: 'London Warehouse',
			country: 'United Kingdom',
			city: 'London',
			streetName: 'Baker Street',
			streetNumber: '221B',
			postalCode: 'NW1 6XE',
			latitude: '51.523767',
			longitude: '-0.158091',
			isActive: true,
		},
		{
			id: 3,
			supervisorId: 202,
			name: 'Berlin Plant',
			country: 'Germany',
			city: 'Berlin',
			streetName: 'Unter den Linden',
			streetNumber: '10',
			postalCode: '10117',
			latitude: '52.517197',
			longitude: '13.391195',
			isActive: true,
		},
	],
};

const dataToDelete = {
	sites: [1, 2, 3],
	products: [1],
};

describe('Machines', () => {

	let request: supertest.Agent;

	let auth101: string;
	let auth102: string;

	let auth201: string;
	let auth202: string;

	let auth301: string;
	let auth401: string;

	const url = '/api/machines';

	withServer((r) => (request = r));

	beforeAll(async () => {

		auth101 = await loginById(request, 101);
		auth102 = await loginById(request, 102);

		auth201 = await loginById(request, 201);
		auth202 = await loginById(request, 202);

		auth301 = await loginById(request, 301);
		auth401 = await loginById(request, 401);

		await prisma.site.createMany({ data: data.sites });
		await prisma.product.createMany({ data: data.products });

	});

	afterAll(async () => {
		await prisma.site.deleteMany({ where: { id: { in: dataToDelete.sites } } });
		await prisma.product.deleteMany({ where: { id: { in: dataToDelete.products } } });
	});

	describe('GET /api/machines', () => {

		beforeAll(async () => {

			await prisma.machine.create({
				data: {
					id: 1,
					code: 'MACH-001',
					location: 'example location',
					productId: 1,
					uptime: faker.date.recent({ days: 10 }),
					siteId: 1,
					technicianId: 101,
					status: MachineStatus.Running,
					productionStatus: ProductionStatus.Healthy,
				},
			});

			await prisma.machine.create({
				data: {
					id: 2,
					code: 'MACH-002',
					location: 'example location',
					productId: 1,
					uptime: null,
					siteId: 2,
					technicianId: 101,
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
			});

			await prisma.machine.create({
				data: {
					id: 3,
					code: 'MACH-003',
					location: 'example location',
					productId: 1,
					uptime: null,
					siteId: 2,
					technicianId: 102,
					status: MachineStatus.Maintenance,
					productionStatus: ProductionStatus.Healthy,
				},
			});

			await prisma.machine.create({
				data: {
					id: 4,
					code: 'MACH-004',
					location: 'example location',
					productId: 1,
					uptime: null,
					siteId: 3,
					technicianId: 102,
					status: MachineStatus.Ready,
					productionStatus: ProductionStatus.Healthy,
				},
			});
		});

		afterAll(async () => {
			await prisma.machine.deleteMany({ where: { id: { in: [1, 2, 3, 4] } } });
		});

		testAuthHeader(() => request.get(url));

		it('should 200 and return all machines maintained by the technician', async () => {
			const response = await request.get(url).set('Authorization', auth101);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(2);
			expect(response.body.items).toEqual(expect.arrayContaining([
				{
					id: 1,
					location: 'example location',
					status: MachineStatus.Running,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 2,
					location: 'example location',
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
			]));
		});

		it('should 200 and return all machines supervised by the supervisor', async () => {
			const response = await request.get(url).set('Authorization', auth201);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(3);
			expect(response.body.items).toEqual(expect.arrayContaining([
				{
					id: 1,
					location: 'example location',
					status: MachineStatus.Running,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 2,
					location: 'example location',
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 3,
					location: 'example location',
					status: MachineStatus.Maintenance,
					productionStatus: ProductionStatus.Healthy,
				},
			]));
		});

		it('should 200 and return all machines for a manager', async () => {
			const response = await request.get(url).set('Authorization', auth301);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(4);
			expect(response.body.items).toEqual(expect.arrayContaining([
				{
					id: 1,
					location: 'example location',
					status: MachineStatus.Running,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 2,
					location: 'example location',
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 3,
					location: 'example location',
					status: MachineStatus.Maintenance,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 4,
					location: 'example location',
					status: MachineStatus.Ready,
					productionStatus: ProductionStatus.Healthy,
				},
			]));
		});

		it('should 200 and return all machines for an admin', async () => {
			const response = await request.get(url).set('Authorization', auth401);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(4);
			expect(response.body.items).toEqual(expect.arrayContaining([
				{
					id: 1,
					location: 'example location',
					status: MachineStatus.Running,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 2,
					location: 'example location',
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 3,
					location: 'example location',
					status: MachineStatus.Maintenance,
					productionStatus: ProductionStatus.Healthy,
				},
				{
					id: 4,
					location: 'example location',
					status: MachineStatus.Ready,
					productionStatus: ProductionStatus.Healthy,
				},
			]));
		});
	});

	describe('GET /api/machines/:id', () => {

		const MACHINE_SELECT = {
			id: true,
			site: {
				select: {
					id: true,
					name: true,
				},
			},
			code: true,
			location: true,
			product: true,
			status: true,
			productionStatus: true,
			uptime: true,
			technician: {
				select: {
					id: true,
					firstName: true,
					lastName: true,
				},
			},
		};

		beforeAll(async () => {

			await prisma.machine.create({
				data: {
					id: 1,
					code: 'MACH-001',
					location: 'example location',
					productId: 1,
					uptime: null,
					siteId: 1,
					technicianId: 101,
					status: MachineStatus.Stopped,
					productionStatus: ProductionStatus.Healthy,
				},
				select: MACHINE_SELECT,
			});
		});

		afterAll(async () => {
			await prisma.machine.deleteMany({ where: { id: 1 } });
		});

		testAuthHeader(() => request.get(`${url}/1`));

		const expect200 = async (auth: string) => {
			const response = await request.get(`${url}/1`).set('Authorization', auth);

			expect(response.statusCode).toBe(200);
			expect(response.body).toMatchObject({
				id: 1,
				code: 'MACH-001',
				location: 'example location',
				product: {
					id: 1,
				},
				uptime: null,
				site: {
					id: 1,
				},
				technician: {
					id: 101,
				},
				status: MachineStatus.Stopped,
				productionStatus: ProductionStatus.Healthy,
			});
		};

		const expect403 = async (auth: string) => {
			const response = await request.get(`${url}/1`).set('Authorization', auth);

			expect(response.statusCode).toBe(403);
			expect(response.body).toMatchObject({
				code: 'FORBIDDEN',
				message: 'You have no access to this machine',
			});
			expect(response.body.stack).toBeTruthy();
		};

		it('should 200 and return the requested machine for an authorized technician', async () => {
			await expect200(auth101);
		});

		it('should 200 and return the requested machine for an authorized supervisor', async () => {
			await expect200(auth201);
		});

		it('should 403 when requesting machine for an unauthorized technician', async () => {
			await expect403(auth102);
		});

		it('should 403 when requesting machine for an unauthorized supervisor', async () => {
			await expect403(auth202);
		});

		it('should 200 and return the requested machine for a manager', async () => {
			await expect200(auth301);
		});

		it('should 200 and return the requested machine for an administrator', async () => {
			await expect200(auth401);
		});

		it('should 404 when requesting nonexistent machine', async () => {
			const response = await request.get(`${url}/999`).set('Authorization', auth401);

			expect(response.statusCode).toBe(404);
			expect(response.body).toMatchObject({
				code: 'NOT_FOUND',
				message: 'Machine not found',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 400 with invalid machine id', async () => {
			const response = await request.get(`${url}/invalid`).set('Authorization', auth401);

			expect(response.statusCode).toBe(400);
			expect(response.body.code).toBe('VALIDATION_FAILED');
			expect(response.body.details.params).toHaveProperty('id');
		});
	});

	describe('PATCH /api/machines/:id', () => {

		const YESTERDAY = new Date(new Date().getTime() - 1000 * 60 * 60 * 24);

		const setupAndTearDown = () => {

			// 1 machine voor elk van de statussen

			beforeAll(async () => {

				// Running
				await prisma.machine.create({
					data: {
						id: 1,
						code: 'MACH-001',
						location: 'example location',
						productId: 1,
						uptime: YESTERDAY,
						siteId: 1,
						technicianId: 101,
						status: MachineStatus.Running,
						productionStatus: ProductionStatus.Healthy,
					},
				});

				// Stopped
				await prisma.machine.create({
					data: {
						id: 2,
						code: 'MACH-002',
						location: 'example location',
						productId: 1,
						uptime: null,
						siteId: 1,
						technicianId: 101,
						status: MachineStatus.Stopped,
						productionStatus: ProductionStatus.Healthy,
					},
				});

				// Ready
				await prisma.machine.create({
					data: {
						id: 3,
						code: 'MACH-003',
						location: 'example location',
						productId: 1,
						uptime: null,
						siteId: 1,
						technicianId: 101,
						status: MachineStatus.Ready,
						productionStatus: ProductionStatus.Healthy,
					},
				});

				// Maintenance
				await prisma.machine.create({
					data: {
						id: 4,
						code: 'MACH-004',
						location: 'example location',
						productId: 1,
						uptime: null,
						siteId: 1,
						technicianId: 101,
						status: MachineStatus.Maintenance,
						productionStatus: ProductionStatus.Healthy,
					},
				});
			});

			afterAll(async () => {
				await prisma.machine.deleteMany({ where: { id: { in: [1, 2, 3, 4] } } });
			});

		};

		testAuthHeader(() => request.patch(`${url}/1`));

		describe('Start machine met authorized technician', () => {
			setupAndTearDown();

			const expect200 = async (id: number, auth: string) => {
				const response = await request.patch(`${url}/${id}`)
					.send({ status: MachineStatus.Running })
					.set('Authorization', auth);

				expect(response.statusCode).toBe(200);
				expect(response.body.id).toEqual(id);
				expect(response.body.status).toEqual(MachineStatus.Running);
				// test of uptime maximaal 1 minuut geleden is
				const uptime = Date.parse(response.body.uptime);
				expect(uptime).not.toBeNaN();
				expect(uptime).toBeGreaterThan(Date.now() - 1000 * 60);
				expect(uptime).toBeLessThanOrEqual(Date.now());
			};

			const expect409 = async (id: number, auth: string) => {
				const response = await request.patch(`${url}/${id}`)
					.send({ status: MachineStatus.Running })
					.set('Authorization', auth);

				expect(response.statusCode).toBe(409);
				expect(response.body).toMatchObject({
					code: 'CONFLICT',
					message: expect.stringContaining('Cannot change machine status'),
				});
			};

			it('should 200 when an authorized technician starts a machine with status "Stopped"', async () => {
				await expect200(2, auth101);
			});
			it('should 200 when an authorized technician starts a machine with status "Ready"', async () => {
				await expect200(3, auth101);
			});

			it('should 409 when an authorized technician starts a machine with status "Running"', async () => {
				await expect409(1, auth101);
			});
			it('should 409 when an authorized technician starts a machine with status "Maintenance"', async () => {
				await expect409(4, auth101);
			});
		});

		describe('Stop machine met authorized supervisor', () => {
			setupAndTearDown();

			const expect200 = async (id: number, auth: string) => {
				const response = await request.patch(`${url}/${id}`)
					.send({ status: MachineStatus.Stopped })
					.set('Authorization', auth);

				expect(response.statusCode).toBe(200);
				expect(response.body.id).toEqual(id);
				expect(response.body.status).toEqual(MachineStatus.Stopped);
				expect(response.body.uptime).toBeNull();
			};

			const expect409 = async (id: number, auth: string) => {
				const response = await request.patch(`${url}/${id}`)
					.send({ status: MachineStatus.Stopped })
					.set('Authorization', auth);

				expect(response.statusCode).toBe(409);
				expect(response.body).toMatchObject({
					code: 'CONFLICT',
					message: expect.stringContaining('Cannot change machine status'),
				});
			};

			it('should 200 when an authorized supervisor stops a machine with status "Running"', async () => {
				await expect200(1, auth201);
			});
			it('should 200 when an authorized supervisor stops a machine with status "Ready"', async () => {
				await expect200(3, auth201);
			});

			it('should 409 when an authorized supervisor stops a machine with status "Stopped"', async () => {
				await expect409(2, auth201);
			});
			it('should 409 when an authorized supervisor stops a machine with status "Maintenance"', async () => {
				await expect409(4, auth201);
			});
		});

		describe('Start machine met unauthorized employee or invalid machine', () => {

			beforeAll(async () => {
				await prisma.machine.create({
					data: {
						id: 1,
						code: 'MACH-001',
						location: 'example location',
						productId: 1,
						uptime: null,
						siteId: 1,
						technicianId: 101,
						status: MachineStatus.Stopped,
						productionStatus: ProductionStatus.Healthy,
					},
				});
			});

			afterAll(async () => {
				await prisma.machine.delete({ where: { id: 1 } });
			});

			const expect403 = async (auth: string) => {
				const response = await request.patch(`${url}/1`)
					.send({ status: MachineStatus.Running })
					.set('Authorization', auth);

				expect(response.statusCode).toBe(403);
				expect(response.body).toMatchObject({
					code: 'FORBIDDEN',
					message: 'You have no access to this machine',
				});
				expect(response.body.stack).toBeTruthy();
			};

			it('should 403 when an unauthorized technician stops a machine', async () => {
				await expect403(auth102);
			});
			it('should 403 when an unauthorized supervisor stops a machine', async () => {
				await expect403(auth202);
			});
			it('should 403 when a manager stops a machine', async () => {
				await expect403(auth301);
			});
			it('should 403 when an administrator stops a machine', async () => {
				await expect403(auth401);
			});

			it.each([
				undefined,
				MachineStatus.Maintenance,
				MachineStatus.Ready,
				'running',
				'stopped',
			])('should 400 with invalid status: %s', async (status) => {
				const response = await request.patch(`${url}/1`)
					.send({ status })
					.set('Authorization', auth101);

				expect(response.statusCode).toBe(400);
				expect(response.body.code).toBe('VALIDATION_FAILED');
				expect(response.body.details.body).toHaveProperty('status');
			});

			it('should 400 with invalid id', async () => {
				const response = await request.patch(`${url}/invalid`)
					.send({ status: MachineStatus.Stopped })
					.set('Authorization', auth101);

				expect(response.statusCode).toBe(400);
				expect(response.body.code).toBe('VALIDATION_FAILED');
				expect(response.body.details.params).toHaveProperty('id');
			});

			it('should 404 for nonexistent machine', async () => {
				const response = await request.patch(`${url}/999`)
					.send({ status: MachineStatus.Stopped })
					.set('Authorization', auth101);

				expect(response.statusCode).toBe(404);
				expect(response.body).toMatchObject({
					code: 'NOT_FOUND',
					message: 'Machine not found',
				});
				expect(response.body.stack).toBeTruthy();
			});
		});
	});
});
