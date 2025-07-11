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
    /// Dodatkowe rozliczenia na fakturze
    /// </summary>
    [DataContract(Name = "Rozliczenie")]
    public partial class Rozliczenie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rozliczenie" /> class.
        /// </summary>
        /// <param name="obciazenia">obciazenia.</param>
        /// <param name="sumaObciazen">Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku.</param>
        /// <param name="odliczenia">odliczenia.</param>
        /// <param name="sumaOdliczen">Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku.</param>
        /// <param name="doZaplaty">Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku.</param>
        /// <param name="doRozliczenia">Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku.</param>
        public Rozliczenie(List<Obciazenia> obciazenia = default, double sumaObciazen = default, List<Odliczenia> odliczenia = default, double sumaOdliczen = default, double doZaplaty = default, double doRozliczenia = default)
        {
            this.Obciazenia = obciazenia;
            this.SumaObciazen = sumaObciazen;
            this.Odliczenia = odliczenia;
            this.SumaOdliczen = sumaOdliczen;
            this.DoZaplaty = doZaplaty;
            this.DoRozliczenia = doRozliczenia;
        }

        /// <summary>
        /// Gets or Sets Obciazenia
        /// </summary>
        [DataMember(Name = "Obciazenia", EmitDefaultValue = false)]
        public List<Obciazenia> Obciazenia { get; set; }

        /// <summary>
        /// Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku
        /// </summary>
        /// <value>Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku</value>
        [DataMember(Name = "SumaObciazen", EmitDefaultValue = false)]
        public double SumaObciazen { get; set; }

        /// <summary>
        /// Gets or Sets Odliczenia
        /// </summary>
        [DataMember(Name = "Odliczenia", EmitDefaultValue = false)]
        public List<Odliczenia> Odliczenia { get; set; }

        /// <summary>
        /// Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku
        /// </summary>
        /// <value>Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku</value>
        [DataMember(Name = "SumaOdliczen", EmitDefaultValue = false)]
        public double SumaOdliczen { get; set; }

        /// <summary>
        /// Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku
        /// </summary>
        /// <value>Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku</value>
        [DataMember(Name = "DoZaplaty", EmitDefaultValue = false)]
        public double DoZaplaty { get; set; }

        /// <summary>
        /// Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku
        /// </summary>
        /// <value>Wartość numeryczna 18 znaków max, w tym 2 znaki po przecinku</value>
        [DataMember(Name = "DoRozliczenia", EmitDefaultValue = false)]
        public double DoRozliczenia { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Rozliczenie {\n");
            sb.Append("  Obciazenia: ").Append(Obciazenia).Append("\n");
            sb.Append("  SumaObciazen: ").Append(SumaObciazen).Append("\n");
            sb.Append("  Odliczenia: ").Append(Odliczenia).Append("\n");
            sb.Append("  SumaOdliczen: ").Append(SumaOdliczen).Append("\n");
            sb.Append("  DoZaplaty: ").Append(DoZaplaty).Append("\n");
            sb.Append("  DoRozliczenia: ").Append(DoRozliczenia).Append("\n");
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
