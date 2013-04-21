using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace GG4NET
{
    /// <summary>
    /// Struktura reprezentująca grupę.
    /// </summary>
    public struct Group
    {
        /// <summary>ID grupy.</summary>
        public string Id;
        /// <summary>Nazwa grupy.</summary>
        public string Name;
        /// <summary>Czy grupa jest rozwinięta w GUI?</summary>
        public bool IsExpanded;
        /// <summary>Czy grupa może być usunięta?</summary>
        public bool IsRemoveable;
    }

    /// <summary>
    /// Struktura reprezentująca kontakt.
    /// </summary>
    public struct Contact
    {
        /// <summary>Id kontaktu.</summary>
        public string Id;
        /// <summary>Numer GG.</summary>
        public uint Uin;
        /// <summary>Nazwa wyświetlana.</summary>
        public string ShowName;
        /// <summary>Grupy do których należy kontakt.</summary>
        public Group[] Groups;
        /// <summary>Linki do avatarów kontaktu.</summary>
        public string[] Avatars;
        /// <summary>Typ kontaktu.</summary>
        public ContactType Type;
        /// <summary>NUmer telefonu komórkowego.</summary>
        public string MobilePhone;
        /// <summary>Numer telefonu domowego.</summary>
        public string HomePhone;
        /// <summary>Adres e-mail.</summary>
        public string Email;
        /// <summary>Strona WWW.</summary>
        public string WwwAddress;
        /// <summary>Imię.</summary>
        public string FirstName;
        /// <summary>Nazwisko.</summary>
        public string LastName;
        /// <summary>Pseudonim.</summary>
        public string NickName;
        /// <summary>Płeć.</summary>
        public Gender Gender;
        /// <summary>Data urodzenia.</summary>
        public string BirthDate;
        /// <summary>Miasto.</summary>
        public string City;
    }
    
    /// <summary>
    /// Reprezentuje listę kontaktów którą można importować i eksportować.
    /// </summary>
    public class ContactList
    {
        #region Properties
        private List<Group> _groups;
        private List<Contact> _contacts;

        /// <summary>
        /// Grupy na liście.
        /// </summary>
        public List<Group> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }
        /// <summary>
        /// Kontakty na liście.
        /// </summary>
        public List<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }
        #endregion

        #region Other
        private delegate T SearchDelegate<T>();
        #endregion

        #region Constructors
        /// <summary>
        /// Stwórz pustą listę kontaktów.
        /// </summary>
        public ContactList()
        {
            _groups = new List<Group>();
            _contacts = new List<Contact>();
        }
        #endregion

        #region Methods
        #region Export
        /// <summary>
        /// Eksportuj listę kontaktów do pliku XML.
        /// </summary>
        /// <param name="fileName">Nazwa pliku</param>
        public void ExportToFile(string fileName)
        {
            ExportToFile(fileName, ContactListType.XML);
        }
        /// <summary>
        /// Eksportuj listę kontaktów do podanego formatu.
        /// </summary>
        /// <param name="fileName">Nazwa pliku</param>
        /// <param name="type">Format fo którego wyeksportować</param>
        public void ExportToFile(string fileName, ContactListType type)
        {
            ExportToFile(this, fileName, type);
        }
        /// <summary>
        /// Eksportuj listę kontaktów do ciągu XML.
        /// </summary>
        /// <returns>Ciąg XML</returns>
        public string ExportToString()
        {
            return ExportToString(ContactListType.XML);
        }
        /// <summary>
        /// Eksportuj listę kontaktów do podanego formatu.
        /// </summary>
        /// <param name="type">Format fo którego wyeksportować</param>
        /// <returns>Lista kontaktów jako ciąg znaków</returns>
        public string ExportToString(ContactListType type)
        {
            return ExportToString(this, type);
        }
        #endregion
        #region StaticImport
        /// <summary>
        /// Importuj listę kontaktów z pliku. Format zostanie wybrany po rozszerzeniu.
        /// </summary>
        /// <param name="fileName">Nazwa pliku</param>
        /// <returns>Zaimportowana lista kontaktów</returns>
        public static ContactList ImportFromFile(string fileName)
        {
            return ImportFromFile(fileName, (fileName.EndsWith(".xml") ? ContactListType.XML : ContactListType.CSV));
        }
        /// <summary>
        /// Importuj listę kontaktów z pliku.
        /// </summary>
        /// <param name="fileName">Nazwa pliku</param>
        /// <param name="type">Format listy kontaktów</param>
        /// <returns>Zaimportowana lista kontaktów</returns>
        public static ContactList ImportFromFile(string fileName, ContactListType type)
        {
            ContactList cl;
            using (StreamReader reader = new StreamReader(new FileStream(fileName, FileMode.Open), (type == ContactListType.XML ? Encoding.UTF8 : Encoding.GetEncoding("windows-1250"))))
            {
                cl = ImportFromString(reader.ReadToEnd(), type);
                reader.Close();
            }
            return cl;
        }
        /// <summary>
        /// Importuj listę kontaktów z ciągu znaków używając formatu XML.
        /// </summary>
        /// <param name="content">Lista kontaktów jako ciąg znaków</param>
        /// <returns>Zaimportowana lista kontaktów</returns>
        public static ContactList ImportFromString(string content)
        {
            return ImportFromString(content, ContactListType.XML);
        }
        /// <summary>
        /// Importuj listę kontaktów z ciągu znaków używając podanego formatu.
        /// </summary>
        /// <param name="content">Lista kontaktów jako ciąg znaków</param>
        /// <param name="type">Format do użycia</param>
        /// <returns>Zaimportowana lista kontaktów</returns>
        public static ContactList ImportFromString(string content, ContactListType type)
        {
            //init
            ContactList cList = new ContactList();

            switch (type)
            {
                #region CSV
                case ContactListType.CSV:
                    string[] lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    string[] header = lines[0].Split(new string[] { ",;" }, StringSplitOptions.None);

                    #region ReadGroups
                    int groupId = 0;
                    foreach (string hG in header)
                    {
                        try
                        {
                            if (hG == "GG70ExportString" || hG == string.Empty) continue;
                            cList.Groups.Add(new Group() { Id = groupId.ToString(), Name = hG, IsExpanded = true, IsRemoveable = true });
                            ++groupId;
                        }
                        catch { }
                    }
                    #endregion

                    lines[0] = "";

                    #region ReadContacts
                    foreach (string line in lines)
                    {
                        try
                        {
                            if (line.StartsWith("i;;;;;;"))
                            {
                                uint uin = uint.Parse(line.Remove(0, 7));
                                for (int i = 0; i < cList.Contacts.Count; i++)
                                {
                                    if (cList.Contacts[i].Uin == uin)
                                    {
                                        Contact c = cList.Contacts[i];
                                        c.Type = ContactType.Blocked;
                                        cList.Contacts[i] = c;
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (line == string.Empty) continue;
                            string[] conSplit = line.Split(';');
                            int contactGroups = conSplit.Length - 13;

                            Contact con = new Contact();
                            con.FirstName = conSplit[0];
                            con.LastName = conSplit[1];
                            con.NickName = conSplit[2];
                            con.ShowName = conSplit[3];
                            con.MobilePhone = conSplit[4];

                            Group[] grps = new Group[contactGroups];
                            for (int i = 0; i < grps.Length; i++)
                            {
                                foreach (Group g in cList.Groups)
                                {
                                    if (g.Id == conSplit[5 + i])
                                    {
                                        grps[i] = g;
                                        break;
                                    }
                                }
                            }
                            con.Groups = grps;

                            con.Uin = uint.Parse(conSplit[5 + contactGroups]);
                            con.Email = conSplit[6 + contactGroups];
                            con.HomePhone = conSplit[12 + contactGroups];
                            con.Type = ContactType.Normal;

                            cList.Contacts.Add(con);
                        }
                        catch { }
                    }
                    #endregion
                    break;
                #endregion
                #region XML
                case ContactListType.XML:

                    XDocument xDoc = XDocument.Parse(content);

                    #region ReadGroups
                    var groups = from r in xDoc.Descendants("Group")
                                 select new Group
                                     {
                                         Id = ValueOrDefault(r, "Id", "0"),
                                         Name = ValueOrDefault(r, "Name", string.Empty),
                                         IsExpanded = bool.Parse(ValueOrDefault(r, "IsExpandedd", "false")),
                                         IsRemoveable = bool.Parse(ValueOrDefault(r, "IsRemovable", "false")),
                                     };
                    cList.Groups = new List<Group>(groups);
                    #endregion

                    #region ReadContacts
                    var contacts = from r in xDoc.Descendants("Contact")
                                   select new Contact
                                   {
                                       Id = ValueOrDefault(r, "Guid", "0"),
                                       Uin = uint.Parse(ValueOrDefault(r, "GGNumber", "0")),                          
                                       ShowName = ValueOrDefault(r, "ShowName", string.Empty),
                                       NickName = ValueOrDefault(r, "ShowName", string.Empty),
                                       MobilePhone = ValueOrDefault(r, "MobilePhone", string.Empty),
                                       HomePhone = ValueOrDefault(r, "HomePhone", string.Empty),
                                       Email = ValueOrDefault(r, "Email", string.Empty),
                                       WwwAddress = ValueOrDefault(r, "WwwAddress", string.Empty),
                                       FirstName = ValueOrDefault(r, "FirstName", string.Empty),
                                       LastName = ValueOrDefault(r, "LastName", string.Empty),
                                       Gender = Utils.ToPublicGender(uint.Parse(ValueOrDefault(r, "Gender", "0"))),
                                       BirthDate = ValueOrDefault(r, "Birth", string.Empty),
                                       City = ValueOrDefault(r, "City", string.Empty),

                                       #region SelectContactGroups
                                       Groups = new SearchDelegate<Group[]>(() =>
                                       {
                                           var grps = from g in r.Descendants("GroupId") select g.Value; //search all groups id in contact
                                           List<Group> contactGroups = new List<Group>();

                                           foreach (Group globalGroup in cList.Groups)
                                           {
                                               foreach (string gGuid in grps)
                                               {
                                                   if (globalGroup.Id == gGuid) // find matching groups
                                                   {
                                                       contactGroups.Add(globalGroup);
                                                       break;
                                                   }
                                               }
                                           }

                                           return contactGroups.ToArray();
                                       }).Invoke(),
                                       #endregion
                                       #region GetAvatarsUrls
                                       Avatars = new SearchDelegate<string[]>(() =>
                                       {
                                           var urls = from u in r.Descendants("URL") select u.Value;
                                           return urls.ToArray();
                                       }).Invoke(),
                                       #endregion
                                       #region FindContactType
                                       Type = new SearchDelegate<ContactType>(() =>
                                       {
                                           bool norm = bool.Parse(ValueOrDefault(r, "FlagNormal", "false"));
                                           bool budd = bool.Parse(ValueOrDefault(r, "FlagBuddy", "false"));
                                           bool ign = bool.Parse(ValueOrDefault(r, "FlagIgnored", "false"));
                                           if (norm)
                                               return ContactType.Normal;
                                           else if (budd)
                                               return ContactType.Offline;
                                           else if (ign)
                                               return ContactType.Blocked;
                                           else
                                               return ContactType.None;
                                       }).Invoke(),
                                       #endregion
                                   };
                    cList.Contacts = new List<Contact>(contacts);
                    #endregion
                    
                    break;
                #endregion
                default: cList = new ContactList(); break;
            }

            return cList;
        }
        #endregion
        #region StaticExport
        /// <summary>
        /// Eksportuj listę kontaktów do pliku XML.
        /// </summary>
        /// <param name="contactList">Lista kontaktów do wyeksportowania</param>
        /// <param name="fileName">Nazwa pliku</param>
        public static void ExportToFile(ContactList contactList, string fileName)
        {
            ExportToFile(contactList, fileName, ContactListType.XML);
        }
        /// <summary>
        /// Eksportuj listę kontaktów do podanego formatu.
        /// </summary>
        /// <param name="contactList">Lista kontaktów do wyeksportowania</param>
        /// <param name="fileName">Nazwa pliku</param>
        /// <param name="type">Format fo którego wyeksportować</param>
        public static void ExportToFile(ContactList contactList, string fileName, ContactListType type)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(fileName, FileMode.Create), (type == ContactListType.XML ? Encoding.UTF8 : Encoding.GetEncoding("windows-1250"))))
            {
                writer.Write(ExportToString(contactList, type));
                writer.Close();
            }
        }
        /// <summary>
        /// Eksportuj listę kontaktów do ciągu XML.
        /// </summary>
        /// <param name="contactList">Lista kontaktów do wyeksportowania</param>
        /// <returns>Ciąg XML</returns>
        public static string ExportToString(ContactList contactList)
        {
            return ExportToString(contactList, ContactListType.XML);
        }
        /// <summary>
        /// Eksportuj listę kontaktów do podanego formatu.
        /// </summary>
        /// <param name="contactList">Lista kontaktów do wyeksportowania</param>
        /// <param name="type">Format fo którego wyeksportować</param>
        /// <returns>Lista kontaktów jako ciąg znaków</returns>
        public static string ExportToString(ContactList contactList, ContactListType type)
        {
            switch (type)
            {
                #region CSV
                case ContactListType.CSV:
                    StringBuilder builder = new StringBuilder();
                    builder.Append("GG70ExportString,;"); //header

                    #region WriteGroups
                    foreach (Group g in contactList.Groups)
                    {
                        builder.Append(g.Name + ",;");
                    }
                    #endregion
                    builder.AppendLine();
                    #region WriteContacts
                    foreach (Contact c in contactList.Contacts)
                    {
                        builder.Append(c.FirstName); // first name
                        builder.Append(';');
                        builder.Append(c.LastName); // last name
                        builder.Append(';');
                        builder.Append(c.NickName); //nick name
                        builder.Append(';');
                        builder.Append(c.ShowName); //show name
                        builder.Append(';');
                        builder.Append(c.MobilePhone); //mobile phone number
                        builder.Append(';');
                        foreach (Group g in c.Groups) //groups
                        {
                            for (int i = 0; i < contactList.Groups.Count; i++)
                            {
                                if (g.Id == contactList.Groups[i].Id)
                                {
                                    builder.Append(i.ToString());
                                    builder.Append(';');
                                    break;
                                }
                            }
                        }
                        builder.Append(c.Uin.ToString()); //gg num
                        builder.Append(';');
                        builder.Append(c.Email); //email
                        builder.Append(';');
                        builder.Append(';'); //message sound
                        builder.Append(';'); //path to message sound
                        builder.Append(';'); //status sound
                        builder.Append(';'); //path to status sound
                        builder.Append(';'); //status change in only friends mode
                        builder.Append(c.HomePhone); //home phone
                        // no ; here
                        builder.AppendLine(); //end contact
                    }
                    #endregion
                    #region WriteIgnore
                    foreach (Contact c in contactList.Contacts)
                    {
                        if (c.Type == ContactType.Blocked)
                        {
                            builder.Append("i;;;;;;");
                            builder.Append(c.Uin.ToString());
                            builder.AppendLine();
                        }
                    }
                    #endregion

                    return builder.ToString();
                #endregion
                #region XML
                case ContactListType.XML:
                    XDocument xDoc = new XDocument();

                    #region WriteGroups
                    XElement[] groups = new XElement[contactList.Groups.Count];
                    for (int i = 0; i < groups.Length; i++)
                    {
                        groups[i] = new XElement("Group",
                            new XElement("Id", contactList.Groups[i].Id),
                            new XElement("Name", contactList.Groups[i].Name),
                            new XElement("IsExpanded", contactList.Groups[i].IsExpanded.ToString().ToLower()),
                            new XElement("IsRemovable", contactList.Groups[i].IsRemoveable.ToString().ToLower()));
                    }
                    #endregion
                    #region WriteContacts
                    XElement[] contacts = new XElement[contactList.Contacts.Count];
                    for (int i = 0; i < contacts.Length; i++)
                    {
                        contacts[i] = new XElement("Contact",
                            new XElement("Guid", contactList.Contacts[i].Id),
                            new XElement("GGNumber", contactList.Contacts[i].Uin.ToString()));

                        string nick = (contactList.Contacts[i].ShowName != string.Empty ? contactList.Contacts[i].ShowName : contactList.Contacts[i].NickName);
                        if (nick != string.Empty) contacts[i].Add(new XElement("ShowName", nick));
                        if (contactList.Contacts[i].MobilePhone != string.Empty) contacts[i].Add(new XElement("MobilePhone", contactList.Contacts[i].MobilePhone));
                        if (contactList.Contacts[i].HomePhone != string.Empty) contacts[i].Add(new XElement("HomePhone", contactList.Contacts[i].HomePhone));
                        if (contactList.Contacts[i].Email != string.Empty) contacts[i].Add(new XElement("Email", contactList.Contacts[i].Email));
                        if (contactList.Contacts[i].WwwAddress != string.Empty) contacts[i].Add(new XElement("WwwAddress", contactList.Contacts[i].WwwAddress));
                        if (contactList.Contacts[i].FirstName != string.Empty) contacts[i].Add(new XElement("FirstName", contactList.Contacts[i].FirstName));
                        if (contactList.Contacts[i].LastName != string.Empty) contacts[i].Add(new XElement("LastName", contactList.Contacts[i].LastName));
                        if (contactList.Contacts[i].Gender != Gender.None) contacts[i].Add(new XElement("Gender", Utils.ToInternalGender(contactList.Contacts[i].Gender).ToString()));
                        if (contactList.Contacts[i].BirthDate != string.Empty) contacts[i].Add(new XElement("Birth", contactList.Contacts[i].BirthDate));
                        if (contactList.Contacts[i].City != string.Empty) contacts[i].Add(new XElement("City", contactList.Contacts[i].City));

                        if (contactList.Contacts[i].Groups != null && contactList.Contacts[i].Groups.Length > 0)
                        {
                            XElement[] grps = new XElement[contactList.Contacts[i].Groups.Length];
                            for (int j = 0; j < grps.Length; j++) grps[j] = new XElement("GroupId", contactList.Contacts[i].Groups[j].Id);
                            if (grps.Length > 0) contacts[i].Add(new XElement("Groups", grps));
                        }

                        if (contactList.Contacts[i].Avatars != null && contactList.Contacts[i].Avatars.Length > 0)
                        {
                            XElement[] avUrls = new XElement[contactList.Contacts[i].Avatars.Length];
                            for (int j = 0; j < avUrls.Length; j++) avUrls[j] = new XElement("URL", contactList.Contacts[i].Avatars[j]);
                            if (avUrls.Length > 0) contacts[i].Add(new XElement("Avatars", avUrls));
                        }
                        else
                        {
                            contacts[i].Add(new XElement("Avatars", new XElement("URL", string.Empty)));
                        }

                        if (contactList.Contacts[i].Type == ContactType.Blocked)
                            contacts[i].Add(new XElement("FlagIgnored", "true"));
                        else if (contactList.Contacts[i].Type == ContactType.Offline)
                            contacts[i].Add(new XElement("FlagBuddy", "true"));
                        else if (contactList.Contacts[i].Type == ContactType.Normal)
                            contacts[i].Add(new XElement("FlagNormal", "true"));
                    }
                #endregion
                    xDoc = new XDocument(
                        new XElement("ContactBook", new XElement("Groups", groups), new XElement("Contacts", contacts)));

                    //remove white chars
                    string xDocStr = xDoc.ToString();
                    xDocStr = xDocStr.Replace("\n", "");
                    xDocStr = xDocStr.Replace("\r", "");
                    xDocStr = xDocStr.Replace("\t", "");
                    xDocStr = xDocStr.Replace(" ", "");
                    return xDocStr;
                #endregion
                default: return string.Empty;
            }
        }
        #endregion

        private static string ValueOrDefault(XElement element, XName name, string def)
        {
            return (element.Element(name) != null ? element.Element(name).Value : def);
        }

        /// <summary>
        /// Eksportuj listę kontaktów używając formatu XML.
        /// </summary>
        /// <returns>Lista kontaktów jako ciąg XML</returns>
        public override string ToString()
        {
            return ExportToString();
        }
        #endregion
    }
}
