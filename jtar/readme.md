# JTAR
<p>Software napsaný v C# sloužící pro kompresi souborů do .tar.zstd formátu tvořený v rámci školního projektu.</p>
<p>Název školy: SPŠE Ječná</p>
<p>Datum poslední úpravy: 1.12.2025</p>

## Autor
Jan Koubek C4b<br>
koubek@spsejecna.cz<br>
Tvořeno v listopadu 2025<br>

## O programu
<p>Program slouží pro archivaci souborů do archivu formátu TAR specifikace USTAR (POSIX specifikace uvedena v normách POSIX.1-1988 a POSIX.1-2001). Na tento archiv se následně aplikuje komprese formátu Zstandard, vytvořený společností Facebook, není-li jinak specifikováno ve spouštěcích argumentech.

Archiv je možné vytvořit i bez použití komprese.
</p>

## Požadavky na uživatele
- Nainstalován .NET SDK verze 8.0 nebo vyšší
- Základní znalost práce s konzolovým rozhraním

## Stažení
<p>Stáhnout zdrojový kód z následujícího repozitáře: https://github.com/KoubekJ1/portfolio</p>
<p>Momentální verze: 1.0.1</p>

## Použití
### Předem zkompilovaný binární soubor
<p>Spusťte s vhodnými spouštěcími argumenty</p>
<p>Syntax: **jtar** [ -o *OUTPUT-PATH* ] [ -t *THREAD-COUNT* ] [ -n ] [ -d ] *FILEPATH1 FILEPATH2 ...*</p>

### Debug
<p>Pro spuštění ve vývojovém prostředí je třeba mít nainstalován .NET SDK verze 8.0 nebo výše. Následně je potřeba stáhnout zdrojový kód z repozitáře, přesunout se v konzoli do adresáře portfolio/jtar/Jtar a použít příkaz "dotnet run" (+ vstupní parametry). Veškeré závislosti se nainstalují sami.</p>
<p>Syntax: **dotnet run** [ -o *OUTPUT-PATH* ] [ -t *THREAD-COUNT* ] [ -n ] [ -d ] *FILEPATH1 FILEPATH2 ...*</p>

### Parametry
<p>Konfigurace, resp. parametry pro spuštění se uvádí formou argumentů uvedených v konzoli za názvem programu.</p>

<p>Jedinným povinným argumentem je cesta k jednomu nebo více vstupním souborům či adresářům, které budou archivovány ve výsledném souboru.</p>

<p>Cesty musí být relativní, nikoliv absolutní</p>

#### Volitelné parametry
**-o, --output**
- Cesta výsledného souboru

**-t, --thread-count**
- Specifikace počtu vláken použitých pro kompresi (výchozí: počet logických vláken CPU - 1)

**-n, --no-compress**
- Nepoužít kompresi, pouze vytvořit archiv .tar

**-d, --debug**
- Zapnout tisk skrytých výstupů do konzole

### Chybové stavy
- Pokud není zadaná žádná platná vstupní cesta, program skončí s chybovým kódem 1
- Pokud není zadaná žádná vstupní cesta, program skončí s chybovým kódem 1
- V případě zadání alespoň 1 platné cesty, se za každou vadnou cestu vypíše chybová hláška, avšak pokračuje se ve zpracovávání ostatních platných cest
- Pokud je jako název výstupního souboru uvedena cesta k existujícímu adresáři, program skončí s chybovým kódem 1
- Pokud je jako název výstupního souboru uvedena cesta spadající pod neexistující adresář, program skončí s chybovým kódem 1

## Implementace
Program byl realizován v jazyce C# pomocí .NET 8.0 SDK s velikým důrazem na použití vláken. Současně běží 4 procesy, které na sebe pevně navazují:
- FileSeeker: Hledání podsouborů ve složkách specifikovaných ve vstupních argumentech
- FileLoader: Načítání nalezených souborů, vytvoření TAR hlaviček a rozdělení souborů na bloky (chunky)
- ChunkCompressor: Komprese načtených bloků (chunků) pomocí specifikovaného algoritmu
- FileOutput: Zápis zkomprimovaných bloků (chunků) ve vhodném pořadí do výsledného souboru

Každá vrstva má svojí manager třídu a worker třídu. Manager třída tvoří instance worker třídy.

Worker třídy používají pro komunikaci s ostatními komponentami vláknově bezpečné kolekce.

<p>Všechny manager objekty jsou použity ve třídě CompressionContext, která orchestruje veškeré konané činnosti.

CompressionContext spouští činnosti na každé úrovni asynchronně.</p>

![Class diagram](https://github.com/KoubekJ1/portfolio/blob/main/jtar/docs/classes.jpg "Class diagram")

### Použité knihovny třetích stran
- ZstdSharp (oleg-st): https://github.com/oleg-st/ZstdSharp
    - Použito pro kompresi výsledného souboru
    - Použité rozhraní: Třída Zstdnet.Compressor

- System.CommandLine (Microsoft): https://github.com/dotnet/command-line-api
    - Použito pro efektivní zpracování vstupních argumentů
    - Použité rozhraní: Třídy RootCommand, Option, Argument

## Testování
<p>Testování programu se provádí formou unit testů, které testují funkčnost klíčových komponent, ze kterých se zdrojový kód skládá. Tyto unit testy zaručují správné fungování programu. Pro spuštění unit testů je potřeba jít v konzoli do adresáře PackagingTests a spustit příkaz "dotnet test". Při poslední úpravě zdrojového kódu byly výsledky všech unit testů úspěšné.</p>

## Licence
Distribuováno pod licencí GNU GPLv3
