/*
 * KSeF API
 *
 * API do systemu KSeF
 *
 * The version of the OpenAPI document: 1.2.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = KsefApi.Client.FileParameter;
using OpenAPIDateConverter = KsefApi.Client.OpenAPIDateConverter;

namespace KsefApi.Model
{
    /// <summary>
    /// KsefInvoiceValidateResponse
    /// </summary>
    [DataContract(Name = "KsefInvoiceValidateResponse")]
    public partial class KsefInvoiceValidateResponse
    {

        /// <summary>
        /// Gets or Sets InvoiceVersion
        /// </summary>
        [DataMember(Name = "invoiceVersion", IsRequired = true, EmitDefaultValue = true)]
        public KsefInvoiceVersion InvoiceVersion { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="KsefInvoiceValidateResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected KsefInvoiceValidateResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="KsefInvoiceValidateResponse" /> class.
        /// </summary>
        /// <param name="valid">valid (required).</param>
        /// <param name="invoiceVersion">invoiceVersion (required).</param>
        public KsefInvoiceValidateResponse(bool valid = default, KsefInvoiceVersion invoiceVersion = default)
        {
            this.Valid = valid;
            this.InvoiceVersion = invoiceVersion;
        }

        /// <summary>
        /// Gets or Sets Valid
        /// </summary>
        [DataMember(Name = "valid", IsRequired = true, EmitDefaultValue = true)]
        public bool Valid { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class KsefInvoiceValidateResponse {\n");
            sb.Append("  Valid: ").Append(Valid).Append("\n");
            sb.Append("  InvoiceVersion: ").Append(InvoiceVersion).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

    }

}
