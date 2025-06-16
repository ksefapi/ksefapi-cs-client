# Klient KSEF API dla .NET C#

[![en](https://img.shields.io/badge/lang-en-green.svg)](https://github.com/ksefapi/ksefapi-cs-client/blob/main/README.en.md)

To jest oficjalne repozytorium klienta KSEF API dla .NET C#: https://ksefapi.pl

Twoja brama do Krajowego Systemu e-Faktur. Witaj w oficjalnym opisie projektu https://ksefapi.pl Poniżej znajdziesz
kluczowe informacje dotyczące Krajowego Systemu e-Faktur (KSeF) oraz tego, jak projekt https://ksefapi.pl ułatwia
integrację i korzystanie z tego systemu.

# Dokumentacja

Dokumentacja i przykłady są dostępne tutaj: https://ksefapi.pl/rest-ksef-api-dokumentacja/

## KSEF REST API

KSEF REST API służy jako interfejs integracyjny dla Krajowego Systemu e-Faktur (KSeF), centralnego repozytorium
ustrukturyzowanych faktur elektronicznych, zarządzanego przez Ministerstwo Finansów.

### Główne funkcje KSEF API

* Generowanie faktur ustrukturyzowanych zgodnych z KSeF.
* Wysyłanie faktur do KSeF w sesji interaktywnej i odbieranie Urzędowego Potwierdzenia Odbioru (UPO).
* Wyszukiwanie i pobieranie faktur z KSeF (zarówno kosztowych, jak i sprzedażowych).
* Pobieranie pojedynczej faktury z KSeF.
* Operacje zarządzania sesjami interaktywnymi (otwieranie, sprawdzanie statusu, zamykanie).
* Pobieranie klucza publicznego KSeF do celów szyfrowania.
* Wizualizacja faktur KSeF (XML) w formacie PDF lub HTML.

### Jak działa KSEF REST API

API wykorzystuje zestaw operacji (metod) do ułatwienia interakcji między systemem IT klienta a systemem KSeF. Dostęp
do tych operacji uzyskuje się za pomocą żądań HTTP GET i POST. API definiuje różne struktury żądań i odpowiedzi, 
wyliczenia i typy zawartości (XML, ZIP, PDF, HTML) do obsługi różnych aspektów przetwarzania faktur. Interakcja
obejmuje komponenty, takie jak system wywołujący REST API, system KSeF i samo KSEF REST API. API zawiera również
mechanizmy obsługi błędów, zapewniając szczegółowe kody błędów i opisy różnych scenariuszy, w tym problemów
z autoryzacją, zarządzaniem sesjami i przetwarzaniem faktur.

## Co to KSeF?

Krajowy System e-Faktur (KSeF) to centralny, ogólnopolski system teleinformatyczny Ministerstwa Finansów, który
rewolucjonizuje proces fakturowania w Polsce. Jego głównym celem jest umożliwienie przedsiębiorcom wystawiania,
otrzymywania i przechowywania faktur w ustandaryzowanej formie elektronicznej, znanej jako faktury ustrukturyzowane
(e-Faktury). Dzięki KSeF, każda faktura otrzymuje unikalny numer identyfikacyjny i jest przechowywana w systemie przez
10 lat. Projekt ksefapi.pl został stworzony, aby maksymalnie uprościć firmom komunikację z KSeF, oferując narzędzia
i API do efektywnego zarządzania e-fakturami. Wprowadzenie KSeF ma na celu uszczelnienie systemu podatkowego,
automatyzację procesów księgowych oraz ułatwienie obiegu dokumentów.

Słowa kluczowe: KSeF, Krajowy System e-Faktur, KSeF co to, e-faktury, fakturowanie elektroniczne, ksefapi.pl, faktura
ustrukturyzowana.

### KSeF od kiedy?

Korzystanie z Krajowego Systemu e-Faktur (KSeF) staje się obowiązkowe etapami. Zgodnie z aktualnymi informacjami:
* **Od 1 lutego 2026 roku** – dla przedsiębiorców, których wartość sprzedaży (wraz z kwotą podatku) przekroczyła w 2024 roku 200 mln zł.
* **Od 1 kwietnia 2026 roku** – dla wszystkich pozostałych przedsiębiorców będących czynnymi podatnikami VAT.

Możliwość dobrowolnego korzystania z KSeF istniała już od 1 stycznia 2022 roku, co pozwoliło wielu firmom na wcześniejsze
przygotowanie się do zmian. Projekt ksefapi.pl wspiera przedsiębiorców w płynnym przejściu na obowiązkowy KSeF,
dostarczając narzędzia niezbędne do integracji i obsługi e-faktur zgodnie z nowymi wymogami i terminami KSeF.

Słowa kluczowe: KSeF od kiedy, terminy KSeF, obowiązkowy KSeF, data KSeF, ksefapi.pl, wdrożenie KSeF, harmonogram KSeF,
dla kogo KSeF.

### Faktury KSeF

Faktury KSeF, czyli faktury ustrukturyzowane, to dokumenty elektroniczne wystawiane i otrzymywane za pośrednictwem Krajowego
Systemu e-Faktur. Każda taka e-faktura ma ściśle określony format XML, zgodny ze strukturą logiczną FA(2) opublikowaną
przez Ministerstwo Finansów (schemat XSD). Po poprawnym przesłaniu do systemu, faktura KSeF otrzymuje unikalny numer
identyfikacyjny KSeF, który potwierdza jej wprowadzenie do oficjalnego obiegu. Korzyści płynące z używania faktur
KSeF to m.in. gwarancja ich autentyczności i integralności, bezpieczne przechowywanie przez 10 lat, eliminacja ryzyka
zagubienia dokumentu, a także potencjalnie szybszy zwrot podatku VAT. Projekt ksefapi.pl umożliwia łatwe generowanie,
wysyłanie, odbieranie i zarządzanie fakturami KSeF, zapewniając zgodność z wymogami systemu.

Aktualnie obowiązujący schemat XSD fakutry KSeF jest dostępny na stronie: https://www.podatki.gov.pl/e-deklaracje/dokumentacja-it/struktury-dokumentow-xml/#ksef

Słowa kluczowe: faktury KSeF, faktura ustrukturyzowana, e-faktura, XML KSeF, numer KSeF, ksefapi.pl, wzór faktury KSeF,
XSD KSeF.

### Integracja z KSeF

Integracja z KSeF polega na połączeniu systemów informatycznych przedsiębiorstwa (takich jak systemy ERP, programy
finansowo-księgowe) z Krajowym Systemem e-Faktur. Celem integracji jest automatyzacja procesów związanych z wystawianiem
i odbieraniem faktur ustrukturyzowanych. Ministerstwo Finansów udostępnia interfejs API KSeF, który umożliwia taką
komunikację. Projekt ksefapi.pl znacząco ułatwia ten proces, oferując gotowe biblioteki (m.in. dla PHP, Java, .NET)
oraz własne, dobrze udokumentowane REST API. Dzięki ksefapi.pl, integracja z KSeF staje się szybsza i mniej skomplikowana,
co pozwala firmom skupić się na swojej podstawowej działalności. Nasze rozwiązania pomagają w obsłudze uwierzytelniania,
w szczególności zarządzania sesją interaktywną oraz wykorzystaniem tokenów i certyfikatów (już wkrótce) KSeF, niezbędnymi
do bezpiecznej komunikacji z systemem.

Słowa kluczowe: Integracja z KSeF, API KSeF, ksefapi.pl, integracja systemów, oprogramowanie księgowe KSeF, token KSeF,
certfikat KSeF.

### Dokumentacja KSeF

Oficjalna dokumentacja Krajowego Systemu e-Faktur, w tym specyfikacje techniczne, schematy XML (struktury logiczne
e-Faktury), oraz informacje dotyczące API, jest publikowana przez Ministerstwo Finansów na portalu https://podatki.gov.pl oraz
na dedykowanej stronie KSeF. Jest to podstawowe źródło wiedzy dla deweloperów i firm wdrażających KSeF. Niezależnie od tego,
projekt ksefapi.pl dostarcza własną, szczegółową dokumentację dotyczącą oferowanego REST API oraz bibliotek klienckich.
Nasza dokumentacja https://ksefapi.pl ma na celu uproszczenie procesu integracji, krok po kroku wyjaśniając, jak korzystać
z naszych narzędzi do komunikacji z KSeF, jak obsługiwać poszczególne operacje (wysyłanie, odbieranie faktur,
zarządzanie sesjami) i jak interpretować odpowiedzi systemu.

Dokumentacja KSEF REST API: https://ksef24.com/

Oficialna dokumentacja KSeF: https://ksef.podatki.gov.pl/baza-wiedzy-ksef/pliki-do-pobrania-ksef/

Słowa kluczowe: Dokumentacja KSeF, API KSeF dokumentacja, ksefapi.pl dokumentacja, specyfikacja KSeF, schemat KSeF,
REST API KSeF, biblioteki KSeF.

# Pomoc i wsparcie

W przypadku pytań lub problemów związanych z Krajowym Systemem e-Faktur, Ministerstwo Finansów oraz Krajowa Administracja
Skarbowa oferują oficjalne kanały wsparcia. Dostępna jest infolinia KSeF oraz informacje na stronie https://podatki.gov.pl.
Użytkownicy projektu ksefapi.pl mogą liczyć na dedykowaną pomoc i wsparcie techniczne związane z naszymi usługami,
API oraz bibliotekami. W przypadku pytań dotyczących integracji za pośrednictwem https://ksefapi.pl/kontakt, problemów
z implementacją naszych rozwiązań lub potrzeby konsultacji, zapraszamy do kontaktu poprzez wskazane na naszej stronie
kanały, w tym adres e-mail: kontakt@ksefapi.pl. Nasz zespół jest gotowy, aby pomóc w pełnym wykorzystaniu możliwości
ksefapi.pl w kontekście Krajowego Systemu e-Faktur.

email: kontakt@ksefapi.pl

telefon: +48 222 199 199 (10:00-16:00)

Słowa kluczowe: Pomoc KSeF, wsparcie KSeF, ksefapi.pl pomoc, ksefapi.pl kontakt, wsparcie techniczne KSeF,
infolinia KSeF, problemy KSeF.

# Licencja

Ten projekt jest udostępniony na licencji Apache License, Version 2.0:

[![License (Apache 2.0)](https://img.shields.io/badge/license-Apache%20version%202.0-blue.svg?style=flat-square)](http://www.apache.org/licenses/LICENSE-2.0)
