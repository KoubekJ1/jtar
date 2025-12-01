# JTAR
<p>Software napsaný v C# sloužící pro kompresi souborů do .tar.zstd formátu tvořený v rámci školního projektu.</p>
<p>Název školy: SPŠE Ječná</p>
<p>Datum poslední úpravy: 24.11.2025</p>

## Autor
Jan Koubek C4b<br>
koubek@spsejecna.cz<br>
Tvořeno v listopadu 2025<br>

## Stažení
<p>Stáhnout release či zdrojový kód z následujícího repozitáře: https://github.com/KoubekJ1/jtar</p>
<p>Momentální verze: 1.0.0</p>

## Použití
<p>Spusťte s vhodnými spouštěcími argumenty</p>
<p>Syntax: **jtar** [ -o *OUTPUT-PATH* ] [ -t *THREAD-COUNT* ] [ -n ] [ -d ] *FILEPATH1 FILEPATH2 ...*</p>

Jedinným povinným argumentem je cesta k jednomu nebo více vstupním souborům.

### Možnosti
**-o, --output**
- Cesta výsledného souboru

**-t, --thread-count**
- Specifikace počtu vláken použitých pro kompresi (výchozí: počet logických vláken CPU - 1)

**-n, --no-compress**
- Nepoužít kompresi, pouze vytvořit archiv .tar

**-d, --debug**
- Zapnout režim ladění (více výstupů do konzole)

### Chybové stavy
- Pokud není zadaná žádná vstupní cesta, program skončí s chybovým kódem 1
- Pokud je libovolná vstupní cesta neplatná, program vypíše chybovou hlášku, ale pokračuje ve zpracování ostatních platných cest

## Implementace
Program byl realizován v jazyce C# pomocí .NET 8.0 SDK s velikým důrazem na použití vláken. Současně běží 4 procesy, které na sebe pevně navazují:
- FileSeeker: Hledání podsouborů ve složkách specifikovaných ve vstupních argumentech
- FileLoader: Načítání nalezených souborů, vytvoření TAR hlaviček a rozdělení souborů na bloky (chunky)
- ChunkCompressor: Komprese načtených bloků (chunků) pomocí specifikovaného algoritmu
- FileOutput: Zápis zkomprimovaných bloků (chunků) ve vhodném pořadí do výsledného souboru
<p>Všechny tyto procesy jsou spojeny pomocí třídy CompressionContext.</p>

### Použité knihovny třetích stran
- ZstdNet (skbkontur): https://github.com/skbkontur/ZstdNet
    - Použito pro kompresi výsledného souboru
    - Použité rozhraní: Třída Zstdnet.Compressor

- System.CommandLine (Microsoft): https://github.com/dotnet/command-line-api
    - Použito pro efektivní zpracování vstupních argumentů
    - Použité rozhraní: Třídy RootCommand, Option, Argument

## Licence
Distribuováno pod licencí GNU GPLv3