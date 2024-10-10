// © 2024, Worth Systems.

using EventsHandler.Mapping.Enums.OpenKlant;
using EventsHandler.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Properties;
using EventsHandler.Services.Settings.Configuration;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Mapping.Models.POCOs.OpenKlant.v2
{
    [TestFixture]
    public sealed class PartyResultsTests
    {
        private WebApiConfiguration _emptyConfiguration = null!;
        private WebApiConfiguration _validAppSettingsConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._emptyConfiguration = ConfigurationHandler.GetWebApiConfiguration();
            this._validAppSettingsConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypes.ValidAppSettings);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._emptyConfiguration.Dispose();
            this._validAppSettingsConfiguration.Dispose();
        }

        #region Party (method)
        [Test]
        public void Party_Method_ForMissingResults_ThrowsHttpRequestException()
        {
            // Arrange
            var partyResults = new CitizenResults();  // Empty "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._emptyConfiguration, partyResults, Resources.HttpRequest_ERROR_EmptyPartiesResults);
        }

        [Test]
        public void Party_Method_ForExistingResults_ButMissingAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            CitizenResults partyResults = GetTestPartyResults();  // Empty "DigitalAddresses" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._emptyConfiguration, partyResults, Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_Method_ForExistingResults_ButEmptyDigitalAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            var partyId = Guid.NewGuid();
            Guid firstAddressId = GetUniqueId(partyId);
            Guid secondAddressId = GetUniqueId(firstAddressId);

            var partyResult = new CitizenResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = partyId
                },
                Identification = new CitizenIdentification
                {
                    Details = new CitizenDetails
                    {
                        Name = "Samantha",
                        SurnamePrefix = string.Empty,
                        Surname = "Rogers"
                    }
                },
                Expansion = new Expansion
                {
                    DigitalAddresses = new List<DigitalAddressLong>
                    {
                        new()  // Just empty address
                        {
                            Id = firstAddressId,
                            Value = string.Empty,
                            Type = this._validAppSettingsConfiguration.AppSettings.Variables.PhoneGenericDescription()
                        },
                        new()  // Just empty address
                        {
                            Id = secondAddressId,
                            Value = string.Empty,
                            Type = this._validAppSettingsConfiguration.AppSettings.Variables.EmailGenericDescription()
                        }
                    }
                }
            };

            CitizenResults partyResults = GetTestPartyResults(partyResult);  // Missing e-mails and phone numbers

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResults, Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_Method_ForExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();

            CitizenResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, testId, testId);
            CitizenResult testPartyPhone = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            CitizenResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);

            // Act
            (CitizenResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"first_{TestEmail}"));
                Assert.That(actualPhoneNumber, Is.Empty);  // Since email is priority, phone should be ignored
            });
        }

        [Test]
        public void Party_Method_ForExistingResults_Without_MatchingPreferredAddress_ReturnsExpectedResult_Email_FirstEncountered_PriorityOverPhone()
        {
            // Arrange
            CitizenResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            CitizenResult testPartyPhone = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            CitizenResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);

            // Act
            (CitizenResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"first_{TestEmail}"));  // First encountered email is returned because the preferred address couldn't be determined
                Assert.That(actualPhoneNumber, Is.Empty);  // Since email is priority, phone should be ignored
            });
        }

        [Test]
        public void Party_Method_ForExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();

            CitizenResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            CitizenResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (CitizenResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Since email is priority, phone should be ignored
                Assert.That(actualPhoneNumber, Is.EqualTo($"first_{TestPhone}"));
            });
        }

        [Test]
        public void Party_Method_ForExistingResults_Without_MatchingPreferredAddress_ReturnsExpectedResult_Phone_FirstEncountered()
        {
            // Arrange
            CitizenResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            CitizenResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (CitizenResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Since email is priority, phone should be ignored
                Assert.That(actualPhoneNumber, Is.EqualTo($"first_{TestPhone}"));
            });
        }

        #endregion

        #region Helper methods
        private const string TestEmail = "john.doe@gmail.com";
        private const string TestPhone = "+44 7911 123456";

        /// <summary>
        /// In this set of test data there are phone numbers and e-mails combined, so e-mail should be always prioritized.
        /// </summary>
        private static CitizenResult GetTestPartyResult_Email(WebApiConfiguration configuration, Guid partyId, Guid addressId)
        {
            return new CitizenResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = partyId
                },
                Identification = new CitizenIdentification
                {
                    Details = new CitizenDetails
                    {
                        Name = "John",
                        SurnamePrefix = string.Empty,
                        Surname = "Doe"
                    }
                },
                Expansion = new Expansion
                {
                    DigitalAddresses = new List<DigitalAddressLong>
                    {
                        new(), // Just empty address (might be not fully initialized)

                        new()  // Preferred address and valid type but empty e-mail (cannot be used)
                        {
                            Id = partyId,
                            Value = string.Empty,
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        },

                        new()  // Preferred address and valid email but empty type (cannot be used)
                        {
                            Id = partyId,
                            Value = $"emptyType_{TestEmail}",
                            Type = string.Empty
                        },

                        new()  // Preferred address but unsupported type (only e-mail and phone numbers)
                        {
                            Id = partyId,
                            Value = "https://www.facebook.com/john.doe",
                            Type = "Facebook"
                        },

                        new()  // Never preferred address but matching 1st phone (e-mails have priority)
                        {
                            Id = GetUniqueId(addressId),
                            Value = TestPhone,
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new()  // Can be preferred address or not (addressId) matching 1st e-mail
                        {
                            Id = addressId,
                            Value = $"first_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        },

                        new()  // Never preferred address but matching 2nd e-mail (not first encountered)
                        {
                            Id = GetUniqueId(addressId),
                            Value = $"second_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        }
                    }
                }
            };
        }

        /// <summary>
        /// In this set of test data there is no emails, so phone numbers will always be used.
        /// </summary>
        private static CitizenResult GetTestPartyResult_Phone(WebApiConfiguration configuration, Guid partyId, Guid addressId)
        {
            return new CitizenResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = partyId
                },
                Identification = new CitizenIdentification
                {
                    Details = new CitizenDetails
                    {
                        Name = "Jane",
                        SurnamePrefix = string.Empty,
                        Surname = "Doe"
                    }
                },
                Expansion = new Expansion
                {
                    DigitalAddresses = new List<DigitalAddressLong>
                    {
                        new(), // Just empty address (might be not fully initialized)

                        new()  // Preferred address and valid type but empty phone (cannot be used)
                        {
                            Id = partyId,
                            Value = string.Empty,
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new()  // Preferred address and valid phone but empty type (cannot be used)
                        {
                            Id = partyId,
                            Value = $"emptyType_{TestPhone}",
                            Type = string.Empty
                        },

                        new()  // Preferred address but unsupported type (only e-mail and phone numbers)
                        {
                            Id = partyId,
                            Value = "https://www.facebook.com/john.doe",
                            Type = "Facebook"
                        },

                        new()  // Can be preferred address or not (addressId) matching 1st phone
                        {
                            Id = addressId,
                            Value = $"first_{TestPhone}",
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new()  // Never preferred address but matching 2nd phone (not first encountered)
                        {
                            Id = GetUniqueId(addressId),
                            Value = $"second_{TestPhone}",
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        }
                    }
                }
            };
        }

        private static Guid GetUniqueId(Guid addressId)
        {
            var newId = Guid.NewGuid();

            return newId != addressId  // NOTE: In turbo rare cases newly generated GUID can be identical
                ? newId
                : GetUniqueId(addressId);  // Retry
        }

        private static CitizenResults GetTestPartyResults(params CitizenResult[] parties)
        {
            var partyResults = new CitizenResults
            {
                Results = new List<CitizenResult>
                {
                    new()  // Just empty result to always have at least a few of them + to handle uninitialized ones
                }
            };

            partyResults.Results.AddRange(parties);

            return partyResults;
        }

        private static void AssertThrows<TException>(WebApiConfiguration configuration, CitizenResults partyResults, string exceptionMessage)
            where TException : Exception
        {
            Assert.Multiple(() =>
            {
                TException? exception = Assert.Throws<TException>(() =>
                    partyResults.Party(configuration));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }
        #endregion
    }
}