
namespace GG4NET
{
    /// <summary>
    /// Informacje o kontakcie.
    /// </summary>
    public struct ContactInfo
    {
        /// <summary>
        /// Numer GG.
        /// </summary>
        public uint Uin;
        /// <summary>
        /// Typ kontaktu.
        /// </summary>
        public ContactType Type;
        /// <summary>
        /// Status kontaktu.
        /// </summary>
        public Status Status { get; internal set; }
        /// <summary>
        /// Opis kontaktu.
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// Maksymalny rozmiar obrazka który może zostać odebrany przez tego klienta.
        /// </summary>
        public uint MaxImageSize { get; internal set; }
        internal uint Features;
        internal uint Flags;
    }
}
