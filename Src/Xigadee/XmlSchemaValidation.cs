﻿#region Copyright
// Copyright Hitachi Consulting
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xigadee
{
    public static class XmlSchemaValidation
    {
        public static bool ValidateXmlToSchema(string schemaUri, string xml)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("", schemaUri);

            XDocument xmlToVal = XDocument.Parse(xml);
            bool errors = false;

            xmlToVal.Validate(schemas, (o, e) =>
            {
                //Console.WriteLine("{0}", e.Message);
                errors = true;
            });

            return errors;
        }

        public static bool ValidateXmlToSchema(XmlSchema xmlSchema, XElement xml, out string errors)
        {
            try
            {
                errors = string.Empty;
                if (xmlSchema == null)
                    return false;

                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(xmlSchema);
                var xmlToVal = new XDocument(xml);

                bool schemaCheckError = false;
                var sb = new StringBuilder("Schema Validation Error : ");
                xmlToVal.Validate(schemaSet, (o, e) =>
                {
                    if (schemaCheckError)
                        sb.Append(", ");

                    sb.Append(e.Message);
                    schemaCheckError = true;
                });

                if (schemaCheckError)
                    errors = $"{xml} errored with {sb}";

                return (!schemaCheckError);
            }
            catch (Exception ex)
            {
                errors = "Schema failure";
#if DEBUG
                errors += (" " + ex.Message);
#endif
                return false;
            }
        }
    }
}
