/**
 * Copyright 2025 NETCAT (www.netcat.pl)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * @author NETCAT <firma@netcat.pl>
 * @copyright 2025 NETCAT (www.netcat.pl)
 * @license http://www.apache.org/licenses/LICENSE-2.0
 */

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KsefApi.Client
{
    /// <summary>
    /// Error codes
    /// </summary>
    public class ClientError
    {
        public const int CLI_INPUT         = 301;
        public const int CLI_CONNECT       = 302;
        public const int CLI_AUTH          = 303;
        public const int CLI_RESPONSE      = 304;
        public const int CLI_EXCEPTION     = 305;
        public const int CLI_SEND          = 306;
        public const int CLI_PKEY_ALG      = 307;
        public const int CLI_PKEY_FORMAT   = 308;
        public const int CLI_RSA_ENCRYPT   = 309;
        public const int CLI_AES_ENCRYPT   = 310;
        public const int CLI_AES_DECRYPT   = 311;

        private static readonly Dictionary<int, string> Codes = new Dictionary<int, string> {
            { CLI_INPUT,         "Nieprawidłowy parametr wejściowy funkcji" },
            { CLI_CONNECT,       "Nie udało się nawiązać połączenia z serwisem KSEF API" },
            { CLI_AUTH,          "Niepoprawne dane do autoryzacji użytkownika" },
            { CLI_RESPONSE,      "Odpowiedź serwisu KSEF API ma nieprawidłowy format" },
            { CLI_EXCEPTION,     "Funkcja wygenerowała wyjątek" },
            { CLI_SEND,          "Nie udało się wysłać zapytania do serwisu KSEF API" },
            { CLI_PKEY_ALG,      "Nieprawidłowy typ algorytmu klucza publicznego KSeF" },
            { CLI_PKEY_FORMAT,   "Nieprawidłowy format klucza publicznego KSeF" },
            { CLI_RSA_ENCRYPT,   "Nie udało się zaszyfrować klucza symetrycznego kluczem publicznym KSeF" },
            { CLI_AES_ENCRYPT,   "Nie udało się zaszyfrować danych kluczem symetrycznym" },
            { CLI_AES_DECRYPT,   "Nie udało się odszyfrować danych kluczem symetrycznym" }
        };

        /// <summary>
        /// Get error message
        /// </summary>
        /// <param name="code">error code</param>
        /// <returns>error message</returns>
        public static string Message(int code)
        {
            if (code < CLI_INPUT || code > CLI_AES_DECRYPT)
            {
                return null;
            }

            return Codes[code];
        }
	}
}
