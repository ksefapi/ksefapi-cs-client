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
    /// Rodzaj transportu (1 - Transport morski, 2 - Transport kolejowy, 3 - Transport drogowy, 4 - Transport lotniczy, 5 - Przesyłka pocztowa, 7 - Stałe instalacje przesyłowe, 8 - Żegluga śródlądowa)
    /// </summary>
    /// <value>Rodzaj transportu (1 - Transport morski, 2 - Transport kolejowy, 3 - Transport drogowy, 4 - Transport lotniczy, 5 - Przesyłka pocztowa, 7 - Stałe instalacje przesyłowe, 8 - Żegluga śródlądowa)</value>
    public enum TRodzajTransportu
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
        /// Enum NUMBER_7 for value: 7
        /// </summary>
        NUMBER_7 = 7,

        /// <summary>
        /// Enum NUMBER_8 for value: 8
        /// </summary>
        NUMBER_8 = 8
    }

}
