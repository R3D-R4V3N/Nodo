using System.Collections.Generic;

namespace Rise.Client.Chats.Components;

public static class AlertCatalog
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } = new List<AlertPrompt.AlertReason>
    {
        new("Pesten", Pesten),
        new("Uitsluiten", Uitsluiten),
        new("Ongepast taalgebruik", Taalgebruik),
        new("Ander probleem", Taalgebruik)
    };

    private const string Pesten = """
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="h-6 w-6" fill="currentColor" aria-hidden="true">
  <path d="M7 12a4 4 0 1 1 0-8 4 4 0 0 1 0 8Z" />
  <path d="M17 13a3.5 3.5 0 1 0-3.5-3.5A3.5 3.5 0 0 0 17 13Z" />
  <path d="M3 19.25A4.25 4.25 0 0 1 7.25 15h1.5A4.25 4.25 0 0 1 13 19.25V20H3z" />
  <path d="M13.5 20v-.75A3.25 3.25 0 0 1 16.75 16H21v1.5h-3.5A1.75 1.75 0 0 0 15.75 19v1z" />
</svg>
""";

    private const string Uitsluiten = """
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="h-6 w-6" fill="none" stroke="currentColor" stroke-width="1.75" aria-hidden="true">
  <circle cx="12" cy="12" r="8.25" />
  <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 8.25 15.75 15.75" />
</svg>
""";

    private const string Taalgebruik = """
<svg xmlns="http://www.w3.org/2000/svg" version="1.0" viewBox="0 0 512.000000 512.000000" preserveAspectRatio="xMidYMid meet" class="h-6 w-6" fill="currentColor">

<g transform="translate(0.000000,512.000000) scale(0.100000,-0.100000)" fill="currentColor" stroke="none">
<path d="M1186 5109 c-508 -67 -952 -435 -1116 -927 -51 -155 -65 -241 -65 -422 0 -120 5 -188 18 -250 117 -554 544 -974 1095 -1077 l92 -17 0 -411 0 -410 408 407 407 407 960 4 c934 3 962 4 1050 24 427 10"/>
<path d="M1210 3760 l0 -150 150 0 150 0 0 150 0 150 -150 0 -150 0 0 -150z"/>
<path d="M3212 1791 c-101 -35 -172 -116 -194 -224 -23 -108 22 -227 110 -294 72 -55 119 -63 366 -63 l221 0 323 -215 322 -215 0 214 0 215 268 3 267 3 57 28 c107 53 163 144 163 267 0 123 -56 214 -163 267"/>
<path d="M202 961 c-101 -35 -172 -116 -194 -224 -23 -108 22 -227 110 -294 74 -56 116 -63 394 -63 l248 0 2 -189 3 -189 319 188 320 189 240 3 c241 3 241 3 298 31 107 53 163 144 163 267 0 123 -56 214 -163 267"/>
</g>
</svg>
""";
}
