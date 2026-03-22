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


using KsefApi.Client;
using KsefApi.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace KsefApi
{
	/// <summary>
	/// KSEF API Service Client
	/// </summary>
	public class KsefApiClient
	{
		public const string VERSION = "2.0.2";

		public const string PRODUCTION_URL = "https://ksefapi.pl/api";
		public const string TEST_URL = "https://ksefapi.pl/api-test";

        private const string ENC_ALG = "aes-256-cbc";
        private const int ENC_ALG_BLOCK_SIZE = 16;
        private const int ENC_ALG_KEY_SIZE = 32;

        private const int CHUNK = 1_048_576;

        private RandomNumberGenerator rng;

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
        /// Last error
        /// </summary>
        public Error LastError { get; set; }

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

			Clear();

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
                RSAParameters par = new RSAParameters();
                par.Modulus = modulus;
                par.Exponent = exponent;

                RSA rsa = RSA.Create();
                rsa.ImportParameters(par);

                return rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
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
                string url = (URL + "/invoice/public/key");

				return (KsefPublicKeyResponse)GetObject(url, null, typeof(KsefPublicKeyResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Open a new KSeF online session
        /// </summary>
        /// <param name="req"request object</param>
        /// <returns>session details or null</returns>
        public KsefSessionOpenOnlineResponse KsefSessionOpenOnline(KsefSessionOpenOnlineRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/session/open/online");

                return (KsefSessionOpenOnlineResponse)PostObject(url, req, null, typeof(KsefSessionOpenOnlineResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Open a new KSeF batch session
        /// </summary>
        /// <param name="req"request object</param>
        /// <returns>session details or null</returns>
        public KsefSessionOpenBatchResponse KsefSessionOpenBatch(KsefSessionOpenBatchRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/session/open/batch");

                return (KsefSessionOpenBatchResponse)PostObject(url, req, null, typeof(KsefSessionOpenBatchResponse));
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
        public KsefSessionStatusResponse KsefSessionStatus(string sessionId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(sessionId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/session/status/" + HttpUtility.UrlEncode(sessionId));

                KsefSessionStatusResponse res = (KsefSessionStatusResponse)GetObject(url, null, typeof(KsefSessionStatusResponse));

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
        /// Close KSeF session
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>closing result</returns>
        public bool KsefSessionClose(string sessionId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(sessionId))
                {
                    Set(ClientError.CLI_INPUT);
                    return false;
                }

                // prepare url
                string url = (URL + "/invoice/session/close/" + HttpUtility.UrlEncode(sessionId));

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
        /// Get session's invoices info
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>session's invoices info or null</returns>
        public KsefSessionInvoicesResponse ksefSessionInvoices(string sessionId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(sessionId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/session/invoices/" + HttpUtility.UrlEncode(sessionId));

                KsefSessionInvoicesResponse res = (KsefSessionInvoicesResponse)GetObject(url, null, typeof(KsefSessionInvoicesResponse));

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
        /// Get UPO for specified session
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>XML with UPO or null</returns>
        public string KsefSessionUpo(string sessionId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(sessionId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/session/upo/" + HttpUtility.UrlEncode(sessionId));

                byte[] res = Send(url, null, null, "text/xml, application/json");

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

                if (invoice == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // req
                KsefInvoiceGenerateRequest req = new KsefInvoiceGenerateRequest(invoice);

                string json = Serialize(req);

                if (json == null)
                {
                    Set(ClientError.CLI_JSON);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/generate");

                byte[] res = Send(url, "application/json", new Body(json), "text/xml, application/json");

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

                if (string.IsNullOrEmpty(invoiceXml))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/validate");

                return (KsefInvoiceValidateResponse)SendObject(url, "text/xml", new Body(invoiceXml),
                    null, typeof(KsefInvoiceValidateResponse));
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
        /// <param name="req">request object</param>
        /// <returns>sending result with invoice id or null</returns>
        public KsefInvoiceSendResponse KsefInvoiceSend(KsefInvoiceSendRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/send");

                return (KsefInvoiceSendResponse)PostObject(url, req, null, typeof(KsefInvoiceSendResponse));
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

                if (string.IsNullOrEmpty(invoiceId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/status/" + HttpUtility.UrlEncode(invoiceId));

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
        /// <param name="ksefNumber">invoice KSeF number</param>
        /// <returns>invoice XML or null</returns>
        public byte[] KsefInvoiceGet(string ksefNumber)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(ksefNumber))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/get/" + HttpUtility.UrlEncode(ksefNumber));

                byte[] res = Send(url, null, null, "text/xml, application/json");

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
        /// <param name="req">request object</param>
        /// <returns>new query id or null</returns>
        public string KsefInvoiceQueryStart(KsefInvoiceQueryStartRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/query/start");

                KsefInvoiceQueryStartResponse res = (KsefInvoiceQueryStartResponse)PostObject(url, req, null, typeof(KsefInvoiceQueryStartResponse));

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
        /// <param name="queryId">query id</param>
        /// <returns>array of result parts numbers or null</returns>
        public KsefInvoiceQueryStatusResponse KsefInvoiceQueryStatus(string queryId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(queryId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }
                
                // prepare url
                string url = (URL + "/invoice/query/status/" + HttpUtility.UrlEncode(queryId));

                return (KsefInvoiceQueryStatusResponse)GetObject(url, null, typeof(KsefInvoiceQueryStatusResponse));
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
        /// <param name="queryId">query id</param>
        /// <param name="partNumber">query part number</param>
        /// <returns>plain or encrypted ZIP archive with invoices (depends on session type)</returns>
        public byte[] KsefInvoiceQueryResult(string queryId, string partNumber)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(queryId) || string.IsNullOrEmpty(partNumber))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/query/result/" + HttpUtility.UrlEncode(queryId) + "/" + HttpUtility.UrlEncode(partNumber));

                return Send(url, null, null, "application/octet-stream, application/json");
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Generate invoice URL links and QR codes
        /// </summary>
        /// <param name="req">request object</param>
        /// <returns>URLs and QR codes or null</returns>
        public KsefInvoiceLinksResponse ksefInvoiceLinks(KsefInvoiceLinksRequest req)
        {
            try
            {
                Clear();

                // req
                string json = Serialize(req);

                if (json == null)
                {
                    Set(ClientError.CLI_JSON);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/links");

                return (KsefInvoiceLinksResponse)PostObject(url, req, null, typeof(KsefInvoiceLinksResponse));
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
        /// <param name="req">request object</param>
        /// <returns>invoice visualization in requested format or null</returns>
        public byte[] KsefInvoiceVisualize(KsefInvoiceVisualizeRequest req)
        {
            try
            {
                Clear();

                // req
                string json = Serialize(req);

                if (json == null)
                {
                    Set(ClientError.CLI_JSON);
                    return null;
                }

                // prepare url
                string url = (URL + "/invoice/visualize");

                return Send(url, "application/json", new Body(json), "application/pdf, text/html, application/json");
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Upload plain invoice XML to KSeF
        /// </summary>
        /// <param name="req">request object</param>
        /// <returns>upload result or null</returns>
        public bool BoxUploadInvoice(BoxUploadInvoiceRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return false;
                }

                // prepare url
                string url = (URL + "/box/upload/invoice");

                BoxUploadInvoiceResponse res = (BoxUploadInvoiceResponse)PostObject(url, req, null, typeof(BoxUploadInvoiceResponse));

			    if (res == null) {
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
        /// Get status of uploaded invoice
        /// </summary>
        /// <param name="uploadId">upload identifier</param>
        /// <returns>upload result or null</returns>
        public BoxUploadInvoiceStatusResponse BoxUploadInvoiceStatus(string uploadId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(uploadId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/box/upload/invoice/" + HttpUtility.UrlEncode(uploadId));

                return (BoxUploadInvoiceStatusResponse)GetObject(url, null, typeof(BoxUploadInvoiceStatusResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Upload batch of plain invoices XMLs to KSeF
        /// </summary>
        /// <param name="req">request object</param>
        /// <param name="file">batch file path (ZIP archive)</param>
        /// <returns>upload result or null</returns>
        public bool BoxUploadBatch(BoxUploadBatchRequest req, string file)
        {
            try
            {
                Clear();

                if (req == null || file == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return false;
                }

                // boundary
                string boundary = Guid.NewGuid().ToString("N");

                // prepare url
                string url = (URL + "/box/upload/batch");

                BoxUploadInvoiceResponse res = (BoxUploadInvoiceResponse)SendObject(url, "multipart/form-data; boundary=" + boundary,
                    new Body(boundary, req, file), null, typeof(BoxUploadInvoiceResponse));

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
        /// Upload batch of plain invoices XMLs to KSeF
        /// </summary>
        /// <param name="req">request object</param>
        /// <param name="file">batch file content (ZIP archive)</param>
        /// <returns>upload result or null</returns>
        public bool BoxUploadBatch(BoxUploadBatchRequest req, byte[] file)
        {
            try
            {
                Clear();

                if (req == null || file == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return false;
                }

                // boundary
                string boundary = Guid.NewGuid().ToString("N");

                // prepare url
                string url = (URL + "/box/upload/batch");

                BoxUploadInvoiceResponse res = (BoxUploadInvoiceResponse)SendObject(url, "multipart/form-data; boundary=" + boundary,
                    new Body(boundary, req, file), null, typeof(BoxUploadInvoiceResponse));

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
        /// Get status of uploaded batch
        /// </summary>
        /// <param name="uploadId">upload identifier</param>
        /// <returns>upload result or null</returns>
        public BoxUploadBatchStatusResponse BoxUploadBatchStatus(string uploadId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(uploadId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/box/upload/batch/" + HttpUtility.UrlEncode(uploadId));

                return (BoxUploadBatchStatusResponse)GetObject(url, null, typeof(BoxUploadBatchStatusResponse));
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Send query for invoice download
        /// </summary>
        /// <param name="req">request object</param>
        /// <returns>sending result or null</returns>
        public bool BoxDownloadInvoices(BoxDownloadInvoicesRequest req)
        {
            try
            {
                Clear();

                if (req == null)
                {
                    Set(ClientError.CLI_INPUT);
                    return false;
                }

                // prepare url
                string url = (URL + "/box/download/invoices");

                BoxDownloadInvoicesResponse res = (BoxDownloadInvoicesResponse)PostObject(url, req, null, typeof(BoxDownloadInvoicesResponse));

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
        /// Get result of download query
        /// </summary>
        /// <param name="downloadId">download identifier</param>
        /// <returns>download result (ZIP archive) or null</returns>
        public byte[] BoxDownloadInvoicesResult(string downloadId)
        {
            try
            {
                Clear();

                if (string.IsNullOrEmpty(downloadId))
                {
                    Set(ClientError.CLI_INPUT);
                    return null;
                }

                // prepare url
                string url = (URL + "/box/download/invoices/" + HttpUtility.UrlEncode(downloadId));

                return Send(url, null, null, "application/zip, application/json");
            }
            catch (Exception e)
            {
                Set(ClientError.CLI_EXCEPTION, e.Message);
            }

            return null;
        }

        /// <summary>
        /// Poll until an asynchronous operation is ready
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">function to call</param>
        /// <param name="tries">number of tries</param>
        /// <param name="seconds">delay between tries (in sec)</param>
        /// <returns>value returned from callback or default in case of error</returns>
        public T WaitForResult<T>(Func<T> callback, int tries = 40, int seconds = 15)
        {
            int ms = seconds * 1000;

            for (int i = 0; i < tries; i++)
            {
                T res = callback();

                if (res == null)
                {
                    if (LastError != null && (LastError.Code == 100 || LastError.Code == 150))
                    {
                        // still processing
                        Thread.Sleep(ms);
                        continue;
                    }
                    else
                    {
                        // error
                        return default;
                    }
                }

                if (res is KsefInvoiceStatusResponse isr)
                {
                    if (isr.InvoiceInfo.Status.Code < 200)
                    {
                        // still processing
                        Thread.Sleep(ms);
                        continue;
                    }
                    else if (isr.InvoiceInfo.Status.Code == 200)
                    {
                        // ready
                        return res;
                    }
                    else
                    {
                        // error
                        return default;
                    }
                }
                else if (res is KsefSessionStatusResponse ssr)
                {
                    if (ssr.SessionInfo.Status.Code < 200 && ssr.SessionInfo.Status.Code != 170)
                    {
                        // still processing
                        Thread.Sleep(ms);
                        continue;
                    }
                    else if (ssr.SessionInfo.Status.Code == 200)
                    {
                        // ready
                        return res;
                    }
                    else
                    {
                        // error
                        return default;
                    }
                }
                else
                {
                    // ready
                    return res;
                }
            }

            // timeout, no result
            return default;
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
        /// HTTP GET/POST
        /// </summary>
        /// <param name="url">request URL</param>
		/// <param name="contentType">request content type</param>
		/// <param name="content">request content bytes</param>
		/// <param name="accept">accepted response mime type (application/xml or application/json)</param>
        /// <returns>response data or null</returns>
        private byte[] Send(string url, string contentType, Body content, string accept)
        {
            try
            {
                bool post = (!string.IsNullOrEmpty(contentType) && content != null);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.AllowWriteStreamBuffering = false;
                request.AllowAutoRedirect = true;
                request.SendChunked = false;

                request.Headers.Add(HttpRequestHeader.Accept, accept);
                request.Headers.Add(HttpRequestHeader.Authorization, GetAuthHeader());
                request.Headers.Add(HttpRequestHeader.UserAgent, GetAgentHeader());

                if (post)
                {
                    request.Method = "POST";
                    request.ContentType = contentType;
                    request.ContentLength = content.Length();

                    using (Stream rs = request.GetRequestStream())
                    {
                        if (content.Prefix != null)
                        {
                            rs.Write(content.Prefix, 0, content.Prefix.Length);
                        }

                        if (!string.IsNullOrEmpty(content.Path))
                        {
                            using (var fs = new FileStream(content.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                byte[] buffer = new byte[CHUNK];
                                int read;

                                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    rs.Write(buffer, 0, read);
                                }
                            }
                        }

                        if (content.Bytes != null)
                        {
                            rs.Write(content.Bytes, 0, content.Bytes.Length);
                        }

                        if (content.Suffix != null)
                        {
                            rs.Write(content.Suffix, 0, content.Suffix.Length);
                        }
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream rs = response.GetResponseStream())
                {
                    if (rs == null)
                    {
                        return Array.Empty<byte>();
                    }

                    return ReadAllBytes(rs);
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
        /// Send request and get response as object
        /// </summary>
        /// <param name="url">request URL</param>
        /// <param name="contentType">request content type</param>
        /// <param name="content">request content bytes</param>
        /// <param name="attr">JSON attribute name to return (null - root element)</param>
        /// <param name="type">object type to return</param>
        /// <returns>response object lub null</returns>
        private object SendObject(string url, string contentType, Body content, string attr, Type type)
        {
            try
            {
                // get response
                byte[] b = Send(url, contentType, content, "application/json");

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
		/// <param name="attr">JSON attribute name to return (null - root element)</param>
        /// <param name="type">object type to return</param>
        /// <returns>response object lub null</returns>
        private object GetObject(string url, string attr, Type type)
		{
            try
            {
				// get response
                byte[] b = Send(url, null, null, "application/json");

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
        /// Post object and get response as object
        /// </summary>
        /// <param name="url">request URL</param>
        /// <param name="obj">request object</param>
        /// <param name="attr">JSON attribute name to return (null - root element)</param>
        /// <param name="type">object type to return</param>
        /// <returns>response object lub null</returns>
        private object PostObject(string url, object obj, string attr, Type type)
        {
            try
            {
                // req
                string req = Serialize(obj);

                if (req == null)
                {
                    Set(ClientError.CLI_JSON);
                    return false;
                }

                // get response
                byte[] b = Send(url, "application/json", new Body(req), "application/json");

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
        /// <returns>JSON string or null</returns>
        private static string Serialize(object obj)
        {
            JsonSerializerSettings s = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            try
            {
                if (obj == null)
                {
                    return null;
                }

                return JsonConvert.SerializeObject(obj, s);
            }
            catch (Exception)
            {
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
            byte[] b = new byte[CHUNK];
            int read;

            while ((read = s.Read(b, 0, b.Length)) > 0)
            {
                ms.Write(b, 0, read);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Http request body
        /// </summary>
        class Body
        {
            public byte[] Prefix { get; }
            public string Path { get; }
            public byte[] Bytes { get; }
            public byte[] Suffix { get; }

            /// <summary>
            /// Create a simple request body
            /// </summary>
            /// <param name="data">body content</param>
            public Body(string data)
            {
                Bytes = Encoding.UTF8.GetBytes(data);
            }

            /// <summary>
            /// Build multipart body with JSON request and ZIP file
            /// </summary>
            /// <param name="boundary">mulitpart boundary name</param>
            /// <param name="obj">request object (name="request")</param>
            /// <param name="path">ZIP file path (name="file")</param>
            public Body(string boundary, object obj, string path)
            {
                string json = Serialize(obj);

                if (json == null)
                {
                    throw new InvalidOperationException("Failed to convert object to JSON");
                }

                string prefix = "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"request\"\r\n"
                    + "Content-Type: application/json\r\n"
                    + "\r\n"
                    + json + "\r\n"
                    + "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"file\"; filename=\"batch.zip\"\r\n"
                    + "Content-Type: application/zip\r\n"
                    + "\r\n";

                string suffix = "\r\n"
                    + "--" + boundary + "--\r\n";

                Prefix = Encoding.UTF8.GetBytes(prefix);
                Path = path;
                Suffix = Encoding.UTF8.GetBytes(suffix);
            }

            /// <summary>
            /// Build multipart body with JSON request and ZIP bytes
            /// </summary>
            /// <param name="boundary">mulitpart boundary name</param>
            /// <param name="obj">request object (name="request")</param>
            /// <param name="file">ZIP file content (name="file")</param>
            public Body(string boundary, object obj, byte[] bytes)
            {
                string json = Serialize(obj);

                if (json == null)
                {
                    throw new InvalidOperationException("Failed to convert object to JSON");
                }

                string prefix = "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"request\"\r\n"
                    + "Content-Type: application/json\r\n"
                    + "\r\n"
                    + json + "\r\n"
                    + "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"file\"; filename=\"batch.zip\"\r\n"
                    + "Content-Type: application/zip\r\n"
                    + "\r\n";

                string suffix = "\r\n"
                    + "--" + boundary + "--\r\n";

                Prefix = Encoding.UTF8.GetBytes(prefix);
                Bytes = bytes;
                Suffix = Encoding.UTF8.GetBytes(suffix);
            }

            /// <summary>
            /// Total length of body parts
            /// </summary>
            /// <returns>body length</returns>
            public long Length()
            {
                long length = 0;

                if (Prefix != null)
                {
                    length += Prefix.Length;
                }

                if (!string.IsNullOrEmpty(Path))
                {
                    FileInfo fi = new FileInfo(Path);

                    if (!fi.Exists)
                    {
                        throw new FileNotFoundException("File not found: " + Path);
                    }

                    length += fi.Length;
                }

                if (Bytes != null)
                {
                    length += Bytes.Length;
                }

                if (Suffix != null)
                {
                    length += Suffix.Length;
                }

                return length;
            }
        }
    }
}