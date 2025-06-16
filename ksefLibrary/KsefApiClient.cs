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


using KsefApi.Client;
using KsefApi.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace KsefApi
{
	/// <summary>
	/// KSEF API Service Client
	/// </summary>
	public class KsefApiClient
	{
		public const string VERSION = "1.2.3";

		public const string PRODUCTION_URL = "https://ksefapi.pl/api";
		public const string TEST_URL = "https://ksefapi.pl/api-test";
        public const string NIP24_URL = "https://www.nip24.pl/api/ksef";

        public const string ENC_ALG = "aes-256-cbc";
        public const int ENC_ALG_BLOCK_SIZE = 16;
        public const int ENC_ALG_KEY_SIZE = 32;

        /// <summary>
        /// Client's version
        /// </summary>
        public string Version
		{
			get { return VERSION; } 
		}
        
		/// <summary>
		/// Service URL address
		/// </summary>
		public string URL { get; set; }

		/// <summary>
		/// API key identifier
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// API key
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Name and version of the appplication using this client
		/// </summary>
		public string Application { get; set; }

		/// <summary>
		/// HTTPS Proxy
		/// </summary>
		public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Last error
        /// </summary>
        public Error LastError { get; set; }

        /// <summary>
        /// Flags which enables legacy SSL/TLS protocols. In case of connection problems, set this flag to true.
        /// </summary>
        public bool LegacyProtocolsEnabled { get; set; }

		private RandomNumberGenerator rng;

		/// <summary>
		/// Create a new client's object
		/// </summary>
		/// <param name="url">service URL address</param>
		/// <param name="id">API key identifier</param>
		/// <param name="key">API key</param>
		public KsefApiClient(string url, string id, string key)
		{
			URL = url;
			Id = id;
			Key = key;

			Proxy = WebRequest.GetSystemWebProxy();
			
			Clear();

#if KSEFAPI_COM
			LegacyProtocolsEnabled = true;
#else
			LegacyProtocolsEnabled = false;
#endif

			rng = new RNGCryptoServiceProvider();
		}

        /// <summary>
        /// Create a new client's object
        /// </summary>
        /// <param name="url">service URL address</param>
        /// <param name="id">API key identifier</param>
        /// <param name="key">API key</param>
        public KsefApiClient(Uri url, string id, string key) : this(url.ToString(), id, key)
		{
		}

        /// <summary>
        /// Generate new init vector for AES256
        /// </summary>
		/// <returns>init vector</returns>
        public byte[] GenerateInitVector()
		{
            try
            {
                Clear();

				byte[] b = new byte[ENC_ALG_BLOCK_SIZE];
                
				rng.GetBytes(b);

				return b;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Generate new AES256 key
        /// </summary>
        /// <returns>AES key</returns>
        public byte[] GenerateKey()
        {
            try
            {
                Clear();

                byte[] b = new byte[ENC_ALG_KEY_SIZE];

                rng.GetBytes(b);

                return b;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Encrypt AES256 key with KSeF public key
        /// </summary>
        /// <param name="publicKey">KSeF public key</param>
        /// <param name="key">AES256 key to encrypt</param>
        /// <returns>encrypted AES256 key</returns>
        public byte[] EncryptKey(KsefPublicKeyResponse publicKey, byte[] key)
		{
            try
            {
                Clear();

                if (publicKey == null || key == null || key.Length != ENC_ALG_KEY_SIZE)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
                byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
                byte[] seq = new byte[15];

                // load public key
                MemoryStream ms = new MemoryStream(publicKey.PublicKey);
                BinaryReader br = new BinaryReader(ms);
                ushort twobytes = 0;
                byte bt = 0;

                // data read as little endian order (actual data order for Sequence is 30 81)
                twobytes = br.ReadUInt16();

                if (twobytes == 0x8130)
                {
                    // advance 1 byte
                    br.ReadByte();
                }
                else if (twobytes == 0x8230) 
                {
                    // advance 2 bytes
                    br.ReadInt16();
                }
                else
                {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // read the Sequence OID
                seq = br.ReadBytes(SeqOID.Length);

                if (!CompareByteArrays(seq, SeqOID))
                {
                    // make sure Sequence for OID is correct
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // data read as little endian order (actual data order for Bit String is 03 81)
                twobytes = br.ReadUInt16();
                
                if (twobytes == 0x8103)
                {
                    // advance 1 byte
                    br.ReadByte();
                }
                else if (twobytes == 0x8203)
                {
                    // advance 2 bytes
                    br.ReadInt16();
                }
                else
                {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // expect null byte next
                bt = br.ReadByte();
                
                if (bt != 0x00) {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // data read as little endian order (actual data order for Sequence is 30 81)
                twobytes = br.ReadUInt16();

                if (twobytes == 0x8130)
                {
                    // advance 1 byte
                    br.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    // advance 2 bytes
                    br.ReadInt16();
                }
                else
                {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // data read as little endian order (actual data order for Integer is 02 81)
                twobytes = br.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102)
                {
                    // read next bytes which is bytes in modulus
                    lowbyte = br.ReadByte();
                }
                else if (twobytes == 0x8202)
                {
                    // advance 2 bytes
                    highbyte = br.ReadByte();
                    lowbyte = br.ReadByte();
                }
                else
                {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // reverse byte order since asn.1 key uses big endian order
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = br.ReadByte();
                br.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {
                    // if first byte (highest order) of modulus is zero, don't include it
                    // skip this null byte
                    br.ReadByte();

                    // reduce modulus buffer size by 1
                    modsize -= 1;
                }

                // read the modulus bytes
                byte[] modulus = br.ReadBytes(modsize);

                // expect an Integer for the exponent data
                if (br.ReadByte() != 0x02)
                {
                    Set(ClientError.CLI_PKEY_FORMAT);
                    return null;
                }

                // should only need one byte for actual exponent data (for all useful values)
                int expbytes = (int)br.ReadByte();
                byte[] exponent = br.ReadBytes(expbytes);

                // rsa encrypt
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters par = new RSAParameters();
                par.Modulus = modulus;
                par.Exponent = exponent;
                rsa.ImportParameters(par);

				return rsa.Encrypt(key, false);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Encrypt data with AES256 key
        /// </summary>
        /// <param name="iv">init vector</param>
        /// <param name="key">AES256 key</param>
        /// <param name="data">data to encrypt</param>
        /// <returns>encrypted data</returns>
        public byte[] EncryptData(byte[] iv, byte[] key, byte[] data)
		{
            try
            {
                Clear();

                if (iv == null || iv.Length != ENC_ALG_BLOCK_SIZE || key == null || key.Length != ENC_ALG_KEY_SIZE
                    || data == null || data.Length <= 0)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // aes encryption
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.BlockSize = ENC_ALG_BLOCK_SIZE * 8;
                    aes.KeySize = ENC_ALG_KEY_SIZE * 8;
                    aes.IV = iv;
                    aes.Key = key;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    byte[] enc;
                    
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                        }

                        enc = ms.ToArray();
                    }

                    return enc;
                }
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Decrypt data with AES256 key
        /// </summary>
        /// <param name="iv">init vector</param>
        /// <param name="key">AES256 key</param>
        /// <param name="encrypted">encrypted data</param>
        /// <returns>decrypted plain data</returns>
        public byte[] DecryptData(byte[] iv, byte[] key, byte[] encrypted)
		{
            try
            {
                Clear();

                if (iv == null || iv.Length != ENC_ALG_BLOCK_SIZE || key == null || key.Length != ENC_ALG_KEY_SIZE
                    || encrypted == null || encrypted.Length <= 0)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // aes encryption
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.BlockSize = ENC_ALG_BLOCK_SIZE * 8;
                    aes.KeySize = ENC_ALG_KEY_SIZE * 8;
                    aes.IV = iv;
                    aes.Key = key;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] dec;

                    using (MemoryStream ms = new MemoryStream(encrypted))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            dec = ReadAllBytes(cs);
                        }
                    }

                    return dec;
                }
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get SHA256 hash
        /// </summary>
        /// <param name="data">input data</param>
        /// <returns>output hash as raw binary</returns>
        public byte[] GetHash(byte[] data)
		{
            try
            {
                Clear();

				SHA256 sha = SHA256.Create();

                return sha.ComputeHash(data);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get KSeF public key
        /// </summary>
		/// <returns>KSeF public key for encryption or null in case of error</returns>
		public KsefPublicKeyResponse KsefPublicKey()
		{
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/public/key");

				return (KsefPublicKeyResponse)GetObject(url, null, typeof(KsefPublicKeyResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Open a new KSeF session
        /// </summary>
        /// <param name="invoiceVersion">requested invoice schema version</param>
        /// <param name="initVector">optional AES256 init vector</param>
        /// <param name="encryptedKey">optional encrypted AES256 key</param>
        /// <returns></returns>
        public KsefSessionOpenResponse KsefSessionOpen(KsefInvoiceVersion invoiceVersion, byte[] initVector = null, byte[] encryptedKey = null)
        {
            try
            {
                Clear();

                // req
                KsefSessionOpenRequest req = new KsefSessionOpenRequest();

                req.InvoiceVersion = invoiceVersion;
                req.InitVector = initVector;
                req.EncryptedKey = encryptedKey;

                string json = Serialize(req);

                if (json == null)
                {
                    return null;
                }

                // prepare url
                Uri url = new Uri(URL + "/invoice/session/open");

                return (KsefSessionOpenResponse)PostObject(url, "application/json", json, null, typeof(KsefSessionOpenResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get KSeF session status
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>session status or null</returns>
        public KsefSessionStatusResponse.StatusEnum? KsefSessionStatus(string sessionId)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/session/status/" + HttpUtility.UrlEncode(sessionId));

                KsefSessionStatusResponse res = (KsefSessionStatusResponse)GetObject(url, null, typeof(KsefSessionStatusResponse));

                if (res == null)
                {
                    return null;
                }

                return res.Status;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Close KSeF session
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>closing result</returns>
        public bool KsefSessionClose(string sessionId)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/session/close/" + HttpUtility.UrlEncode(sessionId));

                KsefSessionCloseResponse res = (KsefSessionCloseResponse)GetObject(url, null, typeof(KsefSessionCloseResponse));

                if (res == null)
                {
                    return false;
                }

                return res.Result;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return false;
        }

        /// <summary>
        /// Get UPO for specified session
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>XML with UPO or null</returns>
        public string KsefSessionUpo(string sessionId)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/session/upo/" + HttpUtility.UrlEncode(sessionId));

                byte[] res = GetBytes(url, "text/xml");

                if (res == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(res);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Generate an invoice XML
        /// </summary>
        /// <param name="fa">invoice object</param>
        /// <returns>invoice XML or null</returns>
        public string KsefInvoiceGenerate(Faktura invoice)
        {
            try
            {
                Clear();

                // req
                KsefInvoiceGenerateRequest req = new KsefInvoiceGenerateRequest(invoice);

                string json = Serialize(req);

                if (json == null)
                {
                    return null;
                }

                // prepare url
                Uri url = new Uri(URL + "/invoice/generate");

                byte[] res = PostBytes(url, "application/json", json, "text/xml");

                if (res == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(res);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Validate invoice XML against XSD schema
        /// </summary>
        /// <param name="invoiceXml">invoice XML</param>
        /// <returns>validation result or null</returns>
        public KsefInvoiceValidateResponse KsefInvoiceValidate(string invoiceXml)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/validate");

                return (KsefInvoiceValidateResponse)PostObject(url, "text/xml", invoiceXml, null, typeof(KsefInvoiceValidateResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }
            return null;
        }

        /// <summary>
        /// Send an invoice
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="size">plain invoice size in bytes (only for encrypted data)</param>
        /// <param name="hash">plain invoice SHA256 hash (only for encrypted data)</param>
        /// <param name="data">plain or encrypted invoice data</param>
        /// <returns>sending result with invoice id or null</returns>
        public KsefInvoiceSendResponse KsefInvoiceSend(string sessionId, int size, byte[] hash, byte[] data)
        {
            try
            {
                Clear();

                // req
                KsefInvoiceSendRequest req = new KsefInvoiceSendRequest(sessionId);

                if (size <= 0 && hash == null)
                {
                    KsefInvoicePlain plain = new KsefInvoicePlain(data);
                    req.Plain = plain;
                }
                else
                {
                    KsefInvoiceEncrypted enc = new KsefInvoiceEncrypted(size, hash, data);
                    req.Encrypted = enc;
                }

                string json = Serialize(req);

                if (json == null)
                {
                    return null;
                }

                // prepare url
                Uri url = new Uri(URL + "/invoice/send");

                return (KsefInvoiceSendResponse)PostObject(url, "application/json", json, null, typeof(KsefInvoiceSendResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get status of specified invoice
        /// </summary>
        /// <param name="invoiceId">invoice id</param>
        /// <returns>invoice status including KSeF ref number and acquisition timestamp or null</returns>
        public KsefInvoiceStatusResponse KsefInvoiceStatus(string invoiceId)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/status/" + HttpUtility.UrlEncode(invoiceId));

                return (KsefInvoiceStatusResponse)GetObject(url, null, typeof(KsefInvoiceStatusResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get an invoice
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="ksefRefNumber">invoice KSeF reference number</param>
        /// <returns>invoice XML as plain or encrypted data (depends on session type)</returns>
        public byte[] KsefInvoiceGet(string sessionId, string ksefRefNumber)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/get/" + HttpUtility.UrlEncode(sessionId) + "/" + HttpUtility.UrlEncode(ksefRefNumber));

                byte[] res = GetBytes(url, "application/octet-stream");

                if (res == null)
                {
                    return null;
                }

                return res;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Start new invoice query
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="subjectType">invoice subject type (subject1, subject2, subject3, subjectAuthorized)</param>
        /// <param name="from">begin of range</param>
        /// <param name="to">end of range</param>
        /// <returns>new query id or null</returns>
        public string KsefInvoiceQueryStart(string sessionId, KsefInvoiceQueryStartRequest.SubjectTypeEnum subjectType, DateTime from, DateTime to)
        {
            try
            {
                Clear();

                // req
                KsefInvoiceQueryStartRange range = new KsefInvoiceQueryStartRange();
                range.From = from;
                range.To = to;

                KsefInvoiceQueryStartRequest req = new KsefInvoiceQueryStartRequest(sessionId);
                req.SubjectType = subjectType;
                req.Range = range;

                string json = Serialize(req);

                if (json == null)
                {
                    return null;
                }

                // prepare url
                Uri url = new Uri(URL + "/invoice/query/start");

                KsefInvoiceQueryStartResponse res = (KsefInvoiceQueryStartResponse)PostObject(url, "application/json", json, null, typeof(KsefInvoiceQueryStartResponse));

                if (res == null)
                {
                    return null;
                }

                return res.QueryId;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get current status of query
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="queryId">query id</param>
        /// <returns>array of result parts numbers</returns>
        public string[] KsefInvoiceQueryStatus(string sessionId, string queryId)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/query/status/" + HttpUtility.UrlEncode(sessionId) + "/" + HttpUtility.UrlEncode(queryId));

                return (string[])GetObject(url, null, typeof(string[]));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get data for specified query part
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="queryId">query id</param>
        /// <param name="partNumber">query part number</param>
        /// <returns>plain or encrypted ZIP archive with invoices (depends on session type)</returns>
        public byte[] KsefInvoiceQueryResult(string sessionId, string queryId, string partNumber)
        {
            try
            {
                Clear();

                // prepare url
                Uri url = new Uri(URL + "/invoice/query/result/" + HttpUtility.UrlEncode(sessionId) + "/" + HttpUtility.UrlEncode(queryId)
                    + "/" + HttpUtility.UrlEncode(partNumber));

                return GetBytes(url, "application/octet-stream");
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Generate visualization of an invoice
        /// </summary>
        /// <param name="ksefRefNumber">KSeF reference number</param>
        /// <param name="invoice">invoice XML data</param>
        /// <param name="logo">include logo</param>
        /// <param name="qrcode">include qr-code</param>
        /// <param name="format">output format (html or pdf)</param>
        /// <param name="lang">output language (pl)</param>
        /// <returns>invoice visualization in requested format</returns>
        public byte[] KsefInvoiceVisualize(string ksefRefNumber, byte[] invoice, bool logo, bool qrcode,
            KsefInvoiceVisualizeRequest.OutputFormatEnum format, KsefInvoiceVisualizeRequest.OutputLanguageEnum lang)
        {
            try
            {
                Clear();

                // req
                KsefInvoiceVisualizeRequest req = new KsefInvoiceVisualizeRequest(logo, qrcode, format, lang, ksefRefNumber, invoice);

                string json = Serialize(req);

                if (json == null)
                {
                    return null;
                }

                // prepare url
                Uri url = new Uri(URL + "/invoice/visualize");

                return PostBytes(url, "application/json", json, "text/html, application/pdf, application/json");
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Clear last error
        /// </summary>
        private void Clear()
		{
			LastError = null;
		}

        /// <summary>
        /// Set last error
        /// </summary>
        /// <param name="code">error code</param>
        /// <param name="description">error description</param>
        /// <param name="details">error details</param>
        private void Set(int code, string description, string details)
        {
            LastError = new Error(code, description, details);
        }


        /// <summary>
        /// Set last error
        /// </summary>
        /// <param name="code">error code</param>
        /// <param name="details">error details</param>
        private void Set(int code, string details = null)
		{
            Set(code, ClientError.Message(code), details);
		}

        /// <summary>
        /// Set last error
        /// </summary>
        /// <param name="we">web exception object</param>
        private void Set(WebException we)
        {
            JObject json = Deserialize(ReadAllBytes(we.Response.GetResponseStream()));

            if (json != null && json.ContainsKey("error"))
            {
                Set((int)json["error"]["code"], (string)json["error"]["description"], (string)json["error"]["details"]);
            }
            else
            {
                Set(ClientError.CLI_EXCEPTION, we.Message);
            }
        }

        /// <summary>
		/// HTTP GET
        /// </summary>
        /// <param name="url">request URL</param>
		/// <param name="accept">accepted response mime type (application/xml or application/json)</param>
        /// <returns>response data or null</returns>
        private byte[] GetBytes(Uri url, string accept)
        {
            try
            {
                if (!LegacyProtocolsEnabled)
                {
                    // SecurityProtocolType:
                    // Tls		192
                    // Tls11	768
                    // Tls12	3072
                    // Tls13	12288
                    try
                    {
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | (SecurityProtocolType)12288;
                    }
                    catch (Exception)
                    {
                        // no tls13
                        try
                        {
                            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                        }
                        catch (Exception)
                        {
                            // no tls12
                            try
                            {
                                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768;
                            }
                            catch (Exception)
                            {
                                // no tls11
                            }
                        }
                    }
                }

                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = Proxy;

					wc.Headers.Set("Accept", accept);
                    wc.Headers.Set("Authorization", GetAuthHeader());
                    wc.Headers.Set("User-Agent", GetAgentHeader());

                    return wc.DownloadData(url);
                }
            }
            catch (WebException we)
            {
                Set(we);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        /// <param name="url">request URL</param>
		/// <param name="contentType">request content type</param>
		/// <param name="content">request content bytes</param>
		/// <param name="accept">accepted response mime type (application/xml or application/json)</param>
        /// <returns>response data or null</returns>
        private byte[] PostBytes(Uri url, string contentType, string content, string accept)
        {
            try
            {
                if (!LegacyProtocolsEnabled)
                {
                    // SecurityProtocolType:
                    // Tls		192
                    // Tls11	768
                    // Tls12	3072
                    // Tls13	12288
                    try
                    {
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | (SecurityProtocolType)12288;
                    }
                    catch (Exception e1)
                    {
                        // no tls13
                        try
                        {
                            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                        }
                        catch (Exception e2)
                        {
                            // no tls12
                            try
                            {
                                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768;
                            }
                            catch (Exception e3)
                            {
                                // no tls11
                            }
                        }
                    }
                }

                byte[] req = Encoding.UTF8.GetBytes(content);

                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = Proxy;

                    wc.Headers.Set("Accept", accept);
                    wc.Headers.Set("Authorization", GetAuthHeader());
                    wc.Headers.Set("Content-Type", contentType + "; charset=UTF-8");
                    wc.Headers.Set("User-Agent", GetAgentHeader());

                    return wc.UploadData(url, "POST", req);
                }
            }
            catch (WebException we)
            {
                Set(we);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
		/// Get response as object
        /// </summary>
        /// <param name="url">request URL</param>
		/// <param name="attr">JSON attribute name to return (null - root element)</param>
        /// <param name="type">object type to return</param>
        /// <returns>response object lub null</returns>
        private object GetObject(Uri url, string attr, Type type)
		{
            try
            {
				// get response
                byte[] b = GetBytes(url, "application/json");

                if (b == null)
                {
                    return null;
                }

                // parse
                JObject json = Deserialize(b);

				if (json == null)
				{
					return null;
                }

				if (json.ContainsKey("error"))
				{
					Set((int)json["error"]["code"], (string)json["error"]["description"], (string)json["error"]["details"]);
					return null;
				}

				return (string.IsNullOrEmpty(attr) ? json.ToObject(type) : json[attr].ToObject(type));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get response as object
        /// </summary>
        /// <param name="url">request URL</param>
        /// <param name="contentType">request content type</param>
        /// <param name="content">request content bytes</param>
        /// <param name="attr">JSON attribute name to return (null - root element)</param>
        /// <param name="type">object type to return</param>
        /// <returns>response object lub null</returns>
        private object PostObject(Uri url, string contentType, string content, string attr, Type type)
        {
            try
            {
                // get response
                byte[] b = PostBytes(url, contentType, content, "application/json");

                if (b == null)
                {
                    return null;
                }

                // parse
                JObject json = Deserialize(b);

                if (json == null)
                {
                    return null;
                }

                if (json.ContainsKey("error"))
                {
                    Set((int)json["error"]["code"], (string)json["error"]["description"], (string)json["error"]["details"]);
                    return null;
                }

                return (string.IsNullOrEmpty(attr) ? json.ToObject(type) : json[attr].ToObject(type));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Serialize object into JSON bytes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string Serialize(object obj)
        {
            JsonSerializerSettings s = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            try
            {
                if (obj == null)
                {
                    Set(ClientError.CLI_SEND);
                    return null;
                }

                return JsonConvert.SerializeObject(obj, s);
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
		/// Deserialize bytes into JSON object
        /// </summary>
        /// <param name="data">data bytes</param>
        /// <returns>JSON object or null</returns>
        private JObject Deserialize(byte[] data)
        {
            JsonSerializerSettings s = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            try
            {
                if (data == null)
                {
                    Set(ClientError.CLI_RESPONSE);
                    return null;
                }

                // parse
                JObject json;

                try
                {
                    json = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(data), s);

                    if (json == null)
                    {
                        Set(ClientError.CLI_RESPONSE);
                        return null;
                    }
                }
                catch (Exception)
                {
                    Set(ClientError.CLI_RESPONSE);
                    return null;
                }

                return json;
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Przygotowanie nagłówka z danymi o kliencie
        /// </summary>
        /// <returns>dane o kliencie</returns>
        private string GetAgentHeader()
		{
			return (string.IsNullOrEmpty(Application) ? "" : Application + " ") + "KsefApiClient/" + VERSION + " .NET/" + Environment.Version;
		}

		/// <summary>
		/// Przygotowanie nagłówka z danymi do autoryzacji zapytania
		/// </summary>
		/// <returns>treść nagłówka HTTP Authorization</returns>
		private string GetAuthHeader()
		{
            string creds = Id + ":" + Key;
            string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(creds));

			return string.Format("Basic {0}", b64);
		}

		/// <summary>
		/// Zwraca wartość węzła XML jako ciąg tekstowy
		/// </summary>
		/// <param name="doc">dokument XML</param>
		/// <param name="path">wyrażenie XPath wybierające wartość</param>
		/// <param name="def">wartość domyślna zwracana w przypadku braku wartości w XML</param>
		/// <returns>ciąg tekstowy</returns>
		private string GetString(XPathDocument doc, string path, string def)
		{
			try
			{
				XPathNavigator xpn = doc.CreateNavigator();

				string val = xpn.SelectSingleNode(path).Value;

				if (val != null)
				{
					return val;
				}
			}
			catch (Exception)
			{
			}

			return def;
		}

		/// <summary>
		/// Zwraca wartość węzła XML jako obiekt daty i czasu lokalnego
		/// </summary>
		/// <param name="doc">dokument XML</param>
		/// <param name="path">wyrażenie XPath wybierające wartość</param>
		/// <returns>data i czas lokalny lub null</returns>
		private DateTime? GetDateTime(XPathDocument doc, string path)
		{
			try
			{
				string val = GetString(doc, path, null);

				if (val != null)
				{
					return XmlConvert.ToDateTime(val, XmlDateTimeSerializationMode.Local);
				}
			}
			catch (Exception)
			{
			}

			return null;
		}

        /// <summary>
        /// Compare two byte arrays
        /// </summary>
        /// <param name="a">first array</param>
        /// <param name="b">second array</param>
        /// <returns>true if arrays are the same</returns>
        private bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int i = 0;

            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }

                i++;
            }

            return true;
        }

        /// <summary>
        /// Read all bytes from stream
        /// </summary>
        /// <param name="s">input stream</param>
        /// <returns>bytes read</returns>
        private byte[] ReadAllBytes(Stream s)
        {
            MemoryStream ms = new MemoryStream();
            byte[] b = new byte[8192];
            int read;

            while ((read = s.Read(b, 0, b.Length)) > 0)
            {
                ms.Write(b, 0, read);
            }

            return ms.ToArray();
        }
    }
}