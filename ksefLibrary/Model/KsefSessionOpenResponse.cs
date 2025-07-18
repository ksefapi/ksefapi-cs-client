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
    /// KsefSessionOpenResponse
    /// </summary>
    [DataContract(Name = "KsefSessionOpenResponse")]
    public partial class KsefSessionOpenResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KsefSessionOpenResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected KsefSessionOpenResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="KsefSessionOpenResponse" /> class.
        /// </summary>
        /// <param name="id">id (required).</param>
        /// <param name="created">created (required).</param>
        public KsefSessionOpenResponse(string id = default, DateTime created = default)
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new ArgumentNullException("id is a required property for KsefSessionOpenResponse and cannot be null");
            }
            this.Id = id;
            this.Created = created;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Created
        /// </summary>
        [DataMember(Name = "created", IsRequired = true, EmitDefaultValue = true)]
        public DateTime Created { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class KsefSessionOpenResponse {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Created: ").Append(Created).Append("\n");
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
