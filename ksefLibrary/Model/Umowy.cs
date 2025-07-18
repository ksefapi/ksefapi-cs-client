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
    /// Umowy
    /// </summary>
    [DataContract(Name = "Umowy")]
    public partial class Umowy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Umowy" /> class.
        /// </summary>
        /// <param name="dataUmowy">Data zdarzenia w okresie od 2016-07-01 do 2050-01-01.</param>
        /// <param name="nrUmowy">Typ znakowy ograniczony do 256 znaków.</param>
        public Umowy(DateTime dataUmowy = default, string nrUmowy = default)
        {
            this.DataUmowy = dataUmowy;
            this.NrUmowy = nrUmowy;
        }

        /// <summary>
        /// Data zdarzenia w okresie od 2016-07-01 do 2050-01-01
        /// </summary>
        /// <value>Data zdarzenia w okresie od 2016-07-01 do 2050-01-01</value>
        [DataMember(Name = "DataUmowy", EmitDefaultValue = false)]
        [JsonConverter(typeof(OpenAPIDateConverter))]
        public DateTime DataUmowy { get; set; }

        /// <summary>
        /// Typ znakowy ograniczony do 256 znaków
        /// </summary>
        /// <value>Typ znakowy ograniczony do 256 znaków</value>
        [DataMember(Name = "NrUmowy", EmitDefaultValue = false)]
        public string NrUmowy { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Umowy {\n");
            sb.Append("  DataUmowy: ").Append(DataUmowy).Append("\n");
            sb.Append("  NrUmowy: ").Append(NrUmowy).Append("\n");
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
