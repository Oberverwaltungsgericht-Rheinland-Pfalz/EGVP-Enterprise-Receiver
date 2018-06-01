using System;
using System.IO;
using System.Xml;

namespace OvgRlp.EgvpEpFetcher.Services
{
  public abstract class ConfigurationServiceBase
  {
    internal readonly XmlDocument xmlDocument;

    protected virtual string XPATH_Common
    {
      get { throw new NotImplementedException("Please Override Getter for 'XPATH_Common' in inherited class '" + this.GetType().Name + "'!"); }
    }

    internal ConfigurationServiceBase(XmlDocument xmlDocument)
    {
      this.xmlDocument = xmlDocument;
    }

    public string GetCommonValue(string propertyName)
    {
      var commonNode = xmlDocument.SelectSingleNode(this.XPATH_Common);
      if (commonNode == null)
        throw new Exception("Common settings not found.");

      var propertyNode = commonNode.SelectSingleNode(propertyName);
      if (propertyNode == null)
        throw new Exception("Property '" + propertyName + "' not found.");

      return propertyNode.InnerText;
    }

    public XmlNode GetNodeByAttribute(string XPATH_Base,
                                    string compareAttributeName,
                                    string compareAttributeValue,
                                    StringComparison cmpPar = StringComparison.CurrentCulture)
    {
      XmlNode rval = null;

      XmlNodeList nodes = xmlDocument.SelectNodes(XPATH_Base);
      foreach (XmlNode node in nodes)
      {
        try
        {
          string cmpValue = node.Attributes[compareAttributeName].Value;
          if (!string.IsNullOrEmpty(cmpValue) && compareAttributeValue.Equals(cmpValue, cmpPar))
            rval = node;
        }
        catch { /* ohne Fehlerbehandlung */ }
      }

      return rval;
    }

    #region static methods

    public static T Load<T>(string filename)
    {
      if (!File.Exists(filename))
        throw new FileNotFoundException(filename);

      var xmlDocument = new XmlDocument();
      xmlDocument.Load(filename);

      return ConfigurationServiceBase.Load<T>(xmlDocument);
    }

    public static T Load<T>(XmlDocument xmlDocument)
    {
      //return new ConfigurationServiceBase(xmlDocument);
      var obj = Activator.CreateInstance(typeof(T), xmlDocument);
      return (T)obj;
    }

    #endregion static methods
  }
}