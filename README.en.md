# KSEF API Client for .NET C#

[![pl](https://img.shields.io/badge/lang-pl-red.svg)](https://github.com/ksefapi/ksefapi-cs-client/blob/main/README.md)

This is the official repository for KSEF API Client for .NET C#: https://ksefapi.pl

# Documentation

The documentation and samples are available at https://ksefapi.pl/rest-ksef-api-dokumentacja/

## KSEF REST API

KSEF REST API serves as an integration interface for the Polish National e-Invoice System (KSeF), a central repository
of structured electronic invoices managed by the Ministry of Finance.

### Key functions of KSEF API

* Generating structured invoices compliant with KSeF.
* Sending invoices to KSeF in an interactive session and receiving an Official Confirmation of Receipt (UPO).
* Searching and downloading invoices from KSeF (both cost and sales).
* Downloading a single invoice from KSeF.
* Interactive session management operations (opening, checking status, closing).
* Downloading the KSeF public key for encryption purposes.
* Visualization of invoices in PDF or HTML format.

### How it works

The API uses a set of operations (methods) to facilitate interaction between the client's IT system and the KSeF system.
These operations are accessed using HTTP GET and POST requests. The API defines various request and response structures,
enumerations, and content types (XML, ZIP, PDF, HTML) to support various aspects of invoice processing. The interaction
includes components such as the REST API caller, the KSeF system, and the KSeF REST API itself. The API also includes
error handling mechanisms, providing detailed error codes and descriptions of various scenarios, including authorization
issues, session management, and invoice processing.

# Help and support

kontakt@ksefapi.pl

+48 222 199 199 (10:00-16:00)

# License

This project is delivered under Apache License, Version 2.0:

[![License (Apache 2.0)](https://img.shields.io/badge/license-Apache%20version%202.0-blue.svg?style=flat-square)](http://www.apache.org/licenses/LICENSE-2.0)
