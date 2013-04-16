using System;

namespace GG4NET
{
    /// <summary>
    /// Wyjątek o niezalogowaniu.
    /// </summary>
    public class NotLoggedException : Exception
    {
        /// <summary>
        /// Stwórz wyjątek o niezalogowaniu.
        /// </summary>
        public NotLoggedException() : base()
        {
        }
        /// <summary>
        /// Stwórz wyjątek o niezalogowaniu.
        /// </summary>
        /// <param name="message">Wiadomość</param>
        public NotLoggedException(string message)
            : base(message)
        {
        }
    }
}
