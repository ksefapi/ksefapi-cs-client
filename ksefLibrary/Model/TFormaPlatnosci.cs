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
    /// Typy form płatności (1 - Gotówka, 2 - Karta, 3 - Bon, 4 - Czek, 5 - Kredyt, 6 - Przelew, 7 - Mobilna)
    /// </summary>
    /// <value>Typy form płatności (1 - Gotówka, 2 - Karta, 3 - Bon, 4 - Czek, 5 - Kredyt, 6 - Przelew, 7 - Mobilna)</value>
    public enum TFormaPlatnosci
    {
        /// <summary>
        /// Enum NUMBER_1 for value: 1
        /// </summary>
        NUMBER_1 = 1,

        /// <summary>
        /// Enum NUMBER_2 for value: 2
        /// </summary>
        NUMBER_2 = 2,

        /// <summary>
        /// Enum NUMBER_3 for value: 3
        /// </summary>
        NUMBER_3 = 3,

        /// <summary>
        /// Enum NUMBER_4 for value: 4
        /// </summary>
        NUMBER_4 = 4,

        /// <summary>
        /// Enum NUMBER_5 for value: 5
        /// </summary>
        NUMBER_5 = 5,

        /// <summary>
        /// Enum NUMBER_6 for value: 6
        /// </summary>
        NUMBER_6 = 6,

        /// <summary>
        /// Enum NUMBER_7 for value: 7
        /// </summary>
        NUMBER_7 = 7
    }

}
