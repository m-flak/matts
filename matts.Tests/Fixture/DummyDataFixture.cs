namespace matts.Tests.Fixture;

public sealed class DummyDataFixture
{
    public static string LinkedIn_AccessToken
        => @"{
                ""access_token"":""AQUvlL_DYEzvT2wz1QJiEPeLioeA"",
                ""expires_in"":5184000,
                ""scope"":""r_basicprofile,r_primarycontact""
             }";

    public static string LinkedIn_Profile
        => @"{
				""firstName"":{
					""localized"":{
						""en_US"":""Bob""
					},
					""preferredLocale"":{
						""country"":""US"",
						""language"":""en""
					}
				},
				""localizedFirstName"": ""Bob"",
				""headline"":{
					""localized"":{
						""en_US"":""API Enthusiast at LinkedIn""
					},
					""preferredLocale"":{
						""country"":""US"",
						""language"":""en""
					}
				},
				""localizedHeadline"": ""API Enthusiast at LinkedIn"",
				""vanityName"": ""bsmith"",
				""id"":""yrZCpj2Z12"",
				""lastName"":{
					""localized"":{
						""en_US"":""Smith""
					},
					""preferredLocale"":{
						""country"":""US"",
						""language"":""en""
					}
				},
				""localizedLastName"": ""Smith"",
				""profilePicture"": {
					""displayImage"": ""urn:li:digitalmediaAsset:C4D00AAAAbBCDEFGhiJ""
				}
			 }";

    public static string LinkedIn_PrimaryContact
        => @"{
                ""elements"": [
                    {
                        ""handle"": ""urn:li:emailAddress:3775708763"",
                        ""handle~"": {
                            ""emailAddress"": ""ding_wei_stub@example.com""
                        },
                        ""primary"": true,
                        ""type"": ""EMAIL""
                    },
                    {
                        ""handle"": ""urn:li:phoneNumber:6146249836070047744"",
                        ""handle~"": {
                            ""phoneNumber"": {
                                    ""number"": ""158****1473""
                                }
                        },
                        ""primary"": true,
                        ""type"": ""PHONE""
                    }
                ]
             }";


    private DummyDataFixture() { }
}
