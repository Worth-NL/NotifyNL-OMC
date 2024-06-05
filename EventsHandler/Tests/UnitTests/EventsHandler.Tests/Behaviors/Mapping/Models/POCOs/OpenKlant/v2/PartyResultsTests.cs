// © 2024, Worth Systems.

using EventsHandler.Behaviors.Mapping.Enums.OpenKlant;
using EventsHandler.Behaviors.Mapping.Models.POCOs.OpenKlant.v2;
using EventsHandler.Configuration;
using EventsHandler.Properties;
using EventsHandler.Utilities._TestHelpers;

namespace EventsHandler.UnitTests.Behaviors.Mapping.Models.POCOs.OpenKlant.v2
{
    [TestFixture]
    public class PartyResultsTests
    {
        #region Party (method)
        [Test]
        public void Party_Method_ForMissingResults_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetWebApiConfiguration();

            var partyResults = new PartyResults();  // Empty "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, partyResults, Resources.HttpRequest_ERROR_EmptyPartiesResults);
        }

        [Test]
        public void Party_Method_ForExistingResults_ButMissingAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetWebApiConfiguration();

            PartyResults partyResults = GetTestPartyResults();  // Empty "DigitalAddresses" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(testConfiguration, partyResults, Resources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_Method_ForExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();

            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();
            PartyResult testPartyEmail = GetTestPartyResult_Email(testConfiguration, testId, testId);
            PartyResult testPartyPhone = GetTestPartyResult_Phone(testConfiguration, testId, testId);
            PartyResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(testConfiguration);

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
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();
            PartyResult testPartyEmail = GetTestPartyResult_Email(testConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResult testPartyPhone = GetTestPartyResult_Phone(testConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(testConfiguration);

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

            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();
            PartyResult testParty = GetTestPartyResult_Phone(testConfiguration, testId, testId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(testConfiguration);

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
            WebApiConfiguration testConfiguration = ConfigurationHandler.GetValidAppSettingsConfiguration();
            PartyResult testParty = GetTestPartyResult_Phone(testConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(testConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Since email is priority, phone should be ignored
                Assert.That(actualPhoneNumber, Is.EqualTo($"first_{TestPhone}"));
            });
        }

        #region Helper methods
        private const string TestEmail = "john.doe@gmail.com";
        private const string TestPhone = "+44 7911 123456";
        
        /// <summary>
        /// In this set of test data there are phone numbers and e-mails combined, so e-mail should be always prioritized.
        /// </summary>
        private static PartyResult GetTestPartyResult_Email(WebApiConfiguration configuration, Guid partyId, Guid addressId)
        {
            return new PartyResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = partyId
                },
                Identification = new PartyIdentification
                {
                    Details = new PartyDetails
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

            static Guid GetUniqueId(Guid addressId)
            {
                var newId = Guid.NewGuid();

                return newId != addressId  // NOTE: In turbo rare cases newly generated GUID can be identical
                    ? newId
                    : GetUniqueId(addressId);  // Retry
            }
        }

        /// <summary>
        /// In this set of test data there is no emails, so phone numbers will always be used.
        /// </summary>
        private static PartyResult GetTestPartyResult_Phone(WebApiConfiguration configuration, Guid partyId, Guid addressId)
        {
            return new PartyResult
            {
                PreferredDigitalAddress = new DigitalAddressShort
                {
                    Id = partyId
                },
                Identification = new PartyIdentification
                {
                    Details = new PartyDetails
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

            static Guid GetUniqueId(Guid addressId)
            {
                var newId = Guid.NewGuid();

                return newId != addressId  // NOTE: In turbo rare cases newly generated GUID can be identical
                    ? newId
                    : GetUniqueId(addressId);  // Retry
            }
        }

        private static PartyResults GetTestPartyResults(params PartyResult[] parties)
        {
            var partyResults = new PartyResults
            {
                Results = new List<PartyResult>
                {
                    new()  // Just empty result to always have at least a few of them + to handle uninitialized ones
                }
            };
            
            partyResults.Results.AddRange(parties);

            return partyResults;
        }

        private static void AssertThrows<TException>(WebApiConfiguration configuration, PartyResults partyResults, string exceptionMessage)
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
        #endregion
    }
}