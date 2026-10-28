// © 2024, Worth Systems.

using ZgwModels.Mapping.Models.POCOs.OpenKlant;
using ZgwModels.Mapping.Models.POCOs.OpenZaak;

namespace WebQueries.DataSending.Models.DTOs
{
    /// <summary>
    /// The package of data retrieved from API web queries.
    /// </summary>
    public readonly struct PreparedData
    {
        /// <inheritdoc cref="CommonPartyData"/>
        public CommonPartyData Party { get; }

        /// <inheritdoc cref="Case.Uri"/>
        public Uri? CaseUri { get; }

        /// <summary>
        /// The type of the onderwerpobject (e.g. the zaaktype), registered as the klantcontact's "hoofdOnderwerpType".
        /// </summary>
        public Uri? SubjectTypeUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparedData"/> struct.
        /// </summary>
        public PreparedData(CommonPartyData party, Uri? caseUri, Uri? subjectTypeUri = null)
        {
            this.Party = party;
            this.CaseUri = caseUri;
            this.SubjectTypeUri = subjectTypeUri;
        }
    }
}