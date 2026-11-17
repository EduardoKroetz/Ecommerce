namespace Ecommerce.IntegrationTests.Fixtures;

// Test-only JWT settings, shared by the services under test, the API host and the tokens built by TestTokens
public static class TestJwt
{
    public const string Key = "integration-tests-signing-key-not-a-secret-0123456789";

    public const string Issuer = "Ecommerce.IntegrationTests";

    public const string Audience = "Ecommerce.IntegrationTests";

    public const int ExpiryInMinutes = 60;
}
