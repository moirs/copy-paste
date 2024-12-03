public async Task TestDbConnectivity()
{
    var connectionString = settings.ConnectionString;

    logger.LogInformation($"DbConnectivityTest - connection string value being used: {connectionString}.");

    try
    {
        bool isLocal = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        SqlConnection connection;

        if (isLocal)
        {
            connection = new SqlConnection(connectionString);
        }
        else
        {
            logger.LogInformation("DbConnectivityTest - assigning environment variables.");

            var clientId = settings.IdentityOptions.ClientId;
            var tenantId = settings.IdentityOptions.TenantId;
            var tokenFilePath = settings.IdentityOptions.FederatedTokenFile;

            logger.LogInformation($"DbConnectivityTest - using clientId: {clientId} and tenantId: {tenantId} and tokenFilePath: {tokenFilePath}");

            AccessToken token;

            if (settings.IdentityOptions.AuthenticationFeatureToUse != "DefaultAzureCredential")
            {
                var credentialOptions = new WorkloadIdentityCredentialOptions
                {
                    ClientId = clientId, TenantId = tenantId, TokenFilePath = tokenFilePath
                };

                logger.LogInformation("DbConnectivityTest - creating WorkloadIdentityCredential.");
                var credential = new WorkloadIdentityCredential(credentialOptions);
                logger.LogInformation("DbConnectivityTest - successfully created WorkloadIdentityCredential.");

                logger.LogInformation("DbConnectivityTest - getting access token.");
                token = await credential.GetTokenAsync(
                    new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                logger.LogInformation($"DbConnectivityTest - successfully obtained access token: {token.Token}");
            }
            else
            {
                logger.LogInformation("DbConnectivityTest - creating DefaultAzureCredential.");
                var credential = new DefaultAzureCredential();
                token = await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                logger.LogInformation("DbConnectivityTest - successfully created DefaultAzureCredential.");
            }

            logger.LogInformation("DbConnectivityTest - creating sql connection using access token.");
            connection = new SqlConnection(connectionString) { AccessToken = token.Token };
            logger.LogInformation("DbConnectivityTest - successfully created sql connection using access token.");
        }

        using (connection)
        {
            logger.LogInformation("DbConnectivityTest - opening database connectivity.");
            connection.Open();
            logger.LogInformation("DbConnectivityTest - database connection successfully opened.");

            logger.LogInformation("DbConnectivityTest - running select query.");
            await ExecuteSelectQuery(connection);
            logger.LogInformation("DbConnectivityTest - successfully executed select query.");

            logger.LogInformation("DbConnectivityTest - running insert query.");
            await ExecuteInsertQuery(connection);
            logger.LogInformation("DbConnectivityTest - successfully executed insert query.");
        }
    }
    catch (Exception e)
    {
        logger.LogError(e, "DbConnectivityTest - DbConnectivity test failed.");
        throw;
    }
}
