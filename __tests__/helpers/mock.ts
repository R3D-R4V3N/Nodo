import { Role, type Employee } from '@prisma/client';
import { faker } from '@faker-js/faker';
import { hashPassword } from '../../src/core/password';

export class EmployeeFactory {

	static async withPassword(password: string) {
		return new this(await hashPassword(password));
	}

	constructor(readonly passwordHash: string) {}

	create(data: { id: number } & Partial<Employee>): Employee {
		return {
			firstName: faker.person.firstName(),
			lastName: faker.person.lastName(),
			email: faker.internet.email(),
			dateOfBirth: faker.date.birthdate(),
			country: faker.location.country(),
			city: faker.location.city(),
			postalCode: faker.location.zipCode(),
			streetName: faker.location.street(),
			streetNumber: String(faker.number.int({ min: 1, max: 200 })),
			phoneNumber: faker.phone.number(),
			role: Role.Technician,
			isActive: true,
			passwordHash: this.passwordHash,
			...data,
		};
	}
}
