-- This runs once when the postgres container is first initialized.
-- It creates a separate database for the auction service alongside keycloak's DB.
SELECT 'CREATE DATABASE auctiondb OWNER keycloak'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'auctiondb')\gexec