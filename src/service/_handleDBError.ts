import ServiceError from '../core/serviceError'; // Import ServiceError voor aangepaste foutafhandeling

const handleDBError = (error: unknown) => {
	if (error instanceof Error) {
		const { code = '', message } = error as any;

		// Handle duplicate unique constraint violations (P2002)
		if (code === 'P2002') {
			switch (true) {
				case message.includes('idx_site_name_unique'):
					throw ServiceError.validationFailed('A site with this name already exists');
				case message.includes('idx_machine_code_unique'):
					throw ServiceError.validationFailed('A machine with this code already exists');
				case message.includes('idx_employee_email_unique'):
					throw ServiceError.validationFailed('An employee with this email already exists');
				case message.includes('idx_employee_phone_unique'):
					throw ServiceError.validationFailed('An employee with this phone number already exists');
				case message.includes('idx_action_name_unique'):
					throw ServiceError.validationFailed('An action with this name already exists');
				default:
					throw ServiceError.validationFailed('This item already exists');
			}
		}

		// Handle record not found (P2025)
		if (code === 'P2025') {
			switch (true) {
				case message.includes('fk_site_supervisor'):
					throw ServiceError.notFound('No supervisor with this ID exists');
				case message.includes('fk_machine_site'):
					throw ServiceError.notFound('No site with this ID exists');
				case message.includes('fk_machine_technician'):
					throw ServiceError.notFound('No technician with this ID exists');
				case message.includes('fk_machine_product'):
					throw ServiceError.notFound('No product with this ID exists');
				case message.includes('fk_maintenance_machine'):
					throw ServiceError.notFound('No machine with this ID exists');
				case message.includes('fk_maintenance_technician'):
					throw ServiceError.notFound('No technician with this ID exists');
				case message.includes('fk_log_action'):
					throw ServiceError.notFound('No action with this ID exists');
				case message.includes('fk_log_employee'):
					throw ServiceError.notFound('No employee with this ID exists');
				case message.includes('fk_log_machine'):
					throw ServiceError.notFound('No machine with this ID exists');
				case message.includes('fk_log_site'):
					throw ServiceError.notFound('No site with this ID exists');
				case message.includes('fk_log_maintenance'):
					throw ServiceError.notFound('No maintenance with this ID exists');
				default:
					throw ServiceError.notFound('Record not found');
			}
		}

		// Handle foreign key constraint violations (P2003)
		if (code === 'P2003') {
			switch (true) {
				case message.includes('fk_site_supervisor'):
					throw ServiceError.conflict('This supervisor is still linked to a site');
				case message.includes('fk_machine_site'):
					throw ServiceError.conflict('This site is still linked to machines');
				case message.includes('fk_machine_technician'):
					throw ServiceError.conflict('This technician is still linked to machines');
				case message.includes('fk_machine_product'):
					throw ServiceError.notFound('This product is still linked to a machine');
				case message.includes('fk_maintenance_machine'):
					throw ServiceError.conflict('This machine is still linked to maintenance records');
				case message.includes('fk_maintenance_technician'):
					throw ServiceError.conflict('This technician is still linked to maintenance records');
				case message.includes('fk_log_action'):
					throw ServiceError.conflict('This action is still linked to log records');
				default:
					throw ServiceError.conflict('Foreign key constraint violated');
			}
		}
	}

	// Rethrow error als het niet in een bekende categorie valt
	throw error;
};

export default handleDBError;
