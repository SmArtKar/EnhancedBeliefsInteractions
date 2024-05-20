using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public class PatchOperationAddOrReplace : PatchOperationAdd
    {
        public override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = value.node;
            bool result = false;

            XmlNode[] array = xml.SelectNodes(xpath).Cast<XmlNode>().ToArray();

            if (array.Any())
            {
                foreach (XmlNode xmlNode in array)
                {
                    result = true;
                    XmlNode parentNode = xmlNode.ParentNode;
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(childNode, deep: true), xmlNode);
                    }
                    parentNode.RemoveChild(xmlNode);
                }

                return result;
            }
            xpath = xpath.Substring(0, xpath.LastIndexOf("/"));
            foreach (object item in xml.SelectNodes(xpath))
            {
                result = true;
                XmlNode xmlNode = item as XmlNode;
                if (order == Order.Append)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
                    }
                }
                else if (order == Order.Prepend)
                {
                    for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
                    {
                        xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true));
                    }
                }
            }

            return result;
        }
    }
}
