/**
 * Copyright 2025-2026 NETCAT (www.netcat.pl)
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
 * @copyright 2025-2026 NETCAT (www.netcat.pl)
 * @license http://www.apache.org/licenses/LICENSE-2.0
 */

using System.Collections.Generic;

namespace KsefApi.Client
{
    /// <summary>
    /// Error codes
    /// </summary>
    public class ClientError
    {
        public const int CLI_INPUT         = 5001;
        public const int CLI_CONNECT       = 5002;
        public const int CLI_AUTH          = 5003;
        public const int CLI_RESPONSE      = 5004;
        public const int CLI_EXCEPTION     = 5005;
        public const int CLI_SEND          = 5006;
        public const int CLI_PKEY_ALG      = 5007;
        public const int CLI_PKEY_FORMAT   = 5008;
        public const int CLI_RSA_ENCRYPT   = 5009;
        public const int CLI_AES_ENCRYPT   = 5010;
        public const int CLI_AES_DECRYPT   = 5011;
        public const int CLI_JSON          = 5012;

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
            { CLI_AES_DECRYPT,   "Nie udało się odszyfrować danych kluczem symetrycznym" },
            { CLI_JSON,          "Nie udała się konwersja JSON na obiekt modelu lub odwrotna" }
        };

        /// <summary>
        /// Get error message
        /// </summary>
        /// <param name="code">error code</param>
        /// <returns>error message</returns>
        public static string Message(int code)
        {
            if (code < CLI_INPUT || code > CLI_JSON)
            {
                return null;
            }

            return Codes[code];
        }
	}
}
