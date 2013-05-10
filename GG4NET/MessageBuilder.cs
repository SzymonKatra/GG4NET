using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Pomocnicza klasa umożliwiająca budowanie wiadomości z formatowaniem tesktu.
    /// </summary>
    public class MessageBuilder
    {
        #region Properties
        private string _plainMessage = string.Empty;
        private string _htmlMessage = string.Empty;
        private byte[] _attributes = null;

        /// <summary>
        /// Wiadomość zapisana czystym tekstem.
        /// </summary>
        public string PlainMessage
        {
            get { return _plainMessage; }
            set { _plainMessage = value; }
        }
        /// <summary>
        /// Wiadomość zapisana w HTML.
        /// </summary>
        public string HtmlMessage
        {
            get { return _htmlMessage; }
            set { _htmlMessage = value; }
        }
        /// <summary>
        /// Atrybuty wiadomości.
        /// </summary>
        public byte[] Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dopisz tekst (html i zwykły) do wiadomości.
        /// Domyślnie bez formatowania, kolor tła i tekstu czarny, czcionka MS Shell Dlg 2 o wielkości 9pt.
        /// </summary>
        /// <param name="text">Tekst.</param>
        /// <param name="formatting">Flagi formatowania.</param>
        /// <param name="redColor">Składowa czerwieni koloru tekstu.</param>
        /// <param name="greenColor">Składowa zieleni koloru tekstu.</param>
        /// <param name="blueColor">Składowa niebieska koloru tekstu.</param>
        /// <param name="fontFamily">Nazwa czcionki.</param>
        /// <param name="fontSize">Rozmiar czcionki, np. 9pt czyli 9 punktów.</param>
        public void AppendText(string text, MessageFormatting formatting = MessageFormatting.None, byte redColor = 0, byte greenColor = 0, byte blueColor = 0, string fontFamily = "MS Shell Dlg 2", string fontSize = "9pt")
        {
            //check color exists. if true then end <span> and set flag
            bool haveColor = false;
            if (redColor != 0 || greenColor != 0 || blueColor != 0)
            {
                haveColor = true;
                if (fontFamily != "MS Shell Dlg 2" || fontSize != "9pt")
                {
                    if (!_htmlMessage.EndsWith("</span>"))
                    {
                        _htmlMessage += "</span>";
                    }
                }
            }

            #region Html
            //open html message if closed
            if (string.IsNullOrEmpty(_htmlMessage) || _htmlMessage.EndsWith("</span>"))
            {
                _htmlMessage += string.Format("<span style=\"color:#{0}{1}{2}; font-family:'{3}'; font-size:{4}; \">",
                    redColor.ToString("X2"),
                    greenColor.ToString("X2"),
                    blueColor.ToString("X2"),
                    fontFamily,
                    fontSize);

            }

            //build tags
            StringBuilder builder = new StringBuilder();

            if (formatting.HasFlag(MessageFormatting.Bold)) builder.Append("<b>");
            if (formatting.HasFlag(MessageFormatting.Erasure)) builder.Append("<s>");
            if (formatting.HasFlag(MessageFormatting.Italic)) builder.Append("<i>");
            if (formatting.HasFlag(MessageFormatting.Subscript)) builder.Append("<sub>");
            if (formatting.HasFlag(MessageFormatting.Superscript)) builder.Append("<sup>");
            if (formatting.HasFlag(MessageFormatting.Underline)) builder.Append("<u>");

            //add text
            builder.Append(text);

            //and close tags
            if (formatting.HasFlag(MessageFormatting.Underline)) builder.Append("</u>");
            if (formatting.HasFlag(MessageFormatting.Superscript)) builder.Append("</sup>");
            if (formatting.HasFlag(MessageFormatting.Subscript)) builder.Append("</sub>");
            if (formatting.HasFlag(MessageFormatting.Italic)) builder.Append("</i>");
            if (formatting.HasFlag(MessageFormatting.Erasure)) builder.Append("</s>");
            if (formatting.HasFlag(MessageFormatting.Bold)) builder.Append("</b>");

            if (formatting.HasFlag(MessageFormatting.NewLine)) builder.Append("<br>");

            //add built message
            _htmlMessage += builder.ToString();
            #endregion

            #region Plain
            //plain message for GG 7.x and oldest
            _plainMessage += text;
            if (formatting.HasFlag(MessageFormatting.NewLine)) _plainMessage += Environment.NewLine;

            if (formatting.HasFlag(MessageFormatting.Bold) || formatting.HasFlag(MessageFormatting.Italic) || formatting.HasFlag(MessageFormatting.Underline) || haveColor)
            {
                //attributes
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(memStream))
                    {
                        //position
                        writer.Write((short)(_plainMessage.Length - text.Length));
                        //flags, bold etc.
                        writer.Write((byte)((formatting.HasFlag(MessageFormatting.Bold) ? Container.GG_FONT_BOLD : 0) |
                            (formatting.HasFlag(MessageFormatting.Italic) ? Container.GG_FONT_ITALIC : 0) |
                            (formatting.HasFlag(MessageFormatting.Underline) ? Container.GG_FONT_UNDERLINE : 0) |
                            (haveColor ? Container.GG_FONT_COLOR : 0)));
                        //color if exists
                        if (haveColor)
                        {
                            writer.Write(redColor);
                            writer.Write(greenColor);
                            writer.Write(blueColor);
                        }
                    }
                    //go attributes to can :D
                    byte[] newData = memStream.ToArray();
                    byte[] structHeader = new byte[3];
                    structHeader[0] = Container.GG_ATTRIBUTES_FLAG;
                    byte[] structLength = BitConverter.GetBytes((short)newData.Length);
                    Buffer.BlockCopy(structLength, 0, structHeader, 1, structLength.Length);

                    if (_attributes == null)
                        _attributes = new byte[newData.Length + structHeader.Length];
                    else
                        Array.Resize(ref _attributes, _attributes.Length + newData.Length + structHeader.Length);

                    //copy new attributes to global can ;P
                    Buffer.BlockCopy(structHeader, 0, _attributes, _attributes.Length - newData.Length - structHeader.Length, structHeader.Length);
                    Buffer.BlockCopy(newData, 0, _attributes, _attributes.Length - newData.Length, newData.Length);
                }
            }
            #endregion
        }

        /// <summary>
        /// Dodaj obrazek do wiadomości.
        /// Obrazek musi być najpierw wysłany na serwer.
        /// </summary>
        /// <param name="hash">Hash obrazka</param>
        public void AppendImage(string hash)
        {
            try
            {
                if (hash.Length != 16) throw new InvalidOperationException("Bad hash length");

                AppendImage(Convert.ToInt64(hash.Remove(8), 16), Convert.ToInt64(hash.Remove(0, 8), 16));
            }
            catch { throw new InvalidOperationException("Bad hash"); }
        }
        /// <summary>
        /// Dodaj obrazek do wiadomości.
        /// Obrazek musi być najpierw wysłany na serwer.
        /// </summary>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        public void AppendImage(long crc32, long length)
        {
            #region Html
            //open html message if closed
            if (string.IsNullOrEmpty(_htmlMessage) || _htmlMessage.EndsWith("</span>"))
            {
                _htmlMessage += string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">");
            }
            _htmlMessage += string.Format("<img name=\"{0}\"></img>", Utils.ComputeHash(crc32, length));
            #endregion

            #region Plain
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    //position
                    writer.Write((short)_plainMessage.Length);
                    writer.Write(Container.GG_FONT_IMAGE);
                    writer.Write(Container.GG_IMAGE_CONST_FIRST);
                    writer.Write(Container.GG_IMAGE_CONST_LAST);
                    writer.Write((uint)length);
                    writer.Write((uint)crc32);
                }

                //go attributes to can :D
                byte[] newData = memStream.ToArray();
                byte[] structHeader = new byte[3];
                structHeader[0] = Container.GG_ATTRIBUTES_FLAG;
                byte[] structLength = BitConverter.GetBytes((short)newData.Length);
                Buffer.BlockCopy(structLength, 0, structHeader, 1, structLength.Length);

                if (_attributes == null)
                    _attributes = new byte[newData.Length + structHeader.Length];
                else
                    Array.Resize(ref _attributes, _attributes.Length + newData.Length + structHeader.Length);

                //copy new attributes to global can ;P
                Buffer.BlockCopy(structHeader, 0, _attributes, _attributes.Length - newData.Length - structHeader.Length, structHeader.Length);
                Buffer.BlockCopy(newData, 0, _attributes, _attributes.Length - newData.Length, newData.Length);
            }
            #endregion
        }
        #endregion
    }
}
