// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using NUnit.Framework;
using ZhvModels.Mapping.Enums.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenKlant.v2;
using ZhvModels.Properties;

namespace ZhvModels.Tests.Unit.Mapping.Models.POCOs.OpenKlant.v2
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
            this._validAppSettingsConfiguration = ConfigurationHandler.GetWebApiConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._emptyConfiguration.Dispose();
            this._validAppSettingsConfiguration.Dispose();
        }

        #region Party (processing multiple roles)
        [Test]
        public void Party_ForMany_MissingResults_ThrowsHttpRequestException()
        {
            // Arrange
            var partyResults = new PartyResults();  // Empty "Results" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._emptyConfiguration, partyResults, ZhvResources.HttpRequest_ERROR_EmptyPartiesResults);
        }

        [Test]
        public void Party_ForMany_ExistingResults_ButMissingAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResults partyResults = GetTestPartyResults();  // Empty "DigitalAddresses" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._emptyConfiguration, partyResults, ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForMany_ExistingResults_ButEmptyDigitalAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult partyResult = GetTestPartyResult_None(this._validAppSettingsConfiguration);
            PartyResults partyResults = GetTestPartyResults(partyResult);  // Missing e-mails and phone numbers

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResults, ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForMany_ExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testPartyPhone = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, testId, testId);  // Email should have priority over phone
            PartyResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);  // If both phone and email are preferred, email would have priority

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Since email is priority, phone should be ignored
            });
        }

        [Test]
        public void Party_ForMany_ExistingResults_Without_MatchingPreferredAddress_ReturnsExpectedResult_Email_FirstEncountered_PriorityOverPhone()
        {
            // Arrange
            PartyResult testPartyPhone = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());  // Email should have priority over phone
            PartyResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);  // If both phone and email are preferred, email would have priority

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
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
        public void Party_ForMany_ExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
            });
        }

        [Test]
        public void Party_ForMany_ExistingResults_Without_MatchingPreferredAddress_ReturnsExpectedResult_Phone_FirstEncountered()
        {
            // Arrange
            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"first_{TestPhone}"));  // First encountered phone is returned because the preferred address couldn't be determined
            });
        }

        #endregion

        #region Party (processing single role)
        [Test]
        public void Party_ForSingle_ExistingResult_ButMissingAddress_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult partyResult = new();  // Empty digital addresses

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResult, ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForSingle_ExistingResult_ButEmptyDigitalAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult partyResult = GetTestPartyResult_None(this._validAppSettingsConfiguration);  // Missing e-mails and phone numbers

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResult, ZhvResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, testId, testId);  // Email should have priority over phone

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = PartyResults.Party(this._validAppSettingsConfiguration, testPartyEmail);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Only email is provided
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_Without_MatchingPreferredAddress_ReturnsExpectedResult_Email_FirstEncountered_PriorityOverPhone()
        {
            // Arrange
            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());  // Email should have priority over phone

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = PartyResults.Party(this._validAppSettingsConfiguration, testPartyEmail);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"first_{TestEmail}"));  // First encountered email is returned because the preferred address couldn't be determined
                Assert.That(actualPhoneNumber, Is.Empty);  // Only email is provided
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_Without_MatchingPreferredAddress_ReturnsExpectedResult_Phone_FirstEncountered()
        {
            // Arrange
            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"first_{TestPhone}"));  // First encountered phone is returned because the preferred address couldn't be determined
            });
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// In this set of test data there are no phone numbers or e-mails.
        /// </summary>
        private static PartyResult GetTestPartyResult_None(WebApiConfiguration testConfiguration)
        {
            var partyId = Guid.NewGuid();
            Guid firstAddressId = GetUniqueId(partyId);
            Guid secondAddressId = GetUniqueId(firstAddressId);

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
                        Name = "Samantha",
                        SurnamePrefix = string.Empty,
                        Surname = "Rogers"
                    }
                },
                Expansion = new Expansion
                {
                    DigitalAddresses =
                    [
                        new DigitalAddressLong
                        {
                            Id = firstAddressId,
                            Value = string.Empty,  // Just empty address
                            Type = testConfiguration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new DigitalAddressLong
                        {
                            Id = secondAddressId,
                            Value = string.Empty,  // Just empty address
                            Type = testConfiguration.AppSettings.Variables.EmailGenericDescription()
                        }
                    ]
                }
            };
        }

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
                    DigitalAddresses =
                    [
                        new DigitalAddressLong(), // Just empty address (might be not fully initialized)

                        new DigitalAddressLong()  // Preferred address and valid type but empty e-mail (cannot be used)
                        {
                            Id = partyId,
                            Value = string.Empty,
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        },

                        new DigitalAddressLong()  // Preferred address and valid email but empty type (cannot be used)
                        {
                            Id = partyId,
                            Value = $"emptyType_{TestEmail}",
                            Type = string.Empty
                        },

                        new DigitalAddressLong()  // Preferred address but unsupported type (only e-mail and phone numbers)
                        {
                            Id = partyId,
                            Value = "https://www.facebook.com/john.doe",
                            Type = "Facebook"
                        },

                        new DigitalAddressLong()  // Not preferred address but valid phone (e-mails have priority => keep processing)
                        {
                            Id = GetUniqueId(addressId),
                            Value = TestPhone,
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new DigitalAddressLong()  // Not preferred address but valid e-mail (should be used if the preferred e-mail is not found => until then keep processing)
                        {
                            Id = GetUniqueId(addressId),
                            Value = $"first_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        },

                        new DigitalAddressLong()  // Can be preferred address with valid e-mail (not first encountered, should be returned if preferred)
                        {
                            Id = addressId,
                            Value = $"second_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        }
                    ]
                }
            };
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
                    DigitalAddresses =
                    [
                        new DigitalAddressLong(), // Just empty address (might be not fully initialized)

                        new DigitalAddressLong()  // Preferred address and valid type but empty phone (cannot be used)
                        {
                            Id = partyId,
                            Value = string.Empty,
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new DigitalAddressLong()  // Preferred address and valid phone but empty type (cannot be used)
                        {
                            Id = partyId,
                            Value = $"emptyType_{TestPhone}",
                            Type = string.Empty
                        },

                        new DigitalAddressLong()  // Preferred address but unsupported type (only e-mail and phone numbers)
                        {
                            Id = partyId,
                            Value = "https://www.facebook.com/john.doe",
                            Type = "Facebook"
                        },

                        new DigitalAddressLong()  // Not preferred address but valid phone (should be used if the preferred phone is not found => until then keep processing)
                        {
                            Id = GetUniqueId(addressId),
                            Value = $"first_{TestPhone}",
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new DigitalAddressLong()  // Can be preferred address with valid phone (not first encountered, should be returned if preferred)
                        {
                            Id = addressId,
                            Value = $"second_{TestPhone}",
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        }
                    ]
                }
            };
        }

        // ReSharper disable once TailRecursiveCall
        private static Guid GetUniqueId(Guid addressId)
        {
            var newId = Guid.NewGuid();

            return newId != addressId  // NOTE: In turbo rare cases newly generated GUID can be identical
                ? newId
                : GetUniqueId(addressId);  // Retry
        }

        private static PartyResults GetTestPartyResults(params PartyResult[] parties)
        {
            var partyResults = new PartyResults
            {
                Results = [default]
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

        private static void AssertThrows<TException>(WebApiConfiguration configuration, PartyResult partyResult, string exceptionMessage)
            where TException : Exception
        {
            Assert.Multiple(() =>
            {
                TException? exception = Assert.Throws<TException>(() =>
                    PartyResults.Party(configuration, partyResult));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }
        #endregion
    }
}