import type supertest from 'supertest';
import withServer from '../helpers/withServer';
import { loginById } from '../helpers/login';
import testAuthHeader from '../helpers/testAuthHeader';
import { MachineStatus, ProductionStatus } from '@prisma/client';
import { prisma } from '../../src/data';

const data = {
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
			supervisorId: 202,
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
	],
	machines: [
		{
			id: 1,
			siteId: 1,
			technicianId: 101,
			// irrelevant
			code: 'MACH-001',
			location: 'example location',
			productId: 1,
			uptime: null,
			status: MachineStatus.Running,
			productionStatus: ProductionStatus.Healthy,
		},
		{
			id: 2,
			siteId: 1,
			technicianId: 103,
			// irrelevant
			code: 'MACH-002',
			location: 'example location',
			productId: 1,
			uptime: null,
			status: MachineStatus.Stopped,
			productionStatus: ProductionStatus.Healthy,
		},
		{
			id: 3,
			siteId: 2,
			technicianId: 101,
			// irrelevant
			code: 'MACH-003',
			location: 'example location',
			productId: 1,
			uptime: null,
			status: MachineStatus.Maintenance,
			productionStatus: ProductionStatus.Healthy,
		},
	],
	products: [
		{
			id: 1,
			info: 'example product info',
		},
	],
};

const dataToDelete = {
	sites: [1, 2, 3],
	machines: [1, 2, 3, 4],
	products: [1],
};

describe('Sites', () => {

	let request: supertest.Agent;

	let auth101: string;
	let auth102: string;

	let auth201: string;
	let auth202: string;

	let auth301: string;

	const url = '/api/sites';

	withServer((r) => (request = r));

	beforeAll(async () => {

		auth101 = await loginById(request, 101);
		auth102 = await loginById(request, 102);

		auth201 = await loginById(request, 201);
		auth202 = await loginById(request, 202);

		auth301 = await loginById(request, 301);

		await prisma.site.createMany({ data: data.sites });
		await prisma.product.createMany({ data: data.products });
		await prisma.machine.createMany({ data: data.machines });
	});

	afterAll(async () => {
		await prisma.machine.deleteMany({ where: { id: { in: dataToDelete.machines } } });
		await prisma.site.deleteMany({ where: { id: { in: dataToDelete.sites } } });
		await prisma.product.deleteMany({ where: { id: { in: dataToDelete.products } } });
	});

	describe('GET /api/sites', () => {

		testAuthHeader(() => request.get(url));

		it('should 403 for any technician', async () => {
			const response = await request.get(url).set('Authorization', auth101);

			expect(response.statusCode).toBe(403);
			expect(response.body).toMatchObject({
				code: 'FORBIDDEN',
				message: 'You are not allowed to view this part of the application',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 200 and return all sites of a supervisor', async () => {
			const response = await request.get(url).set('Authorization', auth201);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(1);
			expect(response.body.items).toEqual(expect.arrayContaining([
				expect.objectContaining({
					id: 1,
					supervisor: {
						id: 201,
						firstName: expect.anything(), // TODO: concrete namen voor testemployees
						lastName: expect.anything(),
					},
					name: 'Paris Facility',
					country: 'France',
					city: 'Paris',
					streetName: 'Champs-Élysées',
					streetNumber: '50',
					postalCode: '75008',
					latitude: '48.869931',
					longitude: '2.357042',
					isActive: true,
					machines: 2,
				}),
			]));
		});

		it('should 200 and return all sites for a manager', async () => {
			const response = await request.get(url).set('Authorization', auth301);

			expect(response.statusCode).toBe(200);
			expect(response.body.items).toHaveLength(2);
			expect(response.body.items).toEqual(expect.arrayContaining([
				expect.objectContaining({
					id: 1,
					supervisor: {
						id: 201,
						firstName: expect.anything(),
						lastName: expect.anything(),
					},
					name: 'Paris Facility',
					country: 'France',
					city: 'Paris',
					streetName: 'Champs-Élysées',
					streetNumber: '50',
					postalCode: '75008',
					latitude: '48.869931',
					longitude: '2.357042',
					isActive: true,
					machines: 2,
				}),
				expect.objectContaining({
					id: 2,
					supervisor: {
						id: 202,
						firstName: expect.anything(),
						lastName: expect.anything(),
					},
					name: 'London Warehouse',
					country: 'United Kingdom',
					city: 'London',
					streetName: 'Baker Street',
					streetNumber: '221B',
					postalCode: 'NW1 6XE',
					latitude: '51.523767',
					longitude: '-0.158091',
					isActive: true,
					machines: 1,
				}),
			]));
		});

		// TODO: admin rechten voor /api/sites ?

	});

	describe('GET /api/sites/:id', () => {

		testAuthHeader(() => request.get(`${url}/1`));

		it('should 200 and return site with selection of machines for an authorized technician', async () => {
			const response = await request.get(`${url}/1`).set('Authorization', auth101);

			// site 1 met enkel machine 1

			expect(response.statusCode).toBe(200);
		});

		it('should 200 and return site with all machines for an authorized supervisor', async () => {
			const response = await request.get(`${url}/1`).set('Authorization', auth201);

			// site 1 met alle machines (1 en 2)

			expect(response.statusCode).toBe(200);
		});

		it('should 200 and return site with all machines for a manager', async () => {
			const response = await request.get(`${url}/1`).set('Authorization', auth301);

			// idem

			expect(response.statusCode).toBe(200);
		});

		// TODO: admin rechten voor /api/sites/:id ?

		const expect403 = async (auth: string) => {
			const response = await request.get(`${url}/1`).set('Authorization', auth);

			expect(response.statusCode).toBe(403);
			expect(response.body).toMatchObject({
				code: 'FORBIDDEN',
				message: 'You have no access to this site',
			});
			expect(response.body.stack).toBeTruthy();
		};

		it('should 403 for an unauthorized technician', async () => {
			await expect403(auth102);
		});

		it('should 403 for an unauthorized supervisor', async () => {
			await expect403(auth202);
		});

		it('should 404 when requesting nonexistent site', async () => {
			const response = await request.get(`${url}/999`).set('Authorization', auth301);

			expect(response.statusCode).toBe(404);
			expect(response.body).toMatchObject({
				code: 'NOT_FOUND',
				message: 'Site not found',
			});
			expect(response.body.stack).toBeTruthy();
		});

		it('should 400 with invalid site id', async () => {
			const response = await request.get(`${url}/invalid`).set('Authorization', auth301);

			expect(response.statusCode).toBe(400);
			expect(response.body.code).toBe('VALIDATION_FAILED');
			expect(response.body.details.params).toHaveProperty('id');
		});
	});

});