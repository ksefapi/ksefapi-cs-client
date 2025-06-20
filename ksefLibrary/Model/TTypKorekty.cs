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
    /// Typ skutku korekty w ewidencji dla podatku od towarów i usług (1 - Korekta skutkująca w dacie ujęcia faktury pierwotnej, 2 - Korekta skutkująca w dacie wystawienia faktury korygującej, 3 - Korekta skutkująca w dacie innej, w tym gdy dla różnych pozycji faktury korygującej daty te są różne)
    /// </summary>
    /// <value>Typ skutku korekty w ewidencji dla podatku od towarów i usług (1 - Korekta skutkująca w dacie ujęcia faktury pierwotnej, 2 - Korekta skutkująca w dacie wystawienia faktury korygującej, 3 - Korekta skutkująca w dacie innej, w tym gdy dla różnych pozycji faktury korygującej daty te są różne)</value>
    public enum TTypKorekty
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
        NUMBER_3 = 3
    }

}
