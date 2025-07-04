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
    /// Nagłówek
    /// </summary>
    [DataContract(Name = "TNaglowek")]
    public partial class TNaglowek
    {

        /// <summary>
        /// Gets or Sets WariantFormularza
        /// </summary>
        [DataMember(Name = "WariantFormularza", EmitDefaultValue = false)]
        public WariantFormularza? WariantFormularza { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TNaglowek" /> class.
        /// </summary>
        /// <param name="kodFormularza">kodFormularza.</param>
        /// <param name="wariantFormularza">wariantFormularza.</param>
        /// <param name="dataWytworzeniaFa">Data i czas wytworzenia faktury.</param>
        /// <param name="systemInfo">Typ znakowy ograniczony do 256 znaków.</param>
        public TNaglowek(TKodFormularza kodFormularza = default, WariantFormularza? wariantFormularza = default, DateTime dataWytworzeniaFa = default, string systemInfo = default)
        {
            this.KodFormularza = kodFormularza;
            this.WariantFormularza = wariantFormularza;
            this.DataWytworzeniaFa = dataWytworzeniaFa;
            this.SystemInfo = systemInfo;
        }

        /// <summary>
        /// Gets or Sets KodFormularza
        /// </summary>
        [DataMember(Name = "KodFormularza", EmitDefaultValue = false)]
        public TKodFormularza KodFormularza { get; set; }

        /// <summary>
        /// Data i czas wytworzenia faktury
        /// </summary>
        /// <value>Data i czas wytworzenia faktury</value>
        [DataMember(Name = "DataWytworzeniaFa", EmitDefaultValue = false)]
        public DateTime DataWytworzeniaFa { get; set; }

        /// <summary>
        /// Typ znakowy ograniczony do 256 znaków
        /// </summary>
        /// <value>Typ znakowy ograniczony do 256 znaków</value>
        [DataMember(Name = "SystemInfo", EmitDefaultValue = false)]
        public string SystemInfo { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TNaglowek {\n");
            sb.Append("  KodFormularza: ").Append(KodFormularza).Append("\n");
            sb.Append("  WariantFormularza: ").Append(WariantFormularza).Append("\n");
            sb.Append("  DataWytworzeniaFa: ").Append(DataWytworzeniaFa).Append("\n");
            sb.Append("  SystemInfo: ").Append(SystemInfo).Append("\n");
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
