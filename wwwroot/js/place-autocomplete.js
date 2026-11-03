/**
 * Tagalong Place Autocomplete Engine
 * Ultra-fast zero-API-key place search & suggestions powered by
 * an extensive Gujarat/Rajkot commercial & transit index +
 * strictly India-bounded OpenStreetMap Photon & Nominatim engines.
 */
(function(window) {
    'use strict';

    // Comprehensive curated transit hubs, shopping showrooms, campuses, and popular hotspots in Rajkot / Gujarat
    const LOCAL_PLACES = [
        // --- Shopping & Commercial Showrooms ---
        {
            name: "JadeBlue Lifestyle Showroom",
            subtitle: "Dr. Yagnik Road, Jagnath Plot, Rajkot, Gujarat",
            lat: 22.2968,
            lng: 70.7954,
            type: "showroom",
            tag: "Showroom",
            aliases: ["jadeblue", "jade", "blue", "showroom", "clothing", "menswear", "yagnik", "jagnath"]
        },
        {
            name: "JadeBlue Showroom",
            subtitle: "Kalawad Road, Near KKV Hall Circle, Rajkot, Gujarat",
            lat: 22.2855,
            lng: 70.7744,
            type: "showroom",
            tag: "Showroom",
            aliases: ["jadeblue", "jade", "blue", "showroom", "clothing", "kalawad", "kkv"]
        },
        {
            name: "Raymond Shop Showroom",
            subtitle: "Dr. Yagnik Road, Jagnath Plot, Rajkot, Gujarat",
            lat: 22.2975,
            lng: 70.7960,
            type: "showroom",
            tag: "Showroom",
            aliases: ["raymond", "showroom", "suit", "yagnik", "clothing"]
        },
        {
            name: "Raymond Made to Measure",
            subtitle: "Kalawad Road, Near Crystal Mall, Rajkot, Gujarat",
            lat: 22.2842,
            lng: 70.7720,
            type: "showroom",
            tag: "Showroom",
            aliases: ["raymond", "kalawad", "tailor", "showroom"]
        },
        {
            name: "Crystal Mall",
            subtitle: "Kalawad Road, Ghanshyam Nagar, Rajkot, Gujarat",
            lat: 22.2811,
            lng: 70.7709,
            type: "mall",
            tag: "Mall",
            aliases: ["crystal", "mall", "shopping", "kalawad", "cinema", "westside", "pantaloons"]
        },
        {
            name: "Reliance Mega Mall",
            subtitle: "150 Feet Ring Road, Near Indira Circle, Rajkot, Gujarat",
            lat: 22.2789,
            lng: 70.7812,
            type: "mall",
            tag: "Mall",
            aliases: ["reliance", "mega", "mall", "inox", "150ft", "ring road", "indira circle"]
        },
        {
            name: "Westside Showroom",
            subtitle: "Crystal Mall, Kalawad Road, Rajkot, Gujarat",
            lat: 22.2812,
            lng: 70.7710,
            type: "showroom",
            tag: "Fashion",
            aliases: ["westside", "clothing", "tata", "crystal mall", "kalawad"]
        },
        {
            name: "Zudio Showroom",
            subtitle: "Kalawad Road, Opp. KKV Hall, Rajkot, Gujarat",
            lat: 22.2825,
            lng: 70.7715,
            type: "showroom",
            tag: "Fashion",
            aliases: ["zudio", "fashion", "clothing", "kalawad", "kkv"]
        },
        {
            name: "Zudio Showroom",
            subtitle: "150 Feet Ring Road, Near Nana Mava Circle, Rajkot, Gujarat",
            lat: 22.2740,
            lng: 70.7850,
            type: "showroom",
            tag: "Fashion",
            aliases: ["zudio", "nana mava", "150ft", "ring road", "clothing"]
        },
        {
            name: "Zudio Showroom",
            subtitle: "Mavdi Main Road, Near Uday Nagar, Rajkot, Gujarat",
            lat: 22.2680,
            lng: 70.7920,
            type: "showroom",
            tag: "Fashion",
            aliases: ["zudio", "mavdi", "clothing", "fashion"]
        },
        {
            name: "Croma Electronics Showroom",
            subtitle: "Kalawad Road, Near KKV Hall Circle, Rajkot, Gujarat",
            lat: 22.2850,
            lng: 70.7738,
            type: "showroom",
            tag: "Electronics",
            aliases: ["croma", "electronics", "tata", "mobile", "laptop", "kalawad", "kkv"]
        },
        {
            name: "Reliance Digital Showroom",
            subtitle: "Crystal Mall, Kalawad Road, Rajkot, Gujarat",
            lat: 22.2810,
            lng: 70.7712,
            type: "showroom",
            tag: "Electronics",
            aliases: ["reliance", "digital", "electronics", "crystal mall", "kalawad"]
        },
        {
            name: "Reliance Digital Showroom",
            subtitle: "150 Feet Ring Road, Near Indira Circle, Rajkot, Gujarat",
            lat: 22.2790,
            lng: 70.7815,
            type: "showroom",
            tag: "Electronics",
            aliases: ["reliance", "digital", "150ft", "ring road", "electronics"]
        },
        {
            name: "D-Mart Supermarket",
            subtitle: "150 Feet Ring Road, Near Mavdi Chowkdi, Rajkot, Gujarat",
            lat: 22.2665,
            lng: 70.7865,
            type: "mall",
            tag: "Grocery",
            aliases: ["dmart", "d-mart", "d mart", "grocery", "mavdi", "150ft"]
        },
        {
            name: "D-Mart Supermarket",
            subtitle: "Bhavnagar Road, Aji Dam Cross Road, Rajkot, Gujarat",
            lat: 22.2780,
            lng: 70.8320,
            type: "mall",
            tag: "Grocery",
            aliases: ["dmart", "d-mart", "bhavnagar road", "aji dam"]
        },
        {
            name: "Tanishq Jewellery Showroom",
            subtitle: "Dr. Yagnik Road, Jagnath Plot, Rajkot, Gujarat",
            lat: 22.2982,
            lng: 70.7965,
            type: "showroom",
            tag: "Jewellery",
            aliases: ["tanishq", "jewellery", "gold", "yagnik", "tata"]
        },
        {
            name: "Decathlon Sports",
            subtitle: "150 Feet Ring Road, Near Raiya Telephone Exchange, Rajkot",
            lat: 22.2890,
            lng: 70.7680,
            type: "showroom",
            tag: "Sports",
            aliases: ["decathlon", "sports", "150ft", "raiya"]
        },
        {
            name: "Apple Store (iScene / Unicorn)",
            subtitle: "Dr. Yagnik Road, Rajkot, Gujarat",
            lat: 22.2960,
            lng: 70.7950,
            type: "showroom",
            tag: "Electronics",
            aliases: ["apple", "iphone", "iscene", "unicorn", "macbook", "yagnik"]
        },
        {
            name: "Pantaloons Showroom",
            subtitle: "Crystal Mall, Kalawad Road, Rajkot, Gujarat",
            lat: 22.2811,
            lng: 70.7709,
            type: "showroom",
            tag: "Fashion",
            aliases: ["pantaloons", "fashion", "clothing", "crystal mall"]
        },
        {
            name: "Cosmoplex Cinema",
            subtitle: "Kalawad Road, Munjka, Rajkot, Gujarat",
            lat: 22.2820,
            lng: 70.7480,
            type: "landmark",
            tag: "Cinema",
            aliases: ["cosmoplex", "cinema", "movie", "theatre", "kalawad", "munjka"]
        },

        // --- Universities & Campuses ---
        {
            name: "Marwadi University",
            subtitle: "Gauridad, Rajkot-Morbi Highway, Rajkot, Gujarat",
            lat: 22.3676,
            lng: 70.7971,
            type: "university",
            tag: "Campus",
            aliases: ["marwadi", "university", "college", "campus", "gauridad", "morbi highway", "mu"]
        },
        {
            name: "Marwadi University Main Gate",
            subtitle: "Rajkot-Morbi Highway, Gauridad, Rajkot, Gujarat",
            lat: 22.3672,
            lng: 70.7965,
            type: "university",
            tag: "Campus Gate",
            aliases: ["marwadi", "gate", "main gate", "gauridad"]
        },
        {
            name: "Marwadi University Lake",
            subtitle: "Main Campus, Rajkot, Gujarat",
            lat: 22.3667,
            lng: 70.7961,
            type: "university",
            tag: "Campus",
            aliases: ["marwadi", "lake", "campus lake"]
        },
        {
            name: "Atmiya University",
            subtitle: "Kalawad Road, Yogidham Gurukul, Rajkot, Gujarat",
            lat: 22.2858,
            lng: 70.7788,
            type: "university",
            tag: "Campus",
            aliases: ["atmiya", "university", "college", "yogidham", "kalawad", "gurukul"]
        },
        {
            name: "RK University",
            subtitle: "Bhavnagar Highway, Kasturbadham, Tramba, Rajkot, Gujarat",
            lat: 22.2415,
            lng: 70.8986,
            type: "university",
            tag: "Campus",
            aliases: ["rk", "rku", "university", "college", "tramba", "bhavnagar highway"]
        },
        {
            name: "Saurashtra University Campus",
            subtitle: "University Road, Rajkot, Gujarat",
            lat: 22.2929,
            lng: 70.7434,
            type: "university",
            tag: "Campus",
            aliases: ["saurashtra", "university", "campus", "university road"]
        },
        {
            name: "Darshan University",
            subtitle: "Rajkot-Morbi Highway, Hadala, Rajkot, Gujarat",
            lat: 22.4225,
            lng: 70.8175,
            type: "university",
            tag: "Campus",
            aliases: ["darshan", "university", "college", "hadala", "morbi highway"]
        },
        {
            name: "VVP Engineering College",
            subtitle: "Vajdi Virda, Kalawad Road, Rajkot, Gujarat",
            lat: 22.2470,
            lng: 70.7180,
            type: "university",
            tag: "Campus",
            aliases: ["vvp", "engineering", "college", "kalawad road", "vajdi"]
        },
        {
            name: "Christ College",
            subtitle: "Vidya Niketan, Munjka, Rajkot, Gujarat",
            lat: 22.2860,
            lng: 70.7420,
            type: "university",
            tag: "Campus",
            aliases: ["christ", "college", "munjka"]
        },
        {
            name: "B.H. Gardi College of Engineering",
            subtitle: "Anandpar, Rajkot-Kalawad Highway, Rajkot",
            lat: 22.2280,
            lng: 70.6650,
            type: "university",
            tag: "Campus",
            aliases: ["gardi", "bh gardi", "engineering", "anandpar"]
        },
        {
            name: "AVPTI College (A. V. Parekh Technical Institute)",
            subtitle: "Dr. Yagnik Road / Tagore Road, Rajkot, Gujarat",
            lat: 22.2920,
            lng: 70.7940,
            type: "university",
            tag: "College",
            aliases: ["avpti", "polytechnic", "tagore road", "yagnik"]
        },

        // --- Hospitals & Healthcare ---
        {
            name: "AIIMS Hospital Rajkot",
            subtitle: "Khandheri, Jamnagar Highway, Rajkot, Gujarat",
            lat: 22.3450,
            lng: 70.7250,
            type: "hospital",
            tag: "Hospital",
            aliases: ["aiims", "hospital", "khandheri", "medical", "jamnagar highway"]
        },
        {
            name: "Civil Hospital (PDU Medical College)",
            subtitle: "Hospital Chowk, Jail Road, Rajkot, Gujarat",
            lat: 22.3080,
            lng: 70.8010,
            type: "hospital",
            tag: "Hospital",
            aliases: ["civil", "hospital", "pdu", "medical", "hospital chowk"]
        },
        {
            name: "Wockhardt Hospital",
            subtitle: "Kalawad Road, Near KKV Hall, Rajkot, Gujarat",
            lat: 22.2855,
            lng: 70.7770,
            type: "hospital",
            tag: "Hospital",
            aliases: ["wockhardt", "hospital", "kalawad road", "heart"]
        },
        {
            name: "Sterling Hospital",
            subtitle: "150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.2765,
            lng: 70.7830,
            type: "hospital",
            tag: "Hospital",
            aliases: ["sterling", "hospital", "150ft", "ring road"]
        },
        {
            name: "Synergy Hospital",
            subtitle: "Ayodhya Chowk, 150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.3050,
            lng: 70.7710,
            type: "hospital",
            tag: "Hospital",
            aliases: ["synergy", "hospital", "ayodhya chowk", "150ft"]
        },
        {
            name: "Giriraj Hospital",
            subtitle: "150 Feet Ring Road, Near Raiya Circle, Rajkot, Gujarat",
            lat: 22.2880,
            lng: 70.7710,
            type: "hospital",
            tag: "Hospital",
            aliases: ["giriraj", "hospital", "150ft", "raiya"]
        },

        // --- Transit Hubs, Stations & Airports ---
        {
            name: "Rajkot Junction Railway Station",
            subtitle: "Station Road, Junction Plot, Rajkot, Gujarat",
            lat: 22.3129,
            lng: 70.8022,
            type: "transit",
            tag: "Train Station",
            aliases: ["rajkot junction", "railway station", "train", "station road", "junction plot"]
        },
        {
            name: "Bhaktinagar Railway Station",
            subtitle: "Bhakti Nagar, Rajkot, Gujarat",
            lat: 22.2801,
            lng: 70.8089,
            type: "transit",
            tag: "Train Station",
            aliases: ["bhaktinagar", "railway station", "train station", "station"]
        },
        {
            name: "Hirasar International Airport (Rajkot)",
            subtitle: "NH 27, Hirasar, Rajkot District, Gujarat",
            lat: 22.3486,
            lng: 70.9856,
            type: "airport",
            tag: "Airport",
            aliases: ["airport", "hirasar", "international airport", "flight", "rajkot airport"]
        },
        {
            name: "Old Rajkot Civil Airport",
            subtitle: "Race Course, Civil Aerodrome, Rajkot, Gujarat",
            lat: 22.3092,
            lng: 70.7797,
            type: "airport",
            tag: "Airport",
            aliases: ["civil airport", "old airport", "race course airport"]
        },
        {
            name: "Gondal Road Central ST Bus Stand",
            subtitle: "Central ST Bus Depot, Gondal Road, Rajkot, Gujarat",
            lat: 22.2891,
            lng: 70.8035,
            type: "transit",
            tag: "Bus Stand",
            aliases: ["bus stand", "st stand", "st bus", "central bus depot", "gondal road"]
        },
        {
            name: "Trikon Baug City Bus Station",
            subtitle: "City Bus Station, Center Rajkot, Gujarat",
            lat: 22.3006,
            lng: 70.8021,
            type: "transit",
            tag: "City Center",
            aliases: ["trikon baug", "trikon bagh", "city bus", "center"]
        },

        // --- Major Chowks & Intersections in Rajkot ---
        {
            name: "Madhapar Chowk",
            subtitle: "Jamnagar Highway Intersection, Rajkot, Gujarat",
            lat: 22.3248,
            lng: 70.7745,
            type: "transit",
            tag: "Chowk",
            aliases: ["madhapar", "chowk", "jamnagar highway", "madhapar circle"]
        },
        {
            name: "Indira Circle",
            subtitle: "150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.2936,
            lng: 70.7723,
            type: "transit",
            tag: "Circle",
            aliases: ["indira", "circle", "150ft", "ring road", "indira chowk"]
        },
        {
            name: "KKV Hall Circle",
            subtitle: "Kalawad Road & 150ft Ring Road, Rajkot, Gujarat",
            lat: 22.2852,
            lng: 70.7741,
            type: "transit",
            tag: "Circle",
            aliases: ["kkv", "kkv hall", "circle", "kalawad road", "150ft"]
        },
        {
            name: "Greenland Chokdi",
            subtitle: "Ahmedabad-Rajkot National Highway, Rajkot, Gujarat",
            lat: 22.3168,
            lng: 70.8384,
            type: "transit",
            tag: "Highway",
            aliases: ["greenland", "chokdi", "ahmedabad highway", "cross road"]
        },
        {
            name: "Morbi Bypass Circle",
            subtitle: "Rajkot-Morbi Highway, Rajkot, Gujarat",
            lat: 22.3551,
            lng: 70.8055,
            type: "transit",
            tag: "Highway",
            aliases: ["morbi bypass", "circle", "bypass", "morbi road"]
        },
        {
            name: "Gondal Chowkdi (Punit Nagar Cross)",
            subtitle: "Gondal Highway, Rajkot, Gujarat",
            lat: 22.2530,
            lng: 70.7980,
            type: "transit",
            tag: "Chowkdi",
            aliases: ["gondal chowkdi", "punit nagar", "gondal road highway"]
        },
        {
            name: "Mavdi Chowkdi",
            subtitle: "150 Feet Ring Road & Mavdi Main Road, Rajkot",
            lat: 22.2690,
            lng: 70.7870,
            type: "transit",
            tag: "Chowkdi",
            aliases: ["mavdi", "chowkdi", "150ft", "mavdi circle"]
        },
        {
            name: "Nana Mava Circle",
            subtitle: "150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.2770,
            lng: 70.7780,
            type: "transit",
            tag: "Circle",
            aliases: ["nana mava", "circle", "150ft", "ring road"]
        },
        {
            name: "Raiya Circle",
            subtitle: "150 Feet Ring Road, Near Telephone Exchange, Rajkot",
            lat: 22.2980,
            lng: 70.7690,
            type: "transit",
            tag: "Circle",
            aliases: ["raiya", "circle", "raiya road", "150ft"]
        },
        {
            name: "Ayodhya Chowk",
            subtitle: "150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.3060,
            lng: 70.7705,
            type: "transit",
            tag: "Chowk",
            aliases: ["ayodhya", "chowk", "150ft", "ring road"]
        },
        {
            name: "Shastri Maidan / Limda Chowk",
            subtitle: "Center Rajkot, Gujarat",
            lat: 22.3025,
            lng: 70.7981,
            type: "landmark",
            tag: "Center",
            aliases: ["shastri maidan", "limda chowk", "center"]
        },
        {
            name: "Race Course Ring Road",
            subtitle: "Race Course Ground, Rajkot, Gujarat",
            lat: 22.3040,
            lng: 70.7890,
            type: "landmark",
            tag: "Recreation",
            aliases: ["race course", "ground", "ring road", "madhavrao scindia"]
        },
        {
            name: "Yagnik Road",
            subtitle: "Jagnath Plot, Commercial Shopping Hub, Rajkot",
            lat: 22.2965,
            lng: 70.7952,
            type: "landmark",
            tag: "Shopping Hub",
            aliases: ["yagnik", "yagnik road", "dr yagnik", "jagnath"]
        },

        // --- Nearby Regional Cities ---
        {
            name: "Morbi City Center",
            subtitle: "Sanala Road, Morbi, Gujarat",
            lat: 22.8120,
            lng: 70.8350,
            type: "city",
            tag: "City",
            aliases: ["morbi", "sanala road", "ceramic"]
        },
        {
            name: "Jamnagar City (Digjam Circle)",
            subtitle: "Lal Bungalow, Jamnagar, Gujarat",
            lat: 22.4707,
            lng: 70.0577,
            type: "city",
            tag: "City",
            aliases: ["jamnagar", "digjam", "lal bungalow"]
        },
        {
            name: "Junagadh City (Majevadi Gate)",
            subtitle: "ST Bus Depot, Junagadh, Gujarat",
            lat: 21.5222,
            lng: 70.4579,
            type: "city",
            tag: "City",
            aliases: ["junagadh", "majevadi", "girnar"]
        },
        {
            name: "Gondal City",
            subtitle: "Swaminarayan Akshar Mandir, Gondal, Gujarat",
            lat: 21.9619,
            lng: 70.7997,
            type: "city",
            tag: "Town",
            aliases: ["gondal", "akshar mandir"]
        },
        {
            name: "Ahmedabad SG Highway",
            subtitle: "Thaltej, SG Highway, Ahmedabad, Gujarat",
            lat: 23.0504,
            lng: 72.5074,
            type: "city",
            tag: "Metro",
            aliases: ["ahmedabad", "sg highway", "thaltej", "satellite", "iskcon"]
        },
        {
            name: "Ahmedabad Geeta Mandir ST Stand",
            subtitle: "Central ST Bus Stand, Ahmedabad, Gujarat",
            lat: 23.0125,
            lng: 72.5894,
            type: "transit",
            tag: "Bus Stand",
            aliases: ["geeta mandir", "ahmedabad st stand"]
        },
        {
            name: "Vadodara Central Railway Station",
            subtitle: "Alkapuri, Vadodara, Gujarat",
            lat: 22.3106,
            lng: 73.1812,
            type: "transit",
            tag: "Station",
            aliases: ["vadodara", "baroda", "alkapuri", "station"]
        },
        {
            name: "Surat Central Station",
            subtitle: "Station Road, Varachha, Surat, Gujarat",
            lat: 21.2052,
            lng: 72.8406,
            type: "transit",
            tag: "Station",
            aliases: ["surat", "varachha", "station road"]
        }
    ];

    function getPlaceIcon(type) {
        switch (type) {
            case 'showroom':
                return '🛍️';
            case 'mall':
                return '🏬';
            case 'university':
                return '🎓';
            case 'hospital':
                return '🏥';
            case 'airport':
                return '✈️';
            case 'transit':
                return '🚆';
            case 'city':
                return '🏙️';
            default:
                return '📍';
        }
    }

    function highlightMatch(text, query) {
        if (!query || !text) return escapeHtml(text || '');
        const words = query.trim().split(/\s+/).filter(w => w.length >= 2);
        if (words.length === 0) return escapeHtml(text);

        const safePattern = words.map(w => w.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('|');
        const regex = new RegExp(`(${safePattern})`, 'gi');
        return escapeHtml(text).replace(regex, '<span class="text-warning fw-bold">$1</span>');
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // Fuzzy scorer for local places
    function scoreLocalPlace(place, query) {
        const qClean = query.toLowerCase().trim();
        const pName = place.name.toLowerCase();
        const pSub = place.subtitle.toLowerCase();
        const aliases = place.aliases || [];

        // Exact match or prefix match on name
        if (pName === qClean) return 200;
        if (pName.startsWith(qClean)) return 150;
        if (pName.includes(qClean)) return 120;

        // Check query words
        const qWords = qClean.split(/\s+/).filter(w => w.length >= 2);
        if (qWords.length === 0) return 0;

        let matchedWords = 0;
        for (const w of qWords) {
            if (pName.includes(w) || aliases.some(a => a.includes(w)) || pSub.includes(w)) {
                matchedWords++;
            }
        }

        if (matchedWords === 0) return 0;
        return (matchedWords / qWords.length) * 100;
    }

    class PlaceAutocomplete {
        constructor(input, options = {}) {
            this.input = typeof input === 'string' ? document.querySelector(input) : input;
            if (!this.input) return;

            this.options = Object.assign({
                latInputId: null,
                lngInputId: null,
                onSelect: null,
                biasLat: 22.3,
                biasLng: 70.8,
                debounceMs: 160,
                maxResults: 6
            }, options);

            this.dropdown = null;
            this.currentResults = [];
            this.activeIndex = -1;
            this.debounceTimer = null;
            this.abortController = null;

            this.init();
        }

        init() {
            this.input.setAttribute('autocomplete', 'off');
            this.input.setAttribute('spellcheck', 'false');

            let wrapper = this.input.closest('.console-input-wrap, .input-wrapper, .form-group, .position-relative');
            if (!wrapper) {
                wrapper = this.input.parentElement;
                if (getComputedStyle(wrapper).position === 'static') {
                    wrapper.style.position = 'relative';
                }
            }

            this.dropdown = document.createElement('ul');
            this.dropdown.className = 'place-autocomplete-dropdown';
            this.dropdown.setAttribute('role', 'listbox');
            this.dropdown.setAttribute('aria-label', 'Location suggestions');
            wrapper.appendChild(this.dropdown);

            this.input.addEventListener('input', (e) => this.onInput(e));
            this.input.addEventListener('keydown', (e) => this.onKeyDown(e));
            this.input.addEventListener('focus', () => {
                if (this.input.value.trim().length >= 2) {
                    this.search(this.input.value.trim());
                }
            });

            document.addEventListener('click', (e) => {
                if (!this.input.contains(e.target) && !this.dropdown.contains(e.target)) {
                    this.hide();
                }
            });
        }

        onInput(e) {
            const query = this.input.value.trim();
            if (query.length < 2) {
                this.hide();
                return;
            }

            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => {
                this.search(query);
            }, this.options.debounceMs);
        }

        async search(query) {
            // 1. Instant local matches with scoring
            const scoredLocal = [];
            for (const p of LOCAL_PLACES) {
                const score = scoreLocalPlace(p, query);
                if (score > 0) {
                    scoredLocal.push({ score, place: p });
                }
            }

            scoredLocal.sort((a, b) => b.score - a.score);
            const localMatches = scoredLocal.map(x => ({
                name: x.place.name,
                subtitle: x.place.subtitle,
                lat: x.place.lat,
                lng: x.place.lng,
                type: x.place.type,
                tag: x.place.tag || 'Popular'
            }));

            // Always render local matches immediately with 0ms latency
            if (localMatches.length > 0) {
                this.currentResults = localMatches.slice(0, this.options.maxResults);
                this.render(query);
            }

            // 2. Query Photon API (Strictly India Bounded)
            if (this.abortController) {
                this.abortController.abort();
            }
            this.abortController = new AbortController();

            try {
                // Bias by current region coordinates
                const photonUrl = `https://photon.komoot.io/api/?q=${encodeURIComponent(query)}&limit=8&lat=${this.options.biasLat}&lon=${this.options.biasLng}&lang=en`;
                const response = await fetch(photonUrl, {
                    signal: this.abortController.signal,
                    headers: { 'Accept': 'application/json' }
                });

                if (response.ok) {
                    const data = await response.json();
                    const validIndiaResults = [];

                    for (const f of (data.features || [])) {
                        const p = f.properties || {};
                        const coords = (f.geometry && f.geometry.coordinates) ? f.geometry.coordinates : [0, 0];
                        const lng = coords[0];
                        const lat = coords[1];

                        // CRITICAL: Strictly ensure the location is in India!
                        // (Avoid Kathmandu, Nepal, Philippines, etc. matching generic words like 'showroom')
                        const country = (p.country || '').toLowerCase();
                        const isIndia = country.includes('india') || country.includes('bharat') ||
                                        (lat >= 6.5 && lat <= 37.5 && lng >= 68.0 && lng <= 97.5);

                        if (!isIndia) continue;

                        const name = p.name || p.street || p.city || query;
                        const parts = [
                            p.street,
                            p.district,
                            p.city,
                            p.state,
                            p.country
                        ].filter(Boolean);

                        const uniqueParts = [...new Set(parts)].filter(part => part !== name);
                        const subtitle = uniqueParts.join(', ') || 'Gujarat, India';

                        validIndiaResults.push({
                            name: name,
                            subtitle: subtitle,
                            lat: lat,
                            lng: lng,
                            type: p.osm_value === 'university' || p.osm_value === 'college' ? 'university' :
                                  p.osm_value === 'station' || p.osm_value === 'bus_stop' ? 'transit' : 'landmark',
                            tag: p.city || 'India'
                        });
                    }

                    // Merge: Keep local matches on top, append valid India results without duplicates
                    const seenNames = new Set(localMatches.map(m => m.name.toLowerCase()));
                    const combined = [...localMatches];

                    for (const r of validIndiaResults) {
                        const key = r.name.toLowerCase();
                        if (!seenNames.has(key)) {
                            seenNames.add(key);
                            combined.push(r);
                        }
                    }

                    this.currentResults = combined.slice(0, this.options.maxResults);
                    this.render(query);
                }
            } catch (err) {
                if (err.name !== 'AbortError') {
                    if (localMatches.length > 0) {
                        this.currentResults = localMatches.slice(0, this.options.maxResults);
                        this.render(query);
                    }
                }
            }
        }

        render(query) {
            this.dropdown.innerHTML = '';
            this.activeIndex = -1;

            // Render matched results
            this.currentResults.forEach((place, index) => {
                const li = document.createElement('li');
                li.className = 'place-autocomplete-item';
                li.setAttribute('role', 'option');
                li.setAttribute('data-index', index);

                const icon = getPlaceIcon(place.type);
                const titleHtml = highlightMatch(place.name, query);
                const subtitleHtml = escapeHtml(place.subtitle);

                li.innerHTML = `
                    <div class="place-item-icon" aria-hidden="true">${icon}</div>
                    <div class="place-item-content">
                        <span class="place-item-title">${titleHtml}</span>
                        <span class="place-item-subtitle">${subtitleHtml}</span>
                    </div>
                    ${place.tag ? `<span class="place-item-tag">${escapeHtml(place.tag)}</span>` : ''}
                `;

                li.addEventListener('mousedown', (e) => {
                    e.preventDefault();
                    this.select(place);
                });

                li.addEventListener('mouseenter', () => {
                    this.setActive(index);
                });

                this.dropdown.appendChild(li);
            });

            // ALWAYS add "Use custom location" option so the user is never stuck
            const customLi = document.createElement('li');
            customLi.className = 'place-autocomplete-item custom-place-item';
            customLi.setAttribute('role', 'option');
            customLi.setAttribute('data-index', this.currentResults.length);
            customLi.innerHTML = `
                <div class="place-item-icon" style="background: rgba(234, 179, 8, 0.2); color: #facc15;" aria-hidden="true">🎯</div>
                <div class="place-item-content">
                    <span class="place-item-title">Use "<span class="text-warning">${escapeHtml(query)}</span>"</span>
                    <span class="place-item-subtitle">Use this exact custom address or landmark</span>
                </div>
                <span class="place-item-tag" style="background: rgba(255, 255, 255, 0.08); color: #fff;">Custom</span>
            `;

            customLi.addEventListener('mousedown', (e) => {
                e.preventDefault();
                this.select({
                    name: query,
                    subtitle: "Custom Location, Rajkot",
                    lat: this.options.biasLat,
                    lng: this.options.biasLng
                });
            });

            customLi.addEventListener('mouseenter', () => {
                this.setActive(this.currentResults.length);
            });

            this.dropdown.appendChild(customLi);
            this.dropdown.classList.add('active');
        }

        setActive(index) {
            const items = this.dropdown.querySelectorAll('.place-autocomplete-item');
            items.forEach((item, i) => {
                if (i === index) {
                    item.classList.add('focused');
                    item.scrollIntoView({ block: 'nearest' });
                } else {
                    item.classList.remove('focused');
                }
            });
            this.activeIndex = index;
        }

        onKeyDown(e) {
            if (!this.dropdown.classList.contains('active')) return;

            const items = this.dropdown.querySelectorAll('.place-autocomplete-item');
            if (items.length === 0) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                let next = this.activeIndex + 1;
                if (next >= items.length) next = 0;
                this.setActive(next);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                let prev = this.activeIndex - 1;
                if (prev < 0) prev = items.length - 1;
                this.setActive(prev);
            } else if (e.key === 'Enter') {
                if (this.activeIndex >= 0 && this.activeIndex < this.currentResults.length) {
                    e.preventDefault();
                    this.select(this.currentResults[this.activeIndex]);
                } else if (this.activeIndex === this.currentResults.length) {
                    e.preventDefault();
                    this.select({
                        name: this.input.value.trim(),
                        subtitle: "Custom Location, Rajkot",
                        lat: this.options.biasLat,
                        lng: this.options.biasLng
                    });
                }
            } else if (e.key === 'Escape') {
                this.hide();
            }
        }

        select(place) {
            this.input.value = place.name;

            if (this.options.latInputId) {
                const latEl = document.getElementById(this.options.latInputId);
                if (latEl) latEl.value = place.lat;
            }
            if (this.options.lngInputId) {
                const lngEl = document.getElementById(this.options.lngInputId);
                if (lngEl) lngEl.value = place.lng;
            }

            const customEvent = new CustomEvent('place_changed', {
                bubbles: true,
                detail: {
                    name: place.name,
                    subtitle: place.subtitle,
                    lat: place.lat,
                    lng: place.lng
                }
            });
            this.input.dispatchEvent(customEvent);

            if (typeof this.options.onSelect === 'function') {
                this.options.onSelect(place);
            }

            this.hide();
        }

        hide() {
            if (this.dropdown) {
                this.dropdown.classList.remove('active');
                this.dropdown.innerHTML = '';
            }
            this.activeIndex = -1;
        }
    }

    window.TagalongPlaceAutocomplete = {
        attach: function(selector, options) {
            const elements = typeof selector === 'string' ? document.querySelectorAll(selector) : [selector];
            const instances = [];
            elements.forEach(el => {
                if (el) instances.push(new PlaceAutocomplete(el, options));
            });
            return instances.length === 1 ? instances[0] : instances;
        },
        places: LOCAL_PLACES
    };

})(window);
