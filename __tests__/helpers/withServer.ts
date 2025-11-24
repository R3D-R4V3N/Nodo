import supertest from 'supertest';
import type { Server } from '../../src/createServer';
import createServer from '../../src/createServer';
import { prisma } from '../../src/data';
import { Role } from '@prisma/client';
import { EmployeeFactory } from './mock';

export default function withServer(setter: (s: supertest.Agent) => void): void {
	let server: Server;

	beforeAll(async () => {
		server = await createServer();
		setter(supertest(server.getApp().callback()));

		// TEMP
		await prisma.machine.deleteMany();
		await prisma.site.deleteMany();
		await prisma.employee.deleteMany();
		await prisma.product.deleteMany();

		const employeeFactory = await EmployeeFactory.withPassword('password');
		await prisma.employee.createMany({
			data: [
				// 100: Technician
				employeeFactory.create({ id: 101, email: '101@test.com', role: Role.Technician }),
				employeeFactory.create({ id: 102, email: '102@test.com', role: Role.Technician }),
				employeeFactory.create({ id: 103, email: '103@test.com', role: Role.Technician }),
				employeeFactory.create({ id: 104, email: '104@test.com', role: Role.Technician }),
				employeeFactory.create({ id: 105, email: '105@test.com', role: Role.Technician }),
				// 200: Supervisor
				employeeFactory.create({ id: 201, email: '201@test.com', role: Role.Supervisor }),
				employeeFactory.create({ id: 202, email: '202@test.com', role: Role.Supervisor }),
				// 300: Manager
				employeeFactory.create({ id: 301, email: '301@test.com', role: Role.Manager }),
				// 400: Administrator
				employeeFactory.create({ id: 401, email: '401@test.com', role: Role.Administrator }),
			],
		});
	});

	afterAll(async () => {
		await server.stop();
	});
}
