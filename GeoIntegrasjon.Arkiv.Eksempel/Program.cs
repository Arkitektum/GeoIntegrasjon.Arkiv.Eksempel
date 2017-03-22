using GeoIntegrasjon.Arkiv.Eksempel.ArkivOppdatering;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GeoIntegrasjon.Arkiv.Eksempel
{
    class Program
    {
        static void Main(string[] args)
        {

            ArkivInnsynEksempler.TestInnsyn();

            //************************************************************
            //Oppsett og autentisering
            string urlTilArkiv = ConfigurationManager.AppSettings["arkivoppdateringurl"];
            SakArkivOppdateringPortClient arkivClient = new SakArkivOppdateringPortClient("SakArkivOppdatering", urlTilArkiv);
            //Hvis basic autentication kreves
            arkivClient.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["brukernavn"];
            arkivClient.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["passord"];

            //************************************************************
            //Oppsett kontekst
            ArkivKontekst kontekst = new ArkivKontekst();
            kontekst.klientnavn = "Fagsystem 1";
            kontekst.klientversjon = "v2.1";
            kontekst.referanseoppsett = "Fagsystem 1 referanseoppsett"; //Nøkkel til oppsett i arkivet, må være registrert der.

            //Starte saksmappe
            Saksmappe nyBasisMappe = NyBasisSaksmappe(arkivClient, kontekst);

            //Inngående brev
            Journalpost nyInngJournalpost = RegistrerInngåendeJournalpost(arkivClient, kontekst, nyBasisMappe);
            NyttDokument(arkivClient, kontekst, nyInngJournalpost);

            //Svar på inngående brev
            Journalpost nyUtgJournalpost = RegistrerUtgåendeJournalpost(arkivClient, kontekst, nyBasisMappe, nyInngJournalpost);
            NyttDokument(arkivClient, kontekst, nyUtgJournalpost);

            //Avslutt saksmappe
            AvsluttMappe(arkivClient, kontekst, nyBasisMappe);

            //Start ny saksmappe
            Saksmappe nyEnkelmappe = NyEnkelSaksmappe(arkivClient, kontekst);
            //Registrere internt notat
            Journalpost nyttNotat = RegistrerInterntNotat(arkivClient, kontekst, nyEnkelmappe);
            NyttDokument(arkivClient, kontekst, nyttNotat);

        }

        private static void AvsluttMappe(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappe)
        {
            arkivClient.OppdaterMappeStatus(new Saksstatus() { kodeverdi = "A" }, mappe.saksnr, kontekst);
        }

        private static Journalpost RegistrerUtgåendeJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe referanseMappe, Journalpost inngJournalpost)
        {
            //************************************************************
            // Ny journalpost, Utgående dokument
            Journalpost jpU = new Journalpost();
            //Referanse til saksmappe
            jpU.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = referanseMappe.systemID } };

            jpU.journalposttype = new Journalposttype() { kodeverdi = "U" }; //Konfigureres og hentes fra kodeliste
            jpU.tittel = "Tittel på det utgående brevet";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og hentes fra kodeliste
            Person p = new Person();
            p.personid = new Personidentifikator();
            p.personid.personidentifikatorNr = "12345678910";
            p.personid.personidentifikatorType = new PersonidentifikatorType() { kodeverdi = "F" };
            p.navn = "Ole Olsen";
            List<EnkelAdresse> EnkelAdresseListe = new List<EnkelAdresse>();
            EnkelAdresse adresse = new EnkelAdresse();
            adresse.adresselinje1 = "Storgata 4A";
            adresse.postadresse = new PostadministrativeOmraader() { postnummer = "3801", poststed = "Bø" };
            adresse.landkode = new Landkode() { kodeverdi = "NO" };
            EnkelAdresseListe.Add(adresse);
            p.adresser = EnkelAdresseListe.ToArray();
            mott.Kontakt = p;
            KorrespondansepartListe.Add(mott);
            jpU.korrespondansepart = KorrespondansepartListe.ToArray();

            //jpU.referanseAvskrivninger = new AvskrivningListe();
            //Avskrivning avskrUtg = new Avskrivning();
            //avskrUtg.avskrivningsmaate = new Avskrivningsmaate() { kodeverdi = "BU" , kodebeskrivelse = "Besvart med brev" };//Konfigureres og hentes fra kodeliste
            //avskrUtg.referanseAvskriverJournalnummer = inngJournalpost.journalnummer;
            //jpU.referanseAvskrivninger.Add(avskrUtg);


            var nyUtgJournalpost = arkivClient.NyJournalpost(jpU, kontekst);

            return nyUtgJournalpost;
        }

        private static void NyttDokument(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Journalpost referanseJournalpost)
        {

            //************************************************************
            // Nytt dokument
            Dokument dok = new Dokument();
            dok.referanseJournalpostSystemID = referanseJournalpost.systemID;
            dok.tittel = "dokumenttittel";
            dok.dokumenttype = new Dokumenttype() { kodeverdi = "VEDTAK" }; //Konfigureres og hentes fra kodeliste
            dok.dokumentstatus = new Dokumentstatus() { kodeverdi = "F" }; //Konfigureres og hentes fra kodeliste
            dok.variantformat = new Variantformat() { kodeverdi = "A" }; //Konfigureres og hentes fra kodeliste
            dok.format = new Format() { kodeverdi = "RA-PDF" }; //Konfigureres og hentes fra kodeliste
            var filinnh = new Filinnhold();
            filinnh.filnavn = "Arkiverbart_brev.pdf";
            filinnh.mimeType = "application/pdf";
            filinnh.base64 = System.IO.File.ReadAllBytes("Arkiverbart_brev.pdf");
            dok.Fil = filinnh;


            var nyDok = arkivClient.NyDokument(dok, false, kontekst);
        }

        private static Saksmappe NyBasisSaksmappe(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst)
        {

            //************************************************************
            // Lag ny saksmappe knyttet til Fagsystem 1 nøkkel1 og matrikkel 1/5
            Saksmappe mappe2 = new Saksmappe();
            mappe2.tittel = "Tittel på saken";

            EksternNoekkel ekstnok = new EksternNoekkel();
            ekstnok.fagsystem = "INNSYN";  //Hent lovlige kodeverdier fra HentKodeliste
            ekstnok.noekkel = "12345";
            mappe2.referanseEksternNoekkel = ekstnok;

            Klasse klasse = new Klasse();
            klasse.rekkefoelge = "1";
            klasse.klassifikasjonssystem = new Klassifikasjonssystem() { kodeverdi = "GNR" }; //Hent lovlige kodeverdier fra HentKodeliste
            klasse.klasseID = "1234-1/5/0/0";
            List<Klasse> klasser = new List<Klasse>();
            klasser.Add(klasse);
            mappe2.klasse = klasser.ToArray();

            Matrikkelnummer eiendom = new Matrikkelnummer();
            eiendom.kommunenummer = "1234";
            eiendom.gaardsnummer = "1";
            eiendom.bruksnummer = "5";
            List<Matrikkelnummer> Matrikkelnummer = new List<Matrikkelnummer>();
            Matrikkelnummer.Add(eiendom);
            mappe2.Matrikkelnummer = Matrikkelnummer.ToArray();
            

            // Legge til arkivdel referanse og mappetype hvis dette ikke kan gis av referanseoppsett eller pålogget bruker
            mappe2.mappetype = new Mappetype() { kodeverdi = "BYG.enkel" }; //Hent lovlige kodeverdier fra HentKodeliste
            mappe2.referanseArkivdel = new Arkivdel() { kodeverdi = "BYGG" }; //Hent lovlige kodeverdier fra HentKodeliste

            var nyMappe2 = arkivClient.NySaksmappe(mappe2, kontekst);
            return nyMappe2;
        }

        private static Saksmappe NyEnkelSaksmappe(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst)
        {
            //Sjekk om referanseoppsett støttes av arkivleverandør og om det kan brukes for å sette standardverdier som feks Arkivdel, klassering, mappetype, mm

            //Ny saksmappe, minimumsdata
            Saksmappe mappe = new Saksmappe();
            mappe.tittel = "Tittel på saken";
       
            var nyMappe = arkivClient.NySaksmappe(mappe, kontekst);
            return nyMappe;
        }

        private static Journalpost RegistrerInngåendeJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappeReferanse)
        {
            // Ny journalpost, Inngående dokument
            Journalpost jp = new Journalpost();
            //Referanse til saksmappe
            jp.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = mappeReferanse.systemID } };

            jp.journalposttype = new Journalposttype() { kodeverdi = "I" }; //Konfigureres og hentes fra kodeliste
            jp.tittel = "Tittel på det mottatte brevet";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            Korrespondansepart avs = new Korrespondansepart();
            avs.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Avsender", kodebeskrivelse = "Avsender" };//Konfigureres og hentes fra kodeliste
            Person p = new Person();
            p.personid = new Personidentifikator();
            p.personid.personidentifikatorNr = "12345678910";
            p.personid.personidentifikatorType = new PersonidentifikatorType() { kodeverdi = "F" };
            p.navn = "Ole Olsen";
            List<EnkelAdresse> EnkelAdresseListe = new List<EnkelAdresse>();
            EnkelAdresse adresse = new EnkelAdresse();
            adresse.adresselinje1 = "Storgata 4A";
            adresse.postadresse = new PostadministrativeOmraader() { postnummer = "3801", poststed = "Bø" };
            adresse.landkode = new Landkode() { kodeverdi = "NO" };
            EnkelAdresseListe.Add(adresse);
            p.adresser = EnkelAdresseListe.ToArray();

            avs.Kontakt = p;
            KorrespondansepartListe.Add(avs);

            //Angi saksbehandler hvis denne er annen enn pålogget bruker
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og hentes fra kodeliste
            mott.behandlingsansvarlig = "1"; // Behandlingsansvarlig
            //mott.administrativEnhetInit = "TEKN.BYG"; //Enhetsforkortelse - Konfigureres og hentes fra kodeliste Mangler?
            mott.saksbehandlerInit = "ar1"; // Initialer saksbehandler - Konfigureres og hentes fra kodeliste Mangler?
            KorrespondansepartListe.Add(mott);
            jp.korrespondansepart = KorrespondansepartListe.ToArray();

            var nyInngJournalpost = arkivClient.NyJournalpost(jp, kontekst);
            return nyInngJournalpost;
        }

        private static Journalpost RegistrerInterntNotat(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappeReferanse)
        {
           
            // Nytt internt notat
            Journalpost jp = new Journalpost();
            //Referanse til saksmappe 
            jp.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = mappeReferanse.systemID } };

            jp.journalposttype = new Journalposttype() { kodeverdi = "N" }; //Konfigureres og hentes fra kodeliste
            jp.tittel = "Innholdsbeskrivelse internt notat med oppfølging";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            //jp.referanseAvskrivninger = new AvskrivningListe();
            //Avskrivning avskr = new Avskrivning();
            //avskr.avskrivningsmaate = new Avskrivningsmaate() { kodeverdi = "TE" };//Konfigureres og hentes fra kodeliste
            //jp.referanseAvskrivninger.Add(avskr);


            //Til saksbehandler som skal følge opp saken
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og hentes fra kodeliste
            mott.behandlingsansvarlig = "1"; // Behandlingsansvarlig
            mott.administrativEnhetInit = "TEKN.BYG"; //Enhetsforkortelse - Konfigureres og hentes fra kodeliste 
            mott.saksbehandlerInit = "SB"; // Initialer saksbehandler - Konfigureres og hentes fra kodeliste 
            KorrespondansepartListe.Add(mott);
            jp.korrespondansepart = KorrespondansepartListe.ToArray();

            var nyJournalpost = arkivClient.NyJournalpost(jp, kontekst);
            return nyJournalpost;
        }
    }
}
