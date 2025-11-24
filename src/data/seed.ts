import {
	PrismaClient,
	Role,
	MachineStatus,
	MaintenanceStatus,
	ProductionStatus,
} from '@prisma/client';
import { hashPassword } from '../core/password';
import * as machineData from './data/machines.json';

const prisma = new PrismaClient();

async function main() {

	// Employees

	const hashedPassword = await hashPassword('password');
	const machineStatusLogs = machineData.machineStatusLogs;
	const machineDailyReports = machineData.machineDailyReports;

	await prisma.employee.create({
		data: {
			firstName: 'Mark',
			lastName: 'Smith',
			dateOfBirth: new Date('1975-09-15'),
			country: 'USA',
			city: 'New York',
			postalCode: '10001',
			streetName: '5th Avenue',
			streetNumber: '10B',
			email: 'mark.smith@example.com',
			phoneNumber: '+1 212 5551234',
			role: Role.Manager,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});

	const employeeAlice = await prisma.employee.create({
		data: {
			firstName: 'Alice',
			lastName: 'Müller',
			dateOfBirth: new Date('1980-04-10'),
			country: 'Germany',
			city: 'Berlin',
			postalCode: '10115',
			streetName: 'Friedrichstraße',
			streetNumber: '1A',
			email: 'alice.mueller@example.com',
			phoneNumber: '+49 30 123456',
			role: Role.Supervisor,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});

	const employeeJohn = await prisma.employee.create({
		data: {
			firstName: 'John',
			lastName: 'Doe',
			dateOfBirth: new Date('1985-07-20'),
			country: 'USA',
			city: 'Los Angeles',
			postalCode: '90001',
			streetName: 'Sunset Boulevard',
			streetNumber: '123',
			email: 'john.doe@example.com',
			phoneNumber: '+1 310 5556789',
			role: Role.Supervisor,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});
	const employeeBob = await prisma.employee.create({
		data: {
			firstName: 'Bob',
			lastName: 'Dupont',
			dateOfBirth: new Date('1985-08-22'),
			country: 'France',
			city: 'Paris',
			postalCode: '75001',
			streetName: 'Rue de Rivoli',
			streetNumber: '20',
			email: 'bob.dupont@example.com',
			phoneNumber: '+33 1 23456789',
			role: Role.Technician,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});

	const employeeCarol = await prisma.employee.create({
		data: {
			firstName: 'Carol',
			lastName: 'Rossi',
			dateOfBirth: new Date('1990-12-05'),
			country: 'Italy',
			city: 'Milan',
			postalCode: '20121',
			streetName: 'Via Monte Napoleone',
			streetNumber: '15',
			email: 'carol.rossi@example.com',
			phoneNumber: '+39 02 987654',
			role: Role.Technician,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});

	const employeeDavid = await prisma.employee.create({
		data: {
			firstName: 'David',
			lastName: 'Smith',
			dateOfBirth: new Date('1995-03-15'),
			country: 'United Kingdom',
			city: 'London',
			postalCode: 'SW1A 1AA',
			streetName: 'Downing Street',
			streetNumber: '10',
			email: 'david.smith@example.com',
			phoneNumber: '+44 20 7123 4567',
			role: Role.Administrator,
			isActive: true,
			passwordHash: hashedPassword,
		},
	});

	// Sites

	const siteBerlin = await prisma.site.create({
		data: {
			name: 'Berlin Plant',
			country: 'Germany',
			city: 'Berlin',
			streetName: 'Unter den Linden',
			streetNumber: '10',
			postalCode: '10117',
			latitude: '52.517197',
			longitude: '13.391195',
			isActive: true,
			supervisorId: employeeAlice.id,
		},
	});

	const siteParis = await prisma.site.create({
		data: {
			name: 'Paris Facility',
			country: 'France',
			city: 'Paris',
			streetName: 'Champs-Élysées',
			streetNumber: '50',
			postalCode: '75008',
			latitude: '48.869931',
			longitude: '2.357042',
			isActive: true,
			supervisorId: employeeAlice.id,
		},
	});

	const siteLondon = await prisma.site.create({
		data: {
			name: 'London Warehouse',
			country: 'United Kingdom',
			city: 'London',
			streetName: 'Baker Street',
			streetNumber: '221B',
			postalCode: 'NW1 6XE',
			latitude: '51.523767',
			longitude: '-0.158091',
			isActive: true,
			supervisorId: employeeAlice.id,
		},
	});

	await prisma.site.create({
		data: {
			name: 'Munich Plant',
			country: 'Germany',
			city: 'Munich',
			streetName: 'Westendstreet',
			streetNumber: '220',
			postalCode: '80686',
			latitude: '48.131938',
			longitude: '11.519196',
			isActive: true,
			supervisorId: employeeJohn.id,
		},
	});

	// Machines

	const machine1 = await prisma.machine.create({
		data: {
			code: 'MACH-001',
			location: 'Assembly Line 1',
			product: { create: { info: 'European Widgets - Model A' } },
			uptime: new Date(),
			site: { connect: { id: siteBerlin.id } },
			technician: { connect: { id: employeeBob.id } },
			status: MachineStatus.Running,
			productionStatus: ProductionStatus.Healthy,
		},
	});

	const machine2 = await prisma.machine.create({
		data: {
			code: 'MACH-002',
			location: 'Assembly Line 2',
			product: { create: { info: 'French Gadgets - Model X' } },
			uptime: new Date(),
			site: { connect: { id: siteParis.id } },
			technician: { connect: { id: employeeCarol.id } },
			status: MachineStatus.Maintenance,
			productionStatus: ProductionStatus.Critical,
		},
	});

	const machine3 = await prisma.machine.create({
		data: {
			code: 'MACH-003',
			location: 'Packaging Area',
			product: { create: { info: 'Berlin Doohickeys - Model Z' } },
			uptime: new Date(),
			site: { connect: { id: siteBerlin.id } },
			technician: { connect: { id: employeeBob.id } },
			status: MachineStatus.Running,
			productionStatus: ProductionStatus.Healthy,
		},
	});

	const machine4 = await prisma.machine.create({
		data: {
			code: 'MACH-004',
			location: 'Warehouse',
			product: { create: { info: 'British Thingamajigs - Model Y' } },
			uptime: new Date(),
			site: { connect: { id: siteLondon.id } },
			technician: { connect: { id: employeeBob.id } },
			status: MachineStatus.Stopped,
			productionStatus: ProductionStatus.Failing,
		},
	});

	await prisma.notification.createMany({
		data: [
			{
				title: 'Machine Status Change',
				message: 'The machine status has been updated to Maintenance.',
				status: 'New',
				employeeId: employeeBob.id,
				machineId: 1, 
				siteId: siteBerlin.id, 
			},
			{
				title: 'Maintenance Completed',
				message: 'The scheduled maintenance has been completed successfully.',
				status: 'Unread',
				employeeId: employeeAlice.id, 
				machineId: 2,
				siteId: siteBerlin.id,
			},
		],
		skipDuplicates: true,
	});

	// Maintenances

	await prisma.maintenance.create({
		data: {
			datePlanned: new Date(),
			startTime: new Date(),
			endTime: new Date(new Date().getTime() + 1000 * 60 * 60), // 1 hour later
			reason: 'Routine checkup',
			maintenanceReport: 'All systems are functioning as expected.',
			comments: 'No issues found during inspection.',
			status: MaintenanceStatus.Completed,
			machine: { connect: { id: machine1.id } },
			technician: { connect: { id: employeeBob.id } },
		},
	});

	await prisma.maintenance.create({
		data: {
			datePlanned: new Date(),
			startTime: new Date(new Date().getTime() - 1000 * 60 * 60), // 1 hour ago
			endTime: new Date(), // now
			reason: 'Minor fault detected',
			maintenanceReport: 'Replaced worn-out component.',
			comments: 'Monitor performance over the next week.',
			status: MaintenanceStatus.Completed,
			machine: { connect: { id: machine2.id } },
			technician: { connect: { id: employeeCarol.id } },
		},
	});

	await prisma.maintenance.create({
		data: {
			datePlanned: new Date(),
			startTime: null,
			endTime: null,
			reason: 'Scheduled maintenance',
			maintenanceReport: null,
			comments: null,
			status: MaintenanceStatus.Scheduled,
			machine: { connect: { id: machine3.id } },
			technician: { connect: { id: employeeBob.id } },
		},
	});

	await prisma.maintenance.create({
		data: {
			datePlanned: new Date(new Date().getDay() + 1),
			startTime: null,
			endTime: null,
			reason: 'Scheduled maintenance',
			maintenanceReport: null,
			comments: 'Maintenance scheduled due to unexpected breakdown.',
			status: MaintenanceStatus.Scheduled,
			machine: { connect: { id: machine4.id } },
			technician: { connect: { id: employeeDavid.id } },
		},
	});

	for (const log of machineStatusLogs) {
		let status: MachineStatus;
		if (log.status === 'Running') {
			status = MachineStatus.Running;
		} else if (log.status === 'Stopped') {
			status = MachineStatus.Stopped;
		} else {
			status = MachineStatus.Maintenance;
		}

		await prisma.machineStatusLog.create({
			data: {
				machineId: log.machineId,
				status: status,
				timestamp: new Date(log.timestamp),
			},
		});
	}

	for (const report of machineDailyReports) {
		await prisma.machineDailyReport.create({
			data: {
				machineId: report.machineId,
				date: new Date(report.date),
				amountProduced: report.amountProduced,
				uptime: report.uptime,
				downtime: report.downtime,
				scrap: report.scrap,
			},
		});
	}

}

main()
	.then(async () => {
		await prisma.$disconnect();
	})
	.catch(async (e) => {
		console.error(e);
		await prisma.$disconnect();
		process.exit(1);
	});
