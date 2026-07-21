ALTER TABLE "PaymentMethods" ADD COLUMN IF NOT EXISTS "Code" text;

UPDATE "PaymentMethods" SET "Code" = 'EFECT'  WHERE "Code" IS NULL AND "Name" = 'Efectivo';
UPDATE "PaymentMethods" SET "Code" = 'TRANSF' WHERE "Code" IS NULL AND "Name" = 'Transferencia';
UPDATE "PaymentMethods" SET "Code" = 'MP'     WHERE "Code" IS NULL AND "Name" = 'Mercado Pago';

ALTER TABLE "PaymentMethods" ALTER COLUMN "Code" SET NOT NULL;
