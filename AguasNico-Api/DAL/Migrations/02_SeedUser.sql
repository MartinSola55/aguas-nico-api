-- Seed default roles
INSERT INTO public."Role" ("Id", "Name")
VALUES ('9aa80896-d3bf-4009-b6a3-a324c8ea9329', 'ADMIN'), ('000bff29-2b57-4fd0-89d3-421ac6c948c0', 'DEALER')
ON CONFLICT DO NOTHING;

-- Seed default admin user
-- Default credentials:
-- Email: admin@localhost
-- Password: Password1!
-- WARNING: change the password (and ideally the email) before deploying to any non-local environment.
-- To rotate the password, generate a new BCrypt hash and replace the value below.
DO $$
DECLARE
    admin_role_id character varying(450);
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public."User" WHERE "Email" = 'admin@localhost') THEN
        admin_role_id := (SELECT "Id" FROM public."Role" WHERE "Name" = 'ADMIN');
        INSERT INTO public."User" ("RoleId", "Name", "LastName", "Email", "PasswordHash")
        VALUES (
            admin_role_id,
            'Admin',
            'Admin',
            'admin@localhost',
            '$2b$11$s4dxamTXdxRFeWnD4DsOM.46BBUQbM6geS6SHNeWPbuozdTKpXpu.'
        );
    END IF;
END $$;