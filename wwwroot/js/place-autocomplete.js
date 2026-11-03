/**
 * Tagalong Place Autocomplete Engine
 * Ultra-fast zero-API-key place search & suggestions powered by
 * instant local transit hubs + OpenStreetMap Photon & Nominatim engines.
 */
(function(window) {
    'use strict';

    // Curated transit hubs, campuses, and popular hotspots in Rajkot / Gujarat / India
    const LOCAL_PLACES = [
        {
            name: "Marwadi University",
            subtitle: "Gauridad, Rajkot-Morbi Highway, Rajkot, Gujarat",
            lat: 22.3676,
            lng: 70.7971,
            type: "university",
            tag: "Campus"
        },
        {
            name: "Marwadi University Lake",
            subtitle: "Main Campus, Rajkot, Gujarat",
            lat: 22.3667,
            lng: 70.7961,
            type: "landmark",
            tag: "Campus"
        },
        {
            name: "Atmiya University",
            subtitle: "Kalawad Road, Yogidham Gurukul, Rajkot, Gujarat",
            lat: 22.2858,
            lng: 70.7788,
            type: "university",
            tag: "Campus"
        },
        {
            name: "RK University",
            subtitle: "Bhavnagar Highway, Kasturbadham, Rajkot, Gujarat",
            lat: 22.2415,
            lng: 70.8986,
            type: "university",
            tag: "Campus"
        },
        {
            name: "Saurashtra University",
            subtitle: "University Road, Rajkot, Gujarat",
            lat: 22.2929,
            lng: 70.7434,
            type: "university",
            tag: "Campus"
        },
        {
            name: "Rajkot Junction Railway Station",
            subtitle: "Station Road, Junction Plot, Rajkot, Gujarat",
            lat: 22.3129,
            lng: 70.8022,
            type: "transit",
            tag: "Station"
        },
        {
            name: "Bhaktinagar Railway Station",
            subtitle: "Bhakti Nagar, Rajkot, Gujarat",
            lat: 22.2801,
            lng: 70.8089,
            type: "transit",
            tag: "Station"
        },
        {
            name: "Hirasar International Airport (Rajkot)",
            subtitle: "NH 27, Hirasar, Rajkot District, Gujarat",
            lat: 22.3486,
            lng: 70.9856,
            type: "airport",
            tag: "Airport"
        },
        {
            name: "Rajkot Civil Airport",
            subtitle: "Race Course, Civil Aerodrome, Rajkot, Gujarat",
            lat: 22.3092,
            lng: 70.7797,
            type: "airport",
            tag: "Airport"
        },
        {
            name: "Madhapar Chowk",
            subtitle: "Jamnagar Highway Intersection, Rajkot, Gujarat",
            lat: 22.3248,
            lng: 70.7745,
            type: "transit",
            tag: "Chowk"
        },
        {
            name: "Crystal Mall",
            subtitle: "Kalawad Road, Ghanshyam Nagar, Rajkot, Gujarat",
            lat: 22.2811,
            lng: 70.7709,
            type: "mall",
            tag: "Mall"
        },
        {
            name: "Reliance Mega Mall",
            subtitle: "150 Feet Ring Road, Near Indira Circle, Rajkot, Gujarat",
            lat: 22.2789,
            lng: 70.7812,
            type: "mall",
            tag: "Mall"
        },
        {
            name: "Indira Circle",
            subtitle: "150 Feet Ring Road, Rajkot, Gujarat",
            lat: 22.2936,
            lng: 70.7723,
            type: "transit",
            tag: "Circle"
        },
        {
            name: "KKV Hall Circle",
            subtitle: "Kalawad Road & 150ft Ring Road, Rajkot, Gujarat",
            lat: 22.2852,
            lng: 70.7741,
            type: "transit",
            tag: "Circle"
        },
        {
            name: "Trikon Baug City Center",
            subtitle: "City Bus Station, Center Rajkot, Gujarat",
            lat: 22.3006,
            lng: 70.8021,
            type: "transit",
            tag: "Central"
        },
        {
            name: "Gondal Road ST Bus Stand",
            subtitle: "Central ST Bus Depot, Rajkot, Gujarat",
            lat: 22.2891,
            lng: 70.8035,
            type: "transit",
            tag: "Bus Stand"
        },
        {
            name: "Yagnik Road",
            subtitle: "Jagnath Plot, Commercial Hub, Rajkot, Gujarat",
            lat: 22.2965,
            lng: 70.7952,
            type: "landmark",
            tag: "Shopping"
        },
        {
            name: "Greenland Chokdi",
            subtitle: "Ahmedabad-Rajkot National Highway, Rajkot, Gujarat",
            lat: 22.3168,
            lng: 70.8384,
            type: "transit",
            tag: "Highway"
        },
        {
            name: "Morbi Bypass Circle",
            subtitle: "Rajkot-Morbi Highway, Rajkot, Gujarat",
            lat: 22.3551,
            lng: 70.8055,
            type: "transit",
            tag: "Highway"
        },
        {
            name: "Morbi City Center",
            subtitle: "Sanala Road, Morbi, Gujarat",
            lat: 22.8120,
            lng: 70.8350,
            type: "city",
            tag: "City"
        },
        {
            name: "Jamnagar Digjam Circle",
            subtitle: "Lal Bungalow, Jamnagar, Gujarat",
            lat: 22.4707,
            lng: 70.0577,
            type: "city",
            tag: "City"
        },
        {
            name: "Junagadh Majevadi Gate",
            subtitle: "ST Bus Depot, Junagadh, Gujarat",
            lat: 21.5222,
            lng: 70.4579,
            type: "city",
            tag: "City"
        },
        {
            name: "Gondal Swaminarayan Temple",
            subtitle: "Akshar Mandir, Gondal, Gujarat",
            lat: 21.9619,
            lng: 70.7997,
            type: "landmark",
            tag: "Town"
        },
        {
            name: "Ahmedabad SG Highway",
            subtitle: "Thaltej, SG Highway, Ahmedabad, Gujarat",
            lat: 23.0504,
            lng: 72.5074,
            type: "city",
            tag: "Metro"
        },
        {
            name: "Ahmedabad Geeta Mandir Central Bus Stand",
            subtitle: "ST Bus Stand, Geeta Mandir, Ahmedabad, Gujarat",
            lat: 23.0125,
            lng: 72.5894,
            type: "transit",
            tag: "Bus Stand"
        },
        {
            name: "Vadodara Central Railway Station",
            subtitle: "Alkapuri, Vadodara, Gujarat",
            lat: 22.3106,
            lng: 73.1812,
            type: "transit",
            tag: "Station"
        },
        {
            name: "Surat Central Station",
            subtitle: "Station Road, Varachha, Surat, Gujarat",
            lat: 21.2052,
            lng: 72.8406,
            type: "transit",
            tag: "Station"
        }
    ];

    function getPlaceIcon(type) {
        switch (type) {
            case 'university':
                return '🎓';
            case 'transit':
                return '🚆';
            case 'airport':
                return '✈️';
            case 'mall':
                return '🛍️';
            case 'city':
                return '🏙️';
            default:
                return '📍';
        }
    }

    function highlightMatch(text, query) {
        if (!query) return escapeHtml(text);
        const safeQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const regex = new RegExp(`(${safeQuery})`, 'gi');
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
                debounceMs: 180,
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
            // Disable native browser autocomplete
            this.input.setAttribute('autocomplete', 'off');
            this.input.setAttribute('spellcheck', 'false');

            // Find or create wrapper with relative positioning
            let wrapper = this.input.closest('.console-input-wrap, .input-wrapper, .form-group, .position-relative');
            if (!wrapper) {
                wrapper = this.input.parentElement;
                if (getComputedStyle(wrapper).position === 'static') {
                    wrapper.style.position = 'relative';
                }
            }

            // Create dropdown container
            this.dropdown = document.createElement('ul');
            this.dropdown.className = 'place-autocomplete-dropdown';
            this.dropdown.setAttribute('role', 'listbox');
            this.dropdown.setAttribute('aria-label', 'Location suggestions');
            wrapper.appendChild(this.dropdown);

            // Bind events
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
            const queryLower = query.toLowerCase();

            // 1. Instant local matches
            const localMatches = LOCAL_PLACES.filter(p => {
                return p.name.toLowerCase().includes(queryLower) ||
                       p.subtitle.toLowerCase().includes(queryLower);
            }).map(p => ({
                name: p.name,
                subtitle: p.subtitle,
                lat: p.lat,
                lng: p.lng,
                type: p.type,
                tag: p.tag || 'Popular'
            }));

            // Display instant local matches immediately
            if (localMatches.length > 0) {
                this.currentResults = localMatches.slice(0, this.options.maxResults);
                this.render(query);
            }

            // 2. Fetch live results from Photon (OSM search-as-you-type)
            if (this.abortController) {
                this.abortController.abort();
            }
            this.abortController = new AbortController();

            try {
                const photonUrl = `https://photon.komoot.io/api/?q=${encodeURIComponent(query)}&limit=6&lat=${this.options.biasLat}&lon=${this.options.biasLng}&lang=en`;
                const response = await fetch(photonUrl, {
                    signal: this.abortController.signal,
                    headers: { 'Accept': 'application/json' }
                });

                if (response.ok) {
                    const data = await response.json();
                    const remoteResults = (data.features || []).map(f => {
                        const p = f.properties || {};
                        const coords = f.geometry && f.geometry.coordinates ? f.geometry.coordinates : [0, 0];
                        const name = p.name || p.street || p.city || query;
                        
                        // Build informative subtitle
                        const parts = [
                            p.street,
                            p.district,
                            p.city,
                            p.state,
                            p.country
                        ].filter(Boolean);
                        
                        // Deduplicate parts
                        const uniqueParts = [...new Set(parts)].filter(part => part !== name);
                        const subtitle = uniqueParts.join(', ') || 'India';

                        return {
                            name: name,
                            subtitle: subtitle,
                            lat: coords[1],
                            lng: coords[0],
                            type: p.osm_value === 'university' || p.osm_value === 'college' ? 'university' : 
                                  p.osm_value === 'station' || p.osm_value === 'bus_stop' ? 'transit' : 'landmark',
                            tag: p.city || 'Place'
                        };
                    });

                    // Merge and deduplicate by clean name
                    const seenNames = new Set(localMatches.map(m => m.name.toLowerCase()));
                    const combined = [...localMatches];

                    for (const r of remoteResults) {
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
                    // Fallback to local matches if remote fails
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

            if (this.currentResults.length === 0) {
                this.hide();
                return;
            }

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
                    e.preventDefault(); // Prevent input blur before click
                    this.select(place);
                });

                li.addEventListener('mouseenter', () => {
                    this.setActive(index);
                });

                this.dropdown.appendChild(li);
            });

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
                }
            } else if (e.key === 'Escape') {
                this.hide();
            }
        }

        select(place) {
            // Update input value with clean location name
            this.input.value = place.name;

            // Update hidden coordinate inputs if configured
            if (this.options.latInputId) {
                const latEl = document.getElementById(this.options.latInputId);
                if (latEl) latEl.value = place.lat;
            }
            if (this.options.lngInputId) {
                const lngEl = document.getElementById(this.options.lngInputId);
                if (lngEl) lngEl.value = place.lng;
            }

            // Custom event for reactive UI/Map updates
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

            // Execute onSelect callback if provided
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

    // Export globally
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
