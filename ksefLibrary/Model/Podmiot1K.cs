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
    /// W przypadku korekty danych sprzedawcy należy podać pełne dane sprzedawcy występujące na fakturze korygowanej. Pole nie dotyczy przypadku korekty błędnego NIP występującego na fakturze pierwotnej - wówczas wymagana jest korekta faktury do wartości zerowych
    /// </summary>
    [DataContract(Name = "Podmiot1K")]
    public partial class Podmiot1K
    {

        /// <summary>
        /// Gets or Sets PrefiksPodatnika
        /// </summary>
        [DataMember(Name = "PrefiksPodatnika", EmitDefaultValue = false)]
        public TKodyKrajowUE? PrefiksPodatnika { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Podmiot1K" /> class.
        /// </summary>
        /// <param name="prefiksPodatnika">prefiksPodatnika.</param>
        /// <param name="daneIdentyfikacyjne">daneIdentyfikacyjne.</param>
        /// <param name="adres">adres.</param>
        public Podmiot1K(TKodyKrajowUE? prefiksPodatnika = default, TPodmiot1 daneIdentyfikacyjne = default, TAdres adres = default)
        {
            this.PrefiksPodatnika = prefiksPodatnika;
            this.DaneIdentyfikacyjne = daneIdentyfikacyjne;
            this.Adres = adres;
        }

        /// <summary>
        /// Gets or Sets DaneIdentyfikacyjne
        /// </summary>
        [DataMember(Name = "DaneIdentyfikacyjne", EmitDefaultValue = false)]
        public TPodmiot1 DaneIdentyfikacyjne { get; set; }

        /// <summary>
        /// Gets or Sets Adres
        /// </summary>
        [DataMember(Name = "Adres", EmitDefaultValue = false)]
        public TAdres Adres { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Podmiot1K {\n");
            sb.Append("  PrefiksPodatnika: ").Append(PrefiksPodatnika).Append("\n");
            sb.Append("  DaneIdentyfikacyjne: ").Append(DaneIdentyfikacyjne).Append("\n");
            sb.Append("  Adres: ").Append(Adres).Append("\n");
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
