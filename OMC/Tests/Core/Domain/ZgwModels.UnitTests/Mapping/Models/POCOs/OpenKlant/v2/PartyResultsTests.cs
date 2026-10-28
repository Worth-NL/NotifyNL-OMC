// © 2024, Worth Systems.

using Common.Settings.Configuration;
using Common.Tests.Utilities._TestHelpers;
using NUnit.Framework;
using ZgwModels.Exceptions;
using ZgwModels.Mapping.Enums.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;
using ZgwModels.Properties;

namespace ZgwModels.Tests.Unit.Mapping.Models.POCOs.OpenKlant.v2
{
    [TestFixture]
    public sealed class PartyResultsTests
    {
        private OmcConfiguration _emptyConfiguration = null!;
        private OmcConfiguration _validAppSettingsConfiguration = null!;

        [OneTimeSetUp]
        public void TestsInitialize()
        {
            this._emptyConfiguration = ConfigurationHandler.GetOmcConfiguration();
            this._validAppSettingsConfiguration = ConfigurationHandler.GetOmcConfigurationWith(ConfigurationHandler.TestLoaderTypesSetup.ValidAppSettings);
        }

        [OneTimeTearDown]
        public void TestsCleanup()
        {
            this._emptyConfiguration.Dispose();
            this._validAppSettingsConfiguration.Dispose();
        }

        #region Party (processing multiple roles)
        [Test]
        public void Party_ForMany_MissingResults_ThrowsPartyNotFoundException()
        {
            // Arrange
            var partyResults = new PartyResults();  // Empty "Results" inside

            // Act & Assert
            // NOTE: Its own type, so a caller can tell an absent party from an unreachable "OpenKlant".
            AssertThrows<PartyNotFoundException>(this._emptyConfiguration, partyResults, ZgwResources.HttpRequest_ERROR_EmptyPartiesResults);
        }

        [Test]
        public void Party_ForMany_ExistingResults_ButMissingAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResults partyResults = GetTestPartyResults();  // Empty "DigitalAddresses" inside

            // Act & Assert
            AssertThrows<HttpRequestException>(this._emptyConfiguration, partyResults, ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForMany_ExistingResults_ButEmptyDigitalAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult partyResult = GetTestPartyResult_None(this._validAppSettingsConfiguration);
            PartyResults partyResults = GetTestPartyResults(partyResult);  // Missing e-mails and phone numbers

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResults, ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
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
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Since email is priority, phone should be ignored
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        [Test]
        public void Party_ForMany_ExistingResults_Without_AQualifyingAddress_ThrowsHttpRequestException()
        {
            // Arrange: plain addresses only - no case link, no preferred one, no default one, no "portaalvoorkeur"
            PartyResult testPartyPhone = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResults testPartyResults = GetTestPartyResults(testPartyEmail, testPartyPhone);

            // Act & Assert: an address is never picked just for being there
            Assert.Throws<HttpRequestException>(() => testPartyResults.Party(this._validAppSettingsConfiguration));
        }

        [Test]
        public void Party_ForMany_ExistingResults_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        [Test]
        public void Party_ForMany_ExistingResults_With_MatchingCaseIdentifier_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Email_CaseMatch(this._validAppSettingsConfiguration, testId, addressId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = testPartyResults.Party(this._validAppSettingsConfiguration, CaseIdentification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"first_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Since email is priority, phone should be ignored
                Assert.That(actualReason, Does.Contain(CaseIdentification!));
            });
        }

        [Test]
        public void Party_ForMany_ExistingResults_Without_AQualifyingAddress_ButTolerant_ReturnsAPartyWithoutAChannel()
        {
            // Arrange
            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, _)
                = testPartyResults.Party(this._validAppSettingsConfiguration, requireDigitalAddress: false);

            // Assert: a caller with a non-digital fallback of its own (MOBB's letter) still gets a party back -
            //         the first result, as before
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyResults.Results[0]));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Unknown));
                Assert.That(actualEmailAddress, Is.Empty);
                Assert.That(actualPhoneNumber, Is.Empty);
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
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResult, ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForSingle_ExistingResult_ButEmptyDigitalAddresses_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult partyResult = GetTestPartyResult_None(this._validAppSettingsConfiguration);  // Missing e-mails and phone numbers

            // Act & Assert
            AssertThrows<HttpRequestException>(this._validAppSettingsConfiguration, partyResult, ZgwResources.HttpRequest_ERROR_NoDigitalAddresses);
        }

        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, testId, testId);  // Email should have priority over phone

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testPartyEmail);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Only email is provided
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingCaseIdentifier_ReturnsExpectedResult_Email()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testPartyEmail = GetTestPartyResult_Email_CaseMatch(this._validAppSettingsConfiguration, testId, addressId);  // Email should have priority over phone

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testPartyEmail, CaseIdentification);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testPartyEmail));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"first_{TestEmail}"));  // The preferred address was found
                Assert.That(actualPhoneNumber, Is.Empty);  // Only email is provided
                Assert.That(actualReason, Does.Contain(CaseIdentification!));
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_Without_AQualifyingEmail_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult testPartyEmail = GetTestPartyResult_Email(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => PartyResults.Party(this._validAppSettingsConfiguration, testPartyEmail));
        }

        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        [Test]
        public void Party_ForSingle_ExistingResult_Without_AQualifyingPhone_ThrowsHttpRequestException()
        {
            // Arrange
            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, Guid.Empty, Guid.NewGuid());

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => PartyResults.Party(this._validAppSettingsConfiguration, testParty));
        }

        // Checks with new digitalAddress Type telefoonnummer instead of Telefoon
        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone_V2()
        {
            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone_V2(testId, testId);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);  // Only phone is provided
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        // Tests the correct setting of Digital Address Type. Should Return "Email"
        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Phone_Email()
        {
            // Arrange
            var partyId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone_Email(this._validAppSettingsConfiguration, partyId, addressId);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));
                Assert.That(actualPhoneNumber, Is.Empty);  // The preferred address was found
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }

        // Tests the correct setting of Digital Address Type.Should Return "SMS"
        [Test]
        public void Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Email_Phone()
        {
            // Arrange
            var partyId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Email_Phone(this._validAppSettingsConfiguration, partyId, addressId);

            // Act
            (PartyResult actualParty, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, string actualReason)
                = PartyResults.Party(this._validAppSettingsConfiguration, testParty);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualParty, Is.EqualTo(testParty));
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));  // The preferred address was found
                Assert.That(actualReason, Does.Contain("preferred"));
            });
        }
        [Test]
        public void Party_WithRequiredChannel_Email_SkipsAPreferredPhone_AndReturnsTheEmailBehindIt()
        {
            // NOTE: The counterpart of
            //       Party_ForSingle_ExistingResult_With_MatchingPreferredAddress_ReturnsExpectedResult_Email_Phone,
            //       which asserts the default behaviour on this very same party: the preferred phone wins
            //       and the e-mail comes back empty. A caller that can only send e-mail would read that as
            //       "no address on file" and never notify someone it could have reached.

            // Arrange: the e-mail behind the preferred phone is the party's default one, so it qualifies
            var partyId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Email_Phone(this._validAppSettingsConfiguration, partyId, addressId);
            DigitalAddressLong email = testParty.Expansion.DigitalAddresses[1];
            email.IsStandardAddress = true;
            testParty.Expansion.DigitalAddresses[1] = email;
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (_, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, _)
                = testPartyResults.Party(this._validAppSettingsConfiguration,
                    caseIdentifier: null, requireDigitalAddress: true, requiredChannel: DistributionChannels.Email);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(actualEmailAddress, Is.EqualTo($"second_{TestEmail}"));
                Assert.That(actualPhoneNumber, Is.Empty);
            });
        }

        [Test]
        public void Party_WithRequiredChannel_Email_ButOnlyPhonesOnFile_ThrowsHttpRequestException()
        {
            // NOTE: Restricting the channel must not invent an address. A party reachable only by phone is
            //       unreachable to an e-mail-only caller, and has to read as such.

            // Arrange
            var testId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Phone(this._validAppSettingsConfiguration, testId, testId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act & Assert
            Assert.Throws<HttpRequestException>(() =>
                testPartyResults.Party(this._validAppSettingsConfiguration,
                    caseIdentifier: null, requireDigitalAddress: true, requiredChannel: DistributionChannels.Email));
        }

        [Test]
        public void Party_WithoutRequiredChannel_KeepsFollowingThePartysOwnPreference()
        {
            // NOTE: The default has to stay exactly as it was - the classic ZGW scenarios deliver on
            //       whichever channel the party prefers, and must not start being filtered.

            // Arrange
            var partyId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            PartyResult testParty = GetTestPartyResult_Email_Phone(this._validAppSettingsConfiguration, partyId, addressId);
            PartyResults testPartyResults = GetTestPartyResults(testParty);

            // Act
            (_, DistributionChannels actualDistChannel, string actualEmailAddress, string actualPhoneNumber, _)
                = testPartyResults.Party(this._validAppSettingsConfiguration);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actualDistChannel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(actualEmailAddress, Is.Empty);
                Assert.That(actualPhoneNumber, Is.EqualTo($"second_{TestPhone}"));
            });
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// In this set of test data there are no phone numbers or e-mails.
        /// </summary>
        private static PartyResult GetTestPartyResult_None(OmcConfiguration testConfiguration)
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
        private const string? CaseIdentification = "caseIdentification";

        /// <summary>
        /// In this set of test data there are phone numbers and e-mails combined, so e-mail should be always prioritized.
        /// </summary>
        private static PartyResult GetTestPartyResult_Email(OmcConfiguration configuration, Guid partyId, Guid addressId)
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
        /// In this set of test data there are phone numbers and e-mails combined, so e-mail should be always prioritized.
        /// </summary>
        private static PartyResult GetTestPartyResult_Email_CaseMatch(OmcConfiguration configuration, Guid partyId, Guid addressId)
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
                            Id = GetUniqueId(partyId),
                            Value = TestPhone,
                            Type = configuration.AppSettings.Variables.PhoneGenericDescription()
                        },

                        new DigitalAddressLong()  // Not preferred address but valid e-mail (should be used if the preferred e-mail is not found => until then keep processing)
                        {
                            Id = GetUniqueId(partyId),
                            Value = $"first_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription(),
                            Reference = CaseIdentification!
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
        private static PartyResult GetTestPartyResult_Phone(OmcConfiguration configuration, Guid partyId, Guid addressId)
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

        /// <summary>
        /// In this set of test data there is no emails, so phone numbers will always be used.
        /// </summary>
        private static PartyResult GetTestPartyResult_Phone_V2(Guid partyId, Guid addressId)
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
                            Type = "telefoonnummer"
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
                            Type = "telefoonnummer"
                        },

                        new DigitalAddressLong()  // Can be preferred address with valid phone (not first encountered, should be returned if preferred)
                        {
                            Id = addressId,
                            Value = $"second_{TestPhone}",
                            Type = "telefoonnummer"
                        }
                    ]
                }
            };
        }

        /// <summary>
        /// In this set of test data there is one email, and one phone number. Tests the correct setting of Digital Address Type. Should Return "Email".
        /// </summary>
        private static PartyResult GetTestPartyResult_Phone_Email(OmcConfiguration configuration, Guid partyId, Guid addressId)
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
                        new DigitalAddressLong()  // Preferred address with valid e-mail should be returned
                        {
                            Id = partyId,
                            Value = $"second_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        },

                        new DigitalAddressLong()  // valid phone number not preferred address should not be returned
                        {
                            Id = addressId,
                            Value = $"second_{TestPhone}",
                            Type = "telefoonnummer"
                        }
                    ]
                }
            };
        }

        /// <summary>
        /// In this set of test data there is one email, and one phone number. Tests the correct setting of Digital Address Type. Should Return "SMS".
        /// </summary>
        private static PartyResult GetTestPartyResult_Email_Phone(OmcConfiguration configuration, Guid partyId, Guid addressId)
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
                        new DigitalAddressLong()  // Preferred address with valid phone should be returned
                        {
                            Id = partyId,
                            Value = $"second_{TestPhone}",
                            Type = "telefoonnummer"
                        },

                        new DigitalAddressLong()  // valid address with valid e-mail not preferred address should not be returned
                        {
                            Id = addressId,
                            Value = $"second_{TestEmail}",
                            Type = configuration.AppSettings.Variables.EmailGenericDescription()
                        }
                    ]
                }
            };
        }

        #region Party (isStandaardAdres and portaalvoorkeur)
        private const string Portaalvoorkeur = "portaalvoorkeur";

        private DigitalAddressLong Email(string value, bool isStandard = false, string reference = "")
            => new() { Id = Guid.NewGuid(), Value = value, Type = this._validAppSettingsConfiguration.AppSettings.Variables.EmailGenericDescription(), IsStandardAddress = isStandard, Reference = reference };

        private static DigitalAddressLong Phone(string value, bool isStandard = false, string reference = "")
            => new() { Id = Guid.NewGuid(), Value = value, Type = "telefoonnummer", IsStandardAddress = isStandard, Reference = reference };

        private static PartyResult PartyWith(Guid? preferredAddressId, params DigitalAddressLong[] addresses)
            => new()
            {
                PreferredDigitalAddress = preferredAddressId is { } id ? new DigitalAddressShort { Id = id } : null,
                Expansion = new Expansion { DigitalAddresses = [.. addresses] }
            };

        private (DistributionChannels Channel, string Address, string Reason) Select(PartyResult party, string? caseIdentifier = null)
        {
            (_, DistributionChannels channel, string email, string phone, string reason)
                = GetTestPartyResults(party).Party(this._validAppSettingsConfiguration, caseIdentifier);

            return (channel, channel == DistributionChannels.Email ? email : phone, reason);
        }

        [Test]
        public void Party_StandardAddress_WinsOverAPlainOne()
        {
            PartyResult party = PartyWith(null, Email("plain@test.nl"), Email("standard@test.nl", isStandard: true));

            (DistributionChannels channel, string address, string reason) = Select(party);

            Assert.Multiple(() =>
            {
                Assert.That(channel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(address, Is.EqualTo("standard@test.nl"));
                Assert.That(reason, Does.Contain("isStandaardAdres"));
            });
        }

        [Test]
        public void Party_StandardEmailAndStandardPhone_PrefersTheEmail()
        {
            PartyResult party = PartyWith(null, Phone("0612345678", isStandard: true), Email("standard@test.nl", isStandard: true));

            Assert.That(Select(party).Address, Is.EqualTo("standard@test.nl"));
        }

        [Test]
        public void Party_OnlyAStandardPhone_IsUsed()
        {
            PartyResult party = PartyWith(null, Email("plain@test.nl"), Phone("0612345678", isStandard: true));

            (DistributionChannels channel, string address, _) = Select(party);

            Assert.Multiple(() =>
            {
                Assert.That(channel, Is.EqualTo(DistributionChannels.Sms));
                Assert.That(address, Is.EqualTo("0612345678"));
            });
        }

        [Test]
        public void Party_PortaalvoorkeurEmail_IsUsed_WhenThereIsNoStandardAddress()
        {
            PartyResult party = PartyWith(null, Email("plain@test.nl"), Email("portal@test.nl", reference: Portaalvoorkeur));

            (DistributionChannels channel, string address, string reason) = Select(party);

            Assert.Multiple(() =>
            {
                Assert.That(channel, Is.EqualTo(DistributionChannels.Email));
                Assert.That(address, Is.EqualTo("portal@test.nl"));
                Assert.That(reason, Does.Contain(Portaalvoorkeur));
            });
        }

        [Test]
        public void Party_PortaalvoorkeurEmailAndPhone_PrefersTheEmail()
        {
            PartyResult party = PartyWith(null, Phone("0612345678", reference: Portaalvoorkeur), Email("portal@test.nl", reference: Portaalvoorkeur));

            Assert.That(Select(party).Address, Is.EqualTo("portal@test.nl"));
        }

        [Test]
        public void Party_StandardAddress_WinsOverPortaalvoorkeur()
        {
            PartyResult party = PartyWith(null, Email("portal@test.nl", reference: Portaalvoorkeur), Phone("0612345678", isStandard: true));

            Assert.That(Select(party).Address, Is.EqualTo("0612345678"));
        }

        [Test]
        public void Party_PreferredAddress_WinsOverStandardAndPortaalvoorkeur()
        {
            DigitalAddressLong preferred = Email("preferred@test.nl");
            PartyResult party = PartyWith(preferred.Id,
                Email("standard@test.nl", isStandard: true), Email("portal@test.nl", reference: Portaalvoorkeur), preferred);

            Assert.That(Select(party).Address, Is.EqualTo("preferred@test.nl"));
        }

        [Test]
        public void Party_CaseLinkedAddress_WinsOverEverything()
        {
            DigitalAddressLong preferred = Email("preferred@test.nl", isStandard: true);
            PartyResult party = PartyWith(preferred.Id, preferred, Email("case@test.nl", reference: "ZAAK-1"));

            Assert.That(Select(party, caseIdentifier: "ZAAK-1").Address, Is.EqualTo("case@test.nl"));
        }

        [Test]
        public void Party_OnlyPlainAddresses_ThrowsHttpRequestException()
        {
            PartyResult party = PartyWith(null, Email("plain@test.nl"), Phone("0612345678"));

            Assert.Throws<HttpRequestException>(() => Select(party));
        }
        #endregion

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

        private static void AssertThrows<TException>(OmcConfiguration configuration, PartyResults partyResults, string exceptionMessage)
            where TException : Exception
        {
            Assert.Multiple(() =>
            {
                TException? exception = Assert.Throws<TException>(() =>
                    partyResults.Party(configuration));

                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));
            });
        }

        private static void AssertThrows<TException>(OmcConfiguration configuration, PartyResult partyResult, string exceptionMessage)
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