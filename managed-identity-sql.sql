-- Step 1: Set the database context
USE [YourDatabaseName]; -- Replace with the name of your database

-- Step 2: Create a user for the managed identity
-- Replace '[ManagedIdentityName]' with the name of your managed identity
CREATE USER [ManagedIdentityName] FROM EXTERNAL PROVIDER;

-- Step 3: Grant necessary roles for read/write access
-- Grant db_datareader for read permissions
ALTER ROLE db_datareader ADD MEMBER [ManagedIdentityName];

-- Grant db_datawriter for write permissions
ALTER ROLE db_datawriter ADD MEMBER [ManagedIdentityName];

-- Optional: Grant additional permissions, such as executing stored procedures
-- Uncomment the following line if needed
-- GRANT EXECUTE TO [ManagedIdentityName];

-- Optional: Verify that the user has been added successfully
SELECT 
    dp.name AS UserName,
    dp.type_desc AS UserType,
    r.name AS RoleName
FROM 
    sys.database_principals dp
    JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE 
    dp.name = 'ManagedIdentityName'; -- Replace with your managed identity name
