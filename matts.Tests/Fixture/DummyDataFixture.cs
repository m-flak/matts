namespace matts.Tests.Fixture;

public sealed class DummyDataFixture
{
    public static string LinkedIn_AccessToken
        => @"{
                ""access_token"":""AQUvlL_DYEzvT2wz1QJiEPeLioeA"",
                ""expires_in"":5184000,
                ""scope"":""openid profile email""
             }";

    public static string LinkedIn_UserInfo
        => @"{
                ""sub"": ""782bbtaQ"",
                ""name"": ""John Doe"",
                ""given_name"": ""John"",
                ""family_name"": ""Doe"",
                ""picture"": ""https://media.licdn-ei.com/dms/image/C5F03AQHqK8v7tB1HCQ/profile-displayphoto-shrink_100_100/0/"",
                ""locale"": ""en-US"",
                ""email"": ""doe@email.com"",
                ""email_verified"": true
             }";

    private DummyDataFixture() { }
}
