-- to start the script in Banking26DDDModules    
-- sqlite3 BankingApi/BankingDb.db <  BankingApi/_4_ApiTests/1ResetTestDb.sql

-- Reset Database for API Tests
-- This script will delete all data from the tables and reset the auto-incrementing primary keys.    
-- Keeps the EF Core migration history table intact.

PRAGMA foreign_keys = OFF;

DELETE FROM "Beneficiaries";
-- DELETE FROM "Transactions";
-- DELETE FROM "Transfers";
DELETE FROM "Accounts";
DELETE FROM "Customers";
-- DELETE FROM "Employees";

PRAGMA foreign_keys = ON;
-- End of 1ResetTestDb.sql
       

       
