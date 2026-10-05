// © 2024, Worth Systems.

using System.Text.Json;
using Common.Settings.Configuration;
using WebQueries.DataQuerying.Adapter.Interfaces;
using WebQueries.DataSending.Models.DTOs;
using WebQueries.MOBB.Models;
using WebQueries.Print.Models;
using WebQueries.Producten.Models;
using WebQueries.Register.Interfaces;
using WebQueries.Versioning.Interfaces;
using ZgwModels.Enums;
using ZgwModels.Extensions;
using ZgwModels.Mapping.Models.POCOs.Objecten.Print;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.Register.v2
{
    /// <inheritdoc cref="ITelemetryService"/>
    /// <remarks>
    ///   Version: "Klantcontacten" Web API service | "OMC workflow" v2.
    /// </remarks>
    /// <seealso cref="IVersionDetails"/>
    public sealed class ContactRegistration : ITelemetryService
    {
        /// <inheritdoc cref="ITelemetryService.QueryContext"/>
        public IQueryContext QueryContext { get; }

        /// <summary>
        /// 
        /// </summary>
        public OmcConfiguration Omc { get; }

        private readonly OmcConfiguration _configuration;

        /// <inheritdoc cref="IVersionDetails.Name"/>
        string IVersionDetails.Name => "Klantcontacten";

        /// <inheritdoc cref="IVersionDetails.Version"/>
        string IVersionDetails.Version => "2.0.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactRegistration"/> class.
        /// </summary>
        public ContactRegistration(OmcConfiguration configuration, IQueryContext queryContext, OmcConfiguration omc)  // Dependency Injection (DI)
        {
            this._configuration = configuration;
            this.QueryContext = queryContext;
            Omc = omc;
        }

        // TODO: Collapse the five Get*ContactMomentJsonBody builders onto one.
        //
        // They emit the same document. The klantcontact core, the betrokkene block, the positional
        // messages[] contract and the four-field onderwerpobjectidentificator are identical in all of
        // them; what actually varies is the objectId source, two configuration keys, whether
        // hoofdOnderwerpType is emitted, where originalResourceUrl comes from, and MOBB prefixing its
        // subject with a channel label. That is a small record passed to one builder, not five copies.
        //
        // The copies have already drifted, and none of it was decided:
        //  - print, MOBB and product escape their values with JsonSerializer; the zaak and v1 bodies
        //    interpolate configuration straight into hand-written quotes, so a quote or backslash in
        //    VARIABLES_OPENKLANT_* silently produces malformed JSON
        //  - indicatieContactGelukt is emitted unquoted and defaults to "true" in print and product, and
        //    to string.Empty in the zaak body - which is invalid JSON whenever messages has fewer than
        //    three entries (unreachable today only because its single caller always passes four)
        //  - objectId is nullable in the zaak body (Guid? CaseId) and un-fallbacked in print, so both can
        //    register a klantcontact against an empty object id
        //
        // Each new scenario copies whichever neighbour it landed next to and inherits that neighbour's
        // accidents, so this gets worse per scenario rather than staying flat.
        //
        // Order matters when picking this up: only GetPrintContactMomentJsonBody has tests. Characterise
        // the other four first, or unifying them quietly changes what every existing scenario writes to
        // OpenKlant.

        #region Polymorphic
        /// <inheritdoc cref="ITelemetryService.GetNewCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        string ITelemetryService.GetNewCreateContactMomentJsonBody(
            NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages) // CaseStatus is only used for v1 implementation
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;
            DateTime sentAt = messages.Count > 3 && DateTime.TryParse(messages[3], out DateTime parsedDate) ? parsedDate : DateTime.Now;

            // Escape string values safely
            string safeSubject = JsonSerializer.Serialize(userMessageSubject);
            string safeBody = JsonSerializer.Serialize(userMessageBody);
            string safeKanaal = JsonSerializer.Serialize(notificationMethod.ToKanaal());
            string safeOriginalResourceUrl = JsonSerializer.Serialize(reference.Notification.ResourceUri.AbsoluteUri);

            return $"{{\"klantcontact\":{{" +
                   $"\"kanaal\":{safeKanaal}," +                                             // ENG: Channel of communication (notification)
                   $"\"onderwerp\":{safeSubject}," +                                         // ENG: Subject (of the message to be sent to the user)
                   $"\"inhoud\":{safeBody}," +                                               // ENG: Content (of the message to be sent to the user)
                   $"\"indicatieContactGelukt\":{isSuccessfullySent}," +                     // ENG: Indication of successful contact
                   $"\"taal\":\"nl\"," +                                                     // ENG: Language (of the notification)
                   $"\"vertrouwelijk\":true," +                                              // Fixed: added comma
                   $"\"plaatsgevondenOp\":\"{sentAt:O}\"," +                                 // Fixed: interpolated variable with ISO 8601 format
                   $"\"metadata\":{{\"originalResourceUrl\":{safeOriginalResourceUrl}}}" +   // The specific resource (e.g. status) the triggering notification was about
                   $"}}," +
                   $"\"betrokkene\":{{" +
                   $"\"wasPartij\":{{\"uuid\":\"{reference.PartyId}\"}}," +
                   $"\"rol\":\"klant\"," +
                   $"\"initiator\":true" +
                   $"}}," +
                   $"\"onderwerpobject\":{{" +
                   $"\"onderwerpobjectidentificator\":{{" +
                   $"\"objectId\":\"{reference.CaseId}\"," +
                   $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType()}\"," +
                   $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister()}\"," +
                   $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                   $"}}" +
                   $"}}" +
                   $"}}";
        }
            

        /// <inheritdoc cref="ITelemetryService.GetPrintContactMomentJsonBody(PrintNotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        /// <remarks>
        ///   <para>
        ///     Two things set this apart from <see cref="ITelemetryService.GetNewCreateContactMomentJsonBody"/>.
        ///   </para>
        ///   <para>
        ///     The "onderwerpobjectidentificator" is taken from the triggering object's payload, with
        ///     configuration only filling in whichever fields were left blank. Everywhere else the flow
        ///     itself knows what it registers against, so config alone is right; here the writing party
        ///     states it, so the payload has to win.
        ///   </para>
        ///   <para>
        ///     The printed PDF is not attached here. "maak-klantcontact" has no field for it - MBO-1025
        ///     records that as requested by Den Haag but deferred - so the bijlage is linked afterwards by
        ///     a second call to "/bijlagen"; see <see cref="ITelemetryService.GetBijlageJsonBody"/>.
        ///   </para>
        /// </remarks>
        string ITelemetryService.GetPrintContactMomentJsonBody(
            PrintNotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : reference.Subject;
            string userMessageBody = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : "true";
            DateTime sentAt = messages.Count > 3 && DateTime.TryParse(messages[3], out DateTime parsedDate) ? parsedDate : DateTime.Now;

            // Escape string values safely
            string safeSubject = JsonSerializer.Serialize(userMessageSubject);
            string safeBody = JsonSerializer.Serialize(userMessageBody);
            string safeKanaal = JsonSerializer.Serialize(notificationMethod.ToKanaal());
            string safeOriginalResourceUrl = JsonSerializer.Serialize(reference.OriginalResourceUrl.AbsoluteUri);
            string safeHoofdOnderwerpType = JsonSerializer.Serialize(reference.SubjectObjectTypeUri?.AbsoluteUri ?? string.Empty);

            SubjectObjectIdentifier subjectObject = reference.SubjectObject ?? new SubjectObjectIdentifier();

            string safeObjectId = JsonSerializer.Serialize(subjectObject.ObjectId);
            string safeCodeObjectType = JsonSerializer.Serialize(Fallback(
                subjectObject.CodeObjectType, this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType()));
            string safeCodeRegister = JsonSerializer.Serialize(Fallback(
                subjectObject.CodeRegister, this._configuration.AppSettings.Variables.OpenKlant.CodeRegister()));
            string safeCodeSoortObjectId = JsonSerializer.Serialize(Fallback(
                subjectObject.CodeSoortObjectId, this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()));

            return $"{{\"klantcontact\":{{" +
                   $"\"kanaal\":{safeKanaal}," +                                             // ENG: Channel of communication (notification)
                   $"\"onderwerp\":{safeSubject}," +                                         // ENG: Subject (of the message to be sent to the user)
                   $"\"inhoud\":{safeBody}," +                                               // ENG: Content (of the message to be sent to the user)
                   $"\"indicatieContactGelukt\":{isSuccessfullySent}," +                     // ENG: Indication of successful contact
                   $"\"taal\":\"nl\"," +                                                     // ENG: Language (of the notification)
                   $"\"vertrouwelijk\":true," +
                   $"\"plaatsgevondenOp\":\"{sentAt:O}\"," +
                   $"\"hoofdOnderwerpType\":{safeHoofdOnderwerpType}," +                     // The zaaktype of the onderwerpobject, supplied by the caller
                   $"\"metadata\":{{\"originalResourceUrl\":{safeOriginalResourceUrl}}}" +   // The print-request object the triggering notification was about
                   $"}}," +
                   $"\"betrokkene\":{{" +
                   $"\"wasPartij\":{{\"uuid\":\"{reference.PartyId}\"}}," +
                   $"\"rol\":\"klant\"," +
                   $"\"initiator\":true" +
                   $"}}," +
                   $"\"onderwerpobject\":{{" +
                   $"\"onderwerpobjectidentificator\":{{" +
                   $"\"objectId\":{safeObjectId}," +
                   $"\"codeObjecttype\":{safeCodeObjectType}," +
                   $"\"codeRegister\":{safeCodeRegister}," +
                   $"\"codeSoortObjectId\":{safeCodeSoortObjectId}" +
                   $"}}" +
                   $"}}" +
                   $"}}";
        }

        /// <summary>
        /// Returns <paramref name="supplied"/> when the payload actually carried it, and the configured
        /// default otherwise.
        /// </summary>
        private static string Fallback(string supplied, string configured)
            => string.IsNullOrWhiteSpace(supplied) ? configured : supplied;

        /// <inheritdoc cref="ITelemetryService.GetCreateContactMomentJsonBody(NotifyReference, NotifyMethods, IReadOnlyList{string}, CaseStatus?)"/>
        string ITelemetryService.GetCreateContactMomentJsonBody(
            NotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages, CaseStatus? caseStatus) // CaseStatus is only used for v1 implementation
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody    = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;

            return $"{{" +
                     $"\"kanaal\":\"{notificationMethod.ToKanaal()}\"," +              // ENG: Channel of communication (notification)
                     $"\"onderwerp\":\"{userMessageSubject}\"," +           // ENG: Subject (of the message to be sent to the user)
                     $"\"inhoud\":\"{userMessageBody}\"," +                 // ENG: Content (of the message to be sent to the user)
                     $"\"indicatieContactGelukt\":{isSuccessfullySent}," +  // ENG: Indication of successful contact
                     $"\"taal\":\"nl\"," +                                  // ENG: Language (of the notification)
                     $"\"vertrouwelijk\":false" +                           // ENG: Confidentiality (of the notification)
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetProductContactMomentJsonBody(ProductNotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        /// <remarks>
        ///   The product is the "onderwerpobject", so unlike the case scenarios there is no zaak here at
        ///   all. Its codes come from configuration rather than from the payload - nothing outside OMC
        ///   states them for a product, unlike the print flow where the writing party does.
        /// </remarks>
        string ITelemetryService.GetProductContactMomentJsonBody(
            ProductNotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : reference.ProductName;
            string userMessageBody = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : "true";
            DateTime sentAt = messages.Count > 3 && DateTime.TryParse(messages[3], out DateTime parsedDate)
                ? parsedDate
                : DateTime.Now;

            // Escape string values safely
            string safeSubject = JsonSerializer.Serialize(userMessageSubject);
            string safeBody = JsonSerializer.Serialize(userMessageBody);
            string safeKanaal = JsonSerializer.Serialize(notificationMethod.ToKanaal());
            string safeOriginalResourceUrl = JsonSerializer.Serialize(reference.OriginalResourceUrl);
            string safeObjectId = JsonSerializer.Serialize(reference.ProductId.ToString());
            string safeCodeObjectType = JsonSerializer.Serialize(
                this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType_Product());
            string safeCodeRegister = JsonSerializer.Serialize(
                this._configuration.AppSettings.Variables.OpenKlant.CodeRegister_Product());
            string safeCodeSoortObjectId = JsonSerializer.Serialize(
                this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId());

            return $"{{\"klantcontact\":{{" +
                   $"\"kanaal\":{safeKanaal}," +                                             // ENG: Channel of communication (notification)
                   $"\"onderwerp\":{safeSubject}," +                                         // ENG: Subject (of the message to be sent to the user)
                   $"\"inhoud\":{safeBody}," +                                               // ENG: Content (of the message to be sent to the user)
                   $"\"indicatieContactGelukt\":{isSuccessfullySent}," +                     // ENG: Indication of successful contact
                   $"\"taal\":\"nl\"," +                                                     // ENG: Language (of the notification)
                   $"\"vertrouwelijk\":true," +
                   $"\"plaatsgevondenOp\":\"{sentAt:O}\"," +
                   $"\"metadata\":{{\"originalResourceUrl\":{safeOriginalResourceUrl}}}" +   // The product the triggering notification was about
                   $"}}," +
                   $"\"betrokkene\":{{" +
                   $"\"wasPartij\":{{\"uuid\":\"{reference.PartyId}\"}}," +
                   $"\"rol\":\"klant\"," +
                   $"\"initiator\":true" +
                   $"}}," +
                   $"\"onderwerpobject\":{{" +
                   $"\"onderwerpobjectidentificator\":{{" +
                   $"\"objectId\":{safeObjectId}," +
                   $"\"codeObjecttype\":{safeCodeObjectType}," +
                   $"\"codeRegister\":{safeCodeRegister}," +
                   $"\"codeSoortObjectId\":{safeCodeSoortObjectId}" +
                   $"}}" +
                   $"}}" +
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetBijlageJsonBody(Guid, Guid)"/>
        string ITelemetryService.GetBijlageJsonBody(Guid klantcontactUuid, Guid documentUuid)
        {
            return $"{{" +
                   $"\"wasBijlageVanKlantcontact\":{{\"uuid\":\"{klantcontactUuid}\"}}," +   // ENG: The klantcontact this attachment belongs to
                   $"\"bijlageidentificator\":{{" +
                   $"\"objectId\":\"{documentUuid}\"," +                                      // ENG: The "enkelvoudiginformatieobject" holding the PDF
                   $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType_Bijlage()}\"," +
                   $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister_Bijlage()}\"," +
                   $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                   $"}}" +
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetActorCustomerContactMomentJsonBody(Guid, Guid)"/>
        string ITelemetryService.GetActorCustomerContactMomentJsonBody(Guid actor, Guid customerContactMoment)
        {
            return $"{{" +
                   $"\"actor\":{{" +
                   $"\"uuid\":\"{actor}\"" +
                   $"}}," +
                   $"\"klantcontact\":{{" +
                   $"\"uuid\":\"{customerContactMoment}\"" +
                   $"}}" +
                   $"}}";
        }

        /// <inheritdoc cref="ITelemetryService.GetMessageBoxContactMomentJsonBody(MessageBoxNotifyReference, NotifyMethods, IReadOnlyList{string})"/>
        /// <remarks>
        ///   First-version draft, mirrors <see cref="ITelemetryService.GetNewCreateContactMomentJsonBody"/> but
        ///   links the "onderwerpobject" to the Bericht (<see cref="MessageBoxNotifyReference.MessageId"/>)
        ///   instead of a Case. TODO (unconfirmed): <c>CodeObjectType_Bericht</c>/<c>CodeRegister_Bericht</c>
        ///   are placeholder config values - this Bericht object type has not actually been registered/
        ///   confirmed in OpenKlant yet, unlike the "zaak"/"open-zaak" values used elsewhere.
        /// </remarks>
        string ITelemetryService.GetMessageBoxContactMomentJsonBody(
            MessageBoxNotifyReference reference, NotifyMethods notificationMethod, IReadOnlyList<string> messages)
        {
            string userMessageSubject = messages.Count > 0 ? messages[0] : string.Empty;
            string userMessageBody = messages.Count > 1 ? messages[1] : string.Empty;
            string isSuccessfullySent = messages.Count > 2 ? messages[2] : string.Empty;
            DateTime sentAt = messages.Count > 3 && DateTime.TryParse(messages[3], out DateTime parsedDate) ? parsedDate : DateTime.Now;

            // Escape string values safely
            // Prefixes the subject with a short channel label (e.g. "[MOBB]", "[MOBB-fallback: e-mail]") so
            // the multiple possible outputs for the same Bericht (MOBB / e-mail fallback / letter fallback)
            // are clearly distinguishable in a portal listing a Bericht's contactmomenten - see the BPMN's
            // shared success-contactmoment step's own annotation ("meerdere outputs... duidelijk weergegeven").
            string safeSubject = JsonSerializer.Serialize($"[{BuildChannelLabel(reference, notificationMethod)}] {userMessageSubject}");
            string safeBody = JsonSerializer.Serialize(userMessageBody);
            string safeKanaal = JsonSerializer.Serialize(notificationMethod.ToKanaal());

            return $"{{\"klantcontact\":{{" +
                   $"\"kanaal\":{safeKanaal}," +                                             // ENG: Channel of communication (notification)
                   $"\"onderwerp\":{safeSubject}," +                                         // ENG: Subject (of the message to be sent to the user)
                   $"\"inhoud\":{safeBody}," +                                               // ENG: Content (of the message to be sent to the user)
                   $"\"indicatieContactGelukt\":{isSuccessfullySent}," +                     // ENG: Indication of successful contact
                   $"\"taal\":\"nl\"," +                                                     // ENG: Language (of the notification)
                   $"\"vertrouwelijk\":true," +
                   $"\"plaatsgevondenOp\":\"{sentAt:O}\"" +
                   $"}}," +
                   $"\"betrokkene\":{{" +
                   $"\"wasPartij\":{{\"uuid\":\"{reference.PartyId}\"}}," +
                   $"\"rol\":\"klant\"," +
                   $"\"initiator\":true" +
                   $"}}," +
                   $"\"onderwerpobject\":{{" +
                   $"\"onderwerpobjectidentificator\":{{" +
                   $"\"objectId\":\"{reference.MessageId}\"," +
                   $"\"codeObjecttype\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectType_Bericht()}\"," +
                   $"\"codeRegister\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeRegister_Bericht()}\"," +
                   $"\"codeSoortObjectId\":\"{this._configuration.AppSettings.Variables.OpenKlant.CodeObjectTypeId()}\"" +
                   $"}}" +
                   $"}}" +
                   $"}}";
        }

        /// <summary>
        /// Builds a short, human-readable label distinguishing which channel a MOBB Bericht's contactmoment
        /// actually went out over, and why - for display in a portal's list of a Bericht's contactmomenten.
        /// </summary>
        private static string BuildChannelLabel(MessageBoxNotifyReference reference, NotifyMethods notificationMethod)
        {
            if (reference.Mobb)
            {
                return "MOBB";
            }

            return reference.WasGefaaldeNotificatie
                ? $"MOBB-fallback: {notificationMethod} (na mislukte notificatie)"
                : $"MOBB-fallback: {notificationMethod}";
        }
        #endregion
    }
}