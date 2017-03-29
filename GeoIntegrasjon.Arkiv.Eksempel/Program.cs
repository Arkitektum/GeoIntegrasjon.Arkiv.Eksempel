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

            //ArkivInnsynEksempler.TestInnsyn();

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

            //Inngående brev tatt til etterretning fra organisasjon
            Saksmappe nyBasisMappeOrg = NyBasisSaksmappe(arkivClient, kontekst);
            Journalpost nyInngJournalpostOrg = RegistrerInngåendeJournalpostFraOrganisasjon(arkivClient, kontekst, nyBasisMappeOrg);
            NyttDokument(arkivClient, kontekst, nyInngJournalpostOrg);
            LukkJournalpost(arkivClient, kontekst, nyInngJournalpostOrg);
            AvsluttMappe(arkivClient, kontekst, nyBasisMappeOrg);

            //Starte saksmappe
            Saksmappe nyBasisMappe = NyBasisSaksmappe(arkivClient, kontekst);
            //Inngående brev
            Journalpost nyInngJournalpost = RegistrerInngåendeJournalpost(arkivClient, kontekst, nyBasisMappe);
            NyttDokument(arkivClient, kontekst, nyInngJournalpost);
            LukkJournalpost(arkivClient, kontekst, nyInngJournalpost);
            //Svar på inngående brev
            Journalpost nyUtgJournalpost = RegistrerUtgåendeJournalpost(arkivClient, kontekst, nyBasisMappe, nyInngJournalpost);
            NyttDokument(arkivClient, kontekst, nyUtgJournalpost);
            EkspederJournalpost(arkivClient, kontekst, nyUtgJournalpost);
            LukkJournalpost(arkivClient, kontekst, nyUtgJournalpost);
            //Avslutt saksmappe
            AvsluttMappe(arkivClient, kontekst, nyBasisMappe);

            //Start ny saksmappe
            Saksmappe nyEnkelmappe = NyEnkelSaksmappe(arkivClient, kontekst);
            //Registrere internt notat
            Journalpost nyttNotat = RegistrerInterntNotat(arkivClient, kontekst, nyEnkelmappe);
            NyttDokument(arkivClient, kontekst, nyttNotat);
            Journalpost nyJpUOff = RegistrerSøknadUntattOffentlighet(arkivClient, kontekst, nyEnkelmappe);

            // Hent egne journalposter under arbeid
            var underarbeid = arkivClient.FinnJournalposterUnderArbeid(new Ansvarlig() { eier = AnsvarligEnum.EGEN }, false, false, true, false, kontekst);

            // Hent egne restanser (ubesvarte)
            var restanser = arkivClient.FinnJournalpostRestanser(new Ansvarlig() { eier = AnsvarligEnum.EGEN }, false, false, true, false, kontekst);

        }
        private static void EkspederJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Journalpost jpost)
        {
            arkivClient.OppdaterJournalpostStatus(new Journalstatus() { kodeverdi = "E" }, jpost.journalnummer, kontekst);
        }
        private static void LukkJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Journalpost jpost)
        {
            arkivClient.OppdaterJournalpostStatus(new Journalstatus() { kodeverdi = "J" }, jpost.journalnummer, kontekst);
        }

        private static void AvsluttMappe(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappe)
        {
            arkivClient.OppdaterMappeStatus(new Saksstatus() { kodeverdi = "A" }, mappe.saksnr, kontekst);
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
            mappe2.mappetype = new Mappetype() { kodeverdi = "BYG.ENKEL" }; //Hent lovlige kodeverdier fra HentKodeliste
            mappe2.referanseArkivdel = new Arkivdel() { kodeverdi = "BYGG" }; //Hent lovlige kodeverdier fra HentKodeliste

            var nyMappe2 = arkivClient.NySaksmappe(mappe2, kontekst);
            return nyMappe2;
        }

        private static Saksmappe NyEnkelSaksmappe(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst)
        {
            //Sjekk om referanseoppsett støttes av arkivleverandør og om det kan brukes for å sette standardverdier som feks Arkivdel, klassering, mappetype, mm

            //Ny saksmappe, minimumsdata
            Saksmappe mappe = new Saksmappe();
            mappe.tittel = "Tittel på enkel notat sak";
       
            var nyMappe = arkivClient.NySaksmappe(mappe, kontekst);
            return nyMappe;
        }

        private static Journalpost RegistrerUtgåendeJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe referanseMappe, Journalpost inngJournalpost)
        {
            //************************************************************
            // Ny journalpost, Utgående dokument
            Journalpost jpU = new Journalpost();
            //Referanse til saksmappe
            jpU.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = referanseMappe.systemID } };
            jpU.saksnr = referanseMappe.saksnr; //Feil i journalpost "Ny journalpost": Feltet sak-ID må ha verdi
            jpU.journalposttype = new Journalposttype() { kodeverdi = "U" }; //Konfigureres og kan hentes fra kodeliste
            jpU.journalstatus = new Journalstatus() { kodeverdi = "R" }; //Konfigureres og kan hentes fra kodeliste
            jpU.tittel = "Tittel på det utgående brevet";

            EksternNoekkel ekstnok = new EksternNoekkel();
            ekstnok.fagsystem = "INNSYN";  //Hent lovlige kodeverdier fra HentKodeliste
            ekstnok.noekkel = "12345-1";
            jpU.referanseEksternNoekkel = ekstnok;

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og kan hentes fra kodeliste
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

            List<Avskrivning> avskrivninger = new List<Avskrivning>();
            Avskrivning avskrUtg = new Avskrivning();
            avskrUtg.avskrivningsmaate = new Avskrivningsmaate() { kodeverdi = "BU", kodebeskrivelse = "Besvart med brev" };//Konfigureres og kan hentes fra kodeliste
            avskrUtg.referanseAvskriverJournalnummer = inngJournalpost.journalnummer;
            avskrivninger.Add(avskrUtg);
            jpU.referanseAvskrivninger = avskrivninger.ToArray();


            var nyUtgJournalpost = arkivClient.NyJournalpost(jpU, kontekst);

            return nyUtgJournalpost;
        }

        

        private static Journalpost RegistrerSøknadUntattOffentlighet(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappeReferanse)
        {
            // Ny journalpost, Inngående dokument
            Journalpost jp = new Journalpost();
            //Referanse til saksmappe
            jp.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = mappeReferanse.systemID } };

            jp.journalposttype = new Journalposttype() { kodeverdi = "I" }; //Konfigureres og kan hentes fra kodeliste
            jp.tittel = "Søknad om hjelpemidler Kari Nordmann";
            jp.offentligTittel = "Søknad om hjelpemidler";
            jp.skjermetTittel = true; // NB! Må settes for å skjerme tittel
            jp.skjermetTittelSpecified = true;
            Skjerming skjerming = new Skjerming();
            skjerming.tilgangsrestriksjon = new Tilgangsrestriksjon() { kodeverdi = "13" }; //Konfigureres og kan hentes fra kodeliste
            skjerming.skjermingshjemmel = "Opplysningar som er underlagde teieplikt";
            skjerming.skjermingOpphoererAksjon = new SkjermingOpphorerAksjon() { kodeverdi = "G" }; //Konfigureres og kan hentes fra kodeliste
            skjerming.skjermingOpphoererDato = DateTime.Now.AddYears(60); // Dagens dato + 60 år
            jp.skjerming = skjerming;
            
            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            Korrespondansepart avs = new Korrespondansepart();
            avs.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Avsender", kodebeskrivelse = "Avsender" };//Konfigureres og kan hentes fra kodeliste
            avs.skjermetKorrespondansepart = true; // Avsender skal skjermes
            avs.skjermetKorrespondansepartSpecified = true;
            Person p = new Person();
            p.personid = new Personidentifikator();
            p.personid.personidentifikatorNr = "12345678910";
            p.personid.personidentifikatorType = new PersonidentifikatorType() { kodeverdi = "F" };
            p.navn = "Kari Nordmann";
            List<EnkelAdresse> EnkelAdresseListe = new List<EnkelAdresse>();
            EnkelAdresse adresse = new EnkelAdresse();
            adresse.adresselinje1 = "Storgata 4A";
            adresse.postadresse = new PostadministrativeOmraader() { postnummer = "3801", poststed = "Bø" };
            adresse.landkode = new Landkode() { kodeverdi = "NO" };
            EnkelAdresseListe.Add(adresse);
            p.adresser = EnkelAdresseListe.ToArray();

            avs.Kontakt = p;
            KorrespondansepartListe.Add(avs);
            jp.korrespondansepart = KorrespondansepartListe.ToArray();

            var nyInngJournalpost = arkivClient.NyJournalpost(jp, kontekst);
            return nyInngJournalpost;
        }

        private static Journalpost RegistrerInngåendeJournalpost(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappeReferanse)
        {
            // Ny journalpost, Inngående dokument
            Journalpost jp = new Journalpost();
            //Referanse til saksmappe
            jp.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = mappeReferanse.systemID } };

            jp.journalposttype = new Journalposttype() { kodeverdi = "I" }; //Konfigureres og kan hentes fra kodeliste
            jp.journalstatus = new Journalstatus() { kodeverdi = "M" }; //Konfigureres og kan hentes fra kodeliste
            jp.tittel = "Tittel på det mottatte brevet";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            Korrespondansepart avs = new Korrespondansepart();
            avs.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Avsender", kodebeskrivelse = "Avsender" };//Konfigureres og kan hentes fra kodeliste
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
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og kan hentes fra kodeliste
            mott.behandlingsansvarlig = "1"; // Behandlingsansvarlig
            //mott.administrativEnhetInit = "TEKN.BYG"; //Enhetsforkortelse - Konfigureres
            mott.saksbehandlerInit = "LIV"; // Initialer saksbehandler - Konfigureres
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
            jp.saksnr = mappeReferanse.saksnr; //Feil i journalpost "Ny journalpost": Feltet sak-ID må ha verdi

            jp.journalposttype = new Journalposttype() { kodeverdi = "N" }; //Konfigureres og kan hentes fra kodeliste
            jp.tittel = "Innholdsbeskrivelse internt notat med oppfølging";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            //Fra saksbehandler - kunne vært pålogget bruker?
            Korrespondansepart avs = new Korrespondansepart();
            avs.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Avsender", kodebeskrivelse = "Avsender" }; //Konfigureres og kan hentes fra kodeliste
            avs.behandlingsansvarlig = "1"; // Behandlingsansvarlig
            avs.saksbehandlerInit = "ar1"; // Initialer saksbehandler - Konfigureres
            KorrespondansepartListe.Add(avs);

            //Til saksbehandler som skal følge opp saken
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og hentes fra kodeliste
            
            //mott.administrativEnhetInit = "TEKN.BYG"; //Enhetsforkortelse - Konfigureres 
            mott.saksbehandlerInit = "LIV"; // Initialer saksbehandler - Konfigureres 
            KorrespondansepartListe.Add(mott);
            jp.korrespondansepart = KorrespondansepartListe.ToArray();

            var nyJournalpost = arkivClient.NyJournalpost(jp, kontekst);
            return nyJournalpost;
        }

        private static Journalpost RegistrerInngåendeJournalpostFraOrganisasjon(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Saksmappe mappeReferanse)
        {
            // Ny journalpost, Inngående dokument
            Journalpost jp = new Journalpost();
            //Referanse til saksmappe
            jp.referanseSakSystemID = new SakSystemId() { systemID = new SystemID() { id = mappeReferanse.systemID } };

            jp.journalposttype = new Journalposttype() { kodeverdi = "I" }; //Konfigureres og kan hentes fra kodeliste
            jp.journalstatus = new Journalstatus() { kodeverdi = "M" }; //Konfigureres og kan hentes fra kodeliste
            jp.tittel = "Tittel på det mottatte brevet";

            List<Korrespondansepart> KorrespondansepartListe = new List<Korrespondansepart>();

            Korrespondansepart avs = new Korrespondansepart();
            avs.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Avsender", kodebeskrivelse = "Avsender" };//Konfigureres og kan hentes fra kodeliste
            Organisasjon org = new Organisasjon();
            org.organisasjonsnummer = "123456789";
            org.navn = "Bedriften AS";
            List<EnkelAdresse> EnkelAdresseListe = new List<EnkelAdresse>();
            EnkelAdresse adresse = new EnkelAdresse();
            adresse.adresselinje1 = "Storgata 11";
            adresse.postadresse = new PostadministrativeOmraader() { postnummer = "3801", poststed = "Bø" };
            adresse.landkode = new Landkode() { kodeverdi = "NO" };
            EnkelAdresseListe.Add(adresse);
            org.adresser = EnkelAdresseListe.ToArray();

            avs.Kontakt = org;
            KorrespondansepartListe.Add(avs);

            //Angi saksbehandler hvis denne er annen enn pålogget bruker
            Korrespondansepart mott = new Korrespondansepart();
            mott.korrespondanseparttype = new Korrespondanseparttype() { kodeverdi = "Mottaker", kodebeskrivelse = "Mottaker" }; //Konfigureres og kan hentes fra kodeliste
            mott.behandlingsansvarlig = "1"; // Behandlingsansvarlig
            //mott.administrativEnhetInit = "TEKN.BYG"; //Enhetsforkortelse - Konfigureres
            mott.saksbehandlerInit = "LIV"; // Initialer saksbehandler - Konfigureres
            KorrespondansepartListe.Add(mott);
            jp.korrespondansepart = KorrespondansepartListe.ToArray();

            List<Avskrivning> avskrivninger = new List<Avskrivning>();
            Avskrivning avskrUtg = new Avskrivning();
            avskrUtg.avskrivningsmaate = new Avskrivningsmaate() { kodeverdi = "TE", kodebeskrivelse = "Tatt til etterretning" };//Konfigureres og kan hentes fra kodeliste
            avskrivninger.Add(avskrUtg);
            jp.referanseAvskrivninger = avskrivninger.ToArray();

            var nyInngJournalpost = arkivClient.NyJournalpost(jp, kontekst);
            return nyInngJournalpost;
        }

        private static void NyttDokument(SakArkivOppdateringPortClient arkivClient, ArkivKontekst kontekst, Journalpost referanseJournalpost)
        {

            //************************************************************
            // Nytt dokument
            Dokument dok = new Dokument();
            dok.referanseJournalpostSystemID = referanseJournalpost.systemID;
            dok.tittel = "dokumenttittel";
            dok.dokumenttype = new Dokumenttype() { kodeverdi = "VEDTAK" }; //Konfigureres og kan hentes fra kodeliste
            dok.dokumentstatus = new Dokumentstatus() { kodeverdi = "F" }; //Konfigureres og kan hentes fra kodeliste
            dok.variantformat = new Variantformat() { kodeverdi = "P" }; //Konfigureres og kan hentes fra kodeliste
            dok.format = new Format() { kodeverdi = "RA-PDF" }; //Konfigureres og kan hentes fra kodeliste
            var filinnh = new Filinnhold();
            filinnh.filnavn = "Arkiverbart_brev.pdf";
            filinnh.mimeType = "application/pdf";
            filinnh.base64 = System.IO.File.ReadAllBytes("Arkiverbart_brev.pdf");
            dok.Fil = filinnh;


            var nyDok = arkivClient.NyDokument(dok, false, kontekst);
        }
    }
}
