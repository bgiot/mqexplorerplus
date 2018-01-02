


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;

namespace Dotc.Wpf.Controls.XmlEditor.CodeCompletion
{
    /// <summary>
    /// Utility class that contains xml parsing routines used to determine
    /// the currently selected element so we can provide intellisense.
    /// </summary>
    /// <remarks>
    /// All of the routines return <see cref="XmlElementPath"/> objects
    /// since we are interested in the complete path or tree to the
    /// currently active element.
    /// </remarks>
    public static class XmlParser
    {
        static readonly char[] WhitespaceCharacters = new char[] { ' ', '\n', '\t', '\r' };

        /// <summary>
        /// Gets path of the xml element start tag that the specified
        /// <paramref name="index"/> is currently inside.
        /// </summary>
        /// <remarks>If the index outside the start tag then an empty path
        /// is returned.</remarks>
        public static XmlElementPath GetActiveElementStartPath(string xml, int index)
        {
            var elementText = GetActiveElementStartText(xml, index);
            if (elementText != null)
            {
                return GetActiveElementStartPath(xml, index, elementText);
            }
            return new XmlElementPath();
        }

        /// <summary>
        /// Gets path of the xml element start tag that the specified
        /// <paramref name="index"/> is currently located. This is different to the
        /// GetActiveElementStartPath method since the index can be inside the element
        /// name.
        /// </summary>
        /// <remarks>If the index outside the start tag then an empty path
        /// is returned.</remarks>
        public static XmlElementPath GetActiveElementStartPathAtIndex(string xml, int index)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            // Find first non xml element name character to the right of the index.
            index = GetCorrectedIndex(xml.Length, index);
            if (index < 0)
            { // can happen when xml.Length==0
                return new XmlElementPath();
            }

            var currentIndex = index;
            for (; currentIndex < xml.Length; ++currentIndex)
            {
                var ch = xml[currentIndex];
                if (!IsXmlNameChar(ch))
                {
                    break;
                }
            }

            var elementText = GetElementNameAtIndex(xml, currentIndex);
            if (elementText != null)
            {
                return GetActiveElementStartPath(xml, currentIndex, elementText);
            }
            return new XmlElementPath();
        }

        /// <summary>
        /// Gets the parent element path based on the index position.
        /// </summary>
        public static XmlElementPath GetParentElementPath(string xml)
        {
            return GetFullParentElementPath(xml);
        }

        /// <summary>
        /// Checks whether the attribute at the end of the string is a
        /// namespace declaration.
        /// </summary>
        public static bool IsNamespaceDeclaration(string xml, int index)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return false;
            }

            index = GetCorrectedIndex(xml.Length, index);

            // Move back one character if the last character is an '='
            if (xml[index] == '=')
            {
                xml = xml.Substring(0, xml.Length - 1);
                --index;
            }

            // From the end of the string work backwards until we have
            // picked out the last attribute and reached some whitespace.
            var reversedAttributeName = new StringBuilder();

            var ignoreWhitespace = true;
            var currentIndex = index;
            for (var i = 0; i < index; ++i)
            {

                var currentChar = xml[currentIndex];

                if (Char.IsWhiteSpace(currentChar))
                {
                    if (ignoreWhitespace == false)
                    {
                        // Reached the start of the attribute name.
                        break;
                    }
                }
                else if (Char.IsLetterOrDigit(currentChar) || (currentChar == ':'))
                {
                    ignoreWhitespace = false;
                    reversedAttributeName.Append(currentChar);
                }
                else
                {
                    // Invalid string.
                    break;
                }

                --currentIndex;
            }

            // Did we get a namespace?

            var isNamespace = false || ((reversedAttributeName.ToString() == "snlmx") || (reversedAttributeName.ToString().EndsWith(":snlmx", StringComparison.Ordinal)));

            return isNamespace;
        }

        /// <summary>
        /// Gets the attribute name and any prefix. The namespace
        /// is not determined.
        /// </summary>
        public static QualifiedName GetQualifiedAttributeName(string xml, int index)
        {
            var name = GetAttributeName(xml, index);
            return QualifiedName.FromString(name);
        }

        /// <summary>
        /// Gets the name of the attribute inside but before the specified
        /// index.
        /// </summary>
        public static string GetAttributeName(string xml, int index)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return String.Empty;
            }

            index = GetCorrectedIndex(xml.Length, index);
            return GetAttributeName(xml, index, true, true, true);
        }

        /// <summary>
        /// Gets the name of the attribute and its prefix at the specified index. The index
        /// can be anywhere inside the attribute name or in the attribute value.
        /// The namespace for the element containing the attribute will also be determined
        /// if the includeNamespace flag is set to true.
        /// </summary>
        public static QualifiedName GetQualifiedAttributeNameAtIndex(string xml, int index, bool includeNamespace)
        {
            var name = GetAttributeNameAtIndex(xml, index);
            var qualifiedName = QualifiedName.FromString(name);
            if (!qualifiedName.IsEmpty && !qualifiedName.HasNamespace && includeNamespace)
            {
                var path = GetActiveElementStartPathAtIndex(xml, index);
                qualifiedName.Namespace = path.GetNamespaceForPrefix(path.Elements.GetLastPrefix());
            }
            return qualifiedName;
        }

        /// <summary>
        /// Gets the name of the attribute and its prefix at the specified index. The index
        /// can be anywhere inside the attribute name or in the attribute value.
        /// </summary>
        public static QualifiedName GetQualifiedAttributeNameAtIndex(string xml, int index)
        {
            return GetQualifiedAttributeNameAtIndex(xml, index, false);
        }

        /// <summary>
        /// Gets the name of the attribute at the specified index. The index
        /// can be anywhere inside the attribute name or in the attribute value.
        /// </summary>
        public static string GetAttributeNameAtIndex(string xml, int index)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return String.Empty;
            }

            index = GetCorrectedIndex(xml.Length, index);

            var ignoreWhitespace = true;
            var ignoreEqualsSign = false;
            var ignoreQuote = false;

            if (IsInsideAttributeValue(xml, index))
            {
                // Find attribute name start.
                var elementStartIndex = GetActiveElementStartIndex(xml, index);
                if (elementStartIndex == -1)
                {
                    return String.Empty;
                }

                // Find equals sign.
                var foundQuoteChar = false;
                for (var i = index; i > elementStartIndex; --i)
                {
                    var ch = xml[i];
                    if (ch == '=' && foundQuoteChar)
                    {
                        index = i;
                        ignoreEqualsSign = true;
                        break;
                    }
                    else if (IsQuoteChar(ch))
                    {
                        foundQuoteChar = true;
                    }
                }
            }
            else
            {
                // Find end of attribute name.
                for (; index < xml.Length; ++index)
                {
                    var ch = xml[index];
                    if (!IsXmlNameChar(ch))
                    {
                        if (ch == '\'' || ch == '\"')
                        {
                            ignoreQuote = true;
                            ignoreEqualsSign = true;
                        }
                        else if (ch == '=')
                        {
                            // Do nothing.
                        }
                        else if (char.IsWhiteSpace(ch))
                        {
                            // fix if index is after an equals sign
                            var oldIndex = index;
                            // move back to first non-whitespace
                            while (index > -1 && char.IsWhiteSpace(xml[index]))
                                index--;
                            // if no equals sign is found reset index
                            if (index > -1 && xml[index] != '=')
                                index = oldIndex;
                        }
                        else
                        {
                            return String.Empty;
                        }
                        break;
                    }
                }
                --index;
            }

            return GetAttributeName(xml, index, ignoreWhitespace, ignoreQuote, ignoreEqualsSign);
        }

        /// <summary>
        /// Checks for valid xml attribute value character
        /// </summary>
        public static bool IsAttributeValueChar(char ch)
        {
            if ((ch == '<') ||
                (ch == '>'))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks for valid xml element or attribute name character.
        /// </summary>
        public static bool IsXmlNameChar(char ch)
        {
            if (Char.IsLetterOrDigit(ch) ||
                (ch == ':') ||
                (ch == '/') ||
                (ch == '_') ||
                (ch == '.') ||
                (ch == '-'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified index is inside an attribute value.
        /// </summary>
        public static bool IsInsideAttributeValue(string xml, int index)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return false;
            }

            if (index > xml.Length)
            {
                index = xml.Length;
            }

            var elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex == -1)
            {
                return false;
            }

            // Count the number of double quotes and single quotes that exist
            // before the first equals sign encountered going backwards to
            // the start of the active element.
            var doubleQuotesCount = 0;
            var singleQuotesCount = 0;
            var lastQuoteChar = ' ';
            for (var i = index - 1; i > elementStartIndex; --i)
            {
                var ch = xml[i];
                if (ch == '\"')
                {
                    lastQuoteChar = ch;
                    ++doubleQuotesCount;
                }
                else if (ch == '\'')
                {
                    lastQuoteChar = ch;
                    ++singleQuotesCount;
                }
            }

            // Odd number of quotes?
            if ((lastQuoteChar == '\"') && ((doubleQuotesCount % 2) > 0))
            {
                return true;
            }
            else if ((lastQuoteChar == '\'') && ((singleQuotesCount % 2) > 0))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the attribute value at the specified index.
        /// </summary>
        /// <returns>An empty string if no attribute value can be found.</returns>
        public static string GetAttributeValueAtIndex(string xml, int index)
        {
            if (!IsInsideAttributeValue(xml, index))
            {
                return String.Empty;
            }

            index = GetCorrectedIndex(xml.Length, index);

            var elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex == -1)
            {
                return String.Empty;
            }

            // Find equals sign.
            var equalsSignIndex = -1;
            var foundQuoteChar = false;
            for (var i = index; i > elementStartIndex; --i)
            {
                var ch = xml[i];
                if (ch == '=' && foundQuoteChar)
                {
                    equalsSignIndex = i;
                    break;
                }
                else if (IsQuoteChar(ch))
                {
                    foundQuoteChar = true;
                }
            }

            if (equalsSignIndex == -1)
            {
                return String.Empty;
            }

            // Find attribute value.
            var quoteChar = ' ';
            foundQuoteChar = false;
            var attributeValue = new StringBuilder();
            for (var i = equalsSignIndex; i < xml.Length; ++i)
            {
                var ch = xml[i];
                if (!foundQuoteChar)
                {
                    if (IsQuoteChar(ch))
                    {
                        quoteChar = ch;
                        foundQuoteChar = true;
                    }
                }
                else
                {
                    if (ch == quoteChar)
                    {
                        // End of attribute value.
                        return attributeValue.ToString();
                    }
                    else if (IsAttributeValueChar(ch) || IsQuoteChar(ch))
                    {
                        attributeValue.Append(ch);
                    }
                    else
                    {
                        // Invalid character found.
                        return String.Empty;
                    }
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the text of the xml element start tag that the index is
        /// currently inside.
        /// </summary>
        /// <returns>
        /// Returns the text up to and including the start tag &lt; character.
        /// </returns>
        static string GetActiveElementStartText(string xml, int index)
        {
            var elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex >= 0)
            {
                if (elementStartIndex < index)
                {
                    var elementEndIndex = GetActiveElementEndIndex(xml, index);
                    if (elementEndIndex >= index)
                    {
                        return xml.Substring(elementStartIndex, elementEndIndex - elementStartIndex);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Locates the index of the start tag &lt; character.
        /// </summary>
        /// <returns>
        /// Returns the index of the start tag character; otherwise
        /// -1 if no start tag character is found or a end tag
        /// &gt; character is found first.
        /// </returns>
        public static int GetActiveElementStartIndex(string xml, int index)
        {
            var elementStartIndex = -1;
            var currentIndex = index - 1;

            while (currentIndex > -1)
            {
                var currentChar = xml[currentIndex];
                if (currentChar == '<')
                {
                    elementStartIndex = currentIndex;
                    break;
                }
                else if (currentChar == '>')
                {
                    break;
                }

                --currentIndex;
            }

            return elementStartIndex;
        }

        /// <summary>
        /// Locates the index of the end tag character.
        /// </summary>
        /// <returns>
        /// Returns the index of the end tag character; otherwise
        /// -1 if no end tag character is found or a start tag
        /// character is found first.
        /// </returns>
        static int GetActiveElementEndIndex(string xml, int index)
        {
            var elementEndIndex = index;

            for (var i = index; i < xml.Length; ++i)
            {

                var currentChar = xml[i];
                if (currentChar == '>')
                {
                    elementEndIndex = i;
                    break;
                }
                else if (currentChar == '<')
                {
                    elementEndIndex = -1;
                    break;
                }
            }

            return elementEndIndex;
        }

        /// <summary>
        /// Gets the element name from the element start tag string.
        /// </summary>
        /// <param name="xml">This string must start at the
        /// element we are interested in.</param>
        static QualifiedName GetElementName(string xml)
        {
            var name = String.Empty;

            // Find the end of the element name.
            xml = xml.Replace("\r\n", " ");
            var index = xml.IndexOf(' ');
            name = index > 0 ? xml.Substring(1, index - 1) : xml.Substring(1);

            return QualifiedName.FromString(name);
        }

        /// <summary>
        /// Gets the element namespace from the element start tag
        /// string.
        /// </summary>
        /// <param name="xml">This string must start at the
        /// element we are interested in.</param>
        static XmlNamespace GetElementNamespace(string xml)
        {
            var namespaceUri = new XmlNamespace();

            var match = Regex.Match(xml, ".*?(xmlns\\s*?|xmlns:.*?)=\\s*?['\\\"](.*?)['\\\"]");
            if (match.Success)
            {
                namespaceUri.Name = match.Groups[2].Value;

                var xmlns = match.Groups[1].Value.Trim();
                var prefixIndex = xmlns.IndexOf(':');
                if (prefixIndex > 0)
                {
                    namespaceUri.Prefix = xmlns.Substring(prefixIndex + 1);
                }
            }

            return namespaceUri;
        }

        static string ReverseString(string text)
        {
            var reversedString = new StringBuilder(text);

            var index = text.Length;
            foreach (var ch in text)
            {
                --index;
                reversedString[index] = ch;
            }

            return reversedString.ToString();
        }

        /// <summary>
        /// Ensures that the index is on the last character if it is
        /// too large.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="index">The current index.</param>
        /// <returns>The index unchanged if the index is smaller than the
        /// length of the string; otherwise it returns length - 1.</returns>
        static int GetCorrectedIndex(int length, int index)
        {
            if (index >= length)
            {
                index = length - 1;
            }
            return index;
        }

        /// <summary>
        /// Gets the active element path given the element text.
        /// </summary>
        static XmlElementPath GetActiveElementStartPath(string xml, int index, string elementText)
        {
            var elementName = GetElementName(elementText);
            if (elementName.IsEmpty)
            {
                return new XmlElementPath();
            }

            var elementNamespace = GetElementNamespace(elementText);

            var path = GetFullParentElementPath(xml.Substring(0, index));

            // Try to get a namespace for the active element's prefix.
            if (elementName.HasPrefix && !elementNamespace.HasName)
            {
                elementName.Namespace = path.GetNamespaceForPrefix(elementName.Prefix);
                elementNamespace.Name = elementName.Namespace;
                elementNamespace.Prefix = elementName.Prefix;
            }

            if (!elementNamespace.HasName)
            {
                if (path.Elements.Count > 0)
                {
                    var parentName = path.Elements[path.Elements.Count - 1];
                    elementNamespace.Name = parentName.Namespace;
                    elementNamespace.Prefix = parentName.Prefix;
                }
            }
            path.AddElement(new QualifiedName(elementName.Name, elementNamespace));
            return path;
        }

        static string GetAttributeName(string xml, int index, bool ignoreWhitespace, bool ignoreQuote, bool ignoreEqualsSign)
        {
            var name = String.Empty;

            // From the end of the string work backwards until we have
            // picked out the attribute name.
            var reversedAttributeName = new StringBuilder();

            var currentIndex = index;
            var invalidString = true;

            for (var i = 0; i <= index; ++i)
            {

                var currentChar = xml[currentIndex];

                if (IsXmlNameChar(currentChar))
                {
                    if (!ignoreEqualsSign)
                    {
                        ignoreWhitespace = false;
                        reversedAttributeName.Append(currentChar);
                    }
                }
                else if (Char.IsWhiteSpace(currentChar))
                {
                    if (ignoreWhitespace == false)
                    {
                        // Reached the start of the attribute name.
                        invalidString = false;
                        break;
                    }
                }
                else if ((currentChar == '\'') || (currentChar == '\"'))
                {
                    if (ignoreQuote)
                    {
                        ignoreQuote = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (currentChar == '=')
                {
                    if (ignoreEqualsSign)
                    {
                        ignoreEqualsSign = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (IsAttributeValueChar(currentChar))
                {
                    if (!ignoreQuote)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                --currentIndex;
            }

            if (!invalidString)
            {
                name = ReverseString(reversedAttributeName.ToString());
            }

            return name;
        }

        /// <summary>
        /// Gets the element name at the specified index.
        /// </summary>
        static string GetElementNameAtIndex(string xml, int index)
        {
            var elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex >= 0 && elementStartIndex < index)
            {
                var elementEndIndex = GetActiveElementEndIndex(xml, index);
                if (elementEndIndex == -1)
                {
                    elementEndIndex = xml.IndexOfAny(WhitespaceCharacters, elementStartIndex);
                }
                if (elementEndIndex >= elementStartIndex)
                {
                    return xml.Substring(elementStartIndex, elementEndIndex - elementStartIndex);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the parent element path based on the index position. This
        /// method does not compact the path so it will include all elements
        /// including those in another namespace in the path.
        /// </summary>
        static XmlElementPath GetFullParentElementPath(string xml)
        {
            var path = new XmlElementPath();
            IDictionary<string, string> namespacesInScope = null;
            var reader = new StringReader(xml);
            using (var xmlReader = new XmlTextReader(reader))
            {
                try
                {
                    xmlReader.XmlResolver = null; // prevent XmlTextReader from loading external DTDs
                    while (xmlReader.Read())
                    {
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (!xmlReader.IsEmptyElement)
                                {
                                    var elementName = new QualifiedName(xmlReader.LocalName, xmlReader.NamespaceURI, xmlReader.Prefix);
                                    path.AddElement(elementName);
                                }
                                break;
                            case XmlNodeType.EndElement:
                                path.Elements.RemoveLast();
                                break;
                        }
                    }
                }
                catch (XmlException)
                {
                    namespacesInScope = xmlReader.GetNamespacesInScope(XmlNamespaceScope.All);
                }

            }

            // Add namespaces in scope for the last element read.
            if (namespacesInScope != null)
            {
                foreach (var ns in namespacesInScope)
                {
                    path.NamespacesInScope.Add(new XmlNamespace(ns.Key, ns.Value));
                }
            }

            return path;
        }

        public static bool IsQuoteChar(char ch)
        {
            return (ch == '\"') || (ch == '\'');
        }

        public static string GetXmlIdentifierBeforeIndex(TextDocument document, int index)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (index < 0 || index > document.TextLength)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be between 0 and " + document.TextLength);
            var i = index - 1;
            while (i >= 0 && IsXmlNameChar(document.GetCharAt(i)) && document.GetCharAt(i) != '/')
                i--;
            return document.GetText(i + 1, index - i - 1);
        }
    }
}
