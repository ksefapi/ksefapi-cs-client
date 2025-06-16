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

using System.Text;
using KsefApi.Model;
namespace KsefApi.Example
{
    /// <summary>
    /// Sample program
    /// </summary>
    class Program
    {

        private static string SellerNIP;
        private static string SellerName;

        private static KsefApiClient KsefApi;
        private static DateTime Now;

        private static string KsefRefNumber;

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

                // test some typical use cases
                ValidateInvoiceXml();
                CreateAndSendInvoice();
                CreateAndSendInvoiceWithEncryption();
                GetInvoiceByKsefNumber();
                GetInvoicesByTimeRange();
                VisualizeInvoiceXml();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERR: " + e.StackTrace);
            }
        }

        private static  Faktura CreateInvoice()
        {
            // create new invoice object (adapt the data to your needs)
            Faktura invoice = new Faktura();

            invoice.Naglowek = new TNaglowek();
            invoice.Naglowek.KodFormularza = new TKodFormularza(TKodFormularza.KodFormularzaEnum.FA, TKodFormularza.KodSystemowyEnum.FAV2, TKodFormularza.WersjaSchemyEnum._10E);
            invoice.Naglowek.WariantFormularza = WariantFormularza.NUMBER_2;
            invoice.Naglowek.DataWytworzeniaFa = Now;
            invoice.Naglowek.SystemInfo = "KSEF API";

            invoice.Podmiot1 = new Podmiot1();
            invoice.Podmiot1.DaneIdentyfikacyjne = new TPodmiot1(SellerNIP, SellerName);
            invoice.Podmiot1.Adres = new TAdres(TKodKraju.PL, "ul. Kwiatowa 1 m. 2", "00-001 Warszawa");

            invoice.Podmiot2 = new Podmiot2();
            invoice.Podmiot2.DaneIdentyfikacyjne = new TPodmiot2("F.H.U. Jan Kowalski", "1111111111");
            invoice.Podmiot2.Adres = new TAdres(TKodKraju.PL, "ul. Polna 1", "00-001 Warszawa");

            invoice.Fa = new Fa();
            invoice.Fa.KodWaluty = TKodWaluty.PLN;
            invoice.Fa.P1 = Now;            // date of issue
            invoice.Fa.P2 = "001/01/2025";  // invoice number
            invoice.Fa.P6 = Now;            // date of sale
            invoice.Fa.P131 = 1666.66;      // total net amount
            invoice.Fa.P141 = 383.33;       // total VAT amount
            invoice.Fa.P133 = 0.95;
            invoice.Fa.P143 = 0.05;
            invoice.Fa.P15 = 2051.0;        // total gross amount
            invoice.Fa.Adnotacje = new Adnotacje(2, 2, 2, 2, new Zwolnienie(0, null, null, null, 1), new NoweSrodkiTransportu(0, 0, null, 1), 2, new PMarzy(0, 0, 0, 0, 0, 1));
            invoice.Fa.RodzajFaktury = TRodzajFaktury.VAT;
            invoice.Fa.FP = 1;
            invoice.Fa.Platnosc = new Platnosc(1, Now, 0, null, null, TFormaPlatnosci.NUMBER_6);

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

        private static string GetInvoiceXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<Faktura xmlns:etd=\"http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2022/01/05/eD/DefinicjeTypy/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                "xmlns=\"http://crd.gov.pl/wzor/2023/06/29/12648/\">\n" +
                "\t<Naglowek>\n" +
                "\t\t<KodFormularza kodSystemowy=\"FA (2)\" wersjaSchemy=\"1-0E\">FA</KodFormularza>\n" +
                "\t\t<WariantFormularza>2</WariantFormularza>\n" +
                "\t\t<DataWytworzeniaFa>" + Now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ss.fffZ") + "</DataWytworzeniaFa>\n" +
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
                "\t</Podmiot2>\n" +
                "\t<Fa>\n" +
                "\t\t<KodWaluty>PLN</KodWaluty>\n" +
                "\t\t<P_1>2022-02-15</P_1>\n" +
                "\t\t<P_1M>Warszawa</P_1M>\n" +
                "\t\t<P_2>FV2022/02/150</P_2>\n" +
                "\t\t<P_6>" + Now.ToString("yyyy-MM-dd") + "</P_6>\n" +
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
                "\t\t\t<DataZaplaty>" + Now.ToString("yyyy-MM-dd") + "</DataZaplaty>\n" +
                "\t\t\t<FormaPlatnosci>6</FormaPlatnosci>\n" +
                "\t\t</Platnosc>\n" +
                "\t\t<WarunkiTransakcji>\n" +
                "\t\t\t<Zamowienia>\n" +
                "\t\t\t\t<DataZamowienia>" + Now.ToString("yyyy-MM-dd") + "</DataZamowienia>\n" +
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
                    Console.WriteLine("ERR: KsefInvoiceValidate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Validation result: " + res);

                Console.WriteLine("ValidateInvoiceXml: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("ValidateInvoiceXml: " + e);
            }
        }

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
                    Console.WriteLine("ERR: KsefInvoiceGenerate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + xml);

                // open new session
                KsefSessionOpenResponse sor = KsefApi.KsefSessionOpen(KsefInvoiceVersion.V2);

                if (sor == null)
                {
                    Console.WriteLine("ERR: KsefSessionOpen failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + sor.Id);

                // send an invoice
                byte[] data = Encoding.UTF8.GetBytes(xml);

                KsefInvoiceSendResponse isr = KsefApi.KsefInvoiceSend(sor.Id, 0, null, data);

                if (isr == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceSend failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF invoice id: " + isr.Id);

                // check an invoice status (and fetch KSeF reference number and date)
                KsefInvoiceStatusResponse str = KsefApi.KsefInvoiceStatus(isr.Id);

                if (str == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceStatus failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF invoice number: " + str.KsefReferenceNumber);
                Console.WriteLine("KSeF invoice date: " + str.AcquisitionTimestamp);

                // save for other tests
                KsefRefNumber = str.KsefReferenceNumber;

                // close session
                bool sc = KsefApi.KsefSessionClose(sor.Id);

                if (!sc)
                {
                    Console.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                // get UPO
                string upo = KsefApi.KsefSessionUpo(sor.Id);

                if (upo == null)
                {
                    Console.WriteLine("ERR: KsefSessionUpo failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("UPO: " + upo);

                File.WriteAllText("UPO-" + str.KsefReferenceNumber + ".xml", upo);

                Console.WriteLine("CreateAndSendInvoice: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateAndSendInvoice: " + e.StackTrace);
            }
        }

        private static void CreateAndSendInvoiceWithEncryption()
        {
            Console.WriteLine("CreateAndSendInvoiceWithEncryption");

            try
            {
                // create new invoice object
                Faktura invoice = CreateInvoice();

                // get invoice as xml
                string xml = KsefApi.KsefInvoiceGenerate(invoice);

                if (xml == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceGenerate failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + xml);

                // get new init vector for session with encryption
                byte[] iv = KsefApi.GenerateInitVector();

                if (iv == null)
                {
                    Console.WriteLine("ERR: GenerateInitVector failed: " + KsefApi.LastError);
                    return;
                }

                // gen new symmetric key for session with encryption
                byte[] skey = KsefApi.GenerateKey();

                if (skey == null)
                {
                    Console.WriteLine("ERR: GenerateKey failed: " + KsefApi.LastError);
                    return;
                }

                // encrypt symmetric key with KSeF public key
                KsefPublicKeyResponse pkr = KsefApi.KsefPublicKey();

                if (pkr == null)
                {
                    Console.WriteLine("ERR: KsefPublicKey failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF public key: " + pkr);

                byte[] encKey = KsefApi.EncryptKey(pkr, skey);

                if (encKey == null)
                {
                    Console.WriteLine("ERR: EncryptKey failed: " + KsefApi.LastError);
                    return;
                }

                // open new session
                KsefSessionOpenResponse sor = KsefApi.KsefSessionOpen(KsefInvoiceVersion.V2, iv, encKey);

                if (sor == null)
                {
                    Console.WriteLine("ERR: KsefSessionOpen failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + sor.Id);

                // encrypt an invoice
                byte[] data = Encoding.UTF8.GetBytes(xml);
                int size = data.Length;

                byte[] hash = KsefApi.GetHash(data);

                if (hash == null)
                {
                    Console.WriteLine("ERR: GetHash failed: " + KsefApi.LastError);
                    return;
                }

                byte[] encData = KsefApi.EncryptData(iv, skey, data);

                if (encData == null)
                {
                    Console.WriteLine("ERR: EncryptData failed: " + KsefApi.LastError);
                    return;
                }

                // send an encrypted invoice
                KsefInvoiceSendResponse isr = KsefApi.KsefInvoiceSend(sor.Id, size, hash, encData);

                if (isr == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceSend failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF invoice id: " + isr.Id);

                // check an invoice status (and fetch KSeF reference number and date)
                KsefInvoiceStatusResponse str = KsefApi.KsefInvoiceStatus(isr.Id);

                if (str == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceStatus failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF invoice number: " + str.KsefReferenceNumber);
                Console.WriteLine("KSeF invoice date: " + str.AcquisitionTimestamp);

                // save for other tests
                KsefRefNumber = str.KsefReferenceNumber;

                // close session
                bool sc = KsefApi.KsefSessionClose(sor.Id);

                if (!sc)
                {
                    Console.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                // get UPO
                string upo = KsefApi.KsefSessionUpo(sor.Id);

                if (upo == null)
                {
                    Console.WriteLine("ERR: KsefSessionUpo failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("UPO: " + upo);

                File.WriteAllText("UPO-" + str.KsefReferenceNumber + ".xml", upo);

                Console.WriteLine("CreateAndSendInvoiceWithEncryption: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateAndSendInvoiceWithEncryption: " + e.StackTrace);
            }
        }

        private static void GetInvoiceByKsefNumber()
        {
            Console.WriteLine("GetInvoiceByKsefNumber");

            try
            {
                // open new session
                KsefSessionOpenResponse sor = KsefApi.KsefSessionOpen(KsefInvoiceVersion.V2);

                if (sor == null)
                {
                    Console.WriteLine("ERR: KsefSessionOpen failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + sor.Id);

                // get by number (we're using number from previous test)
                byte[] xml = KsefApi.KsefInvoiceGet(sor.Id, KsefRefNumber);

                if (xml == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceGet failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Invoice XML: " + Encoding.UTF8.GetString(xml));

                File.WriteAllBytes("invoice-" + KsefRefNumber + ".xml", xml);

                // close session
                bool sc = KsefApi.KsefSessionClose(sor.Id);

                if (!sc)
                {
                    Console.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("GetInvoiceByKsefNumber: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("GetInvoiceByKsefNumber: " + e);
            }
        }

        private static void GetInvoicesByTimeRange()
        {
            Console.WriteLine("GetInvoicesByTimeRange");

            try
            {
                // open new session
                KsefSessionOpenResponse sor = KsefApi.KsefSessionOpen(KsefInvoiceVersion.V2);

                if (sor == null)
                {
                    Console.WriteLine("ERR: KsefSessionOpen failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("KSeF session id: " + sor.Id);

                // start query (get all invoices from last 3 days, Subject2 - means cost invoices)
                DateTime from = Now.AddDays(-3);
                DateTime to = Now;

                string queryId = KsefApi.KsefInvoiceQueryStart(sor.Id, KsefInvoiceQueryStartRequest.SubjectTypeEnum.Subject2,
                    from, to);

                if (queryId == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceQueryStart failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("Query id: " + queryId);

                // wait for result (we're using a simple loop here, in real life you should use a more sophisticated method)
                string[] partNumbers = null;

                for (int i = 0; i < 10; i++)
                {
                    if ((partNumbers = KsefApi.KsefInvoiceQueryStatus(sor.Id, queryId)) != null)
                    {
                        Console.WriteLine("Query result is ready");
                        break;
                    }

                    Console.WriteLine("WARNING: KsefInvoiceQueryStatus returned: " + KsefApi.LastError);
                    Thread.Sleep(5000);
                }

                if (partNumbers == null)
                {
                    Console.WriteLine("Timed out, no query result");
                    return;
                }

                // get results
                foreach (string partNumber in partNumbers)
                {
                    byte[] data = KsefApi.KsefInvoiceQueryResult(sor.Id, queryId, partNumber);

                    if (data == null)
                    {
                        Console.WriteLine("ERR: KsefInvoiceQueryResult failed: " + KsefApi.LastError);
                        return;
                    }

                    File.WriteAllBytes("invoices-" + partNumber + ".zip", data);
                }

                // close session
                bool sc = KsefApi.KsefSessionClose(sor.Id);

                if (!sc)
                {
                    Console.WriteLine("ERR: KsefSessionClose failed: " + KsefApi.LastError);
                    return;
                }

                Console.WriteLine("GetInvoicesByTimeRange: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("GetInvoicesByTimeRange: " + e);
            }
        }

        private static void VisualizeInvoiceXml()
        {
            Console.WriteLine("VisualizeInvoiceXml");

            try
            {
                string xml = GetInvoiceXml();

                byte[] data = Encoding.UTF8.GetBytes(xml);

                // visualize invoice xml as html (official layout from MF)
                byte[] html = KsefApi.KsefInvoiceVisualize(KsefRefNumber, data, true, true,
                    KsefInvoiceVisualizeRequest.OutputFormatEnum.Html, KsefInvoiceVisualizeRequest.OutputLanguageEnum.Pl);

                if (html == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceVisualize failed: " + KsefApi.LastError);
                    return;
                }

                File.WriteAllBytes("invoice.html", html);

                // visualize invoice xml as pdf (still needs improvements)
                byte[] pdf = KsefApi.KsefInvoiceVisualize(KsefRefNumber, data, true, true,
                    KsefInvoiceVisualizeRequest.OutputFormatEnum.Pdf, KsefInvoiceVisualizeRequest.OutputLanguageEnum.Pl);

                if (pdf == null)
                {
                    Console.WriteLine("ERR: KsefInvoiceVisualize failed: " + KsefApi.LastError);
                    return;
                }

                File.WriteAllBytes("invoice.pdf", pdf);

                Console.WriteLine("VisualizeInvoiceXml: done");
            }
            catch (Exception e)
            {
                Console.WriteLine("VisualizeInvoiceXml: " + e);
            }
        }
    }
}
