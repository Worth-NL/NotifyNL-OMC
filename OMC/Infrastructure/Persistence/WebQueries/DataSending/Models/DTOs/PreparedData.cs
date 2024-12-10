// © 2024, Worth Systems.

using ZhvModels.Mapping.Models.POCOs.OpenKlant;
using ZhvModels.Mapping.Models.POCOs.OpenZaak;

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
        /// Initializes a new instance of the <see cref="PreparedData"/> struct.
        /// </summary>
        public PreparedData(CommonPartyData party, Uri? caseUri)
        {
            Party = party;
            CaseUri = caseUri;
        }
    }
}