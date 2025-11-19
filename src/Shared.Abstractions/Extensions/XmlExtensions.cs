using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace Shared.Abstractions.Extensions;

/// <summary>
/// Extension methods for XML serialization and deserialization operations.
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// Serializes an object to XML string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="encoding">The encoding to use. Default is UTF-8.</param>
    /// <param name="omitXmlDeclaration">Whether to omit XML declaration. Default is false.</param>
    /// <returns>XML string representation of the object.</returns>
    public static string ToXml<T>(this T obj, Encoding? encoding = null, bool omitXmlDeclaration = false)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        encoding ??= Encoding.UTF8;
        
        var serializer = new XmlSerializer(typeof(T));
        
        var settings = new XmlWriterSettings
        {
            Encoding = encoding,
            OmitXmlDeclaration = omitXmlDeclaration,
            Indent = true
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        
        serializer.Serialize(xmlWriter, obj);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Deserializes XML string to an object of specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static T FromXml<T>(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML string cannot be null or empty.", nameof(xml));

        var serializer = new XmlSerializer(typeof(T));
        
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);
        
        var result = serializer.Deserialize(xmlReader);
        return (T)result!;
    }

    /// <summary>
    /// Tries to deserialize XML string to an object of specified type.
    /// Returns true if successful, false otherwise.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <param name="result">The deserialized object if successful, default(T) otherwise.</param>
    /// <returns>True if deserialization was successful, false otherwise.</returns>
    public static bool TryFromXml<T>(this string xml, out T? result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            result = xml.FromXml<T>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is valid XML.
    /// </summary>
    /// <param name="xml">The XML string to validate.</param>
    /// <returns>True if the string is valid XML, false otherwise.</returns>
    public static bool IsValidXml(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Formats XML string with proper indentation.
    /// </summary>
    /// <param name="xml">The XML string to format.</param>
    /// <returns>Formatted XML string with proper indentation.</returns>
    public static string FormatXml(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return xml;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            
            doc.Save(xmlWriter);
            return stringWriter.ToString();
        }
        catch
        {
            return xml; // Return original if formatting fails
        }
    }

    /// <summary>
    /// Removes XML namespaces from the XML string.
    /// </summary>
    /// <param name="xml">The XML string to process.</param>
    /// <returns>XML string without namespaces.</returns>
    public static string RemoveNamespaces(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return xml;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Remove namespace declarations
            var elementsToUpdate = doc.SelectNodes("//*")?.Cast<XmlElement>().ToList();
            if (elementsToUpdate != null)
            {
                foreach (var element in elementsToUpdate)
                {
                    element.RemoveAllAttributes();
                    if (element.NamespaceURI != string.Empty)
                    {
                        var newElement = doc.CreateElement(element.LocalName);
                        newElement.InnerXml = element.InnerXml;
                        element.ParentNode?.ReplaceChild(newElement, element);
                    }
                }
            }

            return doc.OuterXml;
        }
        catch
        {
            return xml; // Return original if processing fails
        }
    }
}