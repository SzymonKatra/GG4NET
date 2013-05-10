using System;

namespace GG4NET
{
    /// <summary>
    /// Dostępne porty serwera GG.
    /// </summary>
    public enum GGPort
    {
        /// <summary>Brak.</summary>
        None = 0,
        /// <summary>Port 8074 (domyślny)</summary>
        P8074 = 8074,
        /// <summary>Port 443</summary>
        P443 = 443,
    }

    /// <summary>
    /// Dostępne statusy GG.
    /// </summary>
    public enum Status
    {
        /// <summary>Brak.</summary>
        None = 0,
        /// <summary>Niedostępny.</summary>
        NotAvailable,
        /// <summary>Dostępny.</summary>
        Available,
        /// <summary>Zaraz wracam.</summary>
        Busy,
        /// <summary>Niewidoczny.</summary>
        Invisible,
        /// <summary>Nie przeszkadzać.</summary>
        DoNotDisturb,
        /// <summary>PoGGadaj ze mną.</summary>
        FreeForCall,
        /// <summary>Zablokowany.</summary>
        Blocked,
    }

    /// <summary>
    /// Dostępny typy kontaktów GG.
    /// </summary>
    public enum ContactType
    {
        /// <summary>Brak.</summary>
        None = 0,
        /// <summary>Kontakt dla którego będziemy zawsze niedostępni.</summary>
        Offline,
        /// <summary>Normalny kontakt.</summary>
        Normal,
        /// <summary>Zablokowany kontakt.</summary>
        Blocked,
    }

    /// <summary>
    /// Dostępne tryby powiadomienia o pisaniu.
    /// </summary>
    public enum TypingNotifyType
    {
        /// <summary>Brak.</summary>
        None = 0,
        /// <summary>Zaczęto pisanie.</summary>
        Start,
        /// <summary>Brak tekstu.</summary>
        Stop,
    }

    /// <summary>
    /// Dostępne płcie na GG.
    /// </summary>
    public enum Gender
    {
        /// <summary>Brak.</summary>
        None,
        /// <summary>Kobieta.</summary>
        Female,
        /// <summary>Mężczyzna.</summary>
        Male,
    }

    /// <summary>
    /// Typ listy kontaktów.
    /// </summary>
    public enum ContactListType
    {
        /// <summary>
        /// Brak.
        /// </summary>
        None = 0,
        /// <summary>
        /// Format CSV. Używany przez starsze wersje GG.
        /// </summary>
        CSV,
        /// <summary>
        /// Format XML. Używany przez nowsze wersje GG.
        /// </summary>
        XML,
    }

    /// <summary>
    /// Flagi formatowania tekstu.
    /// </summary>
    [Flags]
    public enum MessageFormatting
    {
        /// <summary>Brak formatowania.</summary>
        None = 0,
        /// <summary>Tekst pogrubiony.</summary>
        Bold = 1 << 0,
        /// <summary>Tekst pochyły.</summary>
        Italic = 1 << 1,
        /// <summary>Tekst podkreślony.</summary>
        Underline = 1 << 2,
        /// <summary>Tekst skreślony.</summary>
        Erasure = 1 << 3,
        /// <summary>Dodaj nową linię na koniec tekstu.</summary>
        NewLine = 1 << 4,
        /// <summary>Indeks górny.</summary>
        Superscript = 1 << 5,
        /// <summary>Indeks dolny.</summary>
        Subscript = 1 << 6,
    }
}
