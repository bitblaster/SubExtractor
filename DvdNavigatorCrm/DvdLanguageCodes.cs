/*
 * Copyright (C) 2007, 2008 Chris Meadowcroft <crmeadowcroft@gmail.com>
 *
 * This file is part of CMPlayer, a free video player.
 * See http://sourceforge.net/projects/crmplayer for updates.
 *
 * CMPlayer is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * CMPlayer is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace DvdNavigatorCrm
{
	public class DvdLanguageCodes
	{
		static Dictionary<string, string> languageCode = new Dictionary<string, string>();
        static Dictionary<string, string> iso639Code = new Dictionary<string, string>();

		static DvdLanguageCodes()
		{
            iso639Code["en"] = "eng";
            iso639Code["fr"] = "fre";
            iso639Code["de"] = "ger";
            iso639Code["ja"] = "jpn";
            iso639Code["zh"] = "chi";
            iso639Code["it"] = "ita";
            iso639Code["mn"] = "mon";
            iso639Code["nl"] = "dut";
            iso639Code["fa"] = "per";
            iso639Code["pl"] = "pol";
            iso639Code["he"] = "heb";
            iso639Code["ru"] = "rus";
            iso639Code["sv"] = "swe";
            iso639Code["hi"] = "hin";

			languageCode["ab"] = "Abkhazian";
			languageCode["aa"] = "Afar";
			languageCode["af"] = "Afrikaans";
			languageCode["sq"] = "Albanian";
			languageCode["am"] = "Amharic, Ameharic";
			languageCode["ar"] = "Arabic";
			languageCode["hy"] = "Armenian";
			languageCode["as"] = "Assamese";
			languageCode["ay"] = "Aymara";
			languageCode["az"] = "Azerbaijani";
			languageCode["ba"] = "Bashkir";
			languageCode["eu"] = "Basque";
			languageCode["bn"] = "Bengali, Bangla";
			languageCode["dz"] = "Bhutani";
			languageCode["bh"] = "Bihari";
			languageCode["bi"] = "Bislama";
			languageCode["br"] = "Breton";
			languageCode["bg"] = "Bulgarian";
			languageCode["my"] = "Burmese";
			languageCode["be"] = "Byelorussian";
			languageCode["km"] = "Cambodian";
			languageCode["ca"] = "Catalan";
			languageCode["zh"] = "Chinese";
			languageCode["co"] = "Corsican";
			languageCode["hr"] = "Hrvatski (Croatian)";
			languageCode["cs"] = "Czech (Ceske)";
			languageCode["da"] = "Dansk (Danish)";
			languageCode["nl"] = "Dutch (Nederlands)";
			languageCode["en"] = "English";
			languageCode["eo"] = "Esperanto";
			languageCode["et"] = "Estonian";
			languageCode["fo"] = "Faroese";
			languageCode["fj"] = "Fiji";
			languageCode["fi"] = "Finnish";
			languageCode["fr"] = "French";
			languageCode["fy"] = "Frisian";
			languageCode["gl"] = "Galician";
			languageCode["ka"] = "Georgian";
			languageCode["de"] = "Deutsch (German)";
			languageCode["el"] = "Greek";
			languageCode["kl"] = "Greenlandic";
			languageCode["gn"] = "Guarani";
			languageCode["gu"] = "Gujarati";
			languageCode["ha"] = "Hausa";
			languageCode["iw"] = "Hebrew";
			languageCode["hi"] = "Hindi";
			languageCode["hu"] = "Hungarian";
			languageCode["is"] = "Islenka (Icelandic)";
			languageCode["in"] = "Indonesian";
			languageCode["ia"] = "Interlingua";
			languageCode["ie"] = "Interlingue";
			languageCode["ik"] = "Inupiak";
			languageCode["ga"] = "Irish";
			languageCode["it"] = "Italian";
			languageCode["ja"] = "Japanese";
			languageCode["jw"] = "Javanese";
			languageCode["kn"] = "Kannada";
			languageCode["ks"] = "Kashmiri";
			languageCode["kk"] = "Kazakh";
			languageCode["rw"] = "Kinyarwanda";
			languageCode["ky"] = "Kirghiz";
			languageCode["rn"] = "Kirundi";
			languageCode["ko"] = "Korean";
			languageCode["ku"] = "Kurdish";
			languageCode["lo"] = "Laothian";
			languageCode["la"] = "Latin";
			languageCode["lv"] = "Latvian, Lettish";
			languageCode["ln"] = "Lingala";
			languageCode["lt"] = "Lithuanian";
			languageCode["mk"] = "Macedonian";
			languageCode["mg"] = "Malagasy";
			languageCode["ms"] = "Malay";
			languageCode["ml"] = "Malayalam";
			languageCode["mt"] = "Maltese";
			languageCode["mi"] = "Maori";
			languageCode["mr"] = "Marathi";
			languageCode["mo"] = "Moldavian";
			languageCode["mn"] = "Mongolian";
			languageCode["na"] = "Nauru";
			languageCode["ne"] = "Nepali";
			languageCode["no"] = "Norwegian (Norsk)";
			languageCode["oc"] = "Occitan";
			languageCode["or"] = "Oriya";
			languageCode["om"] = "Afan (Oromo)";
			languageCode["pa"] = "Panjabi";
			languageCode["ps"] = "Pashto, Pushto";
			languageCode["fa"] = "Persian";
			languageCode["pl"] = "Polish";
			languageCode["pt"] = "Portuguese";
			languageCode["qu"] = "Quechua";
			languageCode["rm"] = "Rhaeto-Romance";
			languageCode["ro"] = "Romanian";
			languageCode["ru"] = "Russian";
			languageCode["sm"] = "Samoan";
			languageCode["sg"] = "Sangho";
			languageCode["sa"] = "Sanskrit";
			languageCode["gd"] = "Scots Gaelic";
			languageCode["sh"] = "Serbo-Crotain";
			languageCode["st"] = "Sesotho";
			languageCode["sr"] = "Serbian";
			languageCode["tn"] = "Setswana";
			languageCode["sn"] = "Shona";
			languageCode["sd"] = "Sindhi";
			languageCode["si"] = "Singhalese";
			languageCode["ss"] = "Siswati";
			languageCode["sk"] = "Slovak";
			languageCode["sl"] = "Slovenian";
			languageCode["so"] = "Somali";
			languageCode["es"] = "Spanish (Espanol)";
			languageCode["su"] = "Sundanese";
			languageCode["sw"] = "Swahili";
			languageCode["sv"] = "Svenska (Swedish)";
			languageCode["tl"] = "Tagalog";
			languageCode["tg"] = "Tajik";
			languageCode["tt"] = "Tatar";
			languageCode["ta"] = "Tamil";
			languageCode["te"] = "Telugu";
			languageCode["th"] = "Thai";
			languageCode["bo"] = "Tibetian";
			languageCode["ti"] = "Tigrinya";
			languageCode["to"] = "Tonga";
			languageCode["ts"] = "Tsonga";
			languageCode["tr"] = "Turkish";
			languageCode["tk"] = "Turkmen";
			languageCode["tw"] = "Twi";
			languageCode["uk"] = "Ukranian";
			languageCode["ur"] = "Urdu";
			languageCode["uz"] = "Uzbek";
			languageCode["vi"] = "Vietnamese";
			languageCode["vo"] = "Volapuk";
			languageCode["cy"] = "Welsh";
			languageCode["wo"] = "Wolof";
			languageCode["ji"] = "Yiddish";
			languageCode["yo"] = "Yoruba";
			languageCode["xh"] = "Xhosa";
			languageCode["zu"] = "Zulu";
		}

		public static string GetLanguageText(string code)
		{
			if(string.IsNullOrEmpty(code) || (code[0] == '\0'))
			{
				return string.Empty;
			}
			string language;
			if(!languageCode.TryGetValue(code.ToLower(), out language))
			{
				language = code;
			}
			return language;
		}

        public static string GetLanguage639Code(string code)
        {
            if(code == null)
            {
                return string.Empty;
            }
            string language;
            if(!iso639Code.TryGetValue(code.ToLower(), out language))
            {
                language = code + " ";
            }
            return language;
        }
    }
}
