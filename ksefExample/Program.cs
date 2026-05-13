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

using KsefApi.Model;
using System.IO.Compression;
using System.Text;

namespace KsefApi.Example
{
    /// <summary>
    /// Sample program
    /// </summary>
    class Program
    {
        private static string? SellerNIP;
        private static string? SellerName;

        private static KsefApiClient? KsefApi;
        private static DateTime Now;

        private static byte[]? Iv;
        private static byte[]? SKey;
        private static byte[]? EncKey;

        // increment on each run to avoid duplicates
        private static int InvoiceNumber = 1;

        private static string? KsefNumber;

        static void Main(string[] args)
        {
            try
            {
                // set some basic data
                Now = DateTime.Now;

                // seller NIP and name (this data must match yours data at KSeF portal)
                SellerNIP = "enter your company's NIP here";
                SellerName = "enter your company's name here";

                // KSEF API client object
                KsefApi = new KsefApiClient(KsefApiClient.TEST_URL, "enter valid API id here", "enter valid API key here");

                GenerateEncryptionData();

                // test some typical use cases

                // basic functions
                CreateInvoiceXml();
                ValidateInvoiceXml();

                CreateAndSendInvoice();
                CreateAndSendBatch();

                GetInvoiceLinks();
                VisualizeInvoiceXml();

                GetInvoiceByKsefNumber();
                GetInvoicesByTimeRange();

                // black-box functions
                UploadInvoice();
                UploadBatch();

                DownloadInvoices();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("main: " + e);
            }
        }

        /// <summary>
        /// Generate init vector and key for symmetric encryption
        /// </summary>
        private static void GenerateEncryptionData()
        {
            Console.WriteLine("GenerateEncryptionData");

            try
            {
                // get new init vector for symmetric encryption
                Iv = KsefApi.GenerateInitVector();

                if (Iv == null)
                {
                    Console.Error.WriteLine("ERR: GenerateInitVector failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("GenerateEncryptionData: init vector: " + Convert.ToBase64String(Iv));

                // gen new symmetric key for encryption
                SKey = KsefApi.GenerateKey();

                if (SKey == null)
                {
                    Console.Error.WriteLine("ERR: GenerateKey failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("GenerateEncryptionData: symmetric key: " + Convert.ToBase64String(SKey));

                // encrypt symmetric key with KSeF public key
                KsefPublicKeyResponse pkr = KsefApi.KsefPublicKey();

                if (pkr == null)
                {
                    Console.Error.WriteLine("ERR: KsefPublicKey failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF public key: " + pkr);

                EncKey = KsefApi.EncryptKey(pkr, SKey);

                if (EncKey == null)
                {
                    Console.Error.WriteLine("ERR: EncryptKey failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("GenerateEncryptionData: encrypted symmetric key: " + Convert.ToBase64String(EncKey));

                Console.WriteLine("GenerateEncryptionData: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GenerateEncryptionData: " + e);
            }
        }

        /// <summary>
        /// Get next invoice number for tests
        /// </summary>
        private static string GenNextInvoiceNumber()
        {
            return string.Format("KSEFAPI/{0:D5}/{1:D2}/{2:D2}/{3:D4}", InvoiceNumber++, Now.Day, Now.Month, Now.Year);
        }

        /// <summary>
        /// Create an invoice object
        /// </summary>
        private static Faktura CreateInvoice()
        {
            // create new invoice object (adapt the data to your needs)
            Faktura invoice = new Faktura();

            invoice.Naglowek = new TNaglowek();
            invoice.Naglowek.KodFormularza = new TKodFormularza();
            invoice.Naglowek.KodFormularza.KodFormularza = TKodFormularza.KodFormularzaEnum.FA;
            invoice.Naglowek.KodFormularza.KodSystemowy = TKodFormularza.KodSystemowyEnum.FAV3;
            invoice.Naglowek.KodFormularza.WersjaSchemy = TKodFormularza.WersjaSchemyEnum._10E;
            invoice.Naglowek.WariantFormularza = WariantFormularza.NUMBER_3;
            invoice.Naglowek.DataWytworzeniaFa = Now;
            invoice.Naglowek.SystemInfo = "KSEF API";

            // seller data
            invoice.Podmiot1 = new Podmiot1();
            invoice.Podmiot1.DaneIdentyfikacyjne = new TPodmiot1();
            invoice.Podmiot1.DaneIdentyfikacyjne.NIP = SellerNIP;
            invoice.Podmiot1.DaneIdentyfikacyjne.Nazwa = SellerName;
            invoice.Podmiot1.Adres = new TAdres();
            invoice.Podmiot1.Adres.KodKraju = TKodKraju.PL;
            invoice.Podmiot1.Adres.AdresL1 = "ul. Kwiatowa 1 m. 2";
            invoice.Podmiot1.Adres.AdresL2 = "00-001 Warszawa";

            // buyer data
            invoice.Podmiot2 = new Podmiot2();
            invoice.Podmiot2.DaneIdentyfikacyjne = new TPodmiot2();
            invoice.Podmiot2.DaneIdentyfikacyjne.NIP = "1111111111";
            invoice.Podmiot2.DaneIdentyfikacyjne.Nazwa = "F.H.U. Jan Kowalski";
            invoice.Podmiot2.Adres = new TAdres();
            invoice.Podmiot2.Adres.KodKraju = TKodKraju.PL;
            invoice.Podmiot2.Adres.AdresL1 = "ul. Polna 1";
            invoice.Podmiot2.Adres.AdresL2 = "00-001 Warszawa";
            invoice.Podmiot2.JST = Podmiot2.JSTEnum.NUMBER_2;
            invoice.Podmiot2.GV = Podmiot2.GVEnum.NUMBER_2;

            invoice.Fa = new Fa();
            invoice.Fa.KodWaluty = TKodWaluty.PLN;
            invoice.Fa.P1 = Now.Date;               // date of issue
            invoice.Fa.P1M = "Warszawa";
            invoice.Fa.P2 = GenNextInvoiceNumber(); // invoice number
            invoice.Fa.P6 = Now.Date;               // date of sale
            invoice.Fa.P131 = 1666.66;              // total net amount
            invoice.Fa.P141 = 383.33;               // total VAT amount
            invoice.Fa.P133 = 0.95;
            invoice.Fa.P143 = 0.05;
            invoice.Fa.P15 = 2051.0;                // total gross amount

            invoice.Fa.Adnotacje = new Adnotacje();
            invoice.Fa.Adnotacje.P16 = 2;
            invoice.Fa.Adnotacje.P17 = 2;
            invoice.Fa.Adnotacje.P18 = 2;
            invoice.Fa.Adnotacje.P18A = 2;
            invoice.Fa.Adnotacje.P23 = 2;

            invoice.Fa.Adnotacje.Zwolnienie = new Zwolnienie();
            invoice.Fa.Adnotacje.Zwolnienie.P19N = 1;

            invoice.Fa.Adnotacje.NoweSrodkiTransportu = new NoweSrodkiTransportu();
            invoice.Fa.Adnotacje.NoweSrodkiTransportu.P22N = 1;

            invoice.Fa.Adnotacje.PMarzy = new PMarzy();
            invoice.Fa.Adnotacje.PMarzy.PPMarzyN = 1;

            invoice.Fa.RodzajFaktury = TRodzajFaktury.VAT;
            invoice.Fa.FP = 1;
            
            invoice.Fa.Platnosc = new Platnosc();
            invoice.Fa.Platnosc.Zaplacono = 1;
            invoice.Fa.Platnosc.DataZaplaty = Now.Date;
            invoice.Fa.Platnosc.FormaPlatnosci = TFormaPlatnosci.NUMBER_6;

            FaWiersz w1 = new FaWiersz();
            w1.NrWierszaFa = 1;
            w1.UU_ID = "aaaa111133339990";
            w1.P7 = "lodówka Zimnotech mk1";
            w1.P8A = "szt.";
            w1.P8B = 1;
            w1.P9A = 1626.01;
            w1.P11 = 1626.01;
            w1.P12 = TStawkaPodatku._23;

            FaWiersz w2 = new FaWiersz();
            w2.NrWierszaFa = 2;
            w2.UU_ID = "aaaa111133339991";
            w2.P7 = "wniesienie sprzętu";
            w2.P8A = "szt.";
            w2.P8B = 1;
            w2.P9A = 40.65;
            w2.P11 = 40.65;
            w2.P12 = TStawkaPodatku._23;

            FaWiersz w3 = new FaWiersz();
            w3.NrWierszaFa = 3;
            w3.UU_ID = "aaaa111133339992";
            w3.P7 = "promocja lodówka pełna mleka";
            w3.P8A = "szt.";
            w3.P8B = 1;
            w3.P9A = 0.95;
            w3.P11 = 0.95;
            w3.P12 = TStawkaPodatku._5;

            invoice.Fa.FaWiersz = new List<FaWiersz>
            {
                w1,
                w2,
                w3
            };

            return invoice;
        }

        /// <summary>
        /// Get sample invoice XML
        /// </summary>
        private static string GetInvoiceXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<Faktura xmlns:etd=\"http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2022/01/05/eD/DefinicjeTypy/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                "xmlns=\"http://crd.gov.pl/wzor/2025/06/25/13775/\">\n" +
                "\t<Naglowek>\n" +
                "\t\t<KodFormularza kodSystemowy=\"FA (3)\" wersjaSchemy=\"1-0E\">FA</KodFormularza>\n" +
                "\t\t<WariantFormularza>3</WariantFormularza>\n" +
                "\t\t<DataWytworzeniaFa>" + ToIsoString(Now) + "</DataWytworzeniaFa>\n" +
                "\t\t<SystemInfo>KSEF API</SystemInfo>\n" +
                "\t</Naglowek>\n" +
                "\t<Podmiot1>\n" +
                "\t\t<DaneIdentyfikacyjne>\n" +
                "\t\t\t<NIP>" + SellerNIP + "</NIP>\n" +
                "\t\t\t<Nazwa>" + SellerName + "</Nazwa>\n" +
                "\t\t</DaneIdentyfikacyjne>\n" +
                "\t\t<Adres>\n" +
                "\t\t\t<KodKraju>PL</KodKraju>\n" +
                "\t\t\t<AdresL1>ul. Kwiatowa 1 m. 2</AdresL1>\n" +
                "\t\t\t<AdresL2>00-001 Warszawa</AdresL2>\n" +
                "\t\t</Adres>\n" +
                "\t\t<DaneKontaktowe>\n" +
                "\t\t\t<Email>abc@abc.pl</Email>\n" +
                "\t\t\t<Telefon>667444555</Telefon>\n" +
                "\t\t</DaneKontaktowe>\n" +
                "\t</Podmiot1>\n" +
                "\t<Podmiot2>\n" +
                "\t\t<DaneIdentyfikacyjne>\n" +
                "\t\t\t<NIP>1111111111</NIP>\n" +
                "\t\t\t<Nazwa>F.H.U. Jan Kowalski</Nazwa>\n" +
                "\t\t</DaneIdentyfikacyjne>\n" +
                "\t\t<Adres>\n" +
                "\t\t\t<KodKraju>PL</KodKraju>\n" +
                "\t\t\t<AdresL1>ul. Polna 1</AdresL1>\n" +
                "\t\t\t<AdresL2>00-001 Warszawa</AdresL2>\n" +
                "\t\t</Adres>\n" +
                "\t\t<DaneKontaktowe>\n" +
                "\t\t\t<Email>jan@kowalski.pl</Email>\n" +
                "\t\t\t<Telefon>555777999</Telefon>\n" +
                "\t\t</DaneKontaktowe>\n" +
                "\t\t<NrKlienta>fdfd778343</NrKlienta>\n" +
                "\t\t<JST>2</JST>\n" +
                "\t\t<GV>2</GV>\n" +
                "\t</Podmiot2>\n" +
                "\t<Fa>\n" +
                "\t\t<KodWaluty>PLN</KodWaluty>\n" +
                "\t\t<P_1>" + ToDateString(Now) + "</P_1>\n" +
                "\t\t<P_1M>Warszawa</P_1M>\n" +
                "\t\t<P_2>" + GenNextInvoiceNumber() + "</P_2>\n" +
                "\t\t<P_6>" + ToDateString(Now) + "</P_6>\n" +
                "\t\t<P_13_1>1666.66</P_13_1>\n" +
                "\t\t<P_14_1>383.33</P_14_1>\n" +
                "\t\t<P_13_3>0.95</P_13_3>\n" +
                "\t\t<P_14_3>0.05</P_14_3>\n" +
                "\t\t<P_15>2051</P_15>\n" +
                "\t\t<Adnotacje>\n" +
                "\t\t\t<P_16>2</P_16>\n" +
                "\t\t\t<P_17>2</P_17>\n" +
                "\t\t\t<P_18>2</P_18>\n" +
                "\t\t\t<P_18A>2</P_18A>\n" +
                "\t\t\t<Zwolnienie>\n" +
                "\t\t\t\t<P_19N>1</P_19N>\n" +
                "\t\t\t</Zwolnienie>\n" +
                "\t\t\t<NoweSrodkiTransportu>\n" +
                "\t\t\t\t<P_22N>1</P_22N>\n" +
                "\t\t\t</NoweSrodkiTransportu>\n" +
                "\t\t\t<P_23>2</P_23>\n" +
                "\t\t\t<PMarzy>\n" +
                "\t\t\t\t<P_PMarzyN>1</P_PMarzyN>\n" +
                "\t\t\t</PMarzy>\n" +
                "\t\t</Adnotacje>\n" +
                "\t\t<RodzajFaktury>VAT</RodzajFaktury>\n" +
                "\t\t<FP>1</FP>\n" +
                "\t\t<DodatkowyOpis>\n" +
                "\t\t\t<Klucz>preferowane godziny dowozu</Klucz>\n" +
                "\t\t\t<Wartosc>dni robocze 17:00 - 20:00</Wartosc>\n" +
                "\t\t</DodatkowyOpis>\n" +
                "\t\t<FaWiersz>\n" +
                "\t\t\t<NrWierszaFa>1</NrWierszaFa>\n" +
                "\t\t\t<UU_ID>aaaa111133339990</UU_ID>\n" +
                "\t\t\t<P_7>lodówka Zimnotech mk1</P_7>\n" +
                "\t\t\t<P_8A>szt.</P_8A>\n" +
                "\t\t\t<P_8B>1</P_8B>\n" +
                "\t\t\t<P_9A>1626.01</P_9A>\n" +
                "\t\t\t<P_11>1626.01</P_11>\n" +
                "\t\t\t<P_12>23</P_12>\n" +
                "\t\t</FaWiersz>\n" +
                "\t\t<FaWiersz>\n" +
                "\t\t\t<NrWierszaFa>2</NrWierszaFa>\n" +
                "\t\t\t<UU_ID>aaaa111133339991</UU_ID>\n" +
                "\t\t\t<P_7>wniesienie sprzętu</P_7>\n" +
                "\t\t\t<P_8A>szt.</P_8A>\n" +
                "\t\t\t<P_8B>1</P_8B>\n" +
                "\t\t\t<P_9A>40.65</P_9A>\n" +
                "\t\t\t<P_11>40.65</P_11>\n" +
                "\t\t\t<P_12>23</P_12>\n" +
                "\t\t</FaWiersz>\n" +
                "\t\t<FaWiersz>\n" +
                "\t\t\t<NrWierszaFa>3</NrWierszaFa>\n" +
                "\t\t\t<UU_ID>aaaa111133339992</UU_ID>\n" +
                "\t\t\t<P_7>promocja lodówka pełna mleka</P_7>\n" +
                "\t\t\t<P_8A>szt.</P_8A>\n" +
                "\t\t\t<P_8B>1</P_8B>\n" +
                "\t\t\t<P_9A>0.95</P_9A>\n" +
                "\t\t\t<P_11>0.95</P_11>\n" +
                "\t\t\t<P_12>5</P_12>\n" +
                "\t\t</FaWiersz>\n" +
                "\t\t<Platnosc>\n" +
                "\t\t\t<Zaplacono>1</Zaplacono>\n" +
                "\t\t\t<DataZaplaty>" + ToDateString(Now) + "</DataZaplaty>\n" +
                "\t\t\t<FormaPlatnosci>6</FormaPlatnosci>\n" +
                "\t\t</Platnosc>\n" +
                "\t\t<WarunkiTransakcji>\n" +
                "\t\t\t<Zamowienia>\n" +
                "\t\t\t\t<DataZamowienia>" + ToDateString(Now) + "</DataZamowienia>\n" +
                "\t\t\t\t<NrZamowienia>4354343</NrZamowienia>\n" +
                "\t\t\t</Zamowienia>\n" +
                "\t\t</WarunkiTransakcji>\n" +
                "\t</Fa>\n" +
                "\t<Stopka>\n" +
                "\t\t<Informacje>\n" +
                "\t\t\t<StopkaFaktury>Kapitał zakładowy 5 000 000</StopkaFaktury>\n" +
                "\t\t</Informacje>\n" +
                "\t\t<Rejestry>\n" +
                "\t\t\t<KRS>0000099999</KRS>\n" +
                "\t\t\t<REGON>999999999</REGON>\n" +
                "\t\t\t<BDO>000099999</BDO>\n" +
                "\t\t</Rejestry>\n" +
                "\t</Stopka>\n" +
                "</Faktura>\n";
        }

        /// <summary>
        /// Create new batch file
        /// </summary>
        private static string CreateBatch()
        {
            string path = CreateTempFile("batch-", ".zip");

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                for (int i = 1; i <= 100; i++)
                {
                    string xml = GetInvoiceXml();
                    ZipArchiveEntry entry = zip.CreateEntry(string.Format("invoice-{0:D3}.xml", i));

                    using (StreamWriter writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
                    {
                        writer.Write(xml);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Create invoice XML
        /// </summary>
        private static void CreateInvoiceXml()
        {
            Console.WriteLine("CreateInvoiceXml");

            try
            {
                // create new invoice object
                Faktura invoice = CreateInvoice();

                // get invoice as xml
                string xml = KsefApi.KsefInvoiceGenerate(invoice);

                if (xml == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceGenerate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + xml);

                Console.WriteLine("CreateInvoiceXml: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("CreateInvoiceXml: " + e);
            }
        }

        /// <summary>
        /// Validate invoice XML
        /// </summary>
        private static void ValidateInvoiceXml()
        {
            Console.WriteLine("ValidateInvoiceXml");

            try
            {
                // validate xml
                string xml = GetInvoiceXml();

                KsefInvoiceValidateResponse res = KsefApi.KsefInvoiceValidate(xml);

                if (res == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceValidate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Validation result: " + res);

                Console.WriteLine("ValidateInvoiceXml: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ValidateInvoiceXml: " + e);
            }
        }

        /// <summary>
        /// Get sample invoice, encrypt it and send
        /// </summary>
        private static void CreateAndSendInvoice()
        {
            Console.WriteLine("CreateAndSendInvoice");

            try
            {
                // create new invoice object
                Faktura invoice = CreateInvoice();

                // get invoice as xml
                string xml = KsefApi.KsefInvoiceGenerate(invoice);

                if (xml == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceGenerate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + xml);

                // open new online session
                EncryptionInfo ei = new EncryptionInfo();
                ei.InitVector = Iv;
                ei.EncryptedKey = EncKey;
                
                KsefSessionOpenOnlineRequest soo = new KsefSessionOpenOnlineRequest();
                soo.InvoiceVersion = KsefInvoiceVersion.V3;
                soo.EncryptionInfo = ei;

                KsefSessionOpenOnlineResponse soor = KsefApi.KsefSessionOpenOnline(soo);

                if (soor == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionOpenOnline failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + soor.Id);

                // encrypt an invoice
                byte[] data = Encoding.UTF8.GetBytes(xml);
                int size = data.Length;

                byte[] hash = KsefApi.GetHash(data);

                if (hash == null)
                {
                    Console.Error.WriteLine("ERR: GetHash failed: " + KsefApi.LastError);
                    return;
                }

                byte[] encData = KsefApi.EncryptData(Iv, SKey, data);

                if (encData == null)
                {
                    Console.Error.WriteLine("ERR: EncryptData failed: " + KsefApi.LastError);
                    return;
                }

                // send an encrypted invoice
                KsefInvoiceEncrypted ie = new KsefInvoiceEncrypted();
                ie.InvoiceSize = size;
                ie.InvoiceHash = hash;
                ie.EncryptedInvoice = encData;

                KsefInvoiceSendRequest req = new KsefInvoiceSendRequest();
                req.SessionId = soor.Id;
                req.Encrypted = ie;

                KsefInvoiceSendResponse isr = KsefApi.KsefInvoiceSend(req);

                if (isr == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceSend failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF invoice id: " + isr.Id);

                // check an invoice status (and fetch KSeF number and acquisition date)
                //KsefInvoiceStatusResponse str = KsefApi.KsefInvoiceStatus(isr.Id);
                KsefInvoiceStatusResponse? str = KsefApi.WaitForResult(() => KsefApi.KsefInvoiceStatus(isr.Id));

                if (str == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceStatus failed: " + KsefApi.LastError);
                    return;
                }

                PrintInvoiceInfo(str.InvoiceInfo);

                // save for other tests
                KsefNumber = str.InvoiceInfo.KsefNumber;

                // close session
                bool sc = KsefApi.KsefSessionClose(soor.Id);

                if (!sc)
                {
                    Console.Error.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                // wait for the UPO
                KsefSessionStatusResponse? ssr = KsefApi.WaitForResult(() => KsefApi.KsefSessionStatus(soor.Id));

                if (ssr == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionStatus failed: " + KsefApi.LastError);
                    return;
                }

                // get UPO
                string upo = KsefApi.KsefSessionUpo(soor.Id);

                if (upo == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionUpo failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("UPO: " + upo);

                string path = CreateTempFile("upo-", ".xml");
                File.WriteAllText(path, upo, Encoding.UTF8);

                Console.WriteLine("UPO saved to: " + path);

                Console.WriteLine("CreateAndSendInvoice: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("CreateAndSendInvoice: " + e);
            }
        }

        /// <summary>
        /// Generate sample batch, encrypt it and send
        /// </summary>
        private static void CreateAndSendBatch()
        {
            Console.WriteLine("CreateAndSendBatch");

            try
            {
                // create new batch
                string batch = CreateBatch();

                Console.WriteLine("Batch file: " + batch);

                // encrypt batch (large batch files must be divided into 50 MB parts, with each part encrypted separately)
                byte[] data = File.ReadAllBytes(batch);
                byte[] dataHash = KsefApi.GetHash(data);

                byte[] encData = KsefApi.EncryptData(Iv, SKey, data);
                byte[] encDataHash = KsefApi.GetHash(encData);

                // open new batch session
                EncryptionInfo ei = new EncryptionInfo();
                ei.InitVector = Iv;
                ei.EncryptedKey = EncKey;

                BatchPartInfo bpi = new BatchPartInfo();
                bpi.Ordinal = 1;
                bpi.PartSize = encData.Length;
                bpi.PartHash = encDataHash;

                BatchInfo bi = new BatchInfo();
                bi.BatchSize = data.Length;
                bi.BatchHash = dataHash;
                bi.BatchParts = new List<BatchPartInfo> { bpi };

                KsefSessionOpenBatchRequest sob = new KsefSessionOpenBatchRequest();
                sob.InvoiceVersion = KsefInvoiceVersion.V3;
                sob.EncryptionInfo = ei;
                sob.Offline = true;
                sob.BatchInfo = bi;

                KsefSessionOpenBatchResponse sobr = KsefApi.KsefSessionOpenBatch(sob);

                if (sobr == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionOpenBatch failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + sobr.Id);

                // upload all batch parts (using received info)
                using (HttpClient http = new HttpClient())
                {
                    foreach (PartUploadInfo pui in sobr.PartUploads)
                    {
                        Console.WriteLine("Uploading part: " + pui.Ordinal);

                        Console.WriteLine("Upload method: " + pui.Method);
                        Console.WriteLine("Upload URL: " + pui.Url);
                        Console.WriteLine("Upload headers: " + pui.Headers);

                        using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(pui.Method), pui.Url))
                        {
                            foreach (PartUploadHeader puh in pui.Headers)
                            {
                                request.Headers.TryAddWithoutValidation(puh.Name, puh.Value);
                            }

                            request.Content = new ByteArrayContent(encData);

                            HttpResponseMessage res = http.Send(request);

                            if ((int)res.StatusCode != 201)
                            {
                                Console.Error.WriteLine("ERR: part upload failed: " + (int)res.StatusCode);
                                return;
                            }
                        }
                    }
                }

                Console.WriteLine("Upload completed");

                // close session
                bool sc = KsefApi.KsefSessionClose(sobr.Id);

                if (!sc)
                {
                    Console.Error.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                // wait for the batch to be processed (we're using a simple loop here, but in real applications
                // you should use a more sophisticated method)
                KsefSessionStatusResponse? ssr = KsefApi.WaitForResult(() => KsefApi.KsefSessionStatus(sobr.Id));

                if (ssr == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionStatus failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Total invoices count: " + ssr.SessionInfo.InvoiceCount);
                Console.WriteLine("Successful invoices count: " + ssr.SessionInfo.SuccessfulInvoiceCount);
                Console.WriteLine("Failed invoices count: " + ssr.SessionInfo.FailedInvoiceCount);

                // get batch invoices statuses (and fetch KSeF numbers and acquisition dates)
                KsefSessionInvoicesResponse sir = KsefApi.ksefSessionInvoices(sobr.Id);

                foreach (InvoiceInfo ii in sir.Invoices)
                {
                    PrintInvoiceInfo(ii);
                }

                // get UPO
                string upo = KsefApi.KsefSessionUpo(sobr.Id);

                if (upo == null)
                {
                    Console.Error.WriteLine("ERR: KsefSessionUpo failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("UPO: " + upo);

                string path = CreateTempFile("upo-", ".xml");
                File.WriteAllText(path, upo, Encoding.UTF8);

                Console.WriteLine("UPO saved to: " + path);

                Console.WriteLine("CreateAndSendBatch: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("CreateAndSendBatch: " + e);
            }
        }

        /// <summary>
        /// Get invoice by its KSeF number
        /// </summary>
        private static void GetInvoiceByKsefNumber()
        {
            Console.WriteLine("GetInvoiceByKsefNumber");

            try
            {
                // get by number (we're using number from previous test)
                byte[] xml = KsefApi.KsefInvoiceGet(KsefNumber);

                if (xml == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceGet failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + Encoding.UTF8.GetString(xml));

                string path = CreateTempFile("invoice-", ".xml");
                File.WriteAllBytes(path, xml);

                Console.WriteLine("Invoice saved to: " + path);

                Console.WriteLine("GetInvoiceByKsefNumber: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetInvoiceByKsefNumber: " + e);
            }
        }

        /// <summary>
        /// Get all invoices from specified time range and type
        /// </summary>
        private static void GetInvoicesByTimeRange()
        {
            Console.WriteLine("GetInvoicesByTimeRange");

            try
            {
                // start query (get all invoices from last 3 days)
                EncryptionInfo ei = new EncryptionInfo();
                ei.InitVector = Iv;
                ei.EncryptedKey = EncKey;

                DateTime from = Now.AddDays(-3);
                DateTime to = Now;

                KsefInvoiceQueryStartRange qr = new KsefInvoiceQueryStartRange();
                qr.From = from;
                qr.To = to;

                KsefInvoiceQueryStartRequest iqs = new KsefInvoiceQueryStartRequest();
                iqs.EncryptionInfo = ei;
                iqs.SubjectType = KsefInvoiceQueryStartRequest.SubjectTypeEnum.Subject1;
                iqs.Range = qr;

                string queryId = KsefApi.KsefInvoiceQueryStart(iqs);

                if (queryId == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceQueryStart failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Query id: " + queryId);

                // wait for the result (we're using a simple loop here, but in real applications
                // you should use a more sophisticated method)
                KsefInvoiceQueryStatusResponse? iqsr = KsefApi.WaitForResult(() => KsefApi.KsefInvoiceQueryStatus(queryId));

                if (iqsr == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceQueryStatus failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Number of invoices: " + iqsr.NumberOfInvoices);

                // get results
                foreach (string partNumber in iqsr.PartNumbers)
                {
                    byte[] data = KsefApi.KsefInvoiceQueryResult(queryId, partNumber);

                    if (data == null)
                    {
                        Console.Error.WriteLine("ERR: KsefInvoiceQueryResult failed: " + KsefApi.LastError);
                        return;
                    }

                    string path = CreateTempFile("invoices-", ".zip.enc");
                    File.WriteAllBytes(path, data);

                    Console.WriteLine("Encrypted part saved to: " + path);
                }

                Console.WriteLine("GetInvoicesByTimeRange: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetInvoicesByTimeRange: " + e);
            }
        }

        /// <summary>
        /// Generate invoice URL links and QR codes
        /// </summary>
        private static void GetInvoiceLinks()
        {
            Console.WriteLine("GetInvoiceLinks");

            try
            {
                string xml = GetInvoiceXml();
                byte[] data = Encoding.UTF8.GetBytes(xml);
                byte[] hash = KsefApi.GetHash(data);

                // get URLs and QR codes for visualization
                KsefInvoiceLinksRequest il = new KsefInvoiceLinksRequest();
                il.Nip = SellerNIP;
                il.IssueDate = Now;
                il.InvoiceHash = hash;
                il.InvoiceKsefNumber = KsefNumber;

                KsefInvoiceLinksResponse ilr = KsefApi.ksefInvoiceLinks(il);

                if (ilr == null)
                {
                    Console.Error.WriteLine("ERR: ksefInvoiceLinks failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice link: " + ilr.Invoice.Link);
                Console.WriteLine("Invoice QR image: " + ilr.Invoice.Image);

                if (ilr.Certificate != null)
                {
                    Console.WriteLine("Certificate link: " + ilr.Certificate.Link);
                    Console.WriteLine("Certificate QR image: " + ilr.Certificate.Image);
                }

                Console.WriteLine("GetInvoiceLinks: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetInvoiceLinks: " + e);
            }
        }

        /// <summary>
        /// Generate an invoice visualization
        /// </summary>
        private static void VisualizeInvoiceXml()
        {
            Console.WriteLine("VisualizeInvoiceXml");

            try
            {
                string xml = GetInvoiceXml();
                byte[] data = Encoding.UTF8.GetBytes(xml);

                // visualize invoice xml as html (official layout from MF)
                KsefInvoiceVisualizeRequest iv = new KsefInvoiceVisualizeRequest();
                iv.Offline = string.IsNullOrEmpty(KsefNumber);
                iv.InvoiceKsefNumber = KsefNumber;
                iv.InvoiceData = data;
                iv.OutputFormat = KsefInvoiceVisualizeRequest.OutputFormatEnum.Html;
                iv.OutputLanguage = KsefInvoiceVisualizeRequest.OutputLanguageEnum.Pl;

                byte[] html = KsefApi.KsefInvoiceVisualize(iv);

                if (html == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceVisualize(html) failed: " + KsefApi.LastError);
                    return;
                }

                string path = CreateTempFile("invoice-", ".html");
                File.WriteAllBytes(path, html);

                Console.WriteLine("HTML saved to: " + path);

                // visualize invoice xml as pdf (still needs improvements)
                iv = new KsefInvoiceVisualizeRequest();
                iv.Offline = string.IsNullOrEmpty(KsefNumber);
                iv.InvoiceKsefNumber = KsefNumber;
                iv.InvoiceData = data;
                iv.OutputFormat = KsefInvoiceVisualizeRequest.OutputFormatEnum.Pdf;
                iv.OutputLanguage = KsefInvoiceVisualizeRequest.OutputLanguageEnum.Pl;

                byte[] pdf = KsefApi.KsefInvoiceVisualize(iv);

                if (pdf == null)
                {
                    Console.Error.WriteLine("ERR: KsefInvoiceVisualize(pdf) failed: " + KsefApi.LastError);
                    return;
                }

                path = CreateTempFile("invoice-", ".pdf");
                File.WriteAllBytes(path, pdf);

                Console.WriteLine("PDF saved to: " + path);

                Console.WriteLine("VisualizeInvoiceXml: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("VisualizeInvoiceXml: " + e);
            }
        }

        /// <summary>
        /// Upload a plain unencrypted invoice to KSeF
        /// </summary>
        private static void UploadInvoice()
        {
            Console.WriteLine("UploadInvoice");

            try
            {
                // sample invoice
                string xml = GetInvoiceXml();

                // this ID should be unique and can be used to link the invoice to any event in the user’s system
                // (e.g., an order number)
                string uploadId = Guid.NewGuid().ToString();

                BoxUploadInvoiceRequest req = new BoxUploadInvoiceRequest();
                req.UploadId = uploadId;
                req.Offline = false;
                req.Notify = false;
                req.Upo = true;
                req.InvoiceData = Encoding.UTF8.GetBytes(xml);

                if (!KsefApi.BoxUploadInvoice(req))
                {
                    Console.Error.WriteLine("ERR: BoxUploadInvoice failed: " + KsefApi.LastError);
                    return;
                }

                // wait for the result (we're using a simple loop here, but in real applications
                // you should use a more sophisticated method)
                BoxUploadInvoiceStatusResponse? res = KsefApi.WaitForResult(() => KsefApi.BoxUploadInvoiceStatus(uploadId));

                if (res == null)
                {
                    Console.Error.WriteLine("ERR: BoxUploadInvoiceStatus failed: " + KsefApi.LastError);
                    return;
                }

                PrintInvoiceInfo(res.InvoiceInfo);
                Console.WriteLine("Session id: " + res.SessionId);

                if (res.Upo != null)
                {
                    string path = CreateTempFile("upo-", ".xml");
                    File.WriteAllBytes(path, res.Upo);

                    Console.WriteLine("UPO saved to: " + path);
                }

                Console.WriteLine("UploadInvoice: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("UploadInvoice: " + e);
            }
        }

        /// <summary>
        /// Upload a ZIP file (batch) with plain unencrypted invoices to KSeF
        /// </summary>
        private static void UploadBatch()
        {
            Console.WriteLine("UploadBatch");

            try
            {
                // sample batch
                string batch = CreateBatch();

                // this ID should be unique and can be used to link the invoice to any event in the user’s system
                // (e.g., an order number)
                string uploadId = Guid.NewGuid().ToString();

                BoxUploadBatchRequest req = new BoxUploadBatchRequest();
                req.UploadId = uploadId;
                req.Offline = false;
                req.Notify = false;
                req.Upo = true;
                req.InvoiceVersion = KsefInvoiceVersion.V3;

                if (!KsefApi.BoxUploadBatch(req, batch))
                {
                    Console.Error.WriteLine("ERR: BoxUploadBatch failed: " + KsefApi.LastError);
                    return;
                }

                // wait for the result (we're using a simple loop here, but in real applications
                // you should use a more sophisticated method)
                BoxUploadBatchStatusResponse? res = KsefApi.WaitForResult(() => KsefApi.BoxUploadBatchStatus(uploadId));

                if (res == null)
                {
                    Console.Error.WriteLine("ERR: BoxUploadBatchStatus failed: " + KsefApi.LastError);
                    return;
                }

                foreach (InvoiceInfo ii in res.InvoiceInfo)
                {
                    PrintInvoiceInfo(ii);
                }

                Console.WriteLine("Session id: " + res.SessionId);

                if (res.Upo != null)
                {
                    string path = CreateTempFile("upo-", ".xml");
                    File.WriteAllBytes(path, res.Upo);

                    Console.WriteLine("UPO saved to: " + path);
                }
                
                Console.WriteLine("UploadBatch: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("UploadBatch: " + e);
            }
        }

        /// <summary>
        /// Download all invoices for specified type and time range from KSeF
        /// </summary>
        private static void DownloadInvoices()
        {
            Console.WriteLine("DownloadInvoices");

            try
            {
                // this ID should be unique and can be used to link the invoice to any event in the user’s system
                string downloadId = Guid.NewGuid().ToString();

                DateTime from = Now.AddDays(-3);
                DateTime to = Now;

                KsefInvoiceQueryStartRange range = new KsefInvoiceQueryStartRange();
                range.From = from;
                range.To = to;

                BoxDownloadInvoicesRequest req = new BoxDownloadInvoicesRequest();
                req.DownloadId = downloadId;
                req.Notify = false;
                req.SubjectType = BoxDownloadInvoicesRequest.SubjectTypeEnum.Subject1;
                req.Range = range;

                if (!KsefApi.BoxDownloadInvoices(req))
                {
                    Console.Error.WriteLine("ERR: BoxDownloadInvoices failed: " + KsefApi.LastError);
                    return;
                }

                // wait for the result (we're using a simple loop here, but in real applications
                // you should use a more sophisticated method)
                byte[]? res = KsefApi.WaitForResult(() => KsefApi.BoxDownloadInvoicesResult(downloadId));

                if (res == null)
                {
                    Console.Error.WriteLine("ERR: BoxDownloadInvoicesResult failed: " + KsefApi.LastError);
                    return;
                }

                // res buffer contains the bytes of a plain, unencrypted ZIP archive that includes the invoices
                // and a metadata file
                string path = CreateTempFile("invoices-", ".zip");
                File.WriteAllBytes(path, res);

                Console.WriteLine("Invoices saved to: " + path);

                Console.WriteLine("DownloadInvoices: done");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("DownloadInvoices: " + e);
            }
        }

        /// <summary>
        /// Print out invoice info
        /// </summary>
        private static void PrintInvoiceInfo(InvoiceInfo info)
        {
            Console.WriteLine("Invoice status code: " + info.Status.Code);
            Console.WriteLine("Invoice status description: " + info.Status.Description);
            Console.WriteLine("Invoice status details: " + info.Status.Details);

            // code = 200 - invoice successfully processed and accepted
            if (info.Status.Code == 200)
            {
                Console.WriteLine("Invoice number: " + info.InvoiceNumber);
                Console.WriteLine("Invoice KSeF number: " + info.KsefNumber);
                Console.WriteLine("Invoice acquisition date: " + info.AcquisitionDate);
            }
        }

        private static string ToIsoString(DateTime dt)
        {
            DateTimeOffset dto = new DateTimeOffset(dt);
            return dto.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }

        private static string ToDateString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        private static string CreateTempFile(string prefix, string extension)
        {
            string name = prefix + Guid.NewGuid().ToString("N") + extension;
            return Path.Combine(Path.GetTempPath(), name);
        }
    }
}
