
namespace GG4NET
{
    /// <summary>
    /// Struktura przechowująca odpowiedź publicznego katalogu GG.
    /// </summary>
    public struct PublicDirectoryReply
    {
        /// <summary>Numer GG.</summary>
        public uint Uin;
        /// <summary>Imię.</summary>
        public string FirstName;
        /// <summary>Nazwisko.</summary>
        public string LastName;
        /// <summary>Pseudonim.</summary>
        public string Nickname;
        /// <summary>Rok urodzenia.</summary>
        public int Birthyear;
        /// <summary>Miejsce zamieszkania.</summary>
        public string City;
        /// <summary>Płeć.</summary>
        public Gender Gender;
        /// <summary>Nazwisko panieńskie.</summary>
        public string FamilyName;
        /// <summary>Miejsce zamieszkania w dzieciństwie.</summary>
        public string FamilyCity;
        /// <summary>Status.</summary>
        public Status Status;
    }
}
