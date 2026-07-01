namespace WebQueries.MijnOverheid.Enums
{
    /// <summary>
    /// Known MijnOverheid CloudEvent types.
    /// </summary>
    public enum MijnOverheidEventType
    {
        /// <summary>Case created or updated (zaak-gemuteerd).</summary>
        CaseMutated,

        /// <summary>Case opened by citizen (zaak-geopend).</summary>
        CaseOpened,

        /// <summary>Case deleted/removed (zaak-verwijderd).</summary>
        CaseDeleted,

        /// <summary>Unknown or unsupported event type.</summary>
        Unknown
    }
}
