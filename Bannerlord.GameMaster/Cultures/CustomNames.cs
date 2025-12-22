using System.Collections.Generic;

namespace Bannerlord.GameMaster.Cultures
{
    /// <summary>
    /// Custom names representing each culture for clans and heroes
    /// </summary>
    public static class CustomNames
    {
        /// <summary>
        /// Cultural Male Hero Names
        /// </summary>
        public static readonly Dictionary<string, List<string>> MaleHeroNames = new Dictionary<string, List<string>>
    {
        /// MARK: Aserai - Male
        ["aserai"] = new List<string>
            {
                // Original set
                "Azhar", "Basir", "Darim", "Faisal", "Habib", "Jalal", "Kamal", "Malik",
                "Nadir", "Qadir", "Rashid", "Salim", "Tariq", "Walid", "Yazid", "Zafar",
                "Bahir", "Fahd", "Hakim", "Idris", "Kamil", "Latif", "Munir", "Nasir",
                "Rami", "Saif", "Talib", "Yasir", "Zahir", "Akram", "Bashir", "Fadil",
                // Additional powerful names
                "Asad", "Badr", "Dawud", "Faris", "Ghalib", "Hamza", "Jabir", "Khalid",
                "Majid", "Nasser", "Omar", "Qasim", "Rayyan", "Saqr", "Thabit", "Umar",
                "Waleed", "Yahya", "Zaki", "Abbas", "Bilal", "Harun", "Jafar", "Kadir",
                "Luqman", "Marwan", "Nabil", "Qutaiba", "Rafiq", "Shams", "Usman", "Wasim",
                "Ammar", "Basim", "Daud", "Farouk", "Ghazi", "Hadi", "Isa", "Jamil",
                "Kareem", "Mahdi", "Naeem", "Rayan", "Sadiq", "Tahir", "Yasin", "Zaid",
                "Adil", "Burhan", "Danish", "Fadl", "Hashim", "Ibrahim", "Jamal", "Karam",
                "Mansur", "Nabeel", "Rauf", "Sami", "Tariq", "Wali", "Yusuf", "Zaheer",
                "Amir", "Bakr", "Dinar", "Fahim", "Ghaith", "Husam", "Jawad", "Kasim"
            },
        /// MARK: Battania - Male
        ["battania"] = new List<string>
            {
                // Original set
                "Bran", "Cael", "Donal", "Eamon", "Fergus", "Gareth", "Hugh", "Ivar",
                "Kevan", "Loran", "Moran", "Niall", "Owen", "Piran", "Ronan", "Senan",
                "Torvin", "Ulric", "Varen", "Wren", "Alwyn", "Bevan", "Cormac", "Damon",
                "Edan", "Finn", "Gavin", "Iain", "Kael", "Lonan", "Merlin", "Nolan",
                // Additional Celtic/Gaelic names
                "Aiden", "Blair", "Connor", "Declan", "Erwin", "Finian", "Griffin", "Hagan",
                "Kellen", "Logan", "Maddox", "Nolan", "Oran", "Patrick", "Quinn", "Rory",
                "Shane", "Teague", "Vaughn", "Warren", "Brennan", "Conan", "Donovan", "Ewan",
                "Galvin", "Hogan", "Kieran", "Liam", "Morgan", "Neil", "Osgar", "Padraig",
                "Rowan", "Sean", "Tiernan", "Ultan", "Aengus", "Brion", "Caolan", "Diarmuid",
                "Fiachra", "Gethin", "Idris", "Kian", "Lorcan", "Murtagh", "Odhran", "Phelan",
                "Riordan", "Soren", "Tadhg", "Urien", "Albin", "Brogan", "Cadoc", "Drustan",
                "Emrys", "Fyfe", "Gruffydd", "Hugh", "Iolyn", "Kendrick", "Llewyn", "Madoc",
                "Nial", "Owain", "Pedr", "Rhys", "Sayer", "Taliesin", "Uilleam", "Wulfric"
            },
        /// MARK: Empire - Male
        ["empire"] = new List<string>
            {
                // Original set (reduced -os endings)
                "Agron", "Castos", "Daron", "Evron", "Gaius", "Heron", "Juron", "Kyros",
                "Marius", "Nero", "Orion", "Petros", "Quron", "Seron", "Titus", "Veron",
                "Aeron", "Beron", "Ceron", "Denos", "Euros", "Geros", "Horos", "Janos",
                "Keros", "Lenos", "Meros", "Noros", "Poros", "Renos", "Soros", "Teros",
                // More variety - Roman/Greek mix with different endings
                "Adrian", "Brutus", "Caesar", "Darius", "Flavius", "Hadrian", "Julius", "Lucian",
                "Marcus", "Aurelius", "Octavian", "Primus", "Quintus", "Remus", "Scipio", "Trajan",
                "Valerian", "Augustus", "Corvus", "Drusus", "Felix", "Gallus", "Horatius", "Magnus",
                "Maximus", "Severus", "Tacitus", "Victor", "Antonius", "Cassius", "Decimus", "Gaius",
                "Lucius", "Appius", "Balbus", "Claudius", "Demetrius", "Erasmus", "Fabius", "Gregorius",
                "Hektor", "Justus", "Konstantinos", "Leonidas", "Marcellus", "Nikias", "Octavius", "Paulus",
                "Quintilian", "Regulus", "Septimus", "Tiberius", "Valentinus", "Alexios", "Basileios", "Chrysanthos",
                "Dion", "Eugenios", "Fotios", "Gavriil", "Herakles", "Iason", "Kosmas", "Lycurgus",
                "Makarios", "Nestor", "Odysseus", "Philippos", "Romanos", "Stephanos", "Theodoros", "Vasilis"
            },
        /// MARK: Khuzait - Male
        ["khuzait"] = new List<string>
            {
                // Original set
                "Baatar", "Chagan", "Dagan", "Erdene", "Ganbat", "Hulagu", "Jebe", "Kublai",
                "Mongke", "Nogai", "Orda", "Qasar", "Subotai", "Temuge", "Ulaanbar", "Yesugei",
                "Arban", "Batu", "Chagatai", "Genghis", "Hadan", "Jochi", "Kadan", "Munglik",
                "Nayan", "Ogodai", "Sorgan", "Tumen", "Ulagan", "Bayan", "Chilaun", "Dorbei",
                // Additional Mongol/Turkic names
                "Altan", "Bataar", "Chingis", "Dorji", "Erden", "Galdan", "Hulan", "Jibek",
                "Khulan", "Ligdan", "Mongol", "Naiman", "Ogedei", "Qutlugh", "Temur", "Uighur",
                "Yeke", "Altai", "Borte", "Chahar", "Duwa", "Esen", "Guyuk", "Hulegu",
                "Jagatai", "Khubilai", "Mangu", "Negudar", "Oirat", "Qaidu", "Rashid", "Sanjar",
                "Toghrul", "Uzbeg", "Baidar", "Chaghri", "Danishmend", "Eljigidei", "Ghazan", "Hulun",
                "Jani", "Kaidu", "Manggala", "Nogai", "Osman", "Qazan", "Sartak", "Turakina",
                "Ulus", "Berkut", "Cagan", "Delger", "Erke", "Ganzorig", "Haraqan", "Irgen",
                "Jelme", "Khadan", "Manlai", "Naranbaatar", "Otgon", "Purevdorj", "Sengge", "Targan",
                "Ulziit", "Bolor", "Chimeg", "Davaajav", "Enkhbayar", "Gansukh", "Khenbish", "Monkhbat"
            },
        /// MARK: Nord - Male
        ["nord"] = new List<string>
            {
                // Original set
                "Bjorn", "Dag", "Erik", "Finn", "Gunnar", "Harald", "Ivar", "Knut",
                "Leif", "Magnus", "Olaf", "Ragnar", "Sigurd", "Thorin", "Ulf", "Vidar",
                "Alrik", "Balder", "Egil", "Frode", "Grimr", "Halvard", "Jorund", "Ketil",
                "Orm", "Rolf", "Sven", "Torsten", "Ulf", "Varg", "Yngve", "Arngeir",
                // Additional Norse/Nordic names
                "Asbjorn", "Bjarni", "Einar", "Finn", "Gorm", "Hakon", "Ingvar", "Jorgen",
                "Kjartan", "Leifr", "Magni", "Njal", "Ove", "Ragnvald", "Sigvald", "Thorfinn",
                "Ulfric", "Viggo", "Arn", "Bard", "Eyvind", "Grim", "Hjalmar", "Isak",
                "Jarl", "Kol", "Lars", "Magne", "Njord", "Orn", "Roald", "Sture",
                "Thrand", "Ulf", "Vali", "Aslak", "Bergthor", "Einar", "Folke", "Gunnar",
                "Hrolf", "Ingolf", "Kjell", "Leif", "Mogens", "Nils", "Oskar", "Roar",
                "Skarde", "Thorfinn", "Ulrik", "Vigleik", "Asgeir", "Bragi", "Egill", "Floki",
                "Gudmund", "Havard", "Ivarr", "Kare", "Lodinn", "Modolf", "Nori", "Ottar",
                "Ravn", "Styrkar", "Tormod", "Vagn", "Aki", "Bodvar", "Eilif", "Geir"
            },
        /// MARK: Sturgia - Male
        ["sturgia"] = new List<string>
            {
                // Original set
                "Bogdan", "Dmitri", "Gregor", "Igor", "Kazimir", "Lazar", "Miro", "Nikolai",
                "Pavel", "Radoslav", "Stefan", "Tomislav", "Vadim", "Yuri", "Zlatan", "Boris",
                "Dragomir", "Goran", "Ivan", "Jaroslav", "Konstantin", "Leonid", "Milos", "Oleg",
                "Petar", "Roman", "Stanislav", "Timur", "Viktor", "Zoran", "Anatoly", "Bronislav",
                // Additional Slavic names
                "Alexei", "Branko", "Davor", "Emil", "Filip", "Gavril", "Hrvoje", "Ivo",
                "Jovan", "Kiril", "Luka", "Marko", "Nedeljko", "Ognjen", "Pavle", "Radomir",
                "Svyatoslav", "Taras", "Uros", "Velimir", "Andrei", "Borislav", "Danilo", "Evgeni",
                "Georgi", "Hristo", "Ilija", "Josip", "Kosta", "Ljubomir", "Milan", "Niko",
                "Ostoja", "Predrag", "Rajko", "Slobodan", "Teodor", "Vasilij", "Aleksandar", "Bojan",
                "Dusan", "Ervin", "Goran", "Hrvojе", "Ivica", "Janko", "Kresimir", "Lubomir",
                "Matej", "Nemanja", "Oleksandr", "Perica", "Ratko", "Srdjan", "Toma", "Vladislav",
                "Yaroslav", "Anto", "Blazenko", "Dalibor", "Emir", "Franjo", "Gradimir", "Hrvoj",
                "Ilija", "Jago", "Krunoslav", "Ljudevit", "Miljenko", "Nikola", "Ozren", "Prvoslav"
            },
        /// MARK: Vlandia - Male
        ["vlandia"] = new List<string>
            {
                // Original set
                "Aldric", "Bertrand", "Conrad", "Drogo", "Edmund", "Fulk", "Geoffrey", "Hugh",
                "Ingmar", "Jocelyn", "Lambert", "Milo", "Norbert", "Odo", "Philip", "Rainald",
                "Sigmund", "Thierry", "Ulrich", "Valeran", "Warin", "Baldwin", "Clovis", "Dreux",
                "Eustace", "Godfrey", "Hector", "Ives", "Leon", "Manfred", "Otto", "Reynard",
                // Additional medieval European names
                "Albert", "Bernard", "Charles", "Denis", "Edgar", "Frederick", "Gunther", "Henry",
                "Ingram", "Jordan", "Kurt", "Louis", "Martin", "Nicolas", "Oswald", "Peter",
                "Quentin", "Richard", "Stephen", "Thomas", "Urban", "Victor", "Walter", "Alaric",
                "Bruno", "Clement", "Dietrich", "Everard", "Felix", "Gerard", "Hartmann", "Ivo",
                "Jerome", "Karl", "Ludwig", "Maurice", "Normann", "Osric", "Paulus", "Raimond",
                "Siegfried", "Thibault", "Ugo", "Volkmar", "Wilhelm", "Arnulf", "Baldric", "Cedric",
                "Dankwart", "Egbert", "Folcard", "Gislebert", "Hartwig", "Isembard", "Jaufre", "Konrad",
                "Ludovic", "Mallory", "Norvin", "Obert", "Percival", "Roland", "Sigurd", "Theodoric",
                "Ulwin", "Valdemar", "Werner", "Adalbert", "Bodo", "Childebert", "Dietmar", "Ernst"
            }
    };

        /// <summary>
        /// Female hero names for each culture
        /// </summary>
        public static readonly Dictionary<string, List<string>> FemaleHeroNames = new Dictionary<string, List<string>>
    {
        /// MARK: Aserai - Female
        ["aserai"] = new List<string>
            {
                // Original set
                "Amira", "Basma", "Dalila", "Farah", "Habiba", "Jalila", "Karima", "Layla",
                "Muna", "Nadia", "Rania", "Safiya", "Tahira", "Yasmin", "Zahra", "Amina",
                "Fatima", "Halima", "Jamila", "Khadija", "Latifa", "Malika", "Naima", "Rasha",
                "Samira", "Tamara", "Wafaa", "Yasmina", "Zainab", "Aziza", "Bahira", "Farida",
                // Additional powerful names
                "Aisha", "Bushra", "Dunya", "Fadila", "Ghada", "Hana", "Iman", "Jamilah",
                "Kalila", "Leila", "Mariam", "Nada", "Qadira", "Rima", "Salma", "Thurayya",
                "Warda", "Yara", "Zaynab", "Almas", "Bahija", "Dima", "Faiza", "Hiba",
                "Inas", "Jumana", "Kamila", "Lubna", "Maysa", "Noor", "Rabia", "Samar",
                "Tala", "Widad", "Zahirah", "Anisa", "Basira", "Duha", "Fatin", "Hala",
                "Ibtisam", "Jalwa", "Kanza", "Lamya", "Maysoon", "Najwa", "Rashida", "Shadha",
                "Thuraya", "Wafa", "Yamna", "Zahra", "Ameera", "Badia", "Dalal", "Fawzia",
                "Hessa", "Izzah", "Janan", "Karima", "Lina", "Maha", "Najma", "Rawiya",
                "Sumaya", "Thana", "Wijdan", "Asma", "Budur", "Dalia", "Fikriyya", "Halah"
            },
        /// MARK: Battania - Female
        ["battania"] = new List<string>
            {
                // Original set
                "Aife", "Brigid", "Cara", "Deirdre", "Eira", "Fiona", "Gwen", "Isolde",
                "Keira", "Lianna", "Maeve", "Nessa", "Rhona", "Seren", "Tara", "Una",
                "Alys", "Brenna", "Ceri", "Eilidh", "Ffion", "Grania", "Isla", "Morna",
                "Niamh", "Oona", "Rhiannon", "Siobhan", "Teagan", "Vanya", "Aisling", "Bronwen",
                // Additional Celtic/Gaelic names
                "Aoife", "Branwen", "Ciara", "Dervla", "Enid", "Fionnuala", "Grainne", "Iona",
                "Keelin", "Lorna", "Mairead", "Neve", "Orla", "Rhian", "Shannon", "Treasa",
                "Ula", "Ailsa", "Brianna", "Clodagh", "Delaney", "Eithne", "Fionnula", "Gwyneth",
                "Imogen", "Kyla", "Lowri", "Meredith", "Nuala", "Oonagh", "Rowena", "Sinead",
                "Tamsin", "Wynne", "Aislinn", "Briony", "Catriona", "Dwyn", "Eleri", "Faye",
                "Glenna", "Isolda", "Kiera", "Lynwen", "Moira", "Nerys", "Olwen", "Rhosyn",
                "Sorcha", "Tegwen", "Una", "Aerona", "Blair", "Carrigan", "Deryn", "Elen",
                "Fallon", "Gwenda", "Iona", "Keara", "Lynet", "Morgan", "Neala", "Owena",
                "Rhianna", "Saoirse", "Tamara", "Valerie", "Ailbhe", "Briana", "Caitlin", "Dearbhla"
            },
        /// MARK: Empire - Female
        ["empire"] = new List<string>
            {
                // Original set (more variety added)
                "Aelia", "Cassia", "Diana", "Elena", "Flora", "Helena", "Iris", "Julia",
                "Lucia", "Marina", "Nora", "Olivia", "Petra", "Rhea", "Silvia", "Thalia",
                "Aurelia", "Camilla", "Delia", "Fabia", "Gaia", "Lydia", "Marcella", "Portia",
                "Sabina", "Valeria", "Acacia", "Cora", "Felicia", "Livia", "Octavia", "Vera",
                // Additional with more variety (not all ending in -a)
                "Antonia", "Claudia", "Drusilla", "Flavia", "Hortensia", "Junia", "Lavinia", "Marcia",
                "Paulina", "Sulpicia", "Tullia", "Agrippina", "Caecilia", "Domitia", "Faustina", "Galeria",
                "Lepida", "Metella", "Pompeia", "Scribonia", "Terentia", "Vipsania", "Aemilia", "Cornelia",
                "Fulvia", "Livia", "Messalina", "Plautia", "Servilia", "Vibia", "Anastasia", "Corina",
                "Eugenia", "Irene", "Melina", "Sophia", "Theodora", "Zoe", "Berenice", "Daphne",
                "Eudoxia", "Galene", "Hypatia", "Kalliope", "Lysandra", "Myrina", "Nike", "Olympia",
                "Phoebe", "Roxana", "Selene", "Thalassa", "Xanthe", "Althea", "Basilia", "Charis",
                "Dione", "Electra", "Galen", "Hero", "Ianthe", "Kassandra", "Larissa", "Melia",
                "Nephele", "Pelagia", "Sibyl", "Thais", "Zenobia", "Ariadne", "Clio", "Eurydice"
            },
        /// MARK: Khuzait - Female
        ["khuzait"] = new List<string>
            {
                // Original set
                "Altani", "Bolormaa", "Chagaan", "Enkhtuya", "Gerel", "Khongor", "Mandukhai", "Naran",
                "Oyuun", "Saran", "Temulun", "Ulaana", "Yesui", "Altantsetseg", "Battsetseg", "Enkhjin",
                "Ganbaatar", "Khulan", "Munkhtsetseg", "Narangerel", "Odtsetseg", "Sarangerel", "Tsogt", "Tuul",
                "Uyanga", "Chimge", "Delger", "Erdenechimeg", "Javkhlan", "Narantsetseg", "Otgonbayar", "Tuvshinbayar",
                // Additional Mongol/Turkic names with more variety
                "Anar", "Bayarmaa", "Cholmon", "Dalaijargal", "Erdene", "Ganbold", "Hoelun", "Idugan",
                "Jendu", "Khutulun", "Lhamaa", "Mandal", "Narantsetseg", "Oghul", "Qutlugh", "Sorkhaqtani",
                "Toregene", "Ulagan", "Yesugei", "Altun", "Bayarma", "Checheyigen", "Dagmar", "Erdeni",
                "Gunjin", "Hulun", "Jochi", "Khatun", "Lkhagva", "Munkhzaya", "Narantuyaa", "Odval",
                "Purev", "Sarnai", "Tsetseg", "Uranchimeg", "Yesuntai", "Ariunaa", "Bolormaa", "Ceceg",
                "Delgermaa", "Enkhmaa", "Gantsetseg", "Horolmaa", "Ider", "Jargal", "Kherlen", "Lhagva",
                "Monkhtsetseg", "Nergui", "Odonchimeg", "Purevsuren", "Sainaa", "Tsolmon", "Uranzaya", "Yesugen",
                "Altan", "Bayar", "Chimeg", "Davaajargal", "Enkhbayar", "Ganzul", "Hulan", "Jargalsaikhan",
                "Khaliun", "Lkhagvadorj", "Munkhbayar", "Narantuya", "Oyunbileg", "Purevdorj", "Saran", "Tsagaan"
            },
        /// MARK: Nord - Female
        ["nord"] = new List<string>
            {
                // Original set
                "Asa", "Brynhild", "Dagny", "Eira", "Freya", "Gudrun", "Hilda", "Ingrid",
                "Jorunn", "Kari", "Liv", "Magna", "Ragna", "Sigrid", "Thora", "Ulfhild",
                "Astrid", "Bodil", "Eldrid", "Frida", "Gerd", "Helga", "Karin", "Signe",
                "Solveig", "Tove", "Vigdis", "Ylva", "Arna", "Bergthora", "Dalla", "Gunnhild",
                // Additional Norse/Nordic names
                "Alfhild", "Bergljot", "Dagmar", "Elin", "Freja", "Gyda", "Hjordis", "Ingibjorg",
                "Jorunn", "Katla", "Linnea", "Maren", "Nanna", "Ragnhild", "Sigrun", "Thorhild",
                "Unn", "Valdis", "Alfrida", "Bothild", "Embla", "Freyja", "Gunhild", "Hertha",
                "Inga", "Kelda", "Lagertha", "Mjoll", "Nerthus", "Randi", "Sigyn", "Thyra",
                "Urd", "Valgerd", "Asdis", "Brenna", "Eira", "Fjola", "Gro", "Hrefna",
                "Idunn", "Jarla", "Kjolvor", "Lilja", "Minna", "Oddny", "Revna", "Svala",
                "Thordis", "Una", "Viga", "Asgerd", "Bera", "Drifa", "Eydis", "Frida",
                "Gunnvor", "Halla", "Idun", "Jorunn", "Kolbrun", "Laufey", "Monika", "Nanna",
                "Oda", "Rannveig", "Saga", "Thorunn", "Unna", "Vigga", "Alva", "Bergthora"
            },
        /// MARK: Sturgia - Female
        ["sturgia"] = new List<string>
            {
                // Original set
                "Alina", "Bogdana", "Daria", "Elena", "Galina", "Irina", "Katya", "Lada",
                "Milena", "Nadia", "Olga", "Rada", "Svetlana", "Tatyana", "Vera", "Yana",
                "Anastasia", "Darinka", "Elina", "Gordana", "Ivana", "Ljuba", "Marina", "Natasha",
                "Radomira", "Stanislava", "Tanya", "Vesna", "Zora", "Agnessa", "Draga", "Ludmila",
                // Additional Slavic names
                "Anja", "Branka", "Danka", "Emilija", "Faina", "Galena", "Hristina", "Iskra",
                "Jelena", "Katica", "Larisa", "Maja", "Nadiya", "Olena", "Polina", "Roksana",
                "Sonja", "Tanja", "Valentina", "Yelena", "Zlata", "Aleksandra", "Biljana", "Daniela",
                "Elizabeta", "Gabriela", "Hana", "Ivanka", "Jasna", "Koviljka", "Ludmilla", "Marija",
                "Natalija", "Olivera", "Pavlina", "Rajna", "Snezana", "Tamara", "Varvara", "Anushka",
                "Borislava", "Danica", "Ekaterina", "Feodora", "Grozda", "Hripsima", "Ilinca", "Juliana",
                "Kristina", "Lyudmila", "Miroslava", "Nikola", "Oksana", "Radmila", "Sofiya", "Tatiana",
                "Ulyana", "Vladislava", "Yelizaveta", "Antonina", "Bozena", "Dobromira", "Evgeniya", "Galina",
                "Hristina", "Inga", "Jaroslava", "Krystyna", "Liliya", "Milica", "Nevena", "Oxana"
            },
        /// MARK: Vlandia - Female
        ["vlandia"] = new List<string>
            {
                // Original set
                "Adelaide", "Beatrice", "Constance", "Eleanor", "Giselle", "Hildegard", "Isolde", "Matilda",
                "Rosamund", "Sybilla", "Agnes", "Blanche", "Cecilia", "Edith", "Genevieve", "Heloise",
                "Isabelle", "Margery", "Philippa", "Richilda", "Alicia", "Bertha", "Clarisse", "Emma",
                "Fleur", "Gwendolyn", "Jeanne", "Leonora", "Mabel", "Odette", "Rosalind", "Yvette",
                // Additional medieval European names
                "Adela", "Brunhilde", "Catherine", "Diane", "Ermengarde", "Felicity", "Gertrude", "Hedwig",
                "Ida", "Judith", "Katherine", "Leticia", "Margaret", "Nicole", "Olive", "Petronilla",
                "Rosaline", "Sophia", "Theodora", "Ursula", "Victoria", "Winifred", "Alice", "Brigitta",
                "Charlotte", "Dorothy", "Elizabeth", "Florence", "Geraldine", "Henrietta", "Ingrid", "Joanna",
                "Leonore", "Magdalena", "Nicolette", "Ottilia", "Prudence", "Rosemary", "Scholastica", "Teresa",
                "Ulrica", "Veronica", "Wilhelmina", "Amelia", "Brigitte", "Cordelia", "Desiree", "Estelle",
                "Gabrielle", "Helene", "Josephine", "Lucille", "Madeline", "Natalie", "Ophelia", "Pauline",
                "Simone", "Therese", "Vivienne", "Annette", "Bernadette", "Colette", "Danielle", "Eloise",
                "Francoise", "Georgette", "Henriette", "Juliette", "Lisette", "Marceline", "Ninette", "Paulette"
            }
    };

        /// <summary>
        /// Cultural Clan Names for each Culture
        /// </summary>
        public static readonly Dictionary<string, List<string>> ClanNames = new Dictionary<string, List<string>>
    {
        /// MARK: Aserai - Clan
        ["aserai"] = new List<string>
            {
                // Original set
                "Banu Harith", "Banu Rashid", "Banu Malik", "Banu Faisal", "Banu Yazid", "Banu Azhar",
                "Banu Nasir", "Banu Walid", "Banu Kamal", "Banu Jalal", "Banu Tariq", "Banu Salim",
                "Banu Zahir", "Banu Hakim", "Banu Idris", "Banu Munir", "Banu Bashir", "Banu Saif",
                "Banu Rami", "Banu Akram", "Banu Fadil", "Banu Latif", "Banu Qadir", "Banu Habib",
                // Additional clans
                "Banu Khalid", "Banu Hamza", "Banu Jafar", "Banu Omar", "Banu Rayyan", "Banu Shams",
                "Banu Thabit", "Banu Umar", "Banu Wasim", "Banu Yasin", "Banu Abbas", "Banu Dawud",
                "Banu Ghazi", "Banu Hadi", "Banu Isa", "Banu Jamil", "Banu Kareem", "Banu Mahdi",
                "Banu Nabil", "Banu Qasim", "Banu Sadiq", "Banu Tahir", "Banu Waleed", "Banu Zaid",
                "Banu Adil", "Banu Burhan", "Banu Dinar", "Banu Fahd", "Banu Ghaith", "Banu Husam",
                "Banu Jawad", "Banu Karam", "Banu Mansur", "Banu Nabeel", "Banu Rauf", "Banu Sami"
            },
        /// MARK: Battania - Clan
        ["battania"] = new List<string>
            {
                // Original set
                "fen Brennan", "fen Cormac", "fen Donnell", "fen Ewan", "fen Garvan", "fen Kelan",
                "fen Lorcan", "fen Murtagh", "fen Niall", "fen Rowan", "fen Seanan", "fen Tiernan",
                "fen Aidan", "fen Cian", "fen Finnian", "fen Kieran", "fen Oran", "fen Tadhg",
                "fen Brogan", "fen Declan", "fen Fionn", "fen Liam", "fen Padraig", "fen Ronan",
                // Additional clans
                "fen Connor", "fen Diarmuid", "fen Ewan", "fen Griffin", "fen Hugh", "fen Kellen",
                "fen Logan", "fen Maddox", "fen Neil", "fen Osgar", "fen Patrick", "fen Quinn",
                "fen Rory", "fen Shane", "fen Teague", "fen Ultan", "fen Vaughn", "fen Warren",
                "fen Aengus", "fen Brion", "fen Conan", "fen Donovan", "fen Fiachra", "fen Galvin",
                "fen Hogan", "fen Kian", "fen Murtagh", "fen Odhran", "fen Phelan", "fen Riordan",
                "fen Soren", "fen Torvin", "fen Ulric", "fen Albin", "fen Cadoc", "fen Drustan"
            },
        /// MARK: Empire - Clan
        ["empire"] = new List<string>
            {
                // Original set (with more variety)
                "Aelios", "Castorios", "Delanos", "Euronos", "Garonios", "Heronos", "Jurios", "Kyronos",
                "Leorios", "Menios", "Neranos", "Oronios", "Petrios", "Qunios", "Seranos", "Tironos",
                "Aronios", "Benios", "Carios", "Denios", "Ferios", "Garios", "Herios", "Janios",
                "Karios", "Lenios", "Morios", "Norios", "Porios", "Renios", "Serios", "Torios",
                // Additional with variety (mixing Roman and Greek styles)
                "Antonius", "Bruticus", "Claudius", "Drusus", "Flavianus", "Gracchus", "Horatius", "Junius",
                "Lucanus", "Marcellus", "Octavius", "Quintus", "Rufus", "Severus", "Valerius", "Aquillius",
                "Cornelius", "Domitius", "Fabius", "Gallus", "Livius", "Metellus", "Pompeius", "Scipio",
                "Basileus", "Chrysos", "Demetrios", "Euphrates", "Gregorios", "Herakles", "Ionicus", "Konstantinos",
                "Leonidas", "Makedon", "Nikephoros", "Olympios", "Philippos", "Romanos", "Stephanos", "Theodosius"
            },
        /// MARK: Khuzait - Clan
        ["khuzait"] = new List<string>
            {
                // Original set
                "Baratai", "Chagait", "Dorbenait", "Ergit", "Genghait", "Hulait", "Jebeit", "Kubait",
                "Manglait", "Nogait", "Ordait", "Qarait", "Subait", "Temuit", "Ulaait", "Yesait",
                "Arbait", "Batuit", "Chilait", "Dorbait", "Ganait", "Hadait", "Jochit", "Kadait",
                "Mongait", "Nayanit", "Ogait", "Sorgait", "Tumait", "Ulagait", "Bayanit", "Dolonait",
                // Additional clans
                "Altanit", "Batuit", "Chaghait", "Duwaait", "Esenait", "Guyukait", "Hulunit", "Jagataait",
                "Khubilait", "Ligdanit", "Manggait", "Negudait", "Oiratait", "Qaiduit", "Rashidait", "Sanjarait",
                "Toghrulit", "Uzbegait", "Baidarit", "Chaghriit", "Danishmendait", "Eljigideait", "Ghazanait", "Hulunait",
                "Janibegait", "Kaiduit", "Manggalait", "Nogaijit", "Osmanait", "Qazanit", "Sartakait", "Turakinait",
                "Ulusait", "Berkutait", "Caganait", "Delgerait", "Erkeait", "Ganzorait", "Haraqanait", "Irgenait"
            },
        /// MARK: Nord - Clan
        ["nord"] = new List<string>
            {
                // Original set
                "Bjorning", "Dagring", "Eriking", "Finnring", "Gunnaring", "Haraldring", "Ivaring", "Knutring",
                "Leifing", "Magnusing", "Olafring", "Ragnaring", "Sigurdring", "Thorring", "Ulfring", "Vidaring",
                "Alriking", "Baldring", "Egilring", "Frodering", "Grimring", "Halvarding", "Jorundring", "Ketilring",
                "Ormring", "Rolfring", "Svenring", "Torstering", "Vargring", "Yngvering", "Arnring", "Bodring",
                // Additional clans
                "Asbjorning", "Bjarniring", "Einarring", "Finnuring", "Gormring", "Hakonring", "Ingvarring", "Jorgenring",
                "Kjartanring", "Leifrring", "Magniring", "Njalring", "Overing", "Ragnvaldring", "Sigvaldring", "Thorfinnring",
                "Ulfricring", "Viggoring", "Arnring", "Bardring", "Eyvindring", "Grimring", "Hjalmarring", "Isakring",
                "Jarlring", "Kolring", "Larsring", "Magnering", "Njordring", "Ornring", "Roaldring", "Sturering",
                "Thrandring", "Ulfring", "Valiring", "Asgeiring", "Braging", "Eilifring", "Flokiring", "Geirring"
            },
        /// MARK: Sturgia - Clan
        ["sturgia"] = new List<string>
            {
                // Original set
                "Bogdanoving", "Dmitroving", "Gregoroving", "Igoroving", "Kaziroving", "Lazaroving", "Miroving", "Nikoloving",
                "Paveloving", "Radoving", "Stefanoving", "Tomioving", "Vadimoving", "Yurioving", "Zlatoving", "Borisoving",
                "Dragoving", "Goranoving", "Ivanoving", "Jaroving", "Konstantoving", "Leonoving", "Milosoving", "Olegoving",
                "Petroving", "Romanoving", "Stanisloving", "Timuroving", "Viktoroving", "Zoranoving", "Anatoloving", "Bronioving",
                // Additional clans
                "Alexeioving", "Brankoving", "Davoroving", "Emiloving", "Filipoving", "Gavriloving", "Hrvojeoving", "Ivoving",
                "Jovanoving", "Kiriloving", "Lukaroving", "Markoving", "Nedeljkoving", "Ognjenoving", "Pavleoving", "Radomiroving",
                "Svyatosloving", "Tarasoving", "Urosoving", "Velimiroving", "Andreioving", "Borislavoving", "Daniloving", "Evgenioving",
                "Georgioving", "Hristoving", "Ilijaroving", "Josipoving", "Kostaroving", "Ljubomiroving", "Milanoving", "Nikoving",
                "Ostojaroving", "Predragoving", "Rajkoving", "Slobodanoving", "Teodoroving", "Vasilijoving", "Aleksandroving", "Bojanoving"
            },
        /// MARK: Vlandia - Clan
        ["vlandia"] = new List<string>
            {
                // Original set
                "dey Aldren", "dey Bertrand", "dey Conran", "dey Drogan", "dey Edmund", "dey Fulken", "dey Geoffren",
                "dey Huguard", "dey Ingmar", "dey Jocelen", "dey Lambard", "dey Miloran", "dey Norben", "dey Odon",
                "dey Philen", "dey Rainalden", "dey Sigmund", "dey Thieren", "dey Ulrichen", "dey Valeran", "dey Warine",
                "dey Baldric", "dey Cloven", "dey Dreugen", "dey Eustacen", "dey Godfren", "dey Hectoren", "dey Leonard",
                // Additional clans
                "dey Alberten", "dey Bernarden", "dey Charlen", "dey Denisen", "dey Edgaren", "dey Frederiken", "dey Guntheren",
                "dey Henrien", "dey Ingramen", "dey Jordanen", "dey Kurten", "dey Louisen", "dey Martinen", "dey Nicolasen",
                "dey Oswalden", "dey Peteren", "dey Quentinen", "dey Richarden", "dey Stephenen", "dey Thomasen", "dey Urbanen",
                "dey Victoren", "dey Walteren", "dey Alaricen", "dey Brunoen", "dey Clementen", "dey Dietrichen", "dey Everarden",
                "dey Felixen", "dey Gerarden", "dey Hartmannen", "dey Jeromen", "dey Karlen", "dey Ludwigen", "dey Mauricen"
            }
    };
    }
}